using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Interfaces.Repositories;
using TaxOmbud.Application.Interfaces.Services;
using TaxOmbud.Application.KnowledgeCenter.DTOs;
using TaxOmbud.Common.Responses;
using TaxOmbud.Domain.Entities.Knowledge;

namespace TaxOmbud.Application.Services;

public class KnowledgeCenterService : IKnowledgeCenterService
{
    private readonly IGenericRepository<KnowledgeCategory> _categoryRepo;
    private readonly IGenericRepository<KnowledgeTopic> _topicRepo;

    public KnowledgeCenterService(
        IGenericRepository<KnowledgeCategory> categoryRepo,
        IGenericRepository<KnowledgeTopic> topicRepo)
    {
        _categoryRepo = categoryRepo;
        _topicRepo = topicRepo;
    }

    public async Task<Response<IReadOnlyList<CategoryDto>>> GetCategoriesAsync(CancellationToken cancellationToken = default)
    {
        var response = new Response<IReadOnlyList<CategoryDto>>();
        try
        {
            var list = await _categoryRepo.Query()
                .Include(c => c.Topics)
                .OrderBy(c => c.Name)
                .ToListAsync(cancellationToken);

            var dtos = list.Select(c => new CategoryDto(
                c.Id,
                c.Name,
                c.Slug,
                c.Description,
                c.Topics.Count(t => !t.IsDeleted)
            )).ToList();

            response.StatusCode = StatusCodes.Status200OK;
            response.Data = dtos;
            response.Message = "Categories retrieved successfully.";
        }
        catch (Exception ex)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = ex.Message;
        }
        return response;
    }

    public async Task<Response<CategoryDto>> CreateCategoryAsync(CreateCategoryCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<CategoryDto>();
        try
        {
            var slug = request.Name.ToLower().Replace(" ", "-").Replace("/", "-");
            var cat = new KnowledgeCategory
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Slug = slug,
                Description = request.Description,
                CreatedAt = DateTime.UtcNow
            };

            await _categoryRepo.AddAsync(cat);
            await _categoryRepo.SaveAsync();

            response.StatusCode = StatusCodes.Status200OK;
            response.Data = new CategoryDto(cat.Id, cat.Name, cat.Slug, cat.Description, 0);
            response.Message = "Category created successfully.";
        }
        catch (Exception ex)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = ex.Message;
        }
        return response;
    }

    public async Task<Response<CategoryDto>> UpdateCategoryAsync(Guid id, UpdateCategoryCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<CategoryDto>();
        try
        {
            var cat = await _categoryRepo.Query()
                .Include(c => c.Topics)
                .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

            if (cat == null)
            {
                response.StatusCode = StatusCodes.Status404NotFound;
                response.Message = "Category not found.";
                return response;
            }

            var slug = request.Name.ToLower().Replace(" ", "-").Replace("/", "-");
            cat.Name = request.Name;
            cat.Slug = slug;
            cat.Description = request.Description;
            cat.LastModifiedAt = DateTime.UtcNow;

            await _categoryRepo.UpdateAsync(cat);
            await _categoryRepo.SaveAsync();

            response.StatusCode = StatusCodes.Status200OK;
            response.Data = new CategoryDto(cat.Id, cat.Name, cat.Slug, cat.Description, cat.Topics.Count(t => !t.IsDeleted));
            response.Message = "Category updated successfully.";
        }
        catch (Exception ex)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = ex.Message;
        }
        return response;
    }

    public async Task<Response<object?>> DeleteCategoryAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var response = new Response<object?>();
        try
        {
            var cat = await _categoryRepo.GetByIdAsync(id);
            if (cat == null)
            {
                response.StatusCode = StatusCodes.Status404NotFound;
                response.Message = "Category not found.";
                return response;
            }

            await _categoryRepo.RemoveAsync(cat);
            await _categoryRepo.SaveAsync();

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Category deleted successfully.";
        }
        catch (Exception ex)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = ex.Message;
        }
        return response;
    }

    public async Task<Response<IReadOnlyList<TopicDto>>> GetTopicsByCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        var response = new Response<IReadOnlyList<TopicDto>>();
        try
        {
            var list = await _topicRepo.Query()
                .Where(t => t.CategoryId == categoryId)
                .OrderBy(t => t.Title)
                .ToListAsync(cancellationToken);

            var dtos = list.Select(t => new TopicDto(
                t.Id,
                t.CategoryId,
                t.Title,
                t.Body,
                !string.IsNullOrWhiteSpace(t.TagsJson) ? JsonSerializer.Deserialize<List<string>>(t.TagsJson) ?? new List<string>() : new List<string>()
            )).ToList();

            response.StatusCode = StatusCodes.Status200OK;
            response.Data = dtos;
            response.Message = "Topics retrieved successfully.";
        }
        catch (Exception ex)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = ex.Message;
        }
        return response;
    }

    public async Task<Response<TopicDto>> CreateTopicAsync(Guid categoryId, CreateTopicCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<TopicDto>();
        try
        {
            var topic = new KnowledgeTopic
            {
                Id = Guid.NewGuid(),
                CategoryId = categoryId,
                Title = request.Title,
                Body = request.Body,
                TagsJson = request.Tags != null ? JsonSerializer.Serialize(request.Tags) : null,
                CreatedAt = DateTime.UtcNow
            };

            await _topicRepo.AddAsync(topic);
            await _topicRepo.SaveAsync();

            response.StatusCode = StatusCodes.Status200OK;
            response.Data = new TopicDto(topic.Id, topic.CategoryId, topic.Title, topic.Body, request.Tags ?? new List<string>());
            response.Message = "Topic created successfully.";
        }
        catch (Exception ex)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = ex.Message;
        }
        return response;
    }

    public async Task<Response<TopicDto>> UpdateTopicAsync(Guid categoryId, Guid topicId, UpdateTopicCommand request, CancellationToken cancellationToken = default)
    {
        var response = new Response<TopicDto>();
        try
        {
            var topic = await _topicRepo.FindAsync(t => t.CategoryId == categoryId && t.Id == topicId);
            if (topic == null)
            {
                response.StatusCode = StatusCodes.Status404NotFound;
                response.Message = "Topic not found.";
                return response;
            }

            topic.Title = request.Title;
            topic.Body = request.Body;
            topic.TagsJson = request.Tags != null ? JsonSerializer.Serialize(request.Tags) : null;
            topic.LastModifiedAt = DateTime.UtcNow;

            await _topicRepo.UpdateAsync(topic);
            await _topicRepo.SaveAsync();

            response.StatusCode = StatusCodes.Status200OK;
            response.Data = new TopicDto(topic.Id, topic.CategoryId, topic.Title, topic.Body, request.Tags ?? new List<string>());
            response.Message = "Topic updated successfully.";
        }
        catch (Exception ex)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = ex.Message;
        }
        return response;
    }

    public async Task<Response<object?>> DeleteTopicAsync(Guid categoryId, Guid topicId, CancellationToken cancellationToken = default)
    {
        var response = new Response<object?>();
        try
        {
            var topic = await _topicRepo.FindAsync(t => t.CategoryId == categoryId && t.Id == topicId);
            if (topic == null)
            {
                response.StatusCode = StatusCodes.Status404NotFound;
                response.Message = "Topic not found.";
                return response;
            }

            await _topicRepo.RemoveAsync(topic);
            await _topicRepo.SaveAsync();

            response.StatusCode = StatusCodes.Status200OK;
            response.Message = "Topic deleted successfully.";
        }
        catch (Exception ex)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = ex.Message;
        }
        return response;
    }

    public async Task<Response<IReadOnlyList<TopicDto>>> SearchTopicsAsync(string query, CancellationToken cancellationToken = default)
    {
        var response = new Response<IReadOnlyList<TopicDto>>();
        try
        {
            var q = query.ToLower();
            var list = await _topicRepo.Query()
                .Where(t => t.Title.ToLower().Contains(q) || t.Body.ToLower().Contains(q) || (t.TagsJson != null && t.TagsJson.ToLower().Contains(q)))
                .ToListAsync(cancellationToken);

            var dtos = list.Select(t => new TopicDto(
                t.Id,
                t.CategoryId,
                t.Title,
                t.Body,
                !string.IsNullOrWhiteSpace(t.TagsJson) ? JsonSerializer.Deserialize<List<string>>(t.TagsJson) ?? new List<string>() : new List<string>()
            )).ToList();

            response.StatusCode = StatusCodes.Status200OK;
            response.Data = dtos;
            response.Message = "Topics searched successfully.";
        }
        catch (Exception ex)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = ex.Message;
        }
        return response;
    }
}

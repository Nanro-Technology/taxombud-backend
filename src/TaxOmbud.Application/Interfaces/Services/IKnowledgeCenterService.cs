using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TaxOmbud.Application.KnowledgeCenter.DTOs;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Interfaces.Services;

public interface IKnowledgeCenterService
{
    // Categories
    Task<Response<IReadOnlyList<CategoryDto>>> GetCategoriesAsync(CancellationToken cancellationToken = default);
    Task<Response<CategoryDto>> CreateCategoryAsync(CreateCategoryCommand request, CancellationToken cancellationToken = default);
    Task<Response<CategoryDto>> UpdateCategoryAsync(Guid id, UpdateCategoryCommand request, CancellationToken cancellationToken = default);
    Task<Response<object?>> DeleteCategoryAsync(Guid id, CancellationToken cancellationToken = default);

    // Topics under a category
    Task<Response<IReadOnlyList<TopicDto>>> GetTopicsByCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default);
    Task<Response<TopicDto>> CreateTopicAsync(Guid categoryId, CreateTopicCommand request, CancellationToken cancellationToken = default);
    Task<Response<TopicDto>> UpdateTopicAsync(Guid categoryId, Guid topicId, UpdateTopicCommand request, CancellationToken cancellationToken = default);
    Task<Response<object?>> DeleteTopicAsync(Guid categoryId, Guid topicId, CancellationToken cancellationToken = default);

    // Global Search
    Task<Response<IReadOnlyList<TopicDto>>> SearchTopicsAsync(string query, CancellationToken cancellationToken = default);
}

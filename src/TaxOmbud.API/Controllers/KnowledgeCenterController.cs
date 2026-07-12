using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TaxOmbud.Application.Interfaces.Services;
using TaxOmbud.Application.KnowledgeCenter.DTOs;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Api.Controllers;

[ApiController]
[Route("api/v1/knowledge-center")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Produces("application/json")]
public class KnowledgeCenterController : ControllerBase
{
    private readonly IKnowledgeCenterService _knowledgeCenterService;

    public KnowledgeCenterController(IKnowledgeCenterService knowledgeCenterService)
    {
        _knowledgeCenterService = knowledgeCenterService;
    }

    /// <summary>Get all knowledge center categories.</summary>
    [HttpGet("categories")]
    [ProducesResponseType(typeof(Response<IReadOnlyList<CategoryDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCategories(CancellationToken ct)
    {
        var result = await _knowledgeCenterService.GetCategoriesAsync(ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Create a new knowledge center category.</summary>
    [HttpPost("categories")]
    [ProducesResponseType(typeof(Response<CategoryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryCommand command, CancellationToken ct)
    {
        var result = await _knowledgeCenterService.CreateCategoryAsync(command, ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Update an existing category.</summary>
    [HttpPut("categories/{id:guid}")]
    [ProducesResponseType(typeof(Response<CategoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateCategory(Guid id, [FromBody] UpdateCategoryCommand command, CancellationToken ct)
    {
        var result = await _knowledgeCenterService.UpdateCategoryAsync(id, command, ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Delete a category.</summary>
    [HttpDelete("categories/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteCategory(Guid id, CancellationToken ct)
    {
        var result = await _knowledgeCenterService.DeleteCategoryAsync(id, ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Get all topics belonging to a category.</summary>
    [HttpGet("categories/{categoryId:guid}/topics")]
    [ProducesResponseType(typeof(Response<IReadOnlyList<TopicDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTopicsByCategory(Guid categoryId, CancellationToken ct)
    {
        var result = await _knowledgeCenterService.GetTopicsByCategoryAsync(categoryId, ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Create a new topic under a category.</summary>
    [HttpPost("categories/{categoryId:guid}/topics")]
    [ProducesResponseType(typeof(Response<TopicDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateTopic(Guid categoryId, [FromBody] CreateTopicCommand command, CancellationToken ct)
    {
        var result = await _knowledgeCenterService.CreateTopicAsync(categoryId, command, ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Update an existing topic.</summary>
    [HttpPut("categories/{categoryId:guid}/topics/{topicId:guid}")]
    [ProducesResponseType(typeof(Response<TopicDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateTopic(Guid categoryId, Guid topicId, [FromBody] UpdateTopicCommand command, CancellationToken ct)
    {
        var result = await _knowledgeCenterService.UpdateTopicAsync(categoryId, topicId, command, ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Delete a topic.</summary>
    [HttpDelete("categories/{categoryId:guid}/topics/{topicId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTopic(Guid categoryId, Guid topicId, CancellationToken ct)
    {
        var result = await _knowledgeCenterService.DeleteTopicAsync(categoryId, topicId, ct);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>Global search for topics matching query.</summary>
    [HttpGet("search")]
    [ProducesResponseType(typeof(Response<IReadOnlyList<TopicDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchTopics([FromQuery] string query, CancellationToken ct)
    {
        var result = await _knowledgeCenterService.SearchTopicsAsync(query, ct);
        return StatusCode(result.StatusCode, result);
    }
}

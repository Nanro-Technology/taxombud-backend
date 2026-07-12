using System;
using System.Collections.Generic;

namespace TaxOmbud.Application.KnowledgeCenter.DTOs;

public record CategoryDto(
    Guid Id,
    string Name,
    string Slug,
    string Description,
    int TopicsCount
);

public record CreateCategoryCommand(
    string Name,
    string Description
);

public record UpdateCategoryCommand(
    string Name,
    string Description
);

public record TopicDto(
    Guid Id,
    Guid CategoryId,
    string Title,
    string Body,
    List<string> Tags
);

public record CreateTopicCommand(
    string Title,
    string Body,
    List<string> Tags
);

public record UpdateTopicCommand(
    string Title,
    string Body,
    List<string> Tags
);

using System;
using System.Collections.Generic;

namespace TaxOmbud.Application.SecuredFiling.DTOs;

public class FilingFolderDto
{
    public Guid Id { get; set; }
    public string FolderCode { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Category { get; set; } = null!;
    public string Priority { get; set; } = "normal";
    public string Confidentiality { get; set; } = "normal";
    public string Dept { get; set; } = null!;
    public string Description { get; set; } = string.Empty;
    public string IntakeMethod { get; set; } = "internal";
    public string SenderName { get; set; } = string.Empty;
    public string SenderOrg { get; set; } = string.Empty;
    public string SenderRef { get; set; } = string.Empty;
    public string InternalRef { get; set; } = string.Empty;
    public string Status { get; set; } = "active";
    public DateTime CreatedAt { get; set; }
}

public class FilingDocumentDto
{
    public Guid Id { get; set; }
    public Guid FolderId { get; set; }
    public string Name { get; set; } = null!;
    public string Size { get; set; } = "0 B";
    public string Type { get; set; } = "PDF";
    public string OcrStatus { get; set; } = "pending";
    public string OcrText { get; set; } = string.Empty;
    public string Sender { get; set; } = string.Empty;
    public string SenderOrg { get; set; } = string.Empty;
    public string SenderRef { get; set; } = string.Empty;
    public string InternalRef { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class FilingCategoryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = "active";
    public DateTime CreatedAt { get; set; }
}

public class FilingInboxRoutingDto
{
    public Guid Id { get; set; }
    public Guid FolderId { get; set; }
    public string FolderCode { get; set; } = null!;
    public string FolderName { get; set; } = null!;
    public string Priority { get; set; } = "normal";
    public string SentBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string Instruction { get; set; } = string.Empty;
    public string Status { get; set; } = "to_acknowledge";
    public string RejectionReason { get; set; } = string.Empty;
}

public class CreateFolderRequest
{
    public string Name { get; set; } = null!;
    public string Category { get; set; } = "General";
    public string Priority { get; set; } = "normal";
    public string Confidentiality { get; set; } = "normal";
    public string Dept { get; set; } = "HQ";
    public string Description { get; set; } = string.Empty;
    public string IntakeMethod { get; set; } = "internal";
    public string SenderName { get; set; } = string.Empty;
    public string SenderOrg { get; set; } = string.Empty;
    public string SenderRef { get; set; } = string.Empty;
    public string InternalRef { get; set; } = string.Empty;
}

public class CreateCategoryRequest
{
    public string Name { get; set; } = null!;
    public string Description { get; set; } = string.Empty;
}

public class RejectRoutingRequest
{
    public string Reason { get; set; } = null!;
}

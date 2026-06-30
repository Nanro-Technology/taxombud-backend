using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

class Program
{
    static string rootDir = @"c:\Users\HP\OneDrive\Desktop\PNC\taxombud-backend";
    static string appDir = Path.Combine(rootDir, "src", "TaxOmbud.Application");
    static string featuresDir = Path.Combine(appDir, "Features");
    static string apiDir = Path.Combine(rootDir, "src", "TaxOmbud.API");
    static string controllersDir = Path.Combine(apiDir, "Controllers");

    public class Dependency
    {
        public string Type { get; set; } = "";
        public string Name { get; set; } = "";
    }

    public class ActionInfo
    {
        public string FilePath { get; set; } = "";
        public string ModuleName { get; set; } = "";
        public string ActionName { get; set; } = "";
        public string RequestTypeName { get; set; } = "";
        public string ResponseTypeName { get; set; } = "";
        public string InnerResponseType { get; set; } = "";
        public bool IsResultType { get; set; }
        public List<string> Usings { get; set; } = new();
        public string RequestBlock { get; set; } = "";
        public string ValidatorBlock { get; set; } = "";
        public string HandlerClassName { get; set; } = "";
        public List<Dependency> Dependencies { get; set; } = new();
        public string HandleMethodBody { get; set; } = "";
    }

    static void Main()
    {
        Console.WriteLine("Starting Automated Architecture Restructure...");
        if (!Directory.Exists(featuresDir))
        {
            Console.WriteLine($"Error: Features directory not found at {featuresDir}");
            return;
        }

        var actions = new List<ActionInfo>();
        var files = Directory.GetFiles(featuresDir, "*.cs", SearchOption.AllDirectories);

        Console.WriteLine($"Found {files.Length} C# files in Features.");

        foreach (var file in files)
        {
            try
            {
                var action = ParseFeatureFile(file);
                if (action != null)
                {
                    actions.Add(action);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing {file}: {ex.Message}");
            }
        }

        Console.WriteLine($"Parsed {actions.Count} actions successfully.");

        // Group actions by module
        var groupedActions = actions.GroupBy(a => a.ModuleName).ToList();

        // Map request types to module names for Controller migration later
        var requestToModuleMap = new Dictionary<string, string>();
        foreach (var action in actions)
        {
            requestToModuleMap[action.RequestTypeName] = action.ModuleName;
        }

        // Migrate each module
        foreach (var group in groupedActions)
        {
            string moduleName = group.Key;
            Console.WriteLine($"Restructuring module: {moduleName}");
            RestructureModule(moduleName, group.ToList());
        }

        // Migrate controllers
        MigrateControllers(requestToModuleMap);

        Console.WriteLine("Architecture Restructure Completed Successfully!");
    }

    static ActionInfo? ParseFeatureFile(string filePath)
    {
        string fileText = File.ReadAllText(filePath);
        string relativePath = Path.GetRelativePath(featuresDir, filePath);
        var pathParts = relativePath.Split(Path.DirectorySeparatorChar);
        if (pathParts.Length < 2) return null;

        string moduleName = pathParts[0];
        string fileName = Path.GetFileNameWithoutExtension(filePath);

        // Group lines into sections
        var lines = File.ReadAllLines(filePath);
        int currentSection = 0; // 0 = Usings/Header, 1 = Request, 2 = Validator, 3 = Handler
        var usingsList = new List<string>();
        var requestLines = new List<string>();
        var validatorLines = new List<string>();
        var handlerLines = new List<string>();

        foreach (var line in lines)
        {
            string trimmed = line.Trim();
            if (trimmed.StartsWith("// ─── Command") || trimmed.StartsWith("// ─── Query"))
            {
                currentSection = 1;
                continue;
            }
            else if (trimmed.StartsWith("// ─── Validator"))
            {
                currentSection = 2;
                continue;
            }
            else if (trimmed.StartsWith("// ─── Handler"))
            {
                currentSection = 3;
                continue;
            }

            if (currentSection == 0)
            {
                if (trimmed.StartsWith("using ") && trimmed.EndsWith(";"))
                {
                    usingsList.Add(trimmed);
                }
            }
            else if (currentSection == 1)
            {
                requestLines.Add(line);
            }
            else if (currentSection == 2)
            {
                validatorLines.Add(line);
            }
            else if (currentSection == 3)
            {
                handlerLines.Add(line);
            }
        }

        string requestBlock = string.Join("\n", requestLines).Trim();
        string validatorBlock = string.Join("\n", validatorLines).Trim();
        string handlerBlock = string.Join("\n", handlerLines).Trim();

        if (string.IsNullOrEmpty(requestBlock) || string.IsNullOrEmpty(handlerBlock))
        {
            // Try fallback if no delimiters are present
            return ParseFallback(fileText, filePath, moduleName);
        }

        // Parse Request Name and Response Type
        if (!ParseRequestDeclaration(requestBlock, out string requestName, out string responseType))
        {
            return null;
        }

        bool isResultType = false;
        string innerResponseType = responseType;
        if (responseType.StartsWith("Result<") && responseType.EndsWith(">"))
        {
            isResultType = true;
            innerResponseType = responseType.Substring(7, responseType.Length - 8);
        }
        else if (responseType == "Result")
        {
            isResultType = true;
            innerResponseType = "object?";
        }

        if (innerResponseType == "Unit")
        {
            innerResponseType = "object?";
        }

        // Parse Handler Name, Dependencies and Method Body
        if (!ParseHandlerDeclaration(handlerBlock, requestName, out string handlerName, out List<Dependency> dependencies, out string handleMethodBody))
        {
            return null;
        }

        // Extract action name (Request name minus Command/Query)
        string actionName = requestName;
        if (actionName.EndsWith("Command")) actionName = actionName.Substring(0, actionName.Length - 7);
        else if (actionName.EndsWith("Query")) actionName = actionName.Substring(0, actionName.Length - 5);
        else if (actionName.EndsWith("Commands")) actionName = actionName.Substring(0, actionName.Length - 8);
        else if (actionName.EndsWith("Queries")) actionName = actionName.Substring(0, actionName.Length - 7);

        return new ActionInfo
        {
            FilePath = filePath,
            ModuleName = moduleName,
            ActionName = actionName,
            RequestTypeName = requestName,
            ResponseTypeName = responseType,
            InnerResponseType = innerResponseType,
            IsResultType = isResultType,
            Usings = usingsList,
            RequestBlock = requestBlock,
            ValidatorBlock = validatorBlock,
            HandlerClassName = handlerName,
            Dependencies = dependencies,
            HandleMethodBody = handleMethodBody
        };
    }

    static ActionInfo? ParseFallback(string fileText, string filePath, string moduleName)
    {
        // Simple fallback parsing for files that don't have separators
        if (!fileText.Contains("IRequestHandler")) return null;

        // Try to locate request and handler via regex or index search
        int iRequestIdx = fileText.IndexOf("IRequest");
        if (iRequestIdx == -1) return null;

        int recordIdx = fileText.LastIndexOf("record", iRequestIdx);
        int classIdx = fileText.LastIndexOf("class", iRequestIdx);
        int typeIdx = Math.Max(recordIdx, classIdx);
        if (typeIdx == -1) return null;

        // Extract Request name
        int nameStart = typeIdx + (recordIdx > classIdx ? 6 : 5);
        while (nameStart < fileText.Length && char.IsWhiteSpace(fileText[nameStart])) nameStart++;
        int nameEnd = nameStart;
        while (nameEnd < fileText.Length && (char.IsLetterOrDigit(fileText[nameEnd]) || fileText[nameEnd] == '_')) nameEnd++;
        string requestName = fileText.Substring(nameStart, nameEnd - nameStart);

        // Find Response Type
        int openBracket = fileText.IndexOf('<', iRequestIdx);
        string responseType = "object?";
        if (openBracket != -1)
        {
            int bracketCount = 1;
            int idx = openBracket + 1;
            while (idx < fileText.Length && bracketCount > 0)
            {
                if (fileText[idx] == '<') bracketCount++;
                else if (fileText[idx] == '>') bracketCount--;
                idx++;
            }
            responseType = fileText.Substring(openBracket + 1, idx - openBracket - 2);
        }

        bool isResultType = false;
        string innerResponseType = responseType;
        if (responseType.StartsWith("Result<") && responseType.EndsWith(">"))
        {
            isResultType = true;
            innerResponseType = responseType.Substring(7, responseType.Length - 8);
        }
        else if (responseType == "Result")
        {
            isResultType = true;
            innerResponseType = "object?";
        }

        if (innerResponseType == "Unit")
        {
            innerResponseType = "object?";
        }

        // Find handler
        int handlerIdx = fileText.IndexOf("IRequestHandler");
        int handlerClassIdx = fileText.LastIndexOf("class", handlerIdx);
        int handlerNameStart = handlerClassIdx + 5;
        while (handlerNameStart < fileText.Length && char.IsWhiteSpace(fileText[handlerNameStart])) handlerNameStart++;
        int handlerNameEnd = handlerNameStart;
        while (handlerNameEnd < fileText.Length && (char.IsLetterOrDigit(fileText[handlerNameEnd]) || fileText[handlerNameEnd] == '_')) handlerNameEnd++;
        string handlerName = fileText.Substring(handlerNameStart, handlerNameEnd - handlerNameStart);

        // Simple body extraction
        int handleIdx = fileText.IndexOf("public async Task");
        if (handleIdx == -1)
        {
            handleIdx = fileText.IndexOf("public Task");
        }
        string handleMethodBody = "";
        if (handleIdx != -1)
        {
            int openBrace = fileText.IndexOf('{', handleIdx);
            if (openBrace != -1)
            {
                int braceCount = 1;
                int idx = openBrace + 1;
                while (idx < fileText.Length && braceCount > 0)
                {
                    if (fileText[idx] == '{') braceCount++;
                    else if (fileText[idx] == '}') braceCount--;
                    idx++;
                }
                handleMethodBody = fileText.Substring(openBrace, idx - openBrace);
            }
        }

        // Extract the request declaration block from typeIdx
        int endIdx = -1;
        int nextOpenBrace = fileText.IndexOf('{', typeIdx);
        int nextSemicolon = fileText.IndexOf(';', typeIdx);
        if (nextSemicolon != -1 && (nextOpenBrace == -1 || nextSemicolon < nextOpenBrace))
        {
            endIdx = nextSemicolon + 1;
        }
        else if (nextOpenBrace != -1)
        {
            int braceCount = 1;
            int ptr = nextOpenBrace + 1;
            while (ptr < fileText.Length && braceCount > 0)
            {
                if (fileText[ptr] == '{') braceCount++;
                else if (fileText[ptr] == '}') braceCount--;
                ptr++;
            }
            endIdx = ptr;
        }
        string requestBlock = endIdx != -1 ? fileText.Substring(typeIdx, endIdx - typeIdx) : fileText;

        // Extract action name
        string actionName = requestName;
        if (actionName.EndsWith("Command")) actionName = actionName.Substring(0, actionName.Length - 7);
        else if (actionName.EndsWith("Query")) actionName = actionName.Substring(0, actionName.Length - 5);
        else if (actionName.EndsWith("Commands")) actionName = actionName.Substring(0, actionName.Length - 8);
        else if (actionName.EndsWith("Queries")) actionName = actionName.Substring(0, actionName.Length - 7);

        // dependencies
        var dependencies = new List<Dependency>();
        string constructorMarker = $"public {handlerName}(";
        int constructorIdx = fileText.IndexOf(constructorMarker);
        if (constructorIdx != -1)
        {
            int openParen = constructorIdx + constructorMarker.Length - 1;
            int closeParen = fileText.IndexOf(')', openParen);
            if (closeParen != -1)
            {
                string paramsStr = fileText.Substring(openParen + 1, closeParen - openParen - 1).Trim();
                if (!string.IsNullOrEmpty(paramsStr))
                {
                    var parts = paramsStr.Split(',');
                    foreach (var part in parts)
                    {
                        var pTrim = part.Trim();
                        int lastSpace = pTrim.LastIndexOf(' ');
                        if (lastSpace != -1)
                        {
                            string type = pTrim.Substring(0, lastSpace).Trim();
                            string name = pTrim.Substring(lastSpace + 1).Trim();
                            dependencies.Add(new Dependency { Type = type, Name = name });
                        }
                    }
                }
            }
        }

        // Split usings
        var usingsList = new List<string>();
        foreach (var line in fileText.Split('\n'))
        {
            string trimmed = line.Trim();
            if (trimmed.StartsWith("using ") && trimmed.EndsWith(";"))
            {
                usingsList.Add(trimmed);
            }
        }

        return new ActionInfo
        {
            FilePath = filePath,
            ModuleName = moduleName,
            ActionName = actionName,
            RequestTypeName = requestName,
            ResponseTypeName = responseType,
            InnerResponseType = innerResponseType,
            IsResultType = isResultType,
            Usings = usingsList,
            RequestBlock = requestBlock,
            ValidatorBlock = "",
            HandlerClassName = handlerName,
            Dependencies = dependencies,
            HandleMethodBody = handleMethodBody
        };
    }

    static bool ParseRequestDeclaration(string requestBlock, out string requestName, out string responseType)
    {
        requestName = "";
        responseType = "object?";

        int iRequestIdx = requestBlock.IndexOf("IRequest");
        if (iRequestIdx == -1) return false;

        int recordIdx = requestBlock.LastIndexOf("record", iRequestIdx);
        int classIdx = requestBlock.LastIndexOf("class", iRequestIdx);
        int typeIdx = Math.Max(recordIdx, classIdx);
        if (typeIdx == -1) return false;

        int nameStart = typeIdx + (recordIdx > classIdx ? 6 : 5);
        while (nameStart < requestBlock.Length && char.IsWhiteSpace(requestBlock[nameStart])) nameStart++;
        int nameEnd = nameStart;
        while (nameEnd < requestBlock.Length && (char.IsLetterOrDigit(requestBlock[nameEnd]) || requestBlock[nameEnd] == '_')) nameEnd++;
        requestName = requestBlock.Substring(nameStart, nameEnd - nameStart);

        int openBracket = requestBlock.IndexOf('<', iRequestIdx);
        if (openBracket != -1)
        {
            int bracketCount = 1;
            int idx = openBracket + 1;
            while (idx < requestBlock.Length && bracketCount > 0)
            {
                if (requestBlock[idx] == '<') bracketCount++;
                else if (requestBlock[idx] == '>') bracketCount--;
                idx++;
            }
            responseType = requestBlock.Substring(openBracket + 1, idx - openBracket - 2);
        }
        return true;
    }

    static bool ParseHandlerDeclaration(string handlerBlock, string requestName, out string handlerName, out List<Dependency> dependencies, out string handleMethodBody)
    {
        handlerName = "";
        dependencies = new List<Dependency>();
        handleMethodBody = "";

        int iRequestHandlerIdx = handlerBlock.IndexOf("IRequestHandler");
        if (iRequestHandlerIdx == -1) return false;

        int classIdx = handlerBlock.LastIndexOf("class", iRequestHandlerIdx);
        if (classIdx == -1) return false;

        int nameStart = classIdx + 5;
        while (nameStart < handlerBlock.Length && char.IsWhiteSpace(handlerBlock[nameStart])) nameStart++;
        int nameEnd = nameStart;
        while (nameEnd < handlerBlock.Length && (char.IsLetterOrDigit(handlerBlock[nameEnd]) || handlerBlock[nameEnd] == '_')) nameEnd++;
        handlerName = handlerBlock.Substring(nameStart, nameEnd - nameStart);

        string constructorMarker = $"public {handlerName}(";
        int constructorIdx = handlerBlock.IndexOf(constructorMarker);
        if (constructorIdx != -1)
        {
            int openParen = constructorIdx + constructorMarker.Length - 1;
            int closeParen = handlerBlock.IndexOf(')', openParen);
            if (closeParen != -1)
            {
                string paramsStr = handlerBlock.Substring(openParen + 1, closeParen - openParen - 1).Trim();
                if (!string.IsNullOrEmpty(paramsStr))
                {
                    var parts = paramsStr.Split(',');
                    foreach (var part in parts)
                    {
                        var pTrim = part.Trim();
                        int lastSpace = pTrim.LastIndexOf(' ');
                        if (lastSpace != -1)
                        {
                            string type = pTrim.Substring(0, lastSpace).Trim();
                            string name = pTrim.Substring(lastSpace + 1).Trim();
                            dependencies.Add(new Dependency { Type = type, Name = name });
                        }
                    }
                }
            }
        }

        int handleIdx = handlerBlock.IndexOf("public async Task");
        if (handleIdx == -1)
        {
            handleIdx = handlerBlock.IndexOf("public Task");
        }
        if (handleIdx != -1)
        {
            int openBrace = handlerBlock.IndexOf('{', handleIdx);
            if (openBrace != -1)
            {
                int braceCount = 1;
                int idx = openBrace + 1;
                while (idx < handlerBlock.Length && braceCount > 0)
                {
                    if (handlerBlock[idx] == '{') braceCount++;
                    else if (handlerBlock[idx] == '}') braceCount--;
                    idx++;
                }
                handleMethodBody = handlerBlock.Substring(openBrace, idx - openBrace);
            }
        }

        return true;
    }

    static void RestructureModule(string moduleName, List<ActionInfo> moduleActions)
    {
        string moduleDir = Path.Combine(appDir, moduleName);
        string dtosDir = Path.Combine(moduleDir, "DTOs");
        string validatorsDir = Path.Combine(moduleDir, "Validators");

        Directory.CreateDirectory(dtosDir);
        Directory.CreateDirectory(validatorsDir);

        // Process DTOs and Validators
        foreach (var action in moduleActions)
        {
            // Write DTO file
            string cleanRequestBlock = Regex.Replace(action.RequestBlock, @":\s*IRequest<[^;]*>", "");
            cleanRequestBlock = Regex.Replace(cleanRequestBlock, @":\s*IRequest", "");

            var filteredUsings = FilterUsings(action.Usings);
            string usingsText = string.Join("\n", filteredUsings);

            string dtoContent = $@"using System;
using System.Collections.Generic;
{usingsText}
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.{moduleName}.DTOs;

{cleanRequestBlock}
";
            string dtoPath = Path.Combine(dtosDir, $"{action.ActionName}Dto.cs");
            File.WriteAllText(dtoPath, dtoContent);

            // Write Validator file if exists
            if (!string.IsNullOrEmpty(action.ValidatorBlock))
            {
                string validatorContent = $@"using System;
using FluentValidation;
using TaxOmbud.Application.{moduleName}.DTOs;
{usingsText}

namespace TaxOmbud.Application.{moduleName}.Validators;

{action.ValidatorBlock}
";
                string valPath = Path.Combine(validatorsDir, $"{action.ActionName}Validator.cs");
                File.WriteAllText(valPath, validatorContent);
            }
        }

        // Collect all unique dependencies across all actions in this module
        var uniqueDeps = new Dictionary<string, Dependency>();
        foreach (var action in moduleActions)
        {
            foreach (var dep in action.Dependencies)
            {
                // Normalize type names
                string depType = dep.Type;
                if (depType == "ICurrentUser") depType = "ICurrentUser"; // Keep as is, let's use the new ICurrentUser
                
                if (!uniqueDeps.ContainsKey(depType))
                {
                    uniqueDeps[depType] = new Dependency { Type = depType, Name = dep.Name };
                }
            }
        }

        // Generate Service Interface
        GenerateServiceInterface(moduleName, moduleActions);

        // Generate Service Implementation
        GenerateServiceImplementation(moduleName, moduleActions, uniqueDeps.Values.ToList());
    }

    static List<string> FilterUsings(List<string> originalUsings)
    {
        var filtered = new List<string>();
        foreach (var u in originalUsings)
        {
            if (u.Contains("MediatR") || u.Contains("TaxOmbud.Application.Common.Models") || u.Contains("TaxOmbud.Application.Common.Interfaces"))
                continue;
            filtered.Add(u);
        }
        return filtered;
    }

    static void GenerateServiceInterface(string moduleName, List<ActionInfo> moduleActions)
    {
        string interfaceDir = Path.Combine(appDir, "Interfaces", "Services");
        Directory.CreateDirectory(interfaceDir);

        var sb = new StringBuilder();
        sb.AppendLine("using System;");
        sb.AppendLine("using System.Threading;");
        sb.AppendLine("using System.Threading.Tasks;");
        sb.AppendLine("using System.Collections.Generic;");
        sb.AppendLine("using TaxOmbud.Common.Responses;");
        sb.AppendLine($"using TaxOmbud.Application.{moduleName}.DTOs;");
        sb.AppendLine("using TaxOmbud.Domain.Entities.Hr;");
        sb.AppendLine("using TaxOmbud.Domain.Entities.Identity;");
        sb.AppendLine("using TaxOmbud.Domain.Entities.Taxpayers;");
        sb.AppendLine("using TaxOmbud.Domain.Entities.Officers;");
        sb.AppendLine("using TaxOmbud.Domain.Entities.Complaints;");
        sb.AppendLine("using TaxOmbud.Domain.Entities.Cases;");
        sb.AppendLine("using TaxOmbud.Domain.Entities.Documents;");
        sb.AppendLine("using TaxOmbud.Domain.Entities.Communications;");
        sb.AppendLine("using TaxOmbud.Domain.Entities.Appeals;");
        sb.AppendLine("using TaxOmbud.Domain.Entities.Appointments;");
        sb.AppendLine("using TaxOmbud.Domain.Entities.Notifications;");
        sb.AppendLine("using TaxOmbud.Domain.Entities.System;");
        sb.AppendLine();
        sb.AppendLine("namespace TaxOmbud.Application.Interfaces.Services;");
        sb.AppendLine();
        sb.AppendLine($"public interface I{moduleName}Service");
        sb.AppendLine("{");

        foreach (var action in moduleActions)
        {
            string returnType = action.IsResultType ? $"Response<{action.InnerResponseType}>" : action.ResponseTypeName;
            sb.AppendLine($"    Task<{returnType}> {action.ActionName}Async({action.RequestTypeName} request, CancellationToken cancellationToken = default);");
        }

        sb.AppendLine("}");

        string interfacePath = Path.Combine(interfaceDir, $"I{moduleName}Service.cs");
        File.WriteAllText(interfacePath, sb.ToString());
    }

    static void GenerateServiceImplementation(string moduleName, List<ActionInfo> moduleActions, List<Dependency> dependencies)
    {
        string serviceDir = Path.Combine(appDir, "Services");
        Directory.CreateDirectory(serviceDir);

        var sb = new StringBuilder();
        sb.AppendLine("using System;");
        sb.AppendLine("using System.Linq;");
        sb.AppendLine("using System.Threading;");
        sb.AppendLine("using System.Threading.Tasks;");
        sb.AppendLine("using System.Collections.Generic;");
        sb.AppendLine("using Microsoft.EntityFrameworkCore;");
        sb.AppendLine("using TaxOmbud.Common.Responses;");
        sb.AppendLine($"using TaxOmbud.Application.{moduleName}.DTOs;");
        sb.AppendLine("using TaxOmbud.Application.Interfaces.Persistence;");
        sb.AppendLine("using TaxOmbud.Application.Interfaces.InfrastructureService;");
        sb.AppendLine("using TaxOmbud.Application.Interfaces.Services;");
        sb.AppendLine("using TaxOmbud.Domain.Enums;");
        sb.AppendLine("using TaxOmbud.Domain.Entities.Hr;");
        sb.AppendLine("using TaxOmbud.Domain.Entities.Identity;");
        sb.AppendLine("using TaxOmbud.Domain.Entities.Taxpayers;");
        sb.AppendLine("using TaxOmbud.Domain.Entities.Officers;");
        sb.AppendLine("using TaxOmbud.Domain.Entities.Complaints;");
        sb.AppendLine("using TaxOmbud.Domain.Entities.Cases;");
        sb.AppendLine("using TaxOmbud.Domain.Entities.Documents;");
        sb.AppendLine("using TaxOmbud.Domain.Entities.Communications;");
        sb.AppendLine("using TaxOmbud.Domain.Entities.Appeals;");
        sb.AppendLine("using TaxOmbud.Domain.Entities.Appointments;");
        sb.AppendLine("using TaxOmbud.Domain.Entities.Notifications;");
        sb.AppendLine("using TaxOmbud.Domain.Entities.System;");

        // Add action-specific usings if any
        var allUsings = moduleActions.SelectMany(a => FilterUsings(a.Usings)).Distinct().ToList();
        foreach (var u in allUsings)
        {
            sb.AppendLine(u);
        }

        sb.AppendLine();
        sb.AppendLine("namespace TaxOmbud.Application.Services;");
        sb.AppendLine();
        sb.AppendLine($"public class {moduleName}Service : I{moduleName}Service");
        sb.AppendLine("{");

        // Fields
        foreach (var dep in dependencies)
        {
            string depType = dep.Type;
            if (depType == "ICurrentUser") depType = "ICurrentUser";
            sb.AppendLine($"    private readonly {depType} _{dep.Name};");
        }

        sb.AppendLine();

        // Constructor
        sb.AppendLine($"    public {moduleName}Service(");
        for (int i = 0; i < dependencies.Count; i++)
        {
            var dep = dependencies[i];
            string depType = dep.Type;
            if (depType == "ICurrentUser") depType = "ICurrentUser";
            string comma = i == dependencies.Count - 1 ? "" : ",";
            sb.AppendLine($"        {depType} {dep.Name}{comma}");
        }
        sb.AppendLine("    )");
        sb.AppendLine("    {");
        foreach (var dep in dependencies)
        {
            sb.AppendLine($"        _{dep.Name} = {dep.Name};");
        }
        sb.AppendLine("    }");
        sb.AppendLine();

        // Service Methods
        foreach (var action in moduleActions)
        {
            string returnType = action.IsResultType ? $"Response<{action.InnerResponseType}>" : action.ResponseTypeName;
            sb.AppendLine($"    public async Task<{returnType}> {action.ActionName}Async({action.RequestTypeName} request, CancellationToken cancellationToken = default)");

            // Translate method body from MediatR handler to Service method
            string translatedBody = TranslateHandleMethodBody(action.HandleMethodBody, action.InnerResponseType);
            sb.AppendLine(translatedBody);
            sb.AppendLine();
        }

        sb.AppendLine("}");

        string servicePath = Path.Combine(serviceDir, $"{moduleName}Service.cs");
        File.WriteAllText(servicePath, sb.ToString());
    }

    static string TranslateHandleMethodBody(string originalBody, string innerResponseType)
    {
        // Replacements inside the body to map Result<T> to Response<T>
        string body = originalBody;

        // MediatR specific replacements
        body = Regex.Replace(body, @"Result<Unit>\.Success\(Unit\.Value\)", $"Response<object?>.Success(null)");
        body = Regex.Replace(body, @"Result<Unit>\.Success\(\)", $"Response<object?>.Success(null)");
        body = Regex.Replace(body, @"Result<Unit>\.Failure", $"Response<object?>.Fail");
        body = Regex.Replace(body, @"Result<Unit>", $"Response<object?>");
        body = Regex.Replace(body, @"Unit\.Value", "null");
        body = Regex.Replace(body, @"Result<object\?>\.Success\(null\)", "Response<object?>.Success(null)");

        // Map Result<T>.Success(val) to Response<T>.Success(val)
        body = Regex.Replace(body, @"Result<.*?>\.Success\(", "Response<" + innerResponseType + ">.Success(");
        body = Regex.Replace(body, @"Result<.*?>\.Failure\(", "Response<" + innerResponseType + ">.Fail(");
        body = Regex.Replace(body, @"Result<.*?>\.NotFound\(", "Response<" + innerResponseType + ">.NotFound(");
        body = Regex.Replace(body, @"Result<.*?>\.Forbidden\(", "Response<" + innerResponseType + ">.Forbidden(");
        body = Regex.Replace(body, @"Result<.*?>\.Conflict\(", "Response<" + innerResponseType + ">.Fail("); // map conflict to fail

        // Generic result without template arg or with template arg but using generic Failure/Success
        body = Regex.Replace(body, @"Result\.Success\(", "Response<" + innerResponseType + ">.Success(");
        body = Regex.Replace(body, @"Result\.Failure\(", "Response<" + innerResponseType + ">.Fail(");
        body = Regex.Replace(body, @"Result\.NotFound\(", "Response<" + innerResponseType + ">.NotFound(");
        body = Regex.Replace(body, @"Result\.Forbidden\(", "Response<" + innerResponseType + ">.Forbidden(");

        // Replace other instances of Result<T> with Response<T>
        body = Regex.Replace(body, @"Result<([\w\.\<\>\[\]\?]+)>", "Response<$1>");

        return body;
    }

    static void MigrateControllers(Dictionary<string, string> requestToModuleMap)
    {
        Console.WriteLine("Migrating API Controllers to Direct Service Injection...");
        if (!Directory.Exists(controllersDir))
        {
            Console.WriteLine($"Error: Controllers directory not found at {controllersDir}");
            return;
        }

        var controllerFiles = Directory.GetFiles(controllersDir, "*.cs");
        foreach (var file in controllerFiles)
        {
            string fileName = Path.GetFileName(file);
            if (fileName == "ApiControllerBase.cs") continue;

            try
            {
                MigrateControllerFile(file, requestToModuleMap);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error migrating controller {fileName}: {ex.Message}");
            }
        }
    }

    static void MigrateControllerFile(string filePath, Dictionary<string, string> requestToModuleMap)
    {
        string fileText = File.ReadAllText(filePath);
        string fileName = Path.GetFileNameWithoutExtension(filePath);

        // Find all requests sent via Mediator in this file
        // E.g., _mediator.Send(new SetupMfaCommand(userId), ct)
        // or _mediator.Send(command, ct)
        // or Mediator.Send(...)
        var mediatorMatches = Regex.Matches(fileText, @"(await\s+)?_?mediator\.Send\(\s*(new\s+)?(\w+)(?:\(.*?\))?\s*,\s*(\w+)\s*\)", RegexOptions.Singleline);
        var modulesNeeded = new HashSet<string>();

        foreach (Match match in mediatorMatches)
        {
            string reqType = match.Groups[3].Value;
            if (requestToModuleMap.TryGetValue(reqType, out string? module))
            {
                modulesNeeded.Add(module);
            }
            else
            {
                // Try fallback logic if exact match not found (e.g. singular/plural or search)
                // Let's guess module based on controller name or namespaces in the file
                string guessedModule = GuessModuleFromRequest(reqType, fileText);
                if (guessedModule != "")
                {
                    modulesNeeded.Add(guessedModule);
                }
            }
        }

        if (modulesNeeded.Count == 0)
        {
            // If no mediator sends were matched but it's a controller, let's look at the class definition or usings
            // to guess which service it might need (e.g., AuthController -> Auth module)
            string guessedModule = GuessModuleFromControllerName(fileName);
            if (guessedModule != "")
            {
                modulesNeeded.Add(guessedModule);
            }
        }

        if (modulesNeeded.Count == 0)
        {
            return; // Nothing to migrate or no module match
        }

        // Prepare the new constructor, fields, and imports
        var servicesToInject = modulesNeeded.Select(m => new { Type = $"I{m}Service", Name = $"_{m.Substring(0, 1).ToLower()}{m.Substring(1)}Service", Field = $"_{m.Substring(0, 1).ToLower()}{m.Substring(1)}Service" }).ToList();

        // 1. Rewrite inheritance from ApiControllerBase to ControllerBase
        fileText = Regex.Replace(fileText, @":\s*ApiControllerBase", ": ControllerBase");

        // 2. Add services usings
        var serviceImports = new StringBuilder();
        serviceImports.AppendLine("using TaxOmbud.Application.Interfaces.Services;");
        foreach (var m in modulesNeeded)
        {
            serviceImports.AppendLine($"using TaxOmbud.Application.{m}.DTOs;");
        }
        
        // Find position to insert usings (after namespace or last using)
        int namespaceIdx = fileText.IndexOf("namespace ");
        if (namespaceIdx != -1)
        {
            int openBrace = fileText.IndexOf('{', namespaceIdx);
            if (openBrace != -1)
            {
                fileText = fileText.Insert(openBrace + 1, "\n" + serviceImports.ToString());
            }
        }

        // Remove old mediator fields and constructor
        // Find constructor
        string constructorPattern = @"public\s+" + fileName + @"\s*\((.*?)\)\s*{(.*?)}";
        var cMatch = Regex.Match(fileText, constructorPattern, RegexOptions.Singleline);
        if (cMatch.Success)
        {
            // Replace constructor and any mediator fields
            // Find class body open brace
            int classDeclIdx = fileText.IndexOf("class " + fileName);
            int classBraceIdx = fileText.IndexOf('{', classDeclIdx);

            // Let's rewrite the constructor and fields block.
            var fieldsAndConstructorSb = new StringBuilder();
            fieldsAndConstructorSb.AppendLine();
            foreach (var svc in servicesToInject)
            {
                fieldsAndConstructorSb.AppendLine($"    private readonly {svc.Type} {svc.Name};");
            }
            fieldsAndConstructorSb.AppendLine();
            fieldsAndConstructorSb.AppendLine($"    public {fileName}(");
            for (int i = 0; i < servicesToInject.Count; i++)
            {
                var svc = servicesToInject[i];
                string comma = i == servicesToInject.Count - 1 ? "" : ",";
                fieldsAndConstructorSb.AppendLine($"        {svc.Type} {svc.Name.Substring(1)}{comma}");
            }
            fieldsAndConstructorSb.AppendLine("    )");
            fieldsAndConstructorSb.AppendLine("    {");
            foreach (var svc in servicesToInject)
            {
                fieldsAndConstructorSb.AppendLine($"        {svc.Name} = {svc.Name.Substring(1)};");
            }
            fieldsAndConstructorSb.AppendLine("    }");

            // Strip the old constructor
            fileText = fileText.Replace(cMatch.Value, fieldsAndConstructorSb.ToString());

            // Remove any field declarations for IMediator or ISender
            fileText = Regex.Replace(fileText, @"private\s+readonly\s+(IMediator|ISender)\s+(_mediator|mediator|Mediator)\s*;", "");
        }

        // 3. Replace Mediator.Send calls with direct service calls
        // await _mediator.Send(command, ct) -> await _authService.LoginAsync(command, ct)
        // E.g., await _mediator.Send(new SetupMfaCommand(userId), ct) -> await _authService.SetupMfaAsync(new SetupMfaCommand(userId), ct)
        var sendRegex = new Regex(@"(await\s+)?_?mediator\.Send\(\s*(new\s+)?(\w+)(?:\(.*?\))?\s*,\s*(\w+)\s*\)", RegexOptions.Singleline);
        fileText = sendRegex.Replace(fileText, match =>
        {
            string reqType = match.Groups[3].Value;
            string requestInstance = match.Groups[2].Value + reqType + match.Value.Substring(match.Value.IndexOf('(') + match.Groups[2].Value.Length + reqType.Length);
            // strip the trailing parenthesis and ct
            int commaIdx = requestInstance.LastIndexOf(',');
            if (commaIdx != -1)
            {
                requestInstance = requestInstance.Substring(0, commaIdx).Trim();
            }
            // If it starts with new and has parentheses, keep it, otherwise if it was a variable like "command" keep it
            if (requestInstance.StartsWith("new "))
            {
                // remove new and constructor call if it is just a parameter
            }
            
            // Re-parse the exact command variable or instantiation
            int firstParen = match.Value.IndexOf('(');
            int lastParen = match.Value.LastIndexOf(')');
            string innerParams = match.Value.Substring(firstParen + 1, lastParen - firstParen - 1).Trim();
            // innerParams should be like: new SetupMfaCommand(userId), ct or command, ct
            string commandExpr = "";
            string ctExpr = "ct";
            int lastComma = innerParams.LastIndexOf(',');
            if (lastComma != -1)
            {
                commandExpr = innerParams.Substring(0, lastComma).Trim();
                ctExpr = innerParams.Substring(lastComma + 1).Trim();
            }
            else
            {
                commandExpr = innerParams;
            }

            // Find the request class name from the command expression
            // If it starts with "new RequestName(...)" then RequestName is the request class name.
            // If it is just a variable name like "command", we look at the action method signature to find its type!
            string actualRequestTypeName = reqType;
            if (!requestToModuleMap.ContainsKey(actualRequestTypeName))
            {
                // Guess or find
                string guessedModule = GuessModuleFromRequest(actualRequestTypeName, fileText);
                if (guessedModule != "")
                {
                    requestToModuleMap[actualRequestTypeName] = guessedModule;
                }
            }

            if (requestToModuleMap.TryGetValue(actualRequestTypeName, out string? module))
            {
                string serviceFieldName = $"_{module.Substring(0, 1).ToLower()}{module.Substring(1)}Service";
                string actionName = actualRequestTypeName;
                if (actionName.EndsWith("Command")) actionName = actionName.Substring(0, actionName.Length - 7);
                else if (actionName.EndsWith("Query")) actionName = actionName.Substring(0, actionName.Length - 5);
                else if (actionName.EndsWith("Commands")) actionName = actionName.Substring(0, actionName.Length - 8);
                else if (actionName.EndsWith("Queries")) actionName = actionName.Substring(0, actionName.Length - 7);

                return $"await {serviceFieldName}.{actionName}Async({commandExpr}, {ctExpr})";
            }

            return match.Value; // Fallback unchanged if module not mapped
        });

        // 4. Replace ToActionResult and HandleResult calls
        // return ToActionResult(result); -> return StatusCode(result.StatusCode, result);
        fileText = Regex.Replace(fileText, @"ToActionResult\((.*?)\)", "StatusCode($1.StatusCode, $1)");
        fileText = Regex.Replace(fileText, @"HandleResult\((.*?)\)", "StatusCode($1.StatusCode, $1)");

        // 5. Replace result.Value with response.Data
        // We will do a generic replacement for .Value on result or response variables
        // if they are mapped from ToActionResult or handled. But since C# code might use .Value for other things,
        // let's only do it for result.Value or response.Value when returning StatusCode or CreatedAtRoute.
        fileText = Regex.Replace(fileText, @"result\.Value", "result.Data");
        fileText = Regex.Replace(fileText, @"result\.IsSuccess", "(result.StatusCode >= 200 && result.StatusCode < 300)");
        fileText = Regex.Replace(fileText, @"result\.Errors", "result.Errors");

        // 6. Clean up old MediatR usings
        fileText = Regex.Replace(fileText, @"using TaxOmbud\.Application\.Features\..*?;", "");

        File.WriteAllText(filePath, fileText);
        Console.WriteLine($"Migrated controller: {fileName}");
    }

    static string GuessModuleFromRequest(string requestType, string fileText)
    {
        // Guess module by looking at namespaces imported in the controller file
        var matches = Regex.Matches(fileText, @"using TaxOmbud\.Application\.Features\.(\w+)");
        foreach (Match match in matches)
        {
            return match.Groups[1].Value;
        }

        // Fallback: see if requestType contains module keywords
        if (requestType.Contains("Auth")) return "Auth";
        if (requestType.Contains("Case")) return "Cases";
        if (requestType.Contains("Complaint")) return "Complaints";
        if (requestType.Contains("Appeal")) return "Appeals";
        if (requestType.Contains("Appointment")) return "Appointments";
        if (requestType.Contains("Officer")) return "Officers";
        if (requestType.Contains("Taxpayer")) return "Taxpayers";
        if (requestType.Contains("Notification")) return "Notifications";
        if (requestType.Contains("Department")) return "Departments";
        if (requestType.Contains("Role")) return "Roles";
        if (requestType.Contains("User")) return "Users";
        if (requestType.Contains("Wallet")) return "Wallet";
        if (requestType.Contains("Webhook")) return "Webhooks";
        if (requestType.Contains("Leave") || requestType.Contains("Loan") || requestType.Contains("Payroll") || requestType.Contains("Staff")) return "Hr";

        return "";
    }

    static string GuessModuleFromControllerName(string controllerName)
    {
        string name = controllerName.Replace("Controller", "");
        if (name == "Auth") return "Auth";
        if (name == "Users") return "Users";
        if (name == "Cases" || name == "PublicCases") return "Cases";
        if (name == "Complaints") return "Complaints";
        if (name == "Appeals") return "Appeals";
        if (name == "Appointments") return "Appointments";
        if (name == "Officers") return "Officers";
        if (name == "Taxpayers") return "Taxpayers";
        if (name == "Notifications") return "Notifications";
        if (name == "Communications" || name == "Mailbox") return "Communications";
        if (name == "Documents") return "Documents";
        if (name == "Reports" || name == "Dashboard") return "Reports";
        if (name == "Roles") return "Roles";
        if (name == "AuditLogs") return "AuditLogs";
        if (name == "System" || name == "Security") return "System";
        if (name == "SystemSettings" || name == "Encryption") return "SystemSettings";
        if (name == "Webhooks") return "Webhooks";
        if (name == "Hr" || name == "Leave" || name == "Attendance" || name == "Performance" || name == "ExitManagement" || name == "Benefits" || name == "Shifts" || name == "Payroll") return "Hr";
        if (name == "HrRequests") return "HrRequests";
        if (name == "PayGrades") return "PayGrades";
        if (name == "Wallet") return "Wallet";
        if (name == "Projects" || name == "Inventory" || name == "Visitors" || name == "Tickets") return "Operations";
        if (name == "Finance" || name == "Contracts" || name == "Quotes") return "Finance";
        if (name == "Lookups") return "Lookups";
        if (name == "Search") return "Search";
        if (name == "PublicGeo") return "Geo";
        if (name == "PublicContact") return "Contact";
        if (name == "IdentityVerification") return "IdentityVerification";
        if (name == "AiChatbot") return "AiChatbot";

        return "";
    }
}

namespace Lzq.AI.Application.Contracts.Requests;

public record ExecuteSkillRequest(string SkillName, string ToolName, Dictionary<string, object>? Arguments);
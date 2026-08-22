namespace RA2IniEditor.IDE.AI;

internal enum Ra2AiCapabilityMode
{
    AdvisoryOnly = 0,
    CurrentDocumentEditPreview
}

/// <summary>集中声明 AI 创作工具，避免 prompt、transport 和 adapter 各自维护协议。</summary>
internal static class Ra2AiAuthoringToolCatalog
{
    public const string PreviewIniEditPlanToolName = "preview_ini_edit_plan";
    public const string TrustedPlanOrigin = "DeepSeekToolCall";

    private static readonly IReadOnlyList<Ra2AiToolDefinition> CurrentDocumentToolList =
        Array.AsReadOnly(
        [
            new Ra2AiToolDefinition(
                PreviewIniEditPlanToolName,
                "Propose a bounded structured edit plan for the current INI document. " +
                "This creates a local preview only and never applies or saves changes.",
                """
                {
                  "type": "object",
                  "additionalProperties": false,
                  "required": ["outcome"],
                  "properties": {
                    "outcome": {
                      "type": "string",
                      "enum": ["proposal", "needs_clarification"]
                    },
                    "summary": {
                      "type": "string",
                      "minLength": 1,
                      "maxLength": 512
                    },
                    "message": {
                      "type": "string",
                      "minLength": 1,
                      "maxLength": 512
                    },
                    "operations": {
                      "type": "array",
                      "minItems": 1,
                      "maxItems": 128,
                      "items": {
                        "type": "object",
                        "additionalProperties": false,
                        "required": ["kind", "section", "key", "value"],
                        "properties": {
                          "kind": {
                            "type": "string",
                            "enum": ["upsert_field", "replace_field_value"]
                          },
                          "section": {
                            "type": "string",
                            "minLength": 1,
                            "maxLength": 256
                          },
                          "key": {
                            "type": "string",
                            "minLength": 1,
                            "maxLength": 256
                          },
                          "value": {
                            "type": "string",
                            "maxLength": 8192
                          }
                        }
                      }
                    }
                  }
                }
                """)
        ]);

    public static IReadOnlyList<Ra2AiToolDefinition> GetTools(Ra2AiCapabilityMode capabilityMode)
        => capabilityMode switch
        {
            Ra2AiCapabilityMode.AdvisoryOnly => [],
            Ra2AiCapabilityMode.CurrentDocumentEditPreview => CurrentDocumentToolList,
            _ => throw new ArgumentOutOfRangeException(nameof(capabilityMode))
        };
}

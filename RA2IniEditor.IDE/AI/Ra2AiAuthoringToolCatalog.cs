namespace RA2IniEditor.IDE.AI;

internal enum Ra2AiCapabilityMode
{
    AdvisoryOnly = 0,
    CurrentDocumentEditPreview,
    CurrentDocumentTemplatePreview,
    CurrentDocumentCompleteTemplatePreview,
    CurrentDocumentDualArmamentPreview,
    CurrentDocumentArcingProjectilePreview,
    CurrentDocumentHomingProjectilePreview,
    CurrentDocumentYrCoreWarheadPreview
}

/// <summary>集中声明 AI 创作工具，避免 prompt、transport 和 adapter 各自维护协议。</summary>
internal static class Ra2AiAuthoringToolCatalog
{
    public const string PreviewIniEditPlanToolName = "preview_ini_edit_plan";
    public const string ExpandIniContentTemplateToolName = "expand_ini_content_template";
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

    private static readonly IReadOnlyList<Ra2AiToolDefinition> CurrentDocumentTemplateToolList =
        Array.AsReadOnly(
        [
            new Ra2AiToolDefinition(
                ExpandIniContentTemplateToolName,
                "Expand one catalogued INI content template into a bounded local preview. " +
                "The tool cannot submit raw INI text and never applies or saves changes.",
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
                    "template_id": {
                      "type": "string",
                      "enum": ["weapon-projectile-warhead-skeleton"]
                    },
                    "template_version": {
                      "type": "integer",
                      "enum": [1]
                    },
                    "arguments": {
                      "type": "array",
                      "minItems": 3,
                      "maxItems": 3,
                      "items": {
                        "type": "object",
                        "additionalProperties": false,
                        "required": ["name", "value"],
                        "properties": {
                          "name": {
                            "type": "string",
                            "enum": ["weaponId", "projectileId", "warheadId"]
                          },
                          "value": {
                            "type": "string",
                            "minLength": 1,
                            "maxLength": 256
                          }
                        }
                      }
                    },
                    "message": {
                      "type": "string",
                      "minLength": 1,
                      "maxLength": 512
                    }
                  }
                }
                """)
        ]);

    private static readonly IReadOnlyList<Ra2AiToolDefinition> CurrentDocumentCompleteTemplateToolList =
        Array.AsReadOnly(
        [
            new Ra2AiToolDefinition(
                ExpandIniContentTemplateToolName,
                "Expand the complete direct-fire Weapon / Projectile / Warhead profile into one bounded local preview. " +
                "It binds one existing TechnoType weapon slot and never applies or saves changes.",
                """
                {
                  "type": "object",
                  "additionalProperties": false,
                  "required": ["outcome"],
                  "properties": {
                    "outcome": { "type": "string", "enum": ["proposal", "needs_clarification"] },
                    "template_id": { "type": "string", "enum": ["weapon-projectile-warhead-direct-fire-complete"] },
                    "template_version": { "type": "integer", "enum": [1] },
                    "arguments": {
                      "type": "object",
                      "additionalProperties": false,
                      "required": ["ownerSectionId", "ownerWeaponSlot", "weaponId", "projectileId", "warheadId", "damage", "rof", "range", "projectileSpeed", "verses", "infDeath", "cellSpread", "percentAtMax", "antiAir", "antiGround"],
                      "properties": {
                        "ownerSectionId": { "type": "string", "minLength": 1, "maxLength": 256 },
                        "ownerWeaponSlot": { "type": "string", "enum": ["Primary", "Secondary"] },
                        "weaponId": { "type": "string", "minLength": 1, "maxLength": 256 },
                        "projectileId": { "type": "string", "minLength": 1, "maxLength": 256 },
                        "warheadId": { "type": "string", "minLength": 1, "maxLength": 256 },
                        "damage": { "type": "integer" },
                        "rof": { "type": "integer" },
                        "range": { "type": "number" },
                        "projectileSpeed": { "type": "integer" },
                        "verses": { "type": "string", "minLength": 1, "maxLength": 256 },
                        "infDeath": { "type": "integer" },
                        "cellSpread": { "type": "number" },
                        "percentAtMax": { "type": "number" },
                        "antiAir": { "type": "boolean" },
                        "antiGround": { "type": "boolean" }
                      }
                    },
                    "message": { "type": "string", "minLength": 1, "maxLength": 512 }
                  }
                }
                """)
        ]);

    private static readonly IReadOnlyList<Ra2AiToolDefinition> CurrentDocumentDualArmamentToolList =
        Array.AsReadOnly(
        [
            new Ra2AiToolDefinition(
                ExpandIniContentTemplateToolName,
                "Expand a complete Primary and Secondary direct-fire armament for one existing TechnoType. " +
                "This is not an alternating or cyclic-fire mechanism and never applies or saves changes.",
                """
                {
                  "type": "object",
                  "additionalProperties": false,
                  "required": ["outcome"],
                  "properties": {
                    "outcome": { "type": "string", "enum": ["proposal", "needs_clarification"] },
                    "template_id": { "type": "string", "enum": ["techno-primary-secondary-direct-fire-complete"] },
                    "template_version": { "type": "integer", "enum": [1] },
                    "arguments": {
                      "type": "object",
                      "additionalProperties": false,
                      "required": [
                        "ownerSectionId",
                        "primaryWeaponId", "primaryProjectileId", "primaryWarheadId", "primaryDamage", "primaryRof", "primaryRange", "primaryProjectileSpeed", "primaryVerses", "primaryInfDeath", "primaryCellSpread", "primaryPercentAtMax", "primaryAntiAir", "primaryAntiGround",
                        "secondaryWeaponId", "secondaryProjectileId", "secondaryWarheadId", "secondaryDamage", "secondaryRof", "secondaryRange", "secondaryProjectileSpeed", "secondaryVerses", "secondaryInfDeath", "secondaryCellSpread", "secondaryPercentAtMax", "secondaryAntiAir", "secondaryAntiGround"
                      ],
                      "properties": {
                        "ownerSectionId": { "type": "string", "minLength": 1, "maxLength": 256 },
                        "primaryWeaponId": { "type": "string", "minLength": 1, "maxLength": 256 },
                        "primaryProjectileId": { "type": "string", "minLength": 1, "maxLength": 256 },
                        "primaryWarheadId": { "type": "string", "minLength": 1, "maxLength": 256 },
                        "primaryDamage": { "type": "integer" },
                        "primaryRof": { "type": "integer" },
                        "primaryRange": { "type": "number" },
                        "primaryProjectileSpeed": { "type": "integer" },
                        "primaryVerses": { "type": "string", "minLength": 1, "maxLength": 256 },
                        "primaryInfDeath": { "type": "integer" },
                        "primaryCellSpread": { "type": "number" },
                        "primaryPercentAtMax": { "type": "number" },
                        "primaryAntiAir": { "type": "boolean" },
                        "primaryAntiGround": { "type": "boolean" },
                        "secondaryWeaponId": { "type": "string", "minLength": 1, "maxLength": 256 },
                        "secondaryProjectileId": { "type": "string", "minLength": 1, "maxLength": 256 },
                        "secondaryWarheadId": { "type": "string", "minLength": 1, "maxLength": 256 },
                        "secondaryDamage": { "type": "integer" },
                        "secondaryRof": { "type": "integer" },
                        "secondaryRange": { "type": "number" },
                        "secondaryProjectileSpeed": { "type": "integer" },
                        "secondaryVerses": { "type": "string", "minLength": 1, "maxLength": 256 },
                        "secondaryInfDeath": { "type": "integer" },
                        "secondaryCellSpread": { "type": "number" },
                        "secondaryPercentAtMax": { "type": "number" },
                        "secondaryAntiAir": { "type": "boolean" },
                        "secondaryAntiGround": { "type": "boolean" }
                      }
                    },
                    "message": { "type": "string", "minLength": 1, "maxLength": 512 }
                  }
                }
                """)
        ]);

    private static readonly IReadOnlyList<Ra2AiToolDefinition> CurrentDocumentArcingProjectileToolList =
        Array.AsReadOnly(
        [
            new Ra2AiToolDefinition(
                ExpandIniContentTemplateToolName,
                "Bind one existing Weapon to a complete original-game arcing Projectile preview. " +
                "This profile never mixes ROT, Vertical, Inviso, or Phobos Trajectory and never applies or saves changes.",
                """
                {
                  "type": "object",
                  "additionalProperties": false,
                  "required": ["outcome"],
                  "properties": {
                    "outcome": { "type": "string", "enum": ["proposal", "needs_clarification"] },
                    "template_id": { "type": "string", "enum": ["weapon-projectile-arcing-complete"] },
                    "template_version": { "type": "integer", "enum": [1] },
                    "arguments": {
                      "type": "object",
                      "additionalProperties": false,
                      "required": ["weaponId", "projectileId", "image", "antiAir", "antiGround", "subjectToWalls", "subjectToElevation", "subjectToCliffs"],
                      "properties": {
                        "weaponId": { "type": "string", "minLength": 1, "maxLength": 256 },
                        "projectileId": { "type": "string", "minLength": 1, "maxLength": 256 },
                        "image": { "type": "string", "minLength": 1, "maxLength": 256 },
                        "antiAir": { "type": "boolean" },
                        "antiGround": { "type": "boolean" },
                        "subjectToWalls": { "type": "boolean" },
                        "subjectToElevation": { "type": "boolean" },
                        "subjectToCliffs": { "type": "boolean" }
                      }
                    },
                    "message": { "type": "string", "minLength": 1, "maxLength": 512 }
                  }
                }
                """)
        ]);

    private static readonly IReadOnlyList<Ra2AiToolDefinition> CurrentDocumentHomingProjectileToolList =
        Array.AsReadOnly(
        [
            new Ra2AiToolDefinition(
                ExpandIniContentTemplateToolName,
                "Bind one existing Weapon to a complete original-game ROT homing Projectile preview. " +
                "This profile never mixes Arcing, Vertical, Inviso, or Phobos Trajectory and never applies or saves changes.",
                """
                {
                  "type": "object",
                  "additionalProperties": false,
                  "required": ["outcome"],
                  "properties": {
                    "outcome": { "type": "string", "enum": ["proposal", "needs_clarification"] },
                    "template_id": { "type": "string", "enum": ["weapon-projectile-homing-complete"] },
                    "template_version": { "type": "integer", "enum": [1] },
                    "arguments": {
                      "type": "object",
                      "additionalProperties": false,
                      "required": ["weaponId", "projectileId", "image", "rot", "antiAir", "antiGround"],
                      "properties": {
                        "weaponId": { "type": "string", "minLength": 1, "maxLength": 256 },
                        "projectileId": { "type": "string", "minLength": 1, "maxLength": 256 },
                        "image": { "type": "string", "minLength": 1, "maxLength": 256 },
                        "rot": { "type": "integer", "minimum": 1 },
                        "antiAir": { "type": "boolean" },
                        "antiGround": { "type": "boolean" }
                      }
                    },
                    "message": { "type": "string", "minLength": 1, "maxLength": 512 }
                  }
                }
                """)
        ]);

    private static readonly IReadOnlyList<Ra2AiToolDefinition> CurrentDocumentYrCoreWarheadToolList =
        Array.AsReadOnly(
        [
            new Ra2AiToolDefinition(
                ExpandIniContentTemplateToolName,
                "Bind one existing Weapon to a complete Yuri's Revenge core Warhead preview. " +
                "This covers the original 11 Verses slots and does not create Ares Versus.* overrides or apply/save changes.",
                """
                {
                  "type": "object",
                  "additionalProperties": false,
                  "required": ["outcome"],
                  "properties": {
                    "outcome": { "type": "string", "enum": ["proposal", "needs_clarification"] },
                    "template_id": { "type": "string", "enum": ["weapon-warhead-yr-core-complete"] },
                    "template_version": { "type": "integer", "enum": [1] },
                    "arguments": {
                      "type": "object",
                      "additionalProperties": false,
                      "required": ["weaponId", "warheadId", "verses", "infDeath", "cellSpread", "percentAtMax", "proneDamage", "conventional", "wall", "wood", "rocker", "sparky", "tiberium", "bright"],
                      "properties": {
                        "weaponId": { "type": "string", "minLength": 1, "maxLength": 256 },
                        "warheadId": { "type": "string", "minLength": 1, "maxLength": 256 },
                        "verses": { "type": "string", "minLength": 1, "maxLength": 256 },
                        "infDeath": { "type": "integer", "minimum": 0, "maximum": 10 },
                        "cellSpread": { "type": "number", "minimum": 0, "maximum": 11 },
                        "percentAtMax": { "type": "number", "minimum": 0 },
                        "proneDamage": { "type": "number", "minimum": 0 },
                        "conventional": { "type": "boolean" },
                        "wall": { "type": "boolean" },
                        "wood": { "type": "boolean" },
                        "rocker": { "type": "boolean" },
                        "sparky": { "type": "boolean" },
                        "tiberium": { "type": "boolean" },
                        "bright": { "type": "boolean" }
                      }
                    },
                    "message": { "type": "string", "minLength": 1, "maxLength": 512 }
                  }
                }
                """)
        ]);

    public static IReadOnlyList<Ra2AiToolDefinition> GetTools(Ra2AiCapabilityMode capabilityMode)
        => capabilityMode switch
        {
            Ra2AiCapabilityMode.AdvisoryOnly => [],
            Ra2AiCapabilityMode.CurrentDocumentEditPreview => CurrentDocumentToolList,
            Ra2AiCapabilityMode.CurrentDocumentTemplatePreview => CurrentDocumentTemplateToolList,
            Ra2AiCapabilityMode.CurrentDocumentCompleteTemplatePreview => CurrentDocumentCompleteTemplateToolList,
            Ra2AiCapabilityMode.CurrentDocumentDualArmamentPreview => CurrentDocumentDualArmamentToolList,
            Ra2AiCapabilityMode.CurrentDocumentArcingProjectilePreview => CurrentDocumentArcingProjectileToolList,
            Ra2AiCapabilityMode.CurrentDocumentHomingProjectilePreview => CurrentDocumentHomingProjectileToolList,
            Ra2AiCapabilityMode.CurrentDocumentYrCoreWarheadPreview => CurrentDocumentYrCoreWarheadToolList,
            _ => throw new ArgumentOutOfRangeException(nameof(capabilityMode))
        };
}

using System;
using System.IO;
using System.Text.Json;

#nullable enable

namespace CPR.ContractTests
{
    internal static class SchemaValidator
    {
        // lightweight validation: checks that array items contain required props and that GUID fields look like GUIDs
        public static void ValidateJson(string schemaRelativePath, string json)
        {
            var doc = JsonDocument.Parse(json);
            if (doc.RootElement.ValueKind != JsonValueKind.Array) throw new Xunit.Sdk.XunitException("Expected JSON array");

            ValidateJsonInternal(schemaRelativePath, json, true);
        }

        // lightweight validation for JSON objects
        public static void ValidateJsonObject(string schemaRelativePath, string json)
        {
            var doc = JsonDocument.Parse(json);
            if (doc.RootElement.ValueKind != JsonValueKind.Object) throw new Xunit.Sdk.XunitException("Expected JSON object");

            ValidateJsonInternal(schemaRelativePath, json, false);
        }

        private static void ValidateJsonInternal(string schemaRelativePath, string json, bool isArray)
        {
            var doc = JsonDocument.Parse(json);

            // locate the schemas directory by walking up from the test assembly directory
            var dir = AppContext.BaseDirectory;
            string? schemasDir = null;
            for (int i = 0; i < 8; i++)
            {
                var candidate = Path.Combine(dir, "schemas");
                if (Directory.Exists(candidate)) { schemasDir = candidate; break; }
                var parent = Path.GetDirectoryName(dir);
                if (string.IsNullOrEmpty(parent)) break;
                dir = parent;
            }
            if (schemasDir is null)
            {
                // fallback: look relative to repository tests folder
                var repoCandidate = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "schemas"));
                if (Directory.Exists(repoCandidate)) schemasDir = repoCandidate;
            }
            if (schemasDir is null) throw new Xunit.Sdk.XunitException("Could not locate 'schemas' directory for JSON schema validation");

            var file = Path.Combine(schemasDir, schemaRelativePath);
            if (!File.Exists(file)) throw new Xunit.Sdk.XunitException($"Schema file not found: {file}");

            var schemaText = File.ReadAllText(file);
            // parse minimal required fields from schema ("required": [...])
            using var st = JsonDocument.Parse(schemaText);
            var required = new System.Collections.Generic.List<string>();

            if (isArray)
            {
                // For arrays, get required fields from items.required
                if (st.RootElement.TryGetProperty("items", out var items) && items.ValueKind == JsonValueKind.Object && items.TryGetProperty("required", out var req))
                {
                    foreach (var it in req.EnumerateArray()) required.Add(it.GetString()!);
                }

                foreach (var item in doc.RootElement.EnumerateArray())
                {
                    ValidateRequiredProperties(item, required);
                }
            }
            else
            {
                // For objects, get required fields from root.required
                if (st.RootElement.TryGetProperty("required", out var req))
                {
                    foreach (var it in req.EnumerateArray()) required.Add(it.GetString()!);
                }

                ValidateRequiredProperties(doc.RootElement, required);
            }
        }

        private static void ValidateRequiredProperties(JsonElement element, System.Collections.Generic.List<string> required)
        {
            foreach (var r in required)
            {
                if (!element.TryGetProperty(r, out var p)) throw new Xunit.Sdk.XunitException($"Missing required property '{r}'");
                if (r.Equals("id", StringComparison.OrdinalIgnoreCase) || r.EndsWith("Id", StringComparison.OrdinalIgnoreCase))
                {
                    // Handle nullable GUID fields
                    if (p.ValueKind == JsonValueKind.Null) continue; // Nullable GUID is OK
                    if (p.ValueKind != JsonValueKind.String || string.IsNullOrWhiteSpace(p.GetString()) || !Guid.TryParse(p.GetString(), out _))
                        throw new Xunit.Sdk.XunitException($"Property '{r}' is not a valid GUID string: {p}");
                }
            }
        }
    }
}

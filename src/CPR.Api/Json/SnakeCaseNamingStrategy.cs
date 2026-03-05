using Newtonsoft.Json.Serialization;
using System.Text;

namespace CPR.Api.Json;

/// <summary>
/// Custom Newtonsoft.Json naming strategy that converts PascalCase property names to snake_case.
/// Per CPR Constitution: JSON/API responses must use snake_case naming convention.
/// </summary>
/// <remarks>
/// Example transformations:
/// - UserId → user_id
/// - DisplayName → display_name
/// - CreatedAt → created_at
/// - IsActive → is_active
/// </remarks>
public class SnakeCaseNamingStrategy : NamingStrategy
{
    /// <summary>
    /// Converts a PascalCase string to snake_case.
    /// </summary>
    /// <param name="name">The property name in PascalCase (e.g., "UserId")</param>
    /// <returns>The property name in snake_case (e.g., "user_id")</returns>
    protected override string ResolvePropertyName(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return name;
        }

        var builder = new StringBuilder();

        for (int i = 0; i < name.Length; i++)
        {
            char c = name[i];

            // If uppercase and not the first character, add underscore before it
            if (char.IsUpper(c) && i > 0)
            {
                // Don't add underscore if previous character is already an underscore
                if (builder.Length > 0 && builder[builder.Length - 1] != '_')
                {
                    builder.Append('_');
                }
            }

            builder.Append(char.ToLowerInvariant(c));
        }

        return builder.ToString();
    }
}

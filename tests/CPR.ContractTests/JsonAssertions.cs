using System;
using System.Text.Json;

namespace CPR.ContractTests
{
    internal static class JsonAssertions
    {
        public static void AssertIsGuidString(JsonElement el)
        {
            if (el.ValueKind != JsonValueKind.String) throw new Xunit.Sdk.XunitException("Expected string GUID");
            var s = el.GetString();
            if (string.IsNullOrWhiteSpace(s) || !Guid.TryParse(s, out _)) throw new Xunit.Sdk.XunitException($"Value is not a GUID: {s}");
        }

        public static void AssertIsStringOrNull(JsonElement el)
        {
            if (el.ValueKind != JsonValueKind.String && el.ValueKind != JsonValueKind.Null)
                throw new Xunit.Sdk.XunitException("Expected string or null");
        }
    }
}

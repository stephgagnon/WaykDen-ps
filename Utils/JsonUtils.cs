using System;
namespace WaykDen.Utils
{
    public static class JsonUtils
    {
        public static bool IsArrayJsonObject(string json)
        {
            return json.StartsWith("[", StringComparison.Ordinal);
        }

        public static bool IsSingleJsonObject(string json)
        {
            return json.StartsWith("{", StringComparison.Ordinal);
        }
    }
}

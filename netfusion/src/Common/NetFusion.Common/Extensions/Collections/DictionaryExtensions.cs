using System.Collections.Generic;
using System.Linq;

namespace NetFusion.Common.Extensions.Collections;

public static class DictionaryExtensions
{
    public static string ToKeyValuePairString(this IDictionary<string, string?> dictionary) =>
        string.Join(", ", dictionary.Where(kv => kv.Value != null)
            .Select((k, v) => $"{k}={v}")
            .ToArray()
        );  
    
    public static string ToKeyValuePairString(this IDictionary<string, object> dictionary) =>
        string.Join(", ", dictionary.Select((k, v) => $"{k}={v}").ToArray());

    public static Dictionary<string, string> RemoveNullValues(this IDictionary<string, string?> dictionary) =>
        dictionary.Where(kv => kv.Value != null).ToDictionary(kv => kv.Key, kv => kv.Value)!;
}
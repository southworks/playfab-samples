using System.Text.Json;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace FantasySoccer.Extensions
{
    public static class TempDataExtension
    {
        public static T Get<T>(this ITempDataDictionary tempData, string key)
        {
            if (!tempData.ContainsKey(key))
            {
                return default;
            }

            return JsonSerializer.Deserialize<T>(tempData[key].ToString());
        }

        public static void Set<T>(this ITempDataDictionary tempData, string key, T data)
        {
            tempData[key] = JsonSerializer.Serialize(data);
        }
    }
}

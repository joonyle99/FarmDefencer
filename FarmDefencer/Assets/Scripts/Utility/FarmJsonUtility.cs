using Newtonsoft.Json.Linq;
using UnityEngine;

public static class FarmJsonUtility
{
    public static T ParsePropertyOrAssign<T>(this JObject json, string propertyName, T valueOnFailed, bool maybeError = false)
    {
        var jToken = json.Property(propertyName)?.Value;
        if (jToken is null)
        {
            if (maybeError)
            {
                Debug.LogError($"존재하지 않는 JSON 프로퍼티: {propertyName}, {valueOnFailed} 반환.");
            }
            return valueOnFailed;
        }

        var value = jToken.Value<T>();
        if (value is null)
        {
            if (maybeError)
            {
                Debug.LogError($"JSON 프로퍼티 {propertyName} {typeof(T)}에 대한 파싱 실패:, {valueOnFailed} 반환.");
            }
            return valueOnFailed;
        }

        return value;
    }
}
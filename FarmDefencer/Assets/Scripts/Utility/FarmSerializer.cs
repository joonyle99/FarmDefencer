using System;
using Newtonsoft.Json.Linq;
using UnityEngine;
using System.IO;
using JetBrains.Annotations;

public interface IFarmSerializable
{
    [NotNull] JObject Serialize();
    
    void Deserialize([NotNull] JObject json);
}

public static class FarmSerializer
{
    public static string SavePath => Path.Combine(Application.dataPath, "FarmDefencerSave.json");

    /// <summary>
    /// { "FarmDefencerSave": {...} } 의 {...} 를 반환하는 함수.
    /// 파일이 없거나 오류가 발생하였다면 빈 오브젝트 반환.
    /// </summary>
    /// <returns></returns>
    [NotNull]
    public static JObject ReadSave()
    {
        if (!File.Exists(SavePath))
        {
            return new JObject();
        }
        
        try
        {
            var file = File.ReadAllText(SavePath);
            return JObject.Parse(file)["FarmDefencerSave"] as JObject ?? new JObject();
        }
        catch (Exception e)
        {
            if (e is not FileNotFoundException)
            {
                Debug.LogError($"세이브 파일 불러오기에 실패하였습니다: {e.Message}");
            }
            return new JObject();
        }
    }

    public static void WriteSave([NotNull] JObject json)
    {
        if (File.Exists(SavePath))
        {
            File.Delete(SavePath);
        }

        var rootObject = new JObject(new JProperty("FarmDefencerSave", json));
        File.WriteAllText(SavePath, rootObject.ToString());
    }
}

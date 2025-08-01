using Newtonsoft.Json.Linq;
using JetBrains.Annotations;
using UnityEngine;
using System.IO;
using System;

public interface IFarmSerializable
{
    [NotNull] JObject Serialize();
    
    void Deserialize([NotNull] JObject json);
}

public sealed class SaveManager : JoonyleGameDevKit.Singleton<SaveManager>
{
    private static string GetSavePath() => Path.Combine(Application.persistentDataPath, "FarmDefencerSave.json");
    
    private JObject _loadedSave;
    public JObject LoadedSave
    {
        get
        {
            if (_loadedSave is null)
            {
                try
                {
                    var file = File.ReadAllText(GetSavePath());
                    _loadedSave = JObject.Parse(file);
                }
                catch (Exception e)
                {
                    if (e is not FileNotFoundException)
                    {
                        Debug.LogError($"세이브 파일 불러오기에 실패하였습니다: {e.Message}");
                    }

                    _loadedSave = new JObject();
                }
            }
            return _loadedSave;
        }
    }

    /// <summary>
    /// 세이브 파일에 저장되어 있는 타이쿤 남은 재배 가능 시간을 가져오는 메소드.
    /// </summary>
    /// <returns></returns>
    public float GetRemainingHarvestableTime()
    {
        if (LoadedSave["FarmClock"] is JObject jsonFarmClock)
        {
            var lengthOfDaytime = jsonFarmClock["LengthOfDaytime"]?.Value<float>() ?? 300.0f;
            var currentDaytime = jsonFarmClock["CurrentDaytime"]?.Value<float>() ?? 0.0f;
            return lengthOfDaytime - currentDaytime;
        }

        return 300.0f;
    }
    
    /// <summary>
    /// 세이브 파일을 로컬 저장소에 저장하며, LoadedSave도 jsonObject로 설정하는 메소드.
    /// </summary>
    /// <param name="jsonObject"></param>
    public void WriteSave([NotNull] JObject jsonObject)
    {
        _loadedSave = jsonObject;
        FlushSave();
    }
    
    /// <summary>
    /// LoadedSave를 파일에 저장하는 메소드. 새로운 JSON Object가 아니라 일부 프로퍼티만 편집할 때에 사용된다.
    /// </summary>
    public void FlushSave()
    {
        if (File.Exists(GetSavePath()))
        {
            File.Delete(GetSavePath());
        }
        
        File.WriteAllText(GetSavePath(), LoadedSave.ToString());
    }
}

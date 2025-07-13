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
    private static readonly string SavePath = Path.Combine(Application.dataPath, "FarmDefencerSave.json");
    
    private JObject _loadedSave;
    public JObject LoadedSave
    {
        get
        {
            if (_loadedSave is null)
            {
                try
                {
                    var file = File.ReadAllText(SavePath);
                    _loadedSave = JObject.Parse(file)["FarmDefencerSave"] as JObject ?? new JObject();
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
    /// 세이브 파일을 로컬 저장소에 저장하며, LoadedSave도 jsonObject로 설정하는 메소드.
    /// </summary>
    /// <param name="jsonObject"></param>
    public void WriteSave([NotNull] JObject jsonObject)
    {
        if (File.Exists(SavePath))
        {
            File.Delete(SavePath);
        }

        var rootObject = new JObject(new JProperty("FarmDefencerSave", jsonObject));
        File.WriteAllText(SavePath, rootObject.ToString());
        _loadedSave = rootObject;
    }
}

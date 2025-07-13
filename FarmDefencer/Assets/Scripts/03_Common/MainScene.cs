using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class MainScene : MonoBehaviour
{
    private Button _goTycoonButton;
    private TimerUI _timerUI;
    
    private void Awake()
    {
        _goTycoonButton = transform.Find("FarmButton").GetComponent<Button>();
        _goTycoonButton.onClick.AddListener(OnTycoonButtonClickedHandler);
        _timerUI = GameObject.Find("TimerUI").GetComponent<TimerUI>();
    }

    private void Start()
    {
        // 매니저들 초기화
        var loadedSave = SaveManager.Instance.LoadedSave;
        MapManager.Instance.Deserialize(loadedSave["MapManager"] as JObject ?? new JObject());
        ResourceManager.Instance.Deserialize(loadedSave["ResourceManager"] as JObject ?? new JObject());
        
        // 타이머 불러오기
        var jsonFarmClock = loadedSave["FarmClock"] ?? new JObject();
        var remainingDaytime = jsonFarmClock["RemainingDaytime"]?.Value<float>() ?? 0.0f;
        var lengthOfDaytime = jsonFarmClock["LengthOfDaytime"]?.Value<float>() ?? 300.0f;
        
        _timerUI.Init(MapManager.Instance.CurrentMapIndex, MapManager.Instance.CurrentStageIndex, () => remainingDaytime / lengthOfDaytime);
    }

    private void OnTycoonButtonClickedHandler()
    {
        SceneManager.LoadScene(1);
    }
}

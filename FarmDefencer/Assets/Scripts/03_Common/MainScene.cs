using System;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class MainScene : MonoBehaviour
{
    private TMP_Text _harvestableTimeText;
    private TMP_Text _availableCoinText;
    private Button _farmButton;
    private Button _battleButton;
    private TimerUI _timerUI;
    private Button _debugResetSaveAndRestartButton;
    
    private void Awake()
    {
        _farmButton = transform.Find("FarmButton").GetComponent<Button>();
        _farmButton.onClick.AddListener(OnFarmButtonClicked);        
        _battleButton = transform.Find("BattleButton").GetComponent<Button>();
        _battleButton.onClick.AddListener(OnBattleButtonClicked);
        _harvestableTimeText = transform.Find("FarmButton/HarvestableTimeText").GetComponent<TMP_Text>();
        _availableCoinText = transform.Find("BattleButton/AvailableCoinText").GetComponent<TMP_Text>();
        _timerUI = GameObject.Find("TimerUI").GetComponent<TimerUI>();
        _debugResetSaveAndRestartButton = transform.Find("Debug_ResetSaveAndRestartButton").GetComponent<Button>();
        _debugResetSaveAndRestartButton.onClick.AddListener(OnResetSaveAndRestartButtonClicked);
    }

    private void Start()
    {
        // 매니저들 초기화
        var loadedSave = SaveManager.Instance.LoadedSave;
        MapManager.Instance.Deserialize(loadedSave["MapManager"] as JObject ?? new JObject());
        ResourceManager.Instance.Deserialize(loadedSave["ResourceManager"] as JObject ?? new JObject());
        
        // 타이머 불러오기
        var jsonFarmClock = loadedSave["FarmClock"] ?? new JObject();
        var currentDaytime = jsonFarmClock["CurrentDaytime"]?.Value<float>() ?? 0.0f;
        var lengthOfDaytime = jsonFarmClock["LengthOfDaytime"]?.Value<float>() ?? 300.0f;
        
        // 재배 가능 시간 설정
        var remainingDaytimeSpan = TimeSpan.FromSeconds(lengthOfDaytime - currentDaytime);
        var remainingDaytimeMinutes = remainingDaytimeSpan.Minutes;
        var remainingDaytimeSeconds = remainingDaytimeSpan.Seconds;
        _harvestableTimeText.text = $"남은 재배 가능 시간\n:{remainingDaytimeMinutes:D2}분 {remainingDaytimeSeconds:D2}초";
        
        // 사용 가능 코인 설정
        _availableCoinText.text = $"사용 가능 코인\n:{ResourceManager.Instance.Gold}";
        
        _timerUI.Init(MapManager.Instance.CurrentMapIndex, MapManager.Instance.CurrentStageIndex, () => (lengthOfDaytime - currentDaytime) / lengthOfDaytime);
    }

    private void OnFarmButtonClicked()
    {
        SceneManager.LoadScene("Tycoon Scene");
    }

    private void OnBattleButtonClicked()
    {
        var currentMap = MapManager.Instance.CurrentMap;
        SceneManager.LoadScene($"MapStageSelectScene_{currentMap.MapCode}");
    }

    private void OnResetSaveAndRestartButtonClicked()
    {
        SaveManager.Instance.WriteSave(new JObject());
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}

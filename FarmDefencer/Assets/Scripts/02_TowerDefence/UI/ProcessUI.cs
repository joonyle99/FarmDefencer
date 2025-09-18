using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProcessUI : MonoBehaviour
{
    [Header("━━━━━━━━ Process UI ━━━━━━━━")]
    [Space]

    [SerializeField] private Button _fightButton; // build state

    [SerializeField] private Button _pauseButton; // wave state / wave after state
    [SerializeField] private Button _resumeButton; // wave state / wave after state

    [Space]

    [SerializeField] private Button _playX1Button;
    [SerializeField] private Button _playX2Button;

    [Space]

    [SerializeField] private Button _settingButton;

    [Space]

    [SerializeField] private TextMeshProUGUI _stageText;

    [Space]

    [SerializeField] private RectTransform _pauseBlocker;
    public RectTransform PauseBlocker => _pauseBlocker;
    [SerializeField] private RectTransform _mainButtons;
    private RectTransform _mainButtonsOriginParent;

    private PanelToggler _panelToggler;

    private void Awake()
    {
        GameStateManager.Instance?.AddCallback(GameState.Build, (Action)RefreshButton);
        GameStateManager.Instance?.AddCallback(GameState.WaveInProgress, (Action)RefreshButton);
        GameStateManager.Instance?.AddCallback(GameState.WaveCompleted, (Action)RefreshButton);
        GameStateManager.Instance?.AddCallback(GameState.DefenceEnd, (Action)BlockProcess);
        GameStateManager.Instance?.AddCallback(GameState.DefenceEnd, (Action)RefreshButton);

        _panelToggler = FindFirstObjectByType<PanelToggler>();
    }
    private void Start()
    {
        _fightButton.gameObject.SetActive(true);

        _pauseButton.gameObject.SetActive(false);
        _resumeButton.gameObject.SetActive(false);

        _playX1Button.gameObject.SetActive(false);
        _playX2Button.gameObject.SetActive(true);

        _settingButton.gameObject.SetActive(true);

        _stageText.text = $"{MapManager.Instance.CurrentMapIndex}-{MapManager.Instance.CurrentStageIndex}";

        _mainButtonsOriginParent = _mainButtons.parent as RectTransform;
    }
    private void OnDestroy()
    {
        GameStateManager.Instance?.RemoveCallback(GameState.Build, (Action)RefreshButton);
        GameStateManager.Instance?.RemoveCallback(GameState.WaveInProgress, (Action)RefreshButton);
        GameStateManager.Instance?.RemoveCallback(GameState.WaveCompleted, (Action)RefreshButton);
        GameStateManager.Instance?.RemoveCallback(GameState.DefenceEnd, (Action)BlockProcess);
        GameStateManager.Instance?.RemoveCallback(GameState.DefenceEnd, (Action)RefreshButton);
    }

    private void BlockProcess()
    {
        Time.timeScale = 1f;
        SoundManager.Instance.StopBgm();
        _pauseBlocker.gameObject.SetActive(true);
    }
    private void RefreshButton()
    {
        if (_fightButton == null ||
            _pauseButton == null ||
            _resumeButton == null ||
            _playX1Button == null ||
            _playX2Button == null)
        {
            return;
        }

        _fightButton.gameObject.SetActive(GameStateManager.Instance.IsBuildState);

        _resumeButton.gameObject.SetActive(GameStateManager.Instance.IsWaveState && GameStateManager.Instance.IsPause);
        _pauseButton.gameObject.SetActive(GameStateManager.Instance.IsWaveState && !GameStateManager.Instance.IsPause);

        _playX1Button.gameObject.SetActive(GameStateManager.Instance.IsPlayX2);
        _playX2Button.gameObject.SetActive(!GameStateManager.Instance.IsPlayX2);
    }

    public void Fight()
    {
        if (GameStateManager.Instance.IsBuildState == true)
        {
            GameStateManager.Instance.ChangeState(GameState.WaveInProgress);
        }
    }

    public void TogglePause()
    {
        if (GameStateManager.Instance.IsWaveState == true)
        {
            GameStateManager.Instance.TogglePause();

            // Pause 상태에 열려져 있다면 패널을 닫는다.
            // Resume 상태에 닫혀져 있다면 패널을 연다.
            if (GameStateManager.Instance.IsPause == _panelToggler.IsExpanded)
            {
                _panelToggler.TogglePanel();
            }

            _mainButtons.SetParent(GameStateManager.Instance.IsPause ? _pauseBlocker : _mainButtonsOriginParent, false);
            _pauseBlocker.gameObject.SetActive(GameStateManager.Instance.IsPause);

            _resumeButton.gameObject.SetActive(GameStateManager.Instance.IsPause);
            _pauseButton.gameObject.SetActive(!GameStateManager.Instance.IsPause);
        }
    }
    public void TogglePlaySpeed()
    {
        if (GameStateManager.Instance.IsPlayableDefenceState == true)
        {
            GameStateManager.Instance.TogglePlaySpeed();

            _playX1Button.gameObject.SetActive(GameStateManager.Instance.IsPlayX2);
            _playX2Button.gameObject.SetActive(!GameStateManager.Instance.IsPlayX2);

            if (GameStateManager.Instance.IsPlayX2)
            {
                SoundManager.Instance.PlayDefenceBgm(MapManager.Instance.CurrentMap, true);
            }
            else
            {
                SoundManager.Instance.PlayDefenceBgm(MapManager.Instance.CurrentMap, false);
            }
        }
    }

    public void Setting()
    {
        if (GameStateManager.Instance.IsPlayableDefenceState == true)
        {
            DefenceSceneTransitioner.OnGiveUp();

            SaveManager.Instance.LoadedSave["MapManager"] = MapManager.Instance.Serialize();
            SaveManager.Instance.LoadedSave["ResourceManager"] = ResourceManager.Instance.Serialize();
            SaveManager.Instance.FlushSave();

            SceneChangeManager.Instance.ChangeScene(SceneType.Main);
        }
    }
}

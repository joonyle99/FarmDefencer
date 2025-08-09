using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
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
        GameStateManager.Instance.OnChangeState -= RefreshButton;
        GameStateManager.Instance.OnChangeState += RefreshButton;

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

    public void RefreshButton()
    {
        var isBuildState = GameStateManager.Instance.IsBuildState;
        var isWaveState = GameStateManager.Instance.IsWaveState;

        if (_fightButton == null ||
            _pauseButton == null ||
            _resumeButton == null ||
            _playX1Button == null ||
            _playX2Button == null)
        {
            return;
        }

        _fightButton.gameObject.SetActive(isBuildState);

        _resumeButton.gameObject.SetActive(isWaveState && GameStateManager.Instance.IsPause);
        _pauseButton.gameObject.SetActive(isWaveState && !GameStateManager.Instance.IsPause);

        _playX1Button.gameObject.SetActive(GameStateManager.Instance.IsPlayX2);
        _playX2Button.gameObject.SetActive(!GameStateManager.Instance.IsPlayX2);
    }

    public void Fight()
    {
        if (GameStateManager.Instance.IsBuildState == false)
        {
            return;
        }

        GameStateManager.Instance.ChangeState(GameState.Wave);
    }

    public void TogglePause()
    {
        if (GameStateManager.Instance.IsWaveState == false)
        {
            return;
        }

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
    public void TogglePlaySpeed()
    {
        if (GameStateManager.Instance.IsWaveState == false)
        {
            return;
        }

        GameStateManager.Instance.TogglePlaySpeed();

        _playX1Button.gameObject.SetActive(GameStateManager.Instance.IsPlayX2);
        _playX2Button.gameObject.SetActive(!GameStateManager.Instance.IsPlayX2);
    }

    public void Setting()
    {
        //var isBuildState = GameStateManager.Instance.IsBuildState;
        //var isWaveState = GameStateManager.Instance.IsWaveState;

        //if (isBuildState == false || isWaveState == false)
        //{
        //    return;
        //}

        // 중단하시겠습까?
        
        DefenceSceneTransitioner.HandleGiveUp();
        SaveManager.Instance.LoadedSave["MapManager"] = MapManager.Instance.Serialize();
        SaveManager.Instance.LoadedSave["ResourceManager"] = ResourceManager.Instance.Serialize();
        SaveManager.Instance.FlushSave();
        
        SceneManager.LoadScene("Main Scene");
    }
}

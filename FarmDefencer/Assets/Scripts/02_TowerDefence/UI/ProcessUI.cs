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

    private void Awake()
    {
        GameStateManager.Instance.OnChangeState -= RefreshButton;
        GameStateManager.Instance.OnChangeState += RefreshButton;
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
        var isBuildState = GameStateManager.Instance.IsBuildState;
        var isWaveState = GameStateManager.Instance.IsWaveState;

        if (isBuildState == false || isWaveState == false)
        {
            return;
        }
    }
}

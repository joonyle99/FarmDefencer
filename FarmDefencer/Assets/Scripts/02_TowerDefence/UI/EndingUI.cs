using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public enum FadeType
{
    FadeIn,
    FadeOut,
}

public enum EndingType
{
    Success,
    Failure,
}

public class EndingUI : MonoBehaviour
{
    [Header("Fade")]
    [SerializeField] private Image _fadeImage; // TODO: 이후에는 EndingUI가 아닌 독립된 클래스로 분리한 후, 현재 팝업? 혹은 UI의 hierarchy index에 넣는 방식으로 변경한다
    [SerializeField] private float _dimAlpha = 0.6f;
    [SerializeField] private float _duration = 2f;

    [Space]

    [Header("result")]
    [SerializeField] private Image _lineLeft;
    [SerializeField] private Image _lineRight;
    [SerializeField] private Image _successImage;
    [SerializeField] private Image _failureImage;
    [SerializeField] private MonsterUI _monsterUI;

    private void Start()
    {
        DefenceContext.Current.WaveSystem.OnSuccess -= ShowSuccess;
        DefenceContext.Current.WaveSystem.OnSuccess += ShowSuccess;

        DefenceContext.Current.WaveSystem.OnFailure -= ShowFailure;
        DefenceContext.Current.WaveSystem.OnFailure += ShowFailure;

        _fadeImage.color = new Color(_fadeImage.color.r, _fadeImage.color.g, _fadeImage.color.b, 0f);
        _lineLeft.color = new Color(_lineLeft.color.r, _lineLeft.color.g, _lineLeft.color.b, 0f);
        _lineRight.color = new Color(_lineRight.color.r, _lineRight.color.g, _lineRight.color.b, 0f);
        _successImage.color = new Color(_successImage.color.r, _successImage.color.g, _successImage.color.b, 0f);
        _failureImage.color = new Color(_failureImage.color.r, _failureImage.color.g, _failureImage.color.b, 0f);
    }
    private void OnDestroy()
    {
        if (DefenceContext.Current == null) return;

        DefenceContext.Current.WaveSystem.OnSuccess -= ShowSuccess;
        DefenceContext.Current.WaveSystem.OnFailure -= ShowFailure;
    }

    public void ShowSuccess()
    {
        SoundManager.Instance.PlaySfx("SFX_D_stage_success", 0.7f);

        _fadeImage.gameObject.SetActive(true);
        _lineLeft.gameObject.SetActive(true);
        _lineRight.gameObject.SetActive(true);
        _successImage.gameObject.SetActive(true);
        _failureImage.gameObject.SetActive(false);

        _fadeImage.DOFade(_dimAlpha, _duration).SetEase(Ease.InOutSine);
        _lineLeft.DOFade(1f, _duration).SetEase(Ease.InOutSine);
        _lineRight.DOFade(1f, _duration).SetEase(Ease.InOutSine);
        _successImage.DOFade(1f, _duration).SetEase(Ease.InOutSine);
        _failureImage.DOFade(1f, _duration).SetEase(Ease.InOutSine);

        _monsterUI.ShowMonsterUI(EndingType.Success);
    }
    public void ShowFailure()
    {
        SoundManager.Instance.PlaySfx("SFX_D_stage_fail", 0.7f);

        _fadeImage.gameObject.SetActive(true);
        _lineLeft.gameObject.SetActive(true);
        _lineRight.gameObject.SetActive(true);
        _successImage.gameObject.SetActive(false);
        _failureImage.gameObject.SetActive(true);

        _fadeImage.DOFade(_dimAlpha, _duration).SetEase(Ease.InOutSine);
        _lineLeft.DOFade(1f, _duration).SetEase(Ease.InOutSine);
        _lineRight.DOFade(1f, _duration).SetEase(Ease.InOutSine);
        _successImage.DOFade(1f, _duration).SetEase(Ease.InOutSine);
        _failureImage.DOFade(1f, _duration).SetEase(Ease.InOutSine);

        _monsterUI.ShowMonsterUI(EndingType.Failure);
    }
}

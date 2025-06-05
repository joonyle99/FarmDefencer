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
        DefenceContext.Current.WaveSystem.OnEnding -= ShowEnding;
        DefenceContext.Current.WaveSystem.OnEnding += ShowEnding;

        // 색상 초기화
        _fadeImage.color = new Color(_fadeImage.color.r, _fadeImage.color.g, _fadeImage.color.b, 0f);
        _lineLeft.color = new Color(_lineLeft.color.r, _lineLeft.color.g, _lineLeft.color.b, 0f);
        _lineRight.color = new Color(_lineRight.color.r, _lineRight.color.g, _lineRight.color.b, 0f);
        _successImage.color = new Color(_successImage.color.r, _successImage.color.g, _successImage.color.b, 0f);
        _failureImage.color = new Color(_failureImage.color.r, _failureImage.color.g, _failureImage.color.b, 0f);
    }
    private void OnDestroy()
    {
        if (DefenceContext.Current == null) return;

        DefenceContext.Current.WaveSystem.OnEnding -= ShowEnding;
    }

    private void ShowEnding(EndingType endingType)
    {
        SoundManager.Instance.PlaySfx($"SFX_D_stage_{ConvertToEndingText(endingType)}", 0.7f);

        _fadeImage.gameObject.SetActive(endingType == EndingType.Success);
        _lineLeft.gameObject.SetActive(endingType == EndingType.Success);
        _lineRight.gameObject.SetActive(endingType == EndingType.Success);
        _successImage.gameObject.SetActive(endingType == EndingType.Success);
        _failureImage.gameObject.SetActive(endingType == EndingType.Failure);

        _fadeImage.DOFade(_dimAlpha, _duration).SetEase(Ease.InOutSine);
        _lineLeft.DOFade(1f, _duration).SetEase(Ease.InOutSine);
        _lineRight.DOFade(1f, _duration).SetEase(Ease.InOutSine);
        _successImage.DOFade(1f, _duration).SetEase(Ease.InOutSine);
        _failureImage.DOFade(1f, _duration).SetEase(Ease.InOutSine);

        _monsterUI.ShowMonsterUI(endingType);
    }

    private string ConvertToEndingText(EndingType endingType)
    {
        if (endingType == EndingType.Success)
        {
            return "success";
        }
        else if (endingType == EndingType.Failure)
        {
            return "fail";
        }

        return "";
    }
}

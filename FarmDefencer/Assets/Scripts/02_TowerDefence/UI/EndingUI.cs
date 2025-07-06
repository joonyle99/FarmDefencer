using DG.Tweening;
using Spine.Unity;
using TMPro;
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

    [Header("Coin")]
    [SerializeField] private SkeletonGraphic _coinSkeletonGraphic01;
    [SerializeField] private SkeletonGraphic _coinSkeletonGraphic02;
    [SerializeField] private TextMeshProUGUI _costText;

    [Space]

    [Header("result")]
    [SerializeField] private Image _successImage;
    [SerializeField] private Image _failureImage;
    [SerializeField] private Image _successLine;
    [SerializeField] private Image _failureLine;

    [Space]

    [Header("Monster")]
    [SerializeField] private MonsterUI _monsterUI;

    private void Start()
    {
        DefenceContext.Current.WaveSystem.OnEnding -= ShowEnding;
        DefenceContext.Current.WaveSystem.OnEnding += ShowEnding;

        // 색상 초기화
        _fadeImage.color = new Color(_fadeImage.color.r, _fadeImage.color.g, _fadeImage.color.b, 0f);
        _successImage.color = new Color(_successImage.color.r, _successImage.color.g, _successImage.color.b, 0f);
        _failureImage.color = new Color(_failureImage.color.r, _failureImage.color.g, _failureImage.color.b, 0f);
        _successLine.color = new Color(_successLine.color.r, _successLine.color.g, _successLine.color.b, 0f);
        _failureLine.color = new Color(_failureLine.color.r, _failureLine.color.g, _failureLine.color.b, 0f);
    }
    private void OnDestroy()
    {
        if (DefenceContext.Current == null) return;

        DefenceContext.Current.WaveSystem.OnEnding -= ShowEnding;
    }

    private void ShowEnding(EndingType endingType)
    {
        SoundManager.Instance.PlaySfx($"SFX_D_stage_{ConvertToEndingText(endingType)}", 0.7f);

        // fade
        _fadeImage.gameObject.SetActive(true);
        _fadeImage.DOFade(_dimAlpha, _duration).SetEase(Ease.InOutSine);

        // coin
        _coinSkeletonGraphic01.gameObject.SetActive(endingType == EndingType.Success);
        _coinSkeletonGraphic02.gameObject.SetActive(endingType == EndingType.Success);

        _coinSkeletonGraphic01.startingAnimation = "coin_rotation";
        _coinSkeletonGraphic02.startingAnimation = "coin_rotation";

        _coinSkeletonGraphic01.Initialize(true);
        _coinSkeletonGraphic02.Initialize(true);

        _costText.gameObject.SetActive(endingType == EndingType.Success);
        _costText.text = $"돌려받은 골드: {(int)(DefenceContext.Current.GridMap.CalculateAllOccupiedTowerCost() * 0.5f)}원";

        // result (success, failure)
        _successImage.gameObject.SetActive(endingType == EndingType.Success);
        _failureImage.gameObject.SetActive(endingType == EndingType.Failure);
        _successLine.gameObject.SetActive(endingType == EndingType.Success);
        _failureLine.gameObject.SetActive(endingType == EndingType.Failure);

        _successImage.DOFade(1f, _duration).SetEase(Ease.InOutSine);
        _failureImage.DOFade(1f, _duration).SetEase(Ease.InOutSine);
        _successLine.DOFade(1f, _duration).SetEase(Ease.InOutSine);
        _failureLine.DOFade(1f, _duration).SetEase(Ease.InOutSine);

        // monster
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

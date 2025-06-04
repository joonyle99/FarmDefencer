using UnityEngine;
using UnityEngine.UI;

public class EndingUI : MonoBehaviour
{
    [SerializeField] private Image _lineLeft;
    [SerializeField] private Image _lineRight;
    [SerializeField] private Image _successImage;
    [SerializeField] private Image _failureImage;

    private void Start()
    {
        DefenceContext.Current.WaveSystem.OnSuccess -= ShowSuccess;
        DefenceContext.Current.WaveSystem.OnSuccess += ShowSuccess;

        DefenceContext.Current.WaveSystem.OnFailure -= ShowFailure;
        DefenceContext.Current.WaveSystem.OnFailure += ShowFailure;
    }
    private void OnDestroy()
    {
        if (DefenceContext.Current == null) return;

        DefenceContext.Current.WaveSystem.OnSuccess -= ShowSuccess;
        DefenceContext.Current.WaveSystem.OnFailure -= ShowFailure;
    }

    public void ShowSuccess()
    {
        _lineLeft.gameObject.SetActive(true);
        _lineRight.gameObject.SetActive(true);
        _successImage.gameObject.SetActive(true);
        _failureImage.gameObject.SetActive(false);
        SoundManager.Instance.PlaySfx("SFX_D_stage_success", 0.7f);
    }
    public void ShowFailure()
    {
        _lineLeft.gameObject.SetActive(true);
        _lineRight.gameObject.SetActive(true);
        _successImage.gameObject.SetActive(false);
        _failureImage.gameObject.SetActive(true);
        SoundManager.Instance.PlaySfx("SFX_D_stage_fail", 0.7f);
    }
}

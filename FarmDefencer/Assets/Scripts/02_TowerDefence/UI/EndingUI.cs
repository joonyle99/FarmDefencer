using Spine;
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

#if UNITY_EDITOR
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1) && Input.GetKey(KeyCode.LeftShift))
        {
            HideFailure();
            ShowSuccess();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2) && Input.GetKey(KeyCode.LeftShift))
        {
            HideSuccess();
            ShowFailure();
        }
    }
#endif

    public void ShowSuccess()
    {
        ShowLine();
        _successImage.gameObject.SetActive(true);
        SoundManager.Instance.PlaySfx("SFX_D_stage_success");
    }
    public void HideSuccess()
    {
        _successImage.gameObject.SetActive(false);
    }
    public void ShowFailure()
    {
        ShowLine();
        _failureImage.gameObject.SetActive(true);
        SoundManager.Instance.PlaySfx("SFX_D_stage_fail");
    }
    public void HideFailure()
    {
        _failureImage.gameObject.SetActive(false);
    }
    public void ShowLine()
    {
        _lineLeft.gameObject.SetActive(true);
        _lineRight.gameObject.SetActive(true);
    }
    public void HideLine()
    {
        _lineLeft.gameObject.SetActive(false);
        _lineRight.gameObject.SetActive(false);
    }
}

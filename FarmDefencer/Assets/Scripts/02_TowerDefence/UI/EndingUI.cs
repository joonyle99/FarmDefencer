using UnityEngine;
using UnityEngine.UI;

public class EndingUI : MonoBehaviour
{
    [SerializeField] private Image _successImage;
    [SerializeField] private Image _failureImage;

    private void OnEnable()
    {
        DefenceContext.Current.WaveSystem.OnSuccess -= ShowSuccess;
        DefenceContext.Current.WaveSystem.OnSuccess += ShowSuccess;

        DefenceContext.Current.WaveSystem.OnFailure -= ShowFailure;
        DefenceContext.Current.WaveSystem.OnFailure += ShowFailure;
    }
    private void OnDisable()
    {
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
        _successImage.gameObject.SetActive(true);
    }
    public void HideSuccess()
    {
        _successImage.gameObject.SetActive(false);
    }
    public void ShowFailure()
    {
        _failureImage.gameObject.SetActive(true);
    }
    public void HideFailure()
    {
        _failureImage.gameObject.SetActive(false);
    }
}

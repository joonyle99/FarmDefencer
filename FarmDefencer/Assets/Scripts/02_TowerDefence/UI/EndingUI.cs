using UnityEngine;

public class EndingUI : JoonyleGameDevKit.Singleton<EndingUI>
{
    public GameObject success;
    public GameObject failure;

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

    public void ShowSuccess()
    {
        success.SetActive(true);
    }
    public void HideSuccess()
    {
        success.SetActive(false);
    }
    public void ShowFailure()
    {
        failure.SetActive(true);
    }
    public void HideFailure()
    {
        failure.SetActive(false);
    }
}

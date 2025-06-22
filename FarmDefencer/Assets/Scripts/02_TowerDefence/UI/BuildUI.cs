using UnityEngine;

public class BuildUI : MonoBehaviour
{
    public void ShowPanel()
    {
        if (this.gameObject.activeSelf)
        {
            return;
        }

        this.gameObject.SetActive(true);
    }
    public void HidePanel()
    {
        if (!this.gameObject.activeSelf)
        {
            return;
        }

        this.gameObject.SetActive(false);
    }
}

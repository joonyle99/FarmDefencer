using UnityEngine;

public class UpgradePanel : MonoBehaviour
{
    private Tower _owner;

    public void SetOwner(Tower owner)
    {
        _owner = owner;
    }

    public void ShowPanel()
    {
        this.gameObject.SetActive(true);
    }
    public void HidePanel()
    {
        this.gameObject.SetActive(false);
    }
}

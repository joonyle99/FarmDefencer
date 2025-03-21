using UnityEngine;

public class UpgradePanel : MonoBehaviour
{
    private Tower _owner;

    public void SetOwner(Tower owner)
    {
        _owner = owner;
    }

    public void Upgrade()
    {
        _owner.Upgrade();
    }
    public void Sell()
    {
        _owner.Sell();
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

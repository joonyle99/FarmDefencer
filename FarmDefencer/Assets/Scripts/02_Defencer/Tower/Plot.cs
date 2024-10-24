using UnityEngine;

public class Plot : MonoBehaviour
{
    [Header("式式式式式式式式 Plot 式式式式式式式式")]

    [Space]

    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private Color _hoverColor;
    private Color _startColor;

    private Tower _occupiedTower;

    private void Start()
    {
        _startColor = _spriteRenderer.color;
    }

    private void OnMouseEnter()
    {
        _spriteRenderer.color = _hoverColor;
    }
    private void OnMouseExit()
    {
        _spriteRenderer.color = _startColor;
    }
    private void OnMouseDown()
    {
        if (_occupiedTower != null)
        {
            Debug.LogWarning("Already tower has occupied, you should build other plot place");
            return;
        }

        var towerToBuild = BuildManager.Instance.GetSelectedTower();

        if (towerToBuild == null)
        {
            Debug.LogWarning("There is no tower to build, you should select tower");
            return;
        }

        _occupiedTower = Instantiate(towerToBuild, transform.position, Quaternion.identity);
    }
}

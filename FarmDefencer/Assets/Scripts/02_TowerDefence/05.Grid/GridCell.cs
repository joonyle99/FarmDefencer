using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Å¸¿ö¸¦ °Ç¼³ÇÒ ¼ö ÀÖ´Â ºÎÁö¸¦ ³ªÅ¸³»´Â ÄÄÆ÷³ÍÆ®ÀÔ´Ï´Ù.
/// ¸¶¿ì½ºÀÔ·Â ¹× ÅÍÄ¡ÀÔ·ÂÀ» ÅëÇØ Å¸¿ö °Ç¼³ ±â´ÉÀ» Á¦°øÇÕ´Ï´Ù
/// </summary>
/// <remarks>
/// - Å¬¸¯ ½Ã BuildSupervisor¿¡¼­ ¼±ÅÃµÈ Å¸¿ö¸¦ ÇØ´ç À§Ä¡¿¡ °Ç¼³ÇÕ´Ï´Ù.
/// </remarks>
public class GridCell : MonoBehaviour
{
    [Header("¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡ GridCell ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡")]
    [Space]

    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private Color _hoverColor;
    private Color _initColor;
    private Color _startColor;

    private Tower _occupiedTower;

    [Space]

    public TextMeshPro textMeshPro;

    [Space]

    public GridCell prevGridCell;
    public Vector2Int cellPosition;

    [Space]

    public bool isUsable;
    public int distanceCost;

    private int _changedColorReferenceCount = 0;

    private void Start()
    {
        _initColor = _spriteRenderer.color;
        _startColor = _spriteRenderer.color;
    }

    private void OnMouseEnter()
    {
        if (EventSystem.current.IsPointerOverGameObject() == true)
        {
            return;
        }

        if (_occupiedTower != null || isUsable == false)
        {
            return;
        }

        OnHover();
    }
    private void OnMouseExit()
    {
        if (EventSystem.current.IsPointerOverGameObject() == true)
        {
            return;
        }

        OffHover();
    }

    private void OnMouseDown()
    {
        if (EventSystem.current.IsPointerOverGameObject() == true)
        {
            return;
        }

        // ï¿½ï¿½ï¿½ï¿½ Å¸ï¿½ï¿½ ï¿½Ç¼ï¿½ ï¿½ï¿½ï¿½Â°ï¿½ ï¿½Æ´Ï¶ï¿½ï¿½ ï¿½Ìºï¿½Æ®ï¿½ï¿½ Ã³ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ ï¿½Ê´Â´ï¿½
        if (GameStateManager.Instance.CurrentState != GameState.Build)
        {
            return;
        }

        if (_occupiedTower != null)
        {
            // °­È­ UI´Â ¾îµð¿¡ ¶ç¿ö¾ß ÇÏ´Â°¡..?

            // Å¸¿ö°¡ Á¡À¯µÇ¾î ÀÖ´Â »óÅÂ, Å¬¸¯ ½Ã °­È­ ¸Þ´º¸¦ ¶ç¿î´Ù
            _occupiedTower.ShowPanel();
        }
        else if (_occupiedTower == null && isUsable == true)
        {
            // Å¸¿ö°¡ Á¡À¯µÇ¾î ÀÖÁö ¾Ê°í, »ç¿ëÇÒ ¼ö ÀÖ´Â »óÅÂ
            Occupy();
        }
        else
        {
            // Å¸¿ö°¡ Á¡À¯ÇÏ°í ÀÖÁö´Â ¾Ê°í, »ç¿ëÇÒ ¼ö ¾ø´Â »óÅÂ
            // e.g) start / end point
        }
    }

    // sprite
    private void OnHover()
    {
        if (_spriteRenderer.color == _hoverColor)
        {
            return;
        }

        _spriteRenderer.color = _hoverColor;
    }
    private void OffHover()
    {
        if (_spriteRenderer.color == _startColor)
        {
            return;
        }

        _spriteRenderer.color = _startColor;
    }
    private void Appear()
    {
        var color = _spriteRenderer.color;
        color.a = _startColor.a;
        _spriteRenderer.color = color;
    }
    private void Disappear()
    {
        var color = _spriteRenderer.color;
        color.a = 0f;
        _spriteRenderer.color = color;
    }

    public void Usable()
    {
        isUsable = true;
        Appear();
        // gameObject.SetActive(true);
    }
    public void UnUsable()
    {
        isUsable = false;
        Disappear();
        // gameObject.SetActive(false);
    }

    public void Occupy()
    {
        _occupiedTower = DefenceContext.Current.BuildSystem.InstantiateTower(transform.position, Quaternion.identity);
        _occupiedTower.OccupyingGridCell(this);

        if (_occupiedTower != null)
        {
            OffHover();
            UnUsable();
        }
    }
    public void DeleteOccupiedTower()
    {
        _occupiedTower = null;
    }

    // debug
    public void DebugChangeColor(Color color)
    {
        _changedColorReferenceCount++;

        _spriteRenderer.color = color;
        _startColor = _spriteRenderer.color;
    }
    public void DebugResetColor()
    {
        _changedColorReferenceCount--;

        if (_changedColorReferenceCount == 0)
        {
            _spriteRenderer.color = _initColor;
            _startColor = _spriteRenderer.color;
        }
    }
}

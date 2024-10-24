using UnityEngine;

public class Plot : MonoBehaviour
{
    [SerializeField] private Tower _tower;

    [Space]

    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private Color _hoverColor;

    private Color _startColor;

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
        Debug.Log($"{gameObject.name}");

        var tower = Instantiate(_tower, transform.position, Quaternion.identity);
    }
}

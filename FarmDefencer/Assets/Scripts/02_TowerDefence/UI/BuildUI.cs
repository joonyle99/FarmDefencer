using UnityEngine;
using UnityEngine.EventSystems;

public class BuildUI : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [SerializeField] private int selectedIndex = 0;
    [SerializeField] private BuildSystem _buildSystem;

    private void OnEnable()
    {
        //if (_buildSystem == null)
        //{
        //    return;
        //}
    }
    private void OnDisable()
    {
        //if (_buildSystem == null)
        //{
        //    return;
        //}
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _buildSystem.selectedIndex = selectedIndex;

        _buildSystem.Pick(eventData);
    }
    public void OnDrag(PointerEventData eventData)
    {
        _buildSystem.Move(eventData);
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        _buildSystem.Place(eventData);
    }
}

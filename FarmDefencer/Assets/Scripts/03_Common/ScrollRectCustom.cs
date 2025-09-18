using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ScrollRectCustom : ScrollRect
{
    private WorldPageController _worldPageController;
    private Vector2 _dragBeginPos;

    protected override void Awake()
    {
        base.Awake();

        _worldPageController = GetComponent<WorldPageController>();
    }

    public override void OnBeginDrag(PointerEventData eventData)
    {
        base.OnBeginDrag(eventData);

        _dragBeginPos = eventData.position;
    }
    public override void OnDrag(PointerEventData eventData)
    {
        var deltaPos = _dragBeginPos - eventData.position;

        if (deltaPos.x < -0.01f && _worldPageController.CanPrevPaging == false) return;
        else if (deltaPos.x > 0.01f && _worldPageController.UnlockNextPaging == false) return;

        base.OnDrag(eventData);

        _worldPageController.UpdateCurrPage();
    }
    public override void OnEndDrag(PointerEventData eventData)
    {
        base.OnEndDrag(eventData);

        _dragBeginPos = Vector2.zero;

        _worldPageController.MoveCurrPage();
    }
}

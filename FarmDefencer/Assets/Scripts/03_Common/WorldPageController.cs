using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WorldPageController : MonoBehaviour
{
    [SerializeField] private WorldUI _worldUI;
    [SerializeField] private RectTransform _contentTransform;

    [Space]

    [SerializeField] private Ease _pagingType;
    [SerializeField] private float _pagingDuration;

    [Space]

    [SerializeField] private Button _prevButton;
    [SerializeField] private Button _nextButton;

    [Space]

    private float _step;
    private int _curPage;

    private List<float> _originPosXList = new();

    public int CurPage => _curPage;
    public float CurPagePosX => -1 * _step * (_curPage - 1);

    public bool CanPrevPaging => _worldUI.TravelMap.MapId > MapManager.Instance.MinMapIdx;
    public bool CanNextPaging => _worldUI.TravelMap.MapId < MapManager.Instance.MaxMapIdx;
    public bool UnlockNextPaging => _worldUI.TravelMap.MapId < MapManager.Instance.MaximumUnlockedMapIndex;

    private void Awake()
    {
        // 페이지 스텝 계산
        var child = _contentTransform.GetChild(0);
        var childRectTransform = child.GetComponent<RectTransform>();
        _step = childRectTransform.rect.width;

        // 페이지 기준 위치 계산
        var spacing = -1 * _step;
        var start = 1 * _step * 0.5f;
        var maxMapIdx = MapManager.Instance.MaxMapIdx;
        for (int i = 0; i < maxMapIdx + 2; i++)
        {
            _originPosXList.Add(start + spacing * i);
        }

        // 버튼 이벤트 등록
        _prevButton.onClick.AddListener(() => MovePrevPage());
        _nextButton.onClick.AddListener(() => MoveNextPage());
    }
    private void Start()
    {
        // 현재 페이지 초기화
        InitCurrPage();

        // 페이지 위치 초기화
        MovePageInstant();

        // UI 갱신
        Refresh();
    }

    public void InitCurrPage()
    {
        _curPage = _worldUI.TravelMap.MapId;
    }
    public void UpdateCurrPage()
    {
        var tempPage = -1;
        var curPosX = _contentTransform.anchoredPosition.x;

        for (int i = 0; i < _originPosXList.Count - 2; i++)
        {
            var left = _originPosXList[i];
            var right = _originPosXList[i + 1];

            if (curPosX <= left && curPosX >= right)
            {
                tempPage = i + 1;
                break;
            }
        }

        if (tempPage != -1 && tempPage != _curPage)
        {
            if (tempPage < _curPage)
            {
                MovePrevPage(true);
            }
            else
            {
                MoveNextPage(true);
            }
        }
    }

    public void MoveCurrPage(bool skipMove = false)
    {
        if (skipMove == false)
        {
            MovePageSmooth();
        }
        Refresh();
    }
    public void MovePrevPage(bool skipMove = false)
    {
        if (CanPrevPaging)
        {
            var prevPage = _curPage - 1;
            _worldUI.ChangeTravelMap(prevPage);
            _curPage = prevPage;
            if (skipMove == false)
            {
                MovePageSmooth();
            }
            Refresh();
        }
    }
    public void MoveNextPage(bool skipMove = false)
    {
        if (CanNextPaging)
        {
            var nextPage = _curPage + 1;
            _worldUI.ChangeTravelMap(nextPage);
            _curPage = nextPage;
            if (skipMove == false)
            {
                MovePageSmooth();
            }
            Refresh();
        }
    }

    private void Refresh()
    {
        // 페이지 이동 버튼 갱신
        RefreshMoveButton();
    }
    private void RefreshMoveButton()
    {
        _prevButton.gameObject.SetActive(CanPrevPaging);
        _nextButton.gameObject.SetActive(CanNextPaging);

        _nextButton.interactable = CanNextPaging && UnlockNextPaging;
    }

    private void MovePageInstant()
    {
        _contentTransform.anchoredPosition = new Vector2(CurPagePosX, _contentTransform.anchoredPosition.y);
    }
    private void MovePageSmooth()
    {
        _contentTransform.DOAnchorPosX(CurPagePosX, _pagingDuration).SetEase(_pagingType);
    }
}

using DG.Tweening;
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

    [SerializeField] private Button _previousButton;
    [SerializeField] private Button _nextButton;

    [Space]

    [SerializeField] private Vector3 _pagingStep; // TODO: 현재 Content와 Map을 가지고 page step 설정하기
    private Vector3 _targetPos;

    private void Start()
    {
        var travelMapIdx = _worldUI.TravelMap.MapId;

        _previousButton.onClick.AddListener(PreviousPage);
        _nextButton.onClick.AddListener(NextPage);

        RefreshButton();

        // 현재 위치한 맵으로 페이지 위치를 설정한다

        if (travelMapIdx == 1)
        {
            _targetPos -= _pagingStep;
        }
        else if (travelMapIdx == 2)
        {
            _targetPos = Vector3.zero;
        }
        else if (travelMapIdx == 3)
        {
            _targetPos += _pagingStep;
        }

        MovePageInstant();
    }

    public void RefreshButton()
    {
        var travelMapIdx = _worldUI.TravelMap.MapId;

        _previousButton.gameObject.SetActive(travelMapIdx > MapManager.Instance.MinMapIdx);
        _nextButton.interactable = travelMapIdx + 1 <= MapManager.Instance.MaximumUnlockedMapIndex;
        _nextButton.gameObject.SetActive(travelMapIdx < MapManager.Instance.MaxMapIdx);
    }

    public void PreviousPage()
    {
        var travelMapIdx = _worldUI.TravelMap.MapId;

        if (travelMapIdx > MapManager.Instance.MinMapIdx)
        {
            _worldUI.ChangeTravelMap(travelMapIdx - 1);

            _targetPos -= _pagingStep;
            MovePage();
        }
    }
    public void NextPage()
    {
        var travelMapIdx = _worldUI.TravelMap.MapId;

        if (travelMapIdx < MapManager.Instance.MaxMapIdx)
        {
            _worldUI.ChangeTravelMap(travelMapIdx + 1);

            _targetPos += _pagingStep;
            MovePage();
        }
    }

    private void MovePageInstant()
    {
        _contentTransform.anchoredPosition = _targetPos;
    }
    private void MovePage()
    {
        _contentTransform.DOLocalMove(_targetPos, _pagingDuration).SetEase(_pagingType);
        RefreshButton();
    }
}

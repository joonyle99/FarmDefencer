using UnityEngine;
using UnityEngine.EventSystems;

public class DragDrop : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    // 월드에 생성할 타워 프리팹
    public GameObject towerPrefab;

    private GameObject ghostTower; // 드래그 시 따라다니는 복제된 타워 이미지

    private Canvas _canvas; // 드래그할 때 사용할 UI 캔버스 (드래그 위치 계산에 필요)
    private RectTransform _rectTransform;

    private void Awake()
    {
        _canvas = GetComponentInParent<Canvas>();
        _rectTransform = GetComponent<RectTransform>();
    }

    // 드래그 시작 시 호출: ghostTower 생성
    public void OnBeginDrag(PointerEventData eventData)
    {
        // 현재 UI 타워를 복제하여 드래그 중 시각적 피드백 제공
        // 필요시 ghostTower의 이미지 투명도나 색상을 조정하여 드래그 중임을 명확히 할 수 있음
        ghostTower = Instantiate(gameObject, transform.parent);
    }

    // 드래그 중 호출: ghostTower를 마우스 위치에 따라 이동
    public void OnDrag(PointerEventData eventData)
    {
        if (ghostTower != null)
        {
            Vector2 localPoint;
            RectTransform canvasRect = _canvas.transform as RectTransform;
            // 마우스 위치(Screen Point)를 캔버스 내의 로컬 좌표로 변환
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, eventData.position, _canvas.worldCamera, out localPoint))
            {
                ghostTower.GetComponent<RectTransform>().localPosition = localPoint;
            }
        }

        // 여기서 GridCell에 설치할 수 있는지에 대한 여부를 판단해야 한다.
        // 우선 가장 간단한 조건 체크부터 한다 (Update로 도는 것이기 때문에 판별을 계속 해주면 안된다..?)
        // 1. 현재 GridCell에 몬스터가 들어와 있는지 체크
        // 2. 현재 GridCell에 타워가 이미 설치되어 있는지 체크
        // 3. 현재 GridCell이 시작점 혹은 도착점인지 체크
        // ..
        // 이런 체크들을 통해 1차로 걸러낸다..
        // 그리고 체크를 할 때 이 OnDrag 함수에서 어떻게 하면 현재 마우스 위치에서 GridCell을 찾을 수 있을지를 생각해보자
    }

    // 드래그 종료 시 호출: ghostTower 제거 및 월드에 타워 생성
    public void OnEndDrag(PointerEventData eventData)
    {
        if (ghostTower != null)
        {
            Camera mainCam = Camera.main;
            Vector3 worldPos = Vector3.zero;
            // 마우스 위치에서 월드 좌표를 얻기 위해 Raycast 사용 (예: 바닥 충돌 판정)
            Ray ray = mainCam.ScreenPointToRay(eventData.position);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                worldPos = hit.point;
            }
            else
            {
                // Raycast 실패 시, 임의의 깊이 값을 사용하여 변환
                Vector3 screenPos = eventData.position;
                screenPos.z = 10f; // 카메라로부터의 거리
                worldPos = mainCam.ScreenToWorldPoint(screenPos);
            }

            // 실제 타워 프리팹을 월드에 생성
            Instantiate(towerPrefab, worldPos, Quaternion.identity);

            // 드래그용 ghostTower 제거
            Destroy(ghostTower);
        }
    }
}

using UnityEngine;
using UnityEngine.InputSystem;

namespace FloweryTest
{
    public class TestPlayerF : MonoBehaviour
    {
        public float CameraMovementScale = 1.0f;
        public Vector2 MousePosition;
        public GameObject FarmObject;
        private Farm _farmComponent;
        private bool _isLeftClicked;
        
        public void OnCameraMove(InputValue inputValue)
        {
            if (_isLeftClicked)
            {
                var scaledInputVector = inputValue.Get<Vector2>() * CameraMovementScale;
                transform.position += (Vector3)scaledInputVector;
            }
        }

        public void OnMouseMove(InputValue inputValue)
        {
            MousePosition = inputValue.Get<Vector2>();
        }

        public void OnPlant()
        {
            // 작물 심기 코드
            var mouseWorldPosition = GetComponent<Camera>().ScreenToWorldPoint(MousePosition);
            if (!_farmComponent.TryFindCropAt(mouseWorldPosition, out var crop))
            {
                return;
            }

            if (crop.TryPlant())
            {
                Debug.Log("작물을 심었습니다.");
            }
            else
            {
                Debug.Log("작물이 이미 심어져 있습니다.");
            }
        }

        public void OnHarvest()
        {
            // 작물 수확 코드
            var mouseWorldPosition = GetComponent<Camera>().ScreenToWorldPoint(MousePosition);
            if (!_farmComponent.TryFindCropAt(mouseWorldPosition, out var crop))
            {
                return;
            }

            if (crop.TryHarvest())
            {
                Debug.Log("작물을 수확했습니다.");
            }
            else
            {
                Debug.Log("작물을 수확할 수 없습니다.");
            }
        }

        public void OnWatering()
        {
            // 물주기 코드
            var mouseWorldPosition = GetComponent<Camera>().ScreenToWorldPoint(MousePosition);
            if (!_farmComponent.TryFindCropAt(mouseWorldPosition, out var crop))
            {
                return;
            }

            if (crop.TryWatering(1.0f))
            {
                Debug.Log("작물에 물을 줬습니다.");
            }
            else
            {
                Debug.Log("작물에 물을 줄 수 없습니다.");
            }
        }

        public void OnMouseClick(InputValue inputValue)
        {
            _isLeftClicked = inputValue.Get<float>() == 1;
        }

        private void Awake()
        {
            _farmComponent = FarmObject.GetComponent<Farm>();
        }
    }
}

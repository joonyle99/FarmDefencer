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
        //
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
            // �۹� �ɱ� �ڵ�
            var mouseWorldPosition = GetComponent<Camera>().ScreenToWorldPoint(MousePosition);
            if (!_farmComponent.TryFindCropAt(mouseWorldPosition, out var crop))
            {
                return;
            }

            crop.TryPlant();
        }

        public void OnHarvest()
        {
            // �۹� ��Ȯ �ڵ�
            var mouseWorldPosition = GetComponent<Camera>().ScreenToWorldPoint(MousePosition);
            if (!_farmComponent.TryFindCropAt(mouseWorldPosition, out var crop))
            {
                return;
            }

            crop.TryHarvest();
        }

        public void OnWatering()
        {
            // ���ֱ� �ڵ�
            var mouseWorldPosition = GetComponent<Camera>().ScreenToWorldPoint(MousePosition);
            if (!_farmComponent.TryFindCropAt(mouseWorldPosition, out var crop))
            {
                return;
            }

            crop.TryWatering(1.0f);
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

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
        public Camera Camera;
        public WateringCan WateringCan;

        public void OnCameraMove(InputValue inputValue)
        {
            if (_isLeftClicked && !WateringCan.Using)
            {
                var scaledInputVector = inputValue.Get<Vector2>() * CameraMovementScale;
                transform.position += (Vector3)scaledInputVector;
            }
        }

        public void OnMouseMove(InputValue inputValue)
        {
            MousePosition = inputValue.Get<Vector2>();
        }

        public void OnMouseClick(InputValue inputValue)
        {
            _isLeftClicked = inputValue.Get<float>() == 1;
            if (_isLeftClicked)
            {
                if (_farmComponent.TryFindCropAt(Camera.ScreenToWorldPoint(MousePosition), out var crop))
                {
                    crop.OnTap();
                }
            }
        }

        public void OnWatering(Vector2Int position)
        {
			if (_farmComponent.TryFindCropAt(position, out var crop))
			{
                crop.OnWatering();
			}
		}

        private void Awake()
        {
            _farmComponent = FarmObject.GetComponent<Farm>();
        }
    }
}

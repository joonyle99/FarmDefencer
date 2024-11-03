using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace FloweryTest
{
    public class TestPlayerF : MonoBehaviour
    {
        public float LongTapThreshold = 1.0f;
        public float CameraMovementScale = 1.0f;
        public Vector2 MousePosition;
        public GameObject FarmObject;
		public Camera Camera;
        public WateringCan WateringCan;
        private Farm _farmComponent;
		private float _holdingTimeElapsed;
		private bool _isPressing;

		// 위치 delta를 받음
        public void OnCameraMove(InputValue inputValue)
        {
            if (_isPressing && !WateringCan.Using)
            {
                var scaledInputVector = inputValue.Get<Vector2>() * CameraMovementScale;
                transform.position += (Vector3)scaledInputVector;
            }
        }

		// 스크린 절대 좌표를 받음
        public void OnMouseMove(InputValue inputValue)
        {
            MousePosition = inputValue.Get<Vector2>();
        }

        public void OnWatering(Vector2Int position)
        {
			if (_farmComponent.TryFindCropAt(position, out var crop))
			{
                crop.OnWatering();
			}
		}

		public void OnTap()
		{
			_farmComponent.TapAction(Camera.ScreenToWorldPoint(MousePosition));
		}

        public void OnHold(InputValue inputValue)
        {
            _isPressing = inputValue.Get<float>() == 1;
        }

		private void Update()
		{
			if (_isPressing)
            {
                _holdingTimeElapsed += Time.deltaTime;
                _farmComponent.HoldingAction(Camera.ScreenToWorldPoint(MousePosition), _holdingTimeElapsed);
            }
            else
            {
                _holdingTimeElapsed = 0.0f;
            }
		}

		private void Awake()
        {
            _farmComponent = FarmObject.GetComponent<Farm>();
        }
    }
}

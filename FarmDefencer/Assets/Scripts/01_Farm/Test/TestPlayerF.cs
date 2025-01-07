using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace FarmTest
{
    public class FarmTestPlayer : MonoBehaviour
    {
        public float LongTapThreshold = 1.0f;
        public float CameraMovementScale = 1.0f;
        public Vector2 MousePosition;
        public GameObject FarmObject;
		public Camera Camera;
        public WateringCan WateringCan;
        public FarmClock FarmClock;
		public FarmManager FarmManager;
        private Farm _farmComponent;

		private float _singleHoldingTimeElapsed;
        private bool _isSingleHolding;
		private bool _isDoubleHolding;
        private Vector2 _singleHoldBeginPosition;
        private TMP_Text _remainingDaytimeText;

		// 위치 delta를 받음
        public void OnCameraMove(InputValue inputValue)
        {
            if (_isDoubleHolding && !WateringCan.Using)
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
            _farmComponent.WateringAction(position);
		}

		public void OnSingleTap()
		{
			_farmComponent.TapAction(Camera.ScreenToWorldPoint(MousePosition));
		}

        public void OnSingleHold(InputValue inputValue)
        {
            var currentHold = inputValue.Get<float>() == 1;

			if (!_isSingleHolding && currentHold)
            {
				_singleHoldBeginPosition = MousePosition;
			}

            _isSingleHolding = currentHold;
		}

		public void OnDoubleHold(InputValue inputValue)
		{
			_isDoubleHolding = inputValue.Get<float>() == 1;
		}

		public void OnToggleAvailability()
        {
            var productUniqueId = transform.Find("Canvas/DebugFieldLockUnlock/ProductUniqueIdInputField").GetComponent<TMP_InputField>().text;
            var currentAvailability = FarmManager.GetAvailability(productUniqueId);
            FarmManager.SetAvailability(productUniqueId, !currentAvailability);
        }

		private void Update()
		{
            _remainingDaytimeText.text = $"Remaining Daytime: {FarmClock.RemainingDaytime:f2}s";

            if (_isSingleHolding)
            {
                _singleHoldingTimeElapsed += Time.deltaTime;

				_farmComponent.SingleHoldingAction(
					Camera.ScreenToWorldPoint(_singleHoldBeginPosition),
					MousePosition - _singleHoldBeginPosition,
					false,
					Time.deltaTime);
                
            }
            else if (_singleHoldingTimeElapsed > 0.0f) // Single Hold가 종료된 직후
            {
				_farmComponent.SingleHoldingAction(
					Camera.ScreenToWorldPoint(_singleHoldBeginPosition),
	                MousePosition - _singleHoldBeginPosition,
	                true,
					Time.deltaTime);
                _singleHoldingTimeElapsed = 0.0f;
			}
		}

		private void Awake()
        {
            _farmComponent = FarmObject.GetComponent<Farm>();
            _remainingDaytimeText = transform.Find("Canvas/DebugArea/DebugTimer/RemainingDaytimeText").GetComponent<TMP_Text>();
            transform.Find("Canvas/DebugArea/DebugTimer/DaytimeRandomSetButton")
                .GetComponent<Button>()
                .onClick.AddListener(() => FarmClock.SetRemainingDaytimeRandom(300.0f, 600.0f));
            transform.Find("Canvas/DebugArea/DebugTimer/Daytime5sSetButton").GetComponent<Button>()
                .GetComponent<Button>()
                .onClick.AddListener(() => FarmClock.SetRemainingDaytimeBy(5.0f));          
            transform.Find("Canvas/DebugArea/DebugTimer/PauseButton").GetComponent<Button>()
                .GetComponent<Button>()
                .onClick.AddListener(
                () =>
                {
                    FarmClock.IsManuallyPaused = !FarmClock.IsManuallyPaused;
                    transform.Find("Canvas/DebugArea/DebugTimer/PauseButton").GetComponentInChildren<TMP_Text>().text = FarmClock.IsManuallyPaused ? "Play" : "Pause";
                });
        }
    }
}

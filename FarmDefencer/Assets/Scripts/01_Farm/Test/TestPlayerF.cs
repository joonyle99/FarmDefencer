using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace FarmTest
{
    public class FarmTestPlayer : MonoBehaviour
    {
        public Vector2 BottomLeftPosition = new Vector2(0.0f, 0.0f);
        public Vector2 TopRightPosition = new Vector2(23.0f, 16.0f);

        public float LongTapThreshold = 1.0f;
        public float CameraMovementScale = 1.0f;
        public Vector2 LastInteractWorldPosition;
        public GameObject FarmObject;
        public FarmClock FarmClock;
		public FarmManager FarmManager;
        private Farm _farmComponent;

		private float _singleHoldingTimeElapsed;
        private bool _isSingleHolding;
		private bool _isDoubleHolding;
        private Vector2 _singleHoldBeginWorldPosition;
        private TMP_Text _remainingDaytimeText;
		private Camera _camera;

		// 위치 delta를 받음
		public void OnCameraMove(InputValue inputValue)
        {
            if (_isDoubleHolding)
            {
                var scaledInputVector = inputValue.Get<Vector2>() * CameraMovementScale;
                var newPosition = transform.position + (Vector3)scaledInputVector;
				newPosition.x = Mathf.Clamp(newPosition.x, BottomLeftPosition.x, TopRightPosition.x);
				newPosition.y = Mathf.Clamp(newPosition.y, BottomLeftPosition.y, TopRightPosition.y);
				newPosition.z = -10.0f;
				transform.position = newPosition;
			}
        }

		// 현재 누르고 있는 스크린 절대 좌표를 받음
        public void OnInteract(InputValue inputValue)
        {
            LastInteractWorldPosition = _camera.ScreenToWorldPoint(inputValue.Get<Vector2>());
        }

		public void OnSingleTap()
		{
			_farmComponent.TapAction(LastInteractWorldPosition);
		}

        public void OnSingleHold(InputValue inputValue)
        {
            var currentHold = inputValue.Get<float>() >= 0.5f;

			if (!_isSingleHolding && currentHold)
            {
				_singleHoldBeginWorldPosition = LastInteractWorldPosition;
			}

            _isSingleHolding = currentHold;
		}

		public void OnDoubleHold(InputValue inputValue)
		{
			_isDoubleHolding = inputValue.Get<float>() >= 0.5f;
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
					_singleHoldBeginWorldPosition,
					LastInteractWorldPosition - _singleHoldBeginWorldPosition,
					false,
					Time.deltaTime);
                
            }
            else if (_singleHoldingTimeElapsed > 0.0f) // Single Hold가 종료된 직후
            {
				_farmComponent.SingleHoldingAction(
					_singleHoldBeginWorldPosition,
	                LastInteractWorldPosition - _singleHoldBeginWorldPosition,
	                true,
					Time.deltaTime);
                _singleHoldingTimeElapsed = 0.0f;
			}
		}

		private void Awake()
        {
            _camera = GetComponent<Camera>();
            _camera.tag = "MainCamera";
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

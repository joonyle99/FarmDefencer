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
        private Farm _farmComponent;

		private float _holdingTimeElapsed;
		private bool _isPressing;
        private TMP_Text _remainingDaytimeText;

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
            _farmComponent.WateringAction(position);
		}

		public void OnTap()
		{
			_farmComponent.TapAction(Camera.ScreenToWorldPoint(MousePosition));
		}

        public void OnHold(InputValue inputValue)
        {
            _isPressing = inputValue.Get<float>() == 1;
        }

        public void OnToggleAvailability()
        {
            var productUniqueId = transform.Find("Canvas/DebugFieldLockUnlock/ProductUniqueIdInputField").GetComponent<TMP_InputField>().text;
            var currentAvailability = _farmComponent.GetFieldAvailability(productUniqueId);
            _farmComponent.SetAvailability(productUniqueId, !currentAvailability);
        }

		private void Update()
		{
            _remainingDaytimeText.text = $"Remaining Daytime: {FarmClock.RemainingDaytime:f2}s";

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
            _remainingDaytimeText = transform.Find("Canvas/DebugTimer/RemainingDaytimeText").GetComponent<TMP_Text>();
            transform.Find("Canvas/DebugTimer/DaytimeRandomSetButton")
                .GetComponent<Button>()
                .onClick.AddListener(() => FarmClock.SetRemainingDaytimeRandom(300.0f, 600.0f));
            transform.Find("Canvas/DebugTimer/Daytime5sSetButton").GetComponent<Button>()
                .GetComponent<Button>()
                .onClick.AddListener(() => FarmClock.SetRemainingDaytimeBy(5.0f));          
            transform.Find("Canvas/DebugTimer/PauseButton").GetComponent<Button>()
                .GetComponent<Button>()
                .onClick.AddListener(
                () =>
                {
                    FarmClock.IsManuallyPaused = !FarmClock.IsManuallyPaused;
                    transform.Find("Canvas/DebugTimer/PauseButton").GetComponentInChildren<TMP_Text>().text = FarmClock.IsManuallyPaused ? "Play" : "Pause";
                });
        }
    }
}

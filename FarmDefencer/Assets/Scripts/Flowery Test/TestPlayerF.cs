using CropInterfaces;
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
        public CropDictionary CropDictionary;

        public void OnCameraMove(InputValue inputValue)
        {
            return; // 물뿌리개 테스트 때문에 드래그시 화면이동 방지하였음. 추후 액션 겹치지 않게 처리해야 할듯
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

            crop.TryPlant();
        }

		public void OnGrowUp()
		{
			// 작물 성장 코드
			var mouseWorldPosition = GetComponent<Camera>().ScreenToWorldPoint(MousePosition);
			if (!_farmComponent.TryFindCropAt(mouseWorldPosition, out var crop))
			{
				return;
			}

			crop.TryGrowUp();
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
                if (!CropDictionary.TryGetCropName(crop, out var cropName))
                {
                    Debug.Log("알 수 없는 작물 이름");
                    return;
                }
                Debug.Log($"{cropName}을(를) 수확했습니다.");
            }
        }

        public void OnWatering()
        {
            // 물주기 코드
            var mouseWorldPosition = GetComponent<Camera>().ScreenToWorldPoint(MousePosition);
            if (!_farmComponent.TryFindCropAt<IWaterable>(mouseWorldPosition, out var waterableCrop))
            {
                return;
            }

            waterableCrop.TryWatering(1.0f);
        }

        public void OnWatering(Vector2Int tilePosition)
        {
			if (!_farmComponent.TryFindCropAt<IWaterable>(tilePosition, out var waterableCrop))
			{
				return;
			}

			waterableCrop.TryWatering(1.0f);
            Debug.Log("물을 줬습니다");
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

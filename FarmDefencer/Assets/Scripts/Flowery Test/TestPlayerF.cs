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
            return; // ���Ѹ��� �׽�Ʈ ������ �巡�׽� ȭ���̵� �����Ͽ���. ���� �׼� ��ġ�� �ʰ� ó���ؾ� �ҵ�
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

		public void OnGrowUp()
		{
			// �۹� ���� �ڵ�
			var mouseWorldPosition = GetComponent<Camera>().ScreenToWorldPoint(MousePosition);
			if (!_farmComponent.TryFindCropAt(mouseWorldPosition, out var crop))
			{
				return;
			}

			crop.TryGrowUp();
		}

		public void OnHarvest()
        {
            // �۹� ��Ȯ �ڵ�
            var mouseWorldPosition = GetComponent<Camera>().ScreenToWorldPoint(MousePosition);
            if (!_farmComponent.TryFindCropAt(mouseWorldPosition, out var crop))
            {
                return;
            }

            if (crop.TryHarvest())
            {
                if (!CropDictionary.TryGetCropName(crop, out var cropName))
                {
                    Debug.Log("�� �� ���� �۹� �̸�");
                    return;
                }
                Debug.Log($"{cropName}��(��) ��Ȯ�߽��ϴ�.");
            }
        }

        public void OnWatering()
        {
            // ���ֱ� �ڵ�
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
            Debug.Log("���� ����ϴ�");
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

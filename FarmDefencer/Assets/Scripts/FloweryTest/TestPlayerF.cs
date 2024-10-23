using UnityEngine;
using UnityEngine.InputSystem;

namespace FloweryTest
{
    public class TestPlayerF : MonoBehaviour
    {
        public float CameraMovementScale = 1.0f;
        public Vector2 MousePosition;
        public GameObject FarmObject;
        public void OnCameraMove(InputValue inputValue)
        {
            var scaledInputVector = inputValue.Get<Vector2>() * CameraMovementScale;

            transform.position += (Vector3)scaledInputVector;
        }

        public void OnMouseMove(InputValue inputValue)
        {
            MousePosition = inputValue.Get<Vector2>();
        }

        public void OnMouseClick()
        {
            // �ش� ��ġ�� Crop�� �ִ��� Ȯ���ϴ� �ڵ�
            var mouseWorldPosition = GetComponent<Camera>().ScreenToWorldPoint(MousePosition);
            if (FarmObject.GetComponent<Farm>().TryFindCropAt(mouseWorldPosition, out var crop))
            {
                Debug.Log($"ã�ҽ��ϴ�: {crop.transform.position}");
            }
            else
            {
                Debug.Log("�� ã�ҽ��ϴ�");
            }
        }
    }
}

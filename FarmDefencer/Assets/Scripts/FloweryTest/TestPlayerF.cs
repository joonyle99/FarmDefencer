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
            // 해당 위치에 Crop이 있는지 확인하는 코드
            var mouseWorldPosition = GetComponent<Camera>().ScreenToWorldPoint(MousePosition);
            if (FarmObject.GetComponent<Farm>().TryFindCropAt(mouseWorldPosition, out var crop))
            {
                Debug.Log($"찾았습니다: {crop.transform.position}");
            }
            else
            {
                Debug.Log("못 찾았습니다");
            }
        }
    }
}

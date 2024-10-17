using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public float CameraMovementScale = 1.0f;
    public void OnCameraMove(InputValue inputValue)
    {
        var scaledInputVector = inputValue.Get<Vector2>() * CameraMovementScale;

        transform.position += (Vector3)scaledInputVector;
    }
}

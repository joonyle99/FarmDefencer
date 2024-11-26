using UnityEngine;
using UnityEngine.UI;

public class TimerUI : MonoBehaviour
{
	public float StartAngle = 170f;
	public float EndAngle = 360f;
	private Image _clockHand;

	/// <summary>
	/// ZeroDegree와 FullDegree 중 어느 위치를 가리켜야 하는지를 설정합니다.
	/// 0~1 사이의 값이며, 0일경우 ZeroDegree, 1일경우 FullDegree에 위치하게 됩니다.
	/// </summary>
	/// <param name="ratio"></param>
	public void SetClockhand(float ratio)
	{
		ratio = Mathf.Clamp01(ratio);

		float angle = Mathf.Lerp(EndAngle, StartAngle, ratio);

		_clockHand.rectTransform.localRotation = Quaternion.Euler(0f, 0f, -angle); // UI의 Z축 회전
	}

	private void Awake()
	{
		_clockHand = transform.Find("Clockhand").GetComponent<Image>();
	}
}

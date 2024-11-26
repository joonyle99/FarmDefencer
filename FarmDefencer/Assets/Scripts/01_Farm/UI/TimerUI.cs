using UnityEngine;
using UnityEngine.UI;

public class TimerUI : MonoBehaviour
{
	public float StartAngle = 170f;
	public float EndAngle = 360f;
	private Image _clockHand;

	/// <summary>
	/// ZeroDegree�� FullDegree �� ��� ��ġ�� �����Ѿ� �ϴ����� �����մϴ�.
	/// 0~1 ������ ���̸�, 0�ϰ�� ZeroDegree, 1�ϰ�� FullDegree�� ��ġ�ϰ� �˴ϴ�.
	/// </summary>
	/// <param name="ratio"></param>
	public void SetClockhand(float ratio)
	{
		ratio = Mathf.Clamp01(ratio);

		float angle = Mathf.Lerp(EndAngle, StartAngle, ratio);

		_clockHand.rectTransform.localRotation = Quaternion.Euler(0f, 0f, -angle); // UI�� Z�� ȸ��
	}

	private void Awake()
	{
		_clockHand = transform.Find("Clockhand").GetComponent<Image>();
	}
}

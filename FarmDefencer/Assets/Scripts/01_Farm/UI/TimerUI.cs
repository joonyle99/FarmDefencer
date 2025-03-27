using UnityEngine;
using UnityEngine.UI;

public sealed class TimerUI : MonoBehaviour
{
	[SerializeField] private float startAngle = 170f;
	[SerializeField] private float endAngle = 360f;
	private Image _clockHand;
	
	public void SetClockhand(float ratio)
	{
		ratio = Mathf.Clamp01(ratio);

		var angle = Mathf.Lerp(endAngle, startAngle, ratio);

		_clockHand.rectTransform.localRotation = Quaternion.Euler(0f, 0f, -angle);
	}

	private void Awake()
	{
		_clockHand = transform.Find("Clockhand").GetComponent<Image>();
	}
}

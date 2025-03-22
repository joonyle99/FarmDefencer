using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 자식으로 CropSign을 가진 오브젝트들을 둘 것.
/// </summary>
public class CropSigns : MonoBehaviour, IFarmInputLayer
{
	public Action<ProductEntry> SignClicked;

	// 클릭 판정 월드 기준 크기
	private static readonly Vector2 SignClickSize = new Vector2 { x = 1.0f, y = 1.0f };
	private List<CropSign> _cropSigns;

	public void OnSingleHolding(Vector2 initialWorldPosition, Vector2 deltaWorldPosition, bool isEnd, float deltaHoldTime)
	{
		return;
	}

	public void OnSingleTap(Vector2 worldPosition)
	{
		var cropSign = _cropSigns
			.Where(cropSign => (Mathf.Abs(cropSign.transform.position.x - worldPosition.x) < SignClickSize.x) && (Mathf.Abs(cropSign.transform.position.y - worldPosition.y) < SignClickSize.y))
			.FirstOrDefault();
		SignClicked?.Invoke(cropSign?.ProductEntry);
	}

	private void Awake()
	{
		_cropSigns = new List<CropSign>();

		for (int i = 0; i < transform.childCount; ++i)
		{
			var childObject = transform.GetChild(i);
			var cropSign = childObject.GetComponent<CropSign>();
			if (cropSign == null)
			{
				Debug.LogError($"CropSigns의 자식 오브젝트 {childObject}(이)가 CropSign 컴포넌트를 갖지 않습니다.");
				continue;
			}

			_cropSigns.Add(cropSign);
		}
	}
}

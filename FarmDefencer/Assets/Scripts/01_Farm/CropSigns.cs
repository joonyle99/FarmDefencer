using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 자식으로 CropSign을 가진 오브젝트들을 둘 것.
/// </summary>
public class CropSigns : MonoBehaviour
{
	// 클릭 판정 월드 기준 크기
	private static readonly Vector2 SignClickSize = new Vector2 { x = 1.0f, y = 1.0f };
	private List<CropSign> _cropSigns;

	public bool TryGetClickedSign(Vector2 inputWorldPosition, out ProductEntry productEntry)
	{
		var cropSign = _cropSigns
			.Where(cropSign => (Mathf.Abs(cropSign.transform.position.x - inputWorldPosition.x) < SignClickSize.x) && (Mathf.Abs(cropSign.transform.position.y - inputWorldPosition.y) < SignClickSize.y))
			.FirstOrDefault();

		if (cropSign == null)
		{
			productEntry = null;
			return false;
		}
		else
		{
			productEntry = cropSign.ProductEntry;
			return true;
		}
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

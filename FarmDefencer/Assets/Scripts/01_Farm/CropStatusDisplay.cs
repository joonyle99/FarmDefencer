using UnityEngine;
using UnityEngine.UI;

public class CropStatusDisplay : MonoBehaviour
{
	private Crop _crop;
	private Text _waterStatus;
	private Text _ageStatus;
	private void Awake()
	{
		_crop = GetComponentInParent<Crop>();
		_waterStatus = transform.Find("Canvas/WaterStatus").GetComponent<Text>();
		_ageStatus = transform.Find("Canvas/AgeStatus").GetComponent<Text>();
	
		if (_crop == null)
		{
			throw new MissingComponentException("CropStatusDisplay는 Crop 컴포넌트를 갖는 부모 오브젝트를 필요로 합니다.");
		}
	}

	private void Update()
	{
		_waterStatus.text = $"{_crop.WaterStored:0.0}L/{_crop.WaterRequired:0.0}L";
		_ageStatus.text = $"{_crop.AgeSeconds:0.0}s/{_crop.GrowthSecondsRequired:0.0}";

		if (_crop.WaterStored == 0)
		{
			_waterStatus.color = Color.red;
		}
		else
		{
			_waterStatus.color = Color.cyan;
		}

		if (_crop.AgeSeconds > _crop.GrowthSecondsRequired)
		{
			_ageStatus.color = Color.green;
		}
		else
		{
			_ageStatus.color = Color.yellow;
		}
	}
}

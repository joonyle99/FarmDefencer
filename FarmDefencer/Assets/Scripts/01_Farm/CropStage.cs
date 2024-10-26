using UnityEngine;

[CreateAssetMenu(fileName = "CropStage", menuName = "Scriptable Objects/Farm/CropStage")]
public class CropStage : ScriptableObject
{
	[Header("�۹� ��������Ʈ")]
	public Sprite CropSprite;
	[Header("�䱸 ���� �ð�(��)")]
	public float GrowthSecondsRequired;
	[Header("�䱸 ���� ��(L)")]
	public float WaterRequired;

	public float WaterConsumptionPerSecond => WaterRequired / GrowthSecondsRequired;
}

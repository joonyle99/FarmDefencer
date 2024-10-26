using UnityEngine;

[CreateAssetMenu(fileName = "CropStage", menuName = "Scriptable Objects/Farm/CropStage")]
public class CropStage : ScriptableObject
{
	[Header("작물 스프라이트")]
	public Sprite CropSprite;
	[Header("요구 성장 시간(초)")]
	public float GrowthSecondsRequired;
	[Header("요구 물의 양(L)")]
	public float WaterRequired;

	public float WaterConsumptionPerSecond => WaterRequired / GrowthSecondsRequired;
}

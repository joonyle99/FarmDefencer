using FarmTest;
using UnityEngine;

public class FarmManager : MonoBehaviour
{
	public HarvestInventory HarvestInventory;
    public FarmClock FarmClock;
    public Farm Farm;
	public FarmTestPlayer FarmTestPlayer;

	private void Awake()
	{
		FarmClock.RegisterFarmUpdatableObject(Farm);
		Farm.OnHarvest.AddListener(
			(productEntry, cropWorldPosition) =>
			{
				var cropScreenPosition = FarmTestPlayer.Camera.WorldToScreenPoint(new Vector3(cropWorldPosition.x, cropWorldPosition.y, 0)); 
				HarvestInventory.PlayHarvestAnimation(productEntry, 1, cropScreenPosition, () => HarvestInventory.AddProduct(productEntry, 1));
			});
	}
}

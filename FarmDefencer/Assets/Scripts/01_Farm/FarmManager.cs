using UnityEngine;

public class FarmManager : MonoBehaviour
{
    public FarmClock FarmClock;
    public Farm Farm;

	private void Awake()
	{
		FarmClock.RegisterFarmUpdatableObject(Farm);
	}
}

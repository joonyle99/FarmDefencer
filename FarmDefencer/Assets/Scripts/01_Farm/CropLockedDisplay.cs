using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CropLockedDisplay : MonoBehaviour, IFarmUpdatable
{
	public Crop Crop;
	private Image _lockImage;
	private TMP_Text _remainingTimeText;

	public void OnFarmUpdate(float deltaTime)
	{
		if (Crop.State != Crop.CropState.Locked)
		{
			_lockImage.enabled = false;
			_remainingTimeText.enabled = false;
			return;
		}

		if (!_lockImage.enabled)
		{
			_lockImage.enabled = true;
		}
		TimeSpan timeSpan = TimeSpan.FromSeconds(Crop.LockRemainingSeconds);
		string formattedTime = $"{timeSpan.Minutes:00}:{timeSpan.Seconds:00}";
		if (!_remainingTimeText.enabled)
		{
			_remainingTimeText.enabled = true;
		}
		_remainingTimeText.text = formattedTime;
	}

	private void Awake()
	{
		_lockImage = GetComponentInChildren<Image>();
		_remainingTimeText = GetComponentInChildren<TMP_Text>();
		_lockImage.enabled = false;
		_remainingTimeText.enabled = false;
	}
}

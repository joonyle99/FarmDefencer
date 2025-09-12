using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System;

public sealed class FarmDebugUI : MonoBehaviour
{
	private TMP_Text _remainingDaytimeText;
	private TMP_Text _playPausedButtonText;
	private Button _earnGoldButton;
	private Button _setDaytimeButton;
	private Button _setDaytime5sButton;
	private Func<float> _getDaytime;
	
	public bool IsPaused { get; private set; }

	public void Init(Action<float> setDaytime, Func<float> getRemainingDaytime)
	{
		_getDaytime = getRemainingDaytime;
		
		_setDaytimeButton.onClick.AddListener(() => setDaytime(0.0f));
		_setDaytime5sButton.onClick.AddListener(() => setDaytime(145.0f));
	}
	
	private void Awake()
	{
		_remainingDaytimeText = transform.Find("Box/DebugTimer/RemainingDaytimeText")
			.GetComponent<TMP_Text>();
		_playPausedButtonText = transform.Find("Box/DebugTimer/PauseButton/Text")
			.GetComponent<TMP_Text>();
		_setDaytimeButton = transform.Find("Box/DebugTimer/SetDaytimeButton")
			.GetComponent<Button>();
		_setDaytime5sButton = transform.Find("Box/DebugTimer/SetDaytime5sButton").GetComponent<Button>()
			.GetComponent<Button>();
		transform.Find("Box/DebugTimer/PauseButton").GetComponent<Button>()
			.GetComponent<Button>()
			.onClick.AddListener(
			() =>
			{
				IsPaused = !IsPaused;
				_playPausedButtonText.text = IsPaused ? "Play" : "Pause";
			});

		_earnGoldButton = transform.Find("Box/EarnGoldButton").GetComponent<Button>();
		_earnGoldButton.onClick.AddListener(() => ResourceManager.Instance.EarnGold(1000));
	}

	private void Update()
	{
		_remainingDaytimeText.text = _getDaytime().ToString();
	}
}

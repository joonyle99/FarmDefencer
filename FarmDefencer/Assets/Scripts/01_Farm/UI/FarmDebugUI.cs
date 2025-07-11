using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System;

public sealed class FarmDebugUI : MonoBehaviour
{
	private TMP_Text _remainingDaytimeText;
	private TMP_Text _playPausedButtonText;
	private Button _goDefenceButton;
	private Button _setDaytimeButton;
	private Button _setDaytime5sButton;
	private Action<float> _setDaytime;
	private Func<float> _getDaytime;
	
	public bool IsPaused { get; private set; }

	public void Init(Action<float> setDaytime, Func<float> getRemainingDaytime, Action onGoDefenceButtonClickedHandler)
	{
		_setDaytime = setDaytime;
		_getDaytime = getRemainingDaytime;
		
		_setDaytimeButton.onClick.AddListener(() => setDaytime(0.0f));
		_setDaytime5sButton.onClick.AddListener(() => setDaytime(295.0f));
		
		_goDefenceButton.onClick.AddListener(() => onGoDefenceButtonClickedHandler());
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

		_goDefenceButton = transform.Find("Box/GoDefenceButton").GetComponent<Button>();
	}

	private void Update()
	{
		var deltaTime = Time.deltaTime;
		_remainingDaytimeText.text = _getDaytime().ToString();
	}
}

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public sealed class FarmDebugUI : MonoBehaviour
{
	private TMP_Text _remainingDaytimeText;
	private TMP_Text _playPausedButtonText;
	private FarmClock _farmClock;

	public void Init(FarmClock farmClock)
	{
		_farmClock = farmClock;
	}

	private void Awake()
	{
		_remainingDaytimeText = transform.Find("DebugTimer/RemainingDaytimeText")
			.GetComponent<TMP_Text>();
		_playPausedButtonText = transform.Find("DebugTimer/PauseButton/Text")
			.GetComponent<TMP_Text>();
		transform.Find("DebugTimer/DaytimeRandomSetButton")
			.GetComponent<Button>()
			.onClick.AddListener(() => _farmClock.SetRemainingDaytimeRandom(300.0f, 600.0f));
		transform.Find("DebugTimer/Daytime5sSetButton").GetComponent<Button>()
			.GetComponent<Button>()
			.onClick.AddListener(() => _farmClock.SetRemainingDaytimeBy(5.0f));
		transform.Find("DebugTimer/PauseButton").GetComponent<Button>()
			.GetComponent<Button>()
			.onClick.AddListener(
			() =>
			{
				if (_farmClock.IsManuallyPaused)
				{
					_farmClock.Resume();	
				}
				else
				{
					_farmClock.Pause();
				}
				_playPausedButtonText.text = _farmClock.IsManuallyPaused ? "Play" : "Pause";
			});
	}

	private void Update()
	{
		var deltaTime = Time.deltaTime;
		_remainingDaytimeText.text = _farmClock.RemainingDaytime.ToString();
	}
}

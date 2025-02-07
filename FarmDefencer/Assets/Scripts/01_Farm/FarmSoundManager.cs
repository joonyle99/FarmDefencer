using UnityEngine;
using System;

public class FarmSoundManager : MonoBehaviour
{
	[Serializable]
	public class SfxData
	{
		public string Name;
		public AudioClip[] AudioClips;
	}

	public SfxData[] SfxDatas;

	private AudioListener _audioListener;
	private AudioSource _sfxAudioSource;

	private static FarmSoundManager _singleton;

	public static void PlaySfx(string name)
	{
		int index = -1;
		for (int i = 0; i<_singleton.SfxDatas.Length; i++)
		{
			if (_singleton.SfxDatas[i].Name == name)
			{
				index = i;
				break;
			}
		}

		if (index == -1)
		{
			Debug.LogError($"SFX {name}(을)를 FarmSoundManager에서 찾을 수 없습니다.");
			return;
		}

		var sfxData = _singleton.SfxDatas[index];
		if (sfxData.AudioClips.Length == 0)
		{
			Debug.LogError($"FarmSoundManager.SfxDatas에 {name}에 대한 AudioClip이 존재하지 않습니다.");
			return;
		}

		var random = UnityEngine.Random.Range(0, sfxData.AudioClips.Length);
		var audioClip = sfxData.AudioClips[random];

		_singleton._sfxAudioSource.PlayOneShot(audioClip);
	}

	private void Awake()
	{
		_singleton = this;
		_audioListener = GetComponent<AudioListener>();
		_sfxAudioSource = GetComponent<AudioSource>();
	}
}

using UnityEngine;
using System.Collections.Generic;
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

	public void PlaySfx(string name)
	{
		int index = -1;
		for (int i = 0; i<SfxDatas.Length; i++)
		{
			if (SfxDatas[i].Name == name)
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

		var sfxData = SfxDatas[index];
		if (sfxData.AudioClips.Length == 0)
		{
			Debug.LogError($"FarmSoundManager.SfxDatas에 {name}에 대한 AudioClip이 존재하지 않습니다.");
			return;
		}

		var random = UnityEngine.Random.Range(0, sfxData.AudioClips.Length);
		var audioClip = sfxData.AudioClips[random];

		_sfxAudioSource.PlayOneShot(audioClip);
	}

	private void Awake()
	{
		_audioListener = GetComponent<AudioListener>();
		_sfxAudioSource = GetComponent<AudioSource>();
	}
}

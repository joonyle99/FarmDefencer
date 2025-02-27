using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 게임에 필요한 사운드를 관리하는 매니저입니다
/// </summary>
/// <remarks>
/// SFX파일은 Resources/Sfx에 둘 것
/// 하나의 이름으로 여러 SFX 버전이 존재하는 경우(ex. sfx_harvest_0, sfx_harvest_1)
/// 반드시 0번부터 연속적으로 이름_번호 규격을 맞출 것(ex. 이름: sfx_harvest, 번호: _0, _1, _2 ...)
/// </remarks>
public class SoundManager : JoonyleGameDevKit.Singleton<SoundManager>
{
	private Dictionary<string, List<AudioClip>> _sfxDictionary;

    private AudioListener _audioListener;
	private AudioSource _sfxAudioSource;

	/// <summary>
	/// 내부 캐시에서 SFX를 불러와 재생하는 메소드
	/// 캐시에 존재하지 않을 경우 Resources/Sfx에서 불러와 캐시에 넣고 재생함
	/// </summary>
	public void PlaySfx(string name)
	{
		if (_sfxDictionary.ContainsKey(name) == false)
		{
			var newSfxList = new List<AudioClip>();

			var singleSfx = Resources.Load<AudioClip>($"Sfx/{name}");
			if (singleSfx != null)
			{
				newSfxList.Add(singleSfx);
			}
			else
			{
				int number = 0;
				while (true)
				{
					var sfxWithNumber = Resources.Load<AudioClip>($"Sfx/{name}_{number}");
					if (sfxWithNumber == null)
					{
						break;
					}
					newSfxList.Add(sfxWithNumber);
					number += 1;
				}
			}

            _sfxDictionary.Add(name, newSfxList);
		}

		var sfxList = _sfxDictionary[name];

		if (sfxList.Count == 0)
		{
			Debug.LogError($"존재하지 않는 SFX: {name}");
			return;
		}

		if (sfxList.Count == 1)
        {
            _sfxAudioSource.PlayOneShot(sfxList[0]);
            return;
        }
		else
        {
            var index = Random.Range(0, sfxList.Count);
            var sfx = sfxList[index];
            _sfxAudioSource.PlayOneShot(sfx);
			return;
        }
	}

	protected override void Awake()
	{
		base.Awake();

		_audioListener = GetComponent<AudioListener>();
		_sfxAudioSource = GetComponent<AudioSource>();

		_sfxDictionary = new Dictionary<string, List<AudioClip>>();
	}
}

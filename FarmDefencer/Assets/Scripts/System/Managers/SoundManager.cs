using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

/// <summary>
/// 게임에 필요한 사운드를 관리하는 매니저입니다
/// </summary>
/// <remarks>
/// SFX파일은 Resources/Sfx에 둘 것
/// 하나의 이름으로 여러 SFX 버전이 존재하는 경우(ex. sfx_T_harvest_0, sfx_T_harvest_1)
/// 반드시 0번부터 연속적으로 이름_번호 규격을 맞출 것(ex. 이름: sfx_T_harvest, 번호: _0, _1, _2 ...)
/// </remarks>
public class SoundManager : JoonyleGameDevKit.Singleton<SoundManager>
{
    private Dictionary<string, AudioClip> _bgmDictionary;
    private Dictionary<string, List<AudioClip>> _sfxDictionary;

	[SerializeField] private AudioSource _bgmAudioSource1;
    [SerializeField] private AudioSource _bgmAudioSource2;
    [SerializeField] private AudioSource _sfxAudioSource;

    [SerializeField][BoxGroup("볼륨 조절")][Range(0f, 1f)] private float _ambVolume = 0.5f;
    [SerializeField][BoxGroup("볼륨 조절")][Range(0f, 1f)] private float _songVolume = 0.5f;

    /// <summary>
    /// 내부 캐시에서 Bgm을 불러와 재생하는 메소드.
    /// 캐시에 존재하지 않을 경우 Resources/_Bgm에서 불러와 캐시에 넣고 재생함.
    /// </summary>
    public void PlayBgm(string name, float volume = 0.5f)
    {
        if (_bgmDictionary.ContainsKey(name) == false)
        {
            var newBgm = Resources.Load<AudioClip>($"_Bgm/{name}");

            if (newBgm == null)
            {
                Debug.LogError($"존재하지 않는 BGM: {name}");
                return;
            }
            _bgmDictionary.Add(name, newBgm);
        }

        var bgm = _bgmDictionary[name];

        _bgmAudioSource1.Stop();
        _bgmAudioSource1.clip = bgm;
        _bgmAudioSource1.volume = volume;
        _bgmAudioSource1.Play();
    }
    public void StopBgm()
    {
        if (_bgmAudioSource1.isPlaying)
        {
            _bgmAudioSource1.Stop();
        }
    }
    public void PlayMapAmb()
    {
        var currentMap = MapManager.Instance.CurrentMap;
        var mapAmbName = $"BGM_D_{currentMap.MapCode}_amb";
        PlayBgm(mapAmbName, _ambVolume);
    }
    public void PlayMapSong()
    {
        var currentMap = MapManager.Instance.CurrentMap;
        var mapSongName = $"BGM_D_{currentMap.MapCode}_song";
        PlayBgm(mapSongName, _songVolume);
    }

    /// <summary>
    /// 내부 캐시에서 SFX를 불러와 재생하는 메소드.
    /// 캐시에 존재하지 않을 경우 Resources/_Sfx에서 불러와 캐시에 넣고 재생함.
    ///
    /// PlayOneShot()에만 의존하는 함수,, AudioSource.volume × volumeScale로 오디오 볼륨을 조절
    /// AudioSource.volume에 영향을 받는다는 치명적인 단점이 존재
    ///
    /// 1. AudioSource.volume: 0.2f volumeScale: 0.5f = 0.1f
    /// 2. PlayOneShot()
    /// 3. AudioSource.volume: 0.8f volumeScale: 0.5f = 0.4f
    /// </summary>
    public void PlaySfx(AudioClip audioClip, float volume = 0.5f, float pitch = 1.0f)
    {
        _sfxAudioSource.PlayOneShot(audioClip, volume);
    }
    public void PlaySfx(SoundData soundData, float volume = 0.5f, float pitch = 1.0f)
    {
        if (soundData.audioClip != null)
        {
            _sfxAudioSource.PlayOneShot(soundData.audioClip, volume);
        }
        else
        {
            Debug.LogError($"사운드 데이터에 오디오 클립이 비어있습니다.: {soundData.audioName}");
        }
    }
    public void PlaySfx(string name, float volume = 0.5f, float pitch = 1.0f)
	{
		if (_sfxDictionary.ContainsKey(name) == false)
		{
			var newSfxList = new List<AudioClip>();

			var singleSfx = Resources.Load<AudioClip>($"_Sfx/{name}");
			if (singleSfx != null) // 1. 하나의 SFX가 단일 버전으로 존재하는 경우
            {
                newSfxList.Add(singleSfx);
			}
			else // 2. 하나의 SFX가 여러 버전으로 존재하는 경우 (TODO: Pitch를 조절하는 방법으로 해결 가능하지 않을까..?)
			{
				int number = 0;
				while (true)
				{
					var multipleSfx = Resources.Load<AudioClip>($"_Sfx/{name}_{number}");
					if (multipleSfx == null)
					{
						break;
					}
					newSfxList.Add(multipleSfx);
					number += 1;
				}
			}

            _sfxDictionary.Add(name, newSfxList);
		}

		var sfxList = _sfxDictionary[name];

		// 유효하지 않은 경우
		if (sfxList.Count == 0)
		{
			Debug.LogError($"존재하지 않는 SFX: {name}");
			return;
		}

		// Single SFX
		if (sfxList.Count == 1)
        {
            var sfx = sfxList[0];
            _sfxAudioSource.PlayOneShot(sfx, volume);
            return;
        }
		// Multiple SFX
		else if (sfxList.Count > 1)
        {
            var randomoIndex = Random.Range(0, sfxList.Count);
            var sfx = sfxList[randomoIndex];
            _sfxAudioSource.PlayOneShot(sfx, volume);
			return;
        }
	}
    public void StopSfx()
    {
        if (_sfxAudioSource.isPlaying)
        {
            _sfxAudioSource.Stop();
        }
    }
    public void PrintSfxDictionary()
    {
        string log = "";
        foreach (var sfx in _sfxDictionary)
        {
            log += "=====================\n";
            log += $"SFX: {sfx.Key}\n";
            foreach (var clip in sfx.Value)
			{
                log += $"===> Clip: {clip.name}\n";
			}
            log += "=====================\n";
        }
        Debug.Log(log);
    }

	protected override void Awake()
	{
		base.Awake();

		_sfxDictionary = new Dictionary<string, List<AudioClip>>();
        _bgmDictionary = new Dictionary<string, AudioClip>();
    }
}

using System;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// 게임에 필요한 사운드를 관리하는 매니저입니다
/// </summary>
/// <remarks>
/// SFX파일은 Resources/Sfx에 둘 것
/// 하나의 이름으로 여러 SFX 버전이 존재하는 경우(ex. sfx_T_harvest_0, sfx_T_harvest_1)
/// 반드시 0번부터 연속적으로 이름_번호 규격을 맞출 것(ex. 이름: sfx_T_harvest, 번호: _0, _1, _2 ...)
/// </remarks>
public class SoundManager : JoonyleGameDevKit.Singleton<SoundManager>, IVolumeControl
{
    private Dictionary<string, AudioClip> _bgmDictionary;
    private Dictionary<string, List<AudioClip>> _sfxDictionary;
    private Dictionary<string, AudioClip> _ambDictionary;

    private List<(AudioClip, float)> _sfxPlayQueue;

	[SerializeField] private AudioSource _bgmAudioSource1;
    //[SerializeField] private AudioSource _bgmAudioSource2;
    [SerializeField] private AudioSource _ambAudioSource;
    [SerializeField] private AudioSource _sfxAudioSource;

    [Space]

    [VolumeControl][BoxGroup("볼륨 조절")][Range(0f, 1f)] public float ambVolume = 0.5f;
    [VolumeControl][BoxGroup("볼륨 조절")][Range(0f, 1f)] public float songVolume = 0.5f;

    [Space]

    [VolumeControl("Tycoon")][BoxGroup("볼륨 조절")][Range(0f, 1f)] public float coinVolume = 0.5f;
    [VolumeControl("Tycoon")][BoxGroup("볼륨 조절")][Range(0f, 1f)] public float waterVolume = 0.5f;
    [VolumeControl("Tycoon")][BoxGroup("볼륨 조절")][Range(0f, 1f)] public float plantVolume = 0.5f;
    [VolumeControl("Tycoon")][BoxGroup("볼륨 조절")][Range(0f, 1f)] public float harvestVolume = 0.5f;
    [VolumeControl("Tycoon")][BoxGroup("볼륨 조절")][Range(0f, 1f)] public float cabbageShakeVolume = 0.5f;
    [VolumeControl("Tycoon")][BoxGroup("볼륨 조절")][Range(0f, 1f)] public float eggPlantLeafDropVolume = 0.5f;
    [VolumeControl("Tycoon")][BoxGroup("볼륨 조절")][Range(0f, 1f)] public float mushroomShotVolume = 0.5f;
    [VolumeControl("Tycoon")][BoxGroup("볼륨 조절")][Range(0f, 1f)] public float potatoDustVolume = 0.5f;
    [VolumeControl("Tycoon")][BoxGroup("볼륨 조절")][Range(0f, 1f)] public float SweetPotatoVinylVolume = 0.5f;
    [VolumeControl("Tycoon")][BoxGroup("볼륨 조절")][Range(0f, 1f)] public float pestSirenVolume = 0.5f;
    [VolumeControl("Tycoon")][BoxGroup("볼륨 조절")][Range(0f, 1f)] public float pestCatchVolume = 0.5f;
    [VolumeControl("Tycoon")][BoxGroup("볼륨 조절")][Range(0f, 1f)] public float orderResetVolume = 0.5f;
    [VolumeControl("Tycoon")][BoxGroup("볼륨 조절")][Range(0f, 1f)] public float eatCropsVolume = 0.5f;

    [CanBeNull] public string CurrentBgmName { get; private set; }
    public float CurrentBgmTime => _bgmAudioSource1.time;
    [CanBeNull] public string CurrentAmbName { get; private set; }
    
    /// <summary>
    /// 내부 캐시에서 Bgm을 불러와 재생하는 메소드.
    /// 캐시에 존재하지 않을 경우 Resources/_Bgm에서 불러와 캐시에 넣고 재생함.
    /// </summary>
    public void PlayBgm(string name, float volume = 0.5f, float playbackTime = 0.0f)
    {
	    if (!name.Equals(CurrentBgmName))
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
			_bgmAudioSource1.Play();
			_bgmAudioSource1.time = playbackTime;
	    }
	    
        _bgmAudioSource1.volume = volume;
        CurrentBgmName = name;
    }
    public void StopBgm()
    {
        if (_bgmAudioSource1.isPlaying)
        {
            _bgmAudioSource1.Stop();
            CurrentBgmName = null;
        }
    }
    public void StopBgmIf(string bgmName)
    {
	    if (_bgmAudioSource1.isPlaying && CurrentBgmName is not null && CurrentBgmName.Equals(bgmName))
	    {
		    _bgmAudioSource1.Stop();
		    CurrentBgmName = null;
	    }
    }

    public void PlayAmb(string ambName, float volume = 0.5f)
    {
	    if (!ambName.Equals(CurrentAmbName))
	    {
		    if (_ambDictionary.ContainsKey(ambName) == false)
		    {
			    var newAmb = Resources.Load<AudioClip>($"_Amb/{ambName}");

			    if (newAmb is null)
			    {
				    Debug.LogError($"존재하지 않는 AMB: {ambName}");
				    return;
			    }
			    _ambDictionary.Add(ambName, newAmb);
		    }

		    var amb = _ambDictionary[ambName];

		    _ambAudioSource.Stop();
		    _ambAudioSource.clip = amb;
		    _ambAudioSource.Play();
	    }
	    
	    _ambAudioSource.volume = volume;
	    CurrentAmbName = ambName;
    }
    public void StopAmbIf(string ambName)
    {
	    if (_ambAudioSource.isPlaying && CurrentAmbName is not null && CurrentAmbName.Equals(ambName))
	    {
		    _ambAudioSource.Stop();
		    CurrentAmbName = null;
	    }
    }
    
    // TODO: 수정 예정
    public void PlayDefenceMapAmb()
    {
        var currentMap = MapManager.Instance.CurrentMap;
        var mapAmbName = $"BGM_D_{currentMap.MapCode}_amb";
        PlayBgm(mapAmbName, ambVolume);
    }
    public void PlayDefenceMapSong()
    {
        var currentMap = MapManager.Instance.CurrentMap;
        var mapSongName = $"BGM_D_{currentMap.MapCode}_song";
        PlayBgm(mapSongName, songVolume);
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
    public void PlaySfx(AudioClip audioClip, float volume = 0.5f)
    {
	    if (_sfxPlayQueue.Any(pair => pair.Item1 == audioClip))
	    {
		    return;
	    }
	    _sfxPlayQueue.Add((audioClip, volume));
    }
    public void PlaySfx(SoundData soundData, float volume = 0.5f)
    {
        if (soundData.audioClip != null)
        {
	        PlaySfx(soundData.audioClip, volume);
        }
        else
        {
            Debug.LogError($"사운드 데이터에 오디오 클립이 비어있습니다.: {soundData.audioName}");
        }
    }
    public void PlaySfx(string name, float volume = 0.5f, Action onComplete = null)
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
        AudioClip sfx = null;
		if (sfxList.Count == 1)
        {
            sfx = sfxList[0];
            PlaySfx(sfx, volume);
        }
		// Multiple SFX
		else if (sfxList.Count > 1)
        {
            var randomoIndex = Random.Range(0, sfxList.Count);
            sfx = sfxList[randomoIndex];
            PlaySfx(sfx, volume);
        }

        if (onComplete != null)
        {
            StartCoroutine(InvokeAfterSeconds(sfx.length, onComplete));
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

    public void PauseAll()
    {
        _bgmAudioSource1.Pause();
        _sfxAudioSource.Pause();
        _ambAudioSource.Pause();
    }
    public void ResumeAll()
    {
        _bgmAudioSource1.UnPause();
        _sfxAudioSource.UnPause();
        _ambAudioSource.UnPause();
    }

    private IEnumerator InvokeAfterSeconds(float seconds, System.Action callback)
    {
        yield return new WaitForSeconds(seconds);
        callback?.Invoke();
    }
    
    protected override void Awake()
	{
		base.Awake();
		
		_sfxDictionary = new Dictionary<string, List<AudioClip>>();
        _bgmDictionary = new Dictionary<string, AudioClip>();
        _ambDictionary = new Dictionary<string, AudioClip>();
        _sfxPlayQueue = new List<(AudioClip, float)>();
    }

    private void Update()
    {
	    foreach (var (audioClip, volume) in _sfxPlayQueue)
	    {
		    _sfxAudioSource.PlayOneShot(audioClip, volume);
	    }
	    _sfxPlayQueue.Clear();
    }
}

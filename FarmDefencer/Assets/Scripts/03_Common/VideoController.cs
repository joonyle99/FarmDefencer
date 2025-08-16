using UnityEngine;
using UnityEngine.Video;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Collections.Generic;

public class VideoController : MonoBehaviour
{
    //[Header("──────── VideoController ────────")]
    //[Space]

    private Dictionary<string, VideoClip> _cachedVideoClips = new();
    private VideoPlayer _videoPlayer;

    private void Awake()
    {
        _videoPlayer = GetComponent<VideoPlayer>();
    }
    private void Start()
    {
        
    }
    private void OnDestroy()
    {
        foreach (var videoClip in _cachedVideoClips.Values)
        {
            Addressables.Release(videoClip);
        }

        _cachedVideoClips.Clear();
        _cachedVideoClips = null;
    }

    public async void PlayVideo(string mapCode)
    {
        if (_cachedVideoClips.TryGetValue(mapCode, out VideoClip cachedClip) == false)
        {
            var handle = Addressables.LoadAssetAsync<VideoClip>($"video_{mapCode}");
            await handle.Task;

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                cachedClip = handle.Result;
                _cachedVideoClips[mapCode] = cachedClip;
            }
            else
            {
                Debug.LogError($"Video load failed: {mapCode}");
                return;
            }
        }

        _videoPlayer.Stop();
        _videoPlayer.clip = cachedClip;

        _videoPlayer.Prepare();
        _videoPlayer.prepareCompleted += OnVideoPrepared;
    }

    private void OnVideoPrepared(VideoPlayer videoPlayer)
    {
        videoPlayer.prepareCompleted -= OnVideoPrepared;
        videoPlayer.Play();

        DefenceContext.Current.DefenceUIController.LoadingUI.gameObject.SetActive(false);
    }
}

using UnityEngine;
using UnityEngine.Video;

public class VideoController : MonoBehaviour
{
    [Header("──────── VideoController ────────")]
    [Space]

    [SerializeField] private VideoClip[] _videoClips;

    private VideoPlayer _videoPlayer;

    private void Awake()
    {
        _videoPlayer = GetComponent<VideoPlayer>();
    }

    public void PlayVideo(int idx)
    {
        _videoPlayer.clip = _videoClips[idx];
        _videoPlayer.Play();
    }
    public void StopVideo()
    {
        _videoPlayer.Stop();
    }
}

using UnityEngine;
using UnityEngine.Video;

public class VideoController : MonoBehaviour
{
    //[Header("──────── VideoController ────────")]
    //[Space]

    private VideoPlayer _videoPlayer;

    private void Awake()
    {
        _videoPlayer = GetComponent<VideoPlayer>();
    }
    private void Start()
    {
        DefenceContext.Current.DefenceUIController.LoadingUI.gameObject.SetActive(true);
    }

    public void PlayVideo(string mapCode)
    {
        var videoClip = ResourceCache.Get<VideoClip>($"{mapCode}_video");

        _videoPlayer.Stop();
        _videoPlayer.clip = videoClip;

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

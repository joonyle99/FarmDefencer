using UnityEngine;
using UnityEngine.Video;

public class VideoController : MonoBehaviour
{
    [SerializeField] private Transform _videoPaper;
    private VideoPlayer _videoPlayer;

    private void Awake()
    {
        _videoPlayer = GetComponent<VideoPlayer>();
    }
    private void Start()
    {
        DefenceContext.Current?.DefenceUIController?.LoadingUI.gameObject.SetActive(true);

        var gridMap = DefenceContext.Current.GridMap;

        if (gridMap == null)
        {
            return;
        }

        _videoPaper.gameObject.SetActive(true);
        _videoPaper.transform.position = gridMap.CenterWorldPos;
    }
    private void OnEnable()
    {
        if (MapManager.Instance is not null)
        {
            MapManager.Instance.OnMapChanged += HandleBackgroundVideo;
        }
    }
    private void OnDisable()
    {
        if (MapManager.Instance is not null)
        {
            MapManager.Instance.OnMapChanged -= HandleBackgroundVideo;
        }
    }

    private void HandleBackgroundVideo(MapEntry map)
    {
        PlayVideo(map.MapCode);
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

        DefenceContext.Current?.DefenceUIController?.LoadingUI.gameObject.SetActive(false);
    }
}

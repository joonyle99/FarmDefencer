using System.Collections;
using UnityEngine;

public class AudioFadeTest : MonoBehaviour
{
    public AudioSource introSource;
    public AudioSource loopSource;

    public float fadeDuration;

    private void Start()
    {
        if (!introSource.isPlaying && !loopSource.isPlaying)
        {
            introSource.Play();
        }
        else if (introSource.isPlaying && loopSource.isPlaying)
        {
            loopSource.Stop();
        }
    }

    private void Update()
    {
        // if (Input.GetKeyDown(KeyCode.Space))
        // {
        //    var playingSource = introSource.isPlaying ? introSource : loopSource;
        //    var otherSource = playingSource == introSource ? loopSource : introSource;
        //    StartCoroutine(FadeCo(playingSource, otherSource, fadeDuration));
        // }
    }

    private IEnumerator FadeCo(AudioSource from, AudioSource to, float duration)
    {
        to.volume = 0f;
        to.Play();

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float ratio = t / duration;
            float easedRatio = 0.5f - 0.5f * Mathf.Cos(ratio * Mathf.PI); // 감정적, 감싸듯 자연스러움

            // float easedRatio = Mathf.SmoothStep(0f, 1f, ratio); // 부드러움, 대부분의 게임에 무난
            // float easedRatio = ratio < 0.5f ? 2f * ratio * ratio : -1f + (4f - 2f * ratio) * ratio; // 드라마틱, 약간 더 동적

            from.volume = Mathf.Lerp(1f, 0f, easedRatio);
            to.volume = Mathf.Lerp(0f, 1f, easedRatio);

            yield return null;
        }

        from.Stop();
    }
}

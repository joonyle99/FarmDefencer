using UnityEngine;

public class AudioLoopTest : MonoBehaviour
{
    public AudioSource audioSource1;
    public AudioSource audioSource2;

    //private float _forest01 = 87.000f;
    //private float _forest02 = 106.666f;

    private float _beach01 = 76.000f;
    private float _beach02 = 96.000f;

    private bool dirty = false;

    private void Start()
    {
        audioSource1.Play();
        audioSource1.time = _beach01;
    }
    private void Update()
    {
        if (dirty)
        {
            return;
        }

        if (audioSource1.time >= _beach02)
        {
            Debug.Log("지금 새로운 오디오 재생");

            dirty = true;
            audioSource2.Play();
        }
    }
}

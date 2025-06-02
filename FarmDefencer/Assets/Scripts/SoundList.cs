using UnityEngine;

[System.Serializable]
public class SoundData
{
    public string audioName; // Name of the audio clip
    public AudioClip audioClip; // The actual audio clip
    public SoundData(string name, AudioClip clip)
    {
        audioName = name;
        audioClip = clip;
    }
}

public class SoundList : MonoBehaviour
{
    // TODO: 사운드 실행 시, 리스트를 순회해야 하므로, Dictionary로 변경하는 것이 좋음
    [SerializeField] private SoundData[] _soundData;
    public SoundData[] SoundDataArray => _soundData;

    public void PlaySound(string soundName)
    {
        foreach (var data in _soundData)
        {
            if (data.audioName == soundName)
            {
                if (data.audioClip != null)
                {
                    SoundManager.Instance.PlaySfx(data);
                }
                else
                {
                    Debug.LogWarning($"{soundName} has not audio clip");
                }
            }
        }
    }
}

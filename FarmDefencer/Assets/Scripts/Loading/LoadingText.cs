using TMPro;
using UnityEngine;
using DG.Tweening;

public class LoadingText : MonoBehaviour
{
    private TextMeshProUGUI _text;

    private void Awake()
    {
        _text = GetComponent<TextMeshProUGUI>();
    }
    private void Start()
    {
        DOTween.Sequence()
            .AppendCallback(() => _text.text = "LOADING.")
            .AppendInterval(0.3f)
            .AppendCallback(() => _text.text = "LOADING..")
            .AppendInterval(0.3f)
            .AppendCallback(() => _text.text = "LOADING...")
            .AppendInterval(0.3f)
            .SetLoops(-1, LoopType.Restart); // -1: 무한 반복
    }
}

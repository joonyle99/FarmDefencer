using UnityEngine;
using UnityEngine.UI;
using System;

public sealed class GoDefenceUI : MonoBehaviour, IFarmInputLayer
{
    private Button _goDefenceButton;

    public void Init(Action onGoDefenceButtonClicked) => _goDefenceButton.onClick.AddListener(() => onGoDefenceButtonClicked());
    
    private void Awake()
    {
        _goDefenceButton = GetComponentInChildren<Button>();
    }

    public int InputPriority => 1000;
    
    public bool OnSingleTap(Vector2 worldPosition)
    {
        return gameObject.activeSelf;
    }

    public bool OnSingleHolding(Vector2 initialWorldPosition, Vector2 deltaWorldPosition, bool isEnd, float deltaHoldTime)
    {
        return gameObject.activeSelf;
    }
}

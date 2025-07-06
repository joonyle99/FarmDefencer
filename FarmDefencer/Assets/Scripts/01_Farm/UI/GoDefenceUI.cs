using UnityEngine;
using UnityEngine.UI;
using System;
using Spine.Unity;

public sealed class GoDefenceUI : MonoBehaviour, IFarmInputLayer
{
    private Button _goDefenceButton;
    private SkeletonGraphic _skeletonGraphic;
    
    public void Init(Action onGoDefenceButtonClicked) => _goDefenceButton.onClick.AddListener(() => onGoDefenceButtonClicked());
    
    private void Awake()
    {
        _goDefenceButton = GetComponentInChildren<Button>();
        _skeletonGraphic = GetComponentInChildren<SkeletonGraphic>();
        ApplyStageIndex();
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

    private void OnEnable() => ApplyStageIndex();

    private void ApplyStageIndex()
    {
        var currentStageIndex = MapManager.Instance.CurrentStageIndex;
        _skeletonGraphic.AnimationState.SetAnimation(0, $"Alarm_{currentStageIndex}", true);
    }
}

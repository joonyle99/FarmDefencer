using Spine.Unity;
using System;
using UnityEngine;
using UnityEngine.UI;

public sealed class GoDefenceUI : MonoBehaviour, IFarmInputLayer
{
    private Button _goDefenceButton;
    private SkeletonGraphic _skeletonGraphic;

    public void Init(Action onGoDefenceButtonClicked) => _goDefenceButton.onClick.AddListener(() => onGoDefenceButtonClicked());
    
    private void Awake()
    {
		gameObject.SetActive(false);
        _goDefenceButton = GetComponentInChildren<Button>();
        _skeletonGraphic = GetComponentInChildren<SkeletonGraphic>();
        ApplyStageIndex();
    }

    public int InputPriority => 1000;
    
    public bool OnTap(Vector2 worldPosition)
    {
        return gameObject.activeSelf;
    }

    public bool OnHold(Vector2 initialWorldPosition, Vector2 deltaWorldPosition, bool isEnd, float deltaHoldTime)
    {
        return gameObject.activeSelf;
    }

    private void OnEnable()
    {
        SoundManager.Instance.PlaySfx("SFX_C_alarm", SoundManager.Instance.alarmVolume);
        ApplyStageIndex();
    }

    private void ApplyStageIndex()
    {
        var currentStageIndex = MapManager.Instance.CurrentStageIndex;
        _skeletonGraphic.AnimationState.SetAnimation(0, $"Alarm_{currentStageIndex}", true);
    }
}

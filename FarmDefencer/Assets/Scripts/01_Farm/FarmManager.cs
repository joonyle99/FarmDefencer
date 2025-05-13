using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// 타이쿤의 최초 로드 동작을 정의하고, 타이쿤을 구성하는 여러 시스템들간의 중재자 역할을 하는 컴포넌트.
/// </summary>
[Serializable]
public sealed class FarmManager : MonoBehaviour
{
    [SerializeField] private FarmUI farmUI;
    [SerializeField] private FarmClock farmClock;
    [SerializeField] private Farm farm;
    [SerializeField] private FarmInput farmInput;
    [SerializeField] private ProductDatabase productDatabase;
    [SerializeField] private PenaltyGiver penaltyGiver;
    [SerializeField] private HarvestTutorialGiver harvestTutorialGiver;
    [SerializeField] private QuotaContext quotaContext;

    private void Start()
    {
        InitializeDependencies();
        DeserializeFromSaveFile();
    }

    private void Update()
    {
        var ratio = farmClock.LengthOfDaytime == 0.0f ? 0.0f : farmClock.RemainingDaytime / farmClock.LengthOfDaytime;
        farmUI.SetTimerClockHand(ratio);

        if (harvestTutorialGiver.IsPlayingTutorial)
        {
            harvestTutorialGiver.gameObject.SetActive(!penaltyGiver.IsAnimationPlaying);
            farmInput.InputPriorityCut = harvestTutorialGiver.InputPriority;
        }
        else
        {
            farmInput.InputPriorityCut = 0;
        }

        farmUI.WateringCanAvailable = !harvestTutorialGiver.gameObject.activeSelf;
    }

    private void QuotaContextChangedHandler()
    {
        farmUI.UpdateHarvestInventory(quotaContext);
        farm.UpdateAvailability(productEntry => quotaContext.IsProductAvailable(productEntry));

        if (quotaContext.IsAllQuotaFilled)
        {
            quotaContext.AssignQuotas(MapManager.Instance.CurrentMap.MapId);
        }
    }

    private void InitializeDependencies()
    {
        farmInput.RegisterInputLayer(farm);
        farmInput.RegisterInputLayer(harvestTutorialGiver);

        farmClock.RegisterFarmUpdatableObject(farm);
        farmClock.RegisterFarmUpdatableObject(penaltyGiver);
        farmClock.AddPauseCondition(() => penaltyGiver.IsAnimationPlaying);
        farmClock.AddPauseCondition(() => harvestTutorialGiver.IsPlayingTutorial);
        farmClock.AddPauseCondition(() => farmUI.IsPaused);
        
        quotaContext.Init(productName => productDatabase.Products.FirstOrDefault(p => p.ProductName == productName));

        farm.Init(
            entry => quotaContext.TryGetQuota(entry.ProductName, out var quota) ? quota : 0,
            (entry, cropWorldPosition, quota) =>
            {
                quotaContext.FillQuota(entry.ProductName, quota);
                farmUI.PlayProductFillAnimation(entry, cropWorldPosition, quota, quotaContext);
                SoundManager.Instance.PlaySfx("SFX_T_coin");
                ResourceManager.Instance.Gold += entry.Price * quota;
            },
            farmUI.ToggleCropGuide);

        farmUI.Init(farmInput,
            productDatabase,
            farm.WateringAction,
            farmClock.SetRemainingDaytimeBy,
            () => farmClock.RemainingDaytime,
            () => farmClock.Stopped);
        
        quotaContext.QuotaContextUpdated += QuotaContextChangedHandler;
    }
    
    private void DeserializeFromSaveFile()
    {
        var saveJson = FarmSerializer.ReadSave();
        quotaContext.Deserialize(saveJson["QuotaContext"] as JObject ?? new JObject());
        
        quotaContext.AssignQuotas(MapManager.Instance.CurrentMap.MapId);
        
        penaltyGiver.Init(farm);

        var a = new List<string> { "Rabbit" };
        penaltyGiver.SpawnMonsters(0, a);

        if (GameStateManager.Instance.IsTycoonInitialLoad)
        {
            MapManager.Instance.Deserialize(saveJson["MapManager"] as JObject ?? new JObject());
            ResourceManager.Instance.Deserialize(saveJson["ResourceManager"] as JObject ?? new JObject());
        }
        GameStateManager.Instance.IsTycoonInitialLoad = false;
    }
    
    private void SerializeToSaveFile()
    {
        var saveJson = new JObject();
        
        saveJson.Add("QuotaContext", quotaContext.Serialize());
        saveJson.Add("MapManager", MapManager.Instance.Serialize());
        saveJson.Add("ResourceManager", ResourceManager.Instance.Serialize());
        
        FarmSerializer.WriteSave(saveJson);
    }

    private void OnDestroy() => SerializeToSaveFile();

    private void OnApplicationQuit() => SerializeToSaveFile();
}
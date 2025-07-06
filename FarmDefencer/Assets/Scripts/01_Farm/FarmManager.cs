using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 타이쿤의 최초 로드 동작을 정의하고, 타이쿤을 구성하는 여러 시스템들간의 중재자 역할을 하는 컴포넌트.
/// </summary>
public sealed class FarmManager : MonoBehaviour
{
    [Header("디버그용 세이브 무시")] [SerializeField]
    private bool ignoreSaveFile;

    [Header("저장 대상 오브젝트들")] [SerializeField]
    private Farm farm;

    [SerializeField] private PenaltyGiver penaltyGiver;

    [SerializeField] private QuotaContext quotaContext;
    // ResourceManager
    // MapManager

    [Header("저장되지 않는 오브젝트들")] [SerializeField]
    private FarmUI farmUI;

    [SerializeField] private FarmClock farmClock;
    [SerializeField] private FarmInput farmInput;
    [SerializeField] private ProductDatabase productDatabase;
    [SerializeField] private HarvestTutorialGiver harvestTutorialGiver;
    [SerializeField] private WeatherShopUI weatherShopUI;
    [SerializeField] private WeatherGiver weatherGiver;

    private void Start()
    {
        InitializeDependencies();
        if (ignoreSaveFile)
        {
            // 디버그용 할당 코드 등 여기에...
            quotaContext.AssignQuotas(MapManager.Instance.CurrentMap.MapId);
            var monsters = new List<string>();
            harvestTutorialGiver.AddTutorial("product_carrot");
            penaltyGiver.SpawnMonsters(MapManager.Instance.CurrentMap.MapId, monsters);
        }
        else
        {
            DeserializeFromSaveFile();
        }

        penaltyGiver.SpawnMonsters(MapManager.Instance.CurrentMap.MapId, ResourceManager.Instance.SurvivedMonsters);
        ResourceManager.Instance.SurvivedMonsters.Clear();
        farmUI.PlayQuotaAssignAnimation(productEntry => quotaContext.IsProductAvailable(productEntry),
            productEntry => quotaContext.TryGetQuota(productEntry.ProductName, out var quota) ? quota : 0);
    }

    private void Update()
    {
        var ratio = farmClock.LengthOfDaytime == 0.0f ? 0.0f : farmClock.RemainingDaytime / farmClock.LengthOfDaytime;

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

        farmUI.GoDefenceUIEnabled = ratio == 0.0f;
    }

    private void QuotaContextChangedHandler()
    {
        UpdateHarvestInventory();
        farm.UpdateAvailability(productEntry => quotaContext.IsProductAvailable(productEntry));

        if (quotaContext.IsAllQuotaFilled)
        {
            quotaContext.AssignQuotas(MapManager.Instance.CurrentMap.MapId);
            farmUI.PlayQuotaAssignAnimation(productEntry => quotaContext.IsProductAvailable(productEntry),
                productEntry => quotaContext.TryGetQuota(productEntry.ProductName, out var quota) ? quota : 0);
        }
    }

    private void InitializeDependencies()
    {
        farmInput.RegisterInputLayer(farm);
        farmInput.RegisterInputLayer(harvestTutorialGiver);
        farmInput.AddCanInputCondition(() => farmClock.RemainingDaytime > 0.0f);

        farmClock.RegisterFarmUpdatableObject(farm);
        farmClock.RegisterFarmUpdatableObject(penaltyGiver);
        farmClock.AddPauseCondition(() => penaltyGiver.IsAnimationPlaying);
        farmClock.AddPauseCondition(() => harvestTutorialGiver.IsPlayingTutorial);
        farmClock.AddPauseCondition(() => farmUI.IsPaused);
        farmClock.AddPauseCondition(() => weatherGiver.IsWeatherOnGoing);

        quotaContext.Init(productName => productDatabase.Products.FirstOrDefault(p => p.ProductName == productName));

        farm.Init(
            entry => quotaContext.TryGetQuota(entry.ProductName, out var quota) ? quota : 0,
            OnFarmQuotaFilledHandler,
            farmUI.ToggleCropGuide);

        farmUI.Init(farmInput,
            productDatabase,
            MapManager.Instance.CurrentMap.MapId,
            MapManager.Instance.CurrentStageIndex,
            farm.WateringAction,
            farmClock.SetRemainingDaytimeBy,
            OpenDefenceScene,
            () => farmClock.RemainingDaytime,
            () => farmClock.Stopped,
            () => farmClock.LengthOfDaytime == 0.0f ? 0.0f : farmClock.RemainingDaytime / farmClock.LengthOfDaytime);

        penaltyGiver.Init(farm);

        quotaContext.QuotaContextUpdated += QuotaContextChangedHandler;
        
        weatherShopUI.Init(OnWeatherShopItemBought);
        weatherShopUI.AddItem(new SunItem(20 + MapManager.Instance.CurrentMap.MapId*10));
        weatherShopUI.AddItem(new RainItem(10, 10));
        weatherShopUI.AddItem(new RainItem(30, 30));
        weatherShopUI.AddItem(new RainItem(50, 50));
        
        weatherGiver.Init(farm.ApplyCropCommand);
    }

    private void DeserializeFromSaveFile()
    {
        var saveJson = FarmSerializer.ReadSave();

        quotaContext.Deserialize(saveJson["QuotaContext"] as JObject ?? new JObject());
        penaltyGiver.Deserialize(saveJson["PenaltyGiver"] as JObject ?? new JObject());
        farm.Deserialize(saveJson["Farm"] as JObject ?? new JObject());

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
        saveJson.Add("PenaltyGiver", penaltyGiver.Serialize());
        saveJson.Add("Farm", farm.Serialize());
        saveJson.Add("MapManager", MapManager.Instance.Serialize());
        saveJson.Add("ResourceManager", ResourceManager.Instance.Serialize());

        FarmSerializer.WriteSave(saveJson);
    }

    private void OpenDefenceScene()
    {
        SerializeToSaveFile();
        SceneManager.LoadScene(2);
    }

    private void OnFarmQuotaFilledHandler(ProductEntry entry, Vector2 cropWorldPosition, int quota)
    {
        var currentMapId = MapManager.Instance.CurrentMap.MapId;
        ResourceManager.Instance.Gold += quotaContext.FillQuota(entry.ProductName, quota, currentMapId);
        farmUI.PlayProductFillAnimation(entry, cropWorldPosition, quota);
        farmUI.PlayCoinAnimation();
        UpdateHarvestInventory();
        SoundManager.Instance.PlaySfx("SFX_T_coin");
    }

    private void UpdateHarvestInventory()
    {
        farmUI.UpdateHarvestInventory(
            productEntry => quotaContext.IsProductAvailable(productEntry),
            productEntry => quotaContext.TryGetQuota(productEntry.ProductName, out var outQuota) ? outQuota : 0,
            () => quotaContext.HotProduct,
            () => quotaContext.SpecialProduct);
    }

    private bool OnWeatherShopItemBought(WeatherShopItem item)
    {
        if (ResourceManager.Instance.Gold < item.Price)
        {
            return false;
        }

        var isWeatherGiven = weatherGiver.SetWeather(item);
        if (isWeatherGiven)
        {
            ResourceManager.Instance.Gold -= item.Price;
        }
        
        return isWeatherGiven;
    }
}
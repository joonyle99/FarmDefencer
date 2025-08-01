using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 타이쿤의 최초 로드 동작을 정의하고, 타이쿤을 구성하는 여러 시스템들간의 중재자 역할을 하는 컴포넌트.
/// </summary>
public sealed class FarmManager : MonoBehaviour
{
    [Header("조정 가능 수치")] 
    [SerializeField] private float fastBgmStartTime = 150.0f;
    [SerializeField] private float fastBgmSpeedMultiplier = 1.5f;
    
    [Header("디버그용 세이브 무시")] [SerializeField]
    private bool ignoreSaveFile;

    [Header("의존성 오브젝트 설정")]
    [Tooltip("여기에서는 프로그래밍 파트 외에서 조정할 것은 없음.")]
    [SerializeField] private Farm farm;
    [SerializeField] private QuotaContext quotaContext;
    [SerializeField] private FarmClock farmClock;
    [SerializeField] private PenaltyGiver penaltyGiver;
    [SerializeField] private PestGiver pestGiver;
    [SerializeField] private HarvestTutorialGiver harvestTutorialGiver;
    // ResourceManager
    // MapManager

    // 저장되지 않는 오브젝트들
    [SerializeField] private FarmUI farmUI;
    [SerializeField] private FarmInput farmInput;
    [SerializeField] private ProductDatabase productDatabase;
    [SerializeField] private HarvestAnimationPlayer harvestAnimationPlayer;
    [SerializeField] private WateringCan wateringCan;
    [SerializeField] private WeatherShopUI weatherShopUI;
    [SerializeField] private GoDefenceUI goDefenceUI;
    [SerializeField] private WeatherGiver weatherGiver;
    [SerializeField] private TimerUI timerUI;
    [SerializeField] private FarmDebugUI farmDebugUI;
    
    private bool CanGoDefence => !weatherGiver.IsWeatherOnGoing && !pestGiver.IsWarningShowing && !harvestTutorialGiver.IsPlayingTutorial && ResourceManager.Instance.Gold > 0;

    private void Start()
    {
        InitializeDependencies();
#if DEBUG
        if (ignoreSaveFile)
        {
            // 디버그용 할당 코드 등 여기에...
            quotaContext.AssignQuotas(MapManager.Instance.MaximumUnlockedMapIndex, MapManager.Instance.MaximumUnlockedStageIndex);
        }
        else
#endif
        {
            DeserializeFromSaveFile();
        }

        penaltyGiver.SpawnMonsters(MapManager.Instance.MaximumUnlockedMapIndex, ResourceManager.Instance.SurvivedMonsters);
        ResourceManager.Instance.SurvivedMonsters.Clear();
        farmUI.PlayQuotaAssignAnimation(IsProductAvailableNow,
            productEntry => quotaContext.TryGetQuota(productEntry.ProductName, out var quota) ? quota : 0);

        if (farmClock.CurrentDaytime == 0.0f)
        {
            farmInput.FullZoomOut();
            pestGiver.ReserveRandomPestSpawn();
            quotaContext.AssignQuotas(MapManager.Instance.MaximumUnlockedMapIndex, MapManager.Instance.MaximumUnlockedStageIndex);
        }

        Application.wantsToQuit += SaveOnQuit;
        
        harvestTutorialGiver.PlayTutorialsToDo(productDatabase.Products, IsProductAvailableNow);
    }

    private void Update()
    {
        if (harvestTutorialGiver.IsPlayingTutorial)
        {
            harvestTutorialGiver.gameObject.SetActive(!penaltyGiver.IsAnimationPlaying);
            farmInput.InputPriorityCut = harvestTutorialGiver.InputPriority;
        }
        else
        {
            farmInput.InputPriorityCut = 0;
        }

        if (pestGiver.IsWarningShowing)
        {
            farmInput.FullZoomOut();
        }

        wateringCan.gameObject.SetActive(!harvestTutorialGiver.IsPlayingTutorial);
        weatherShopUI.gameObject.SetActive(!harvestTutorialGiver.IsPlayingTutorial);
        goDefenceUI.gameObject.SetActive(farmClock.CurrentDaytime >= farmClock.LengthOfDaytime && !harvestTutorialGiver.IsPlayingTutorial);
        
        harvestTutorialGiver.Init(farmUI.ToggleCropGuide, () => farmUI.IsCropGuideShowing);
        
        SoundManager.Instance.PlayBgm("BGM_T_main_origin", 0.25f, farmClock.CurrentDaytime > fastBgmStartTime ? fastBgmSpeedMultiplier : 1.0f);

        if (!pestGiver.IsPestRunning)
        {
            SoundManager.Instance.PlayAmb("AMB_T_main");
        }
    }
    
    private void InitializeDependencies()
    {
        farmInput.RegisterInputLayer(farm);
        farmInput.RegisterInputLayer(harvestTutorialGiver);
        farmInput.RegisterInputLayer(goDefenceUI);
        farmInput.RegisterInputLayer(pestGiver);
        farmInput.RegisterInputLayer(wateringCan);
        farmInput.AddCanInputCondition(() => farmClock.RemainingDaytime > 0.0f || harvestTutorialGiver.gameObject.activeSelf);

        farmClock.RegisterFarmUpdatableObject(farm);
        farmClock.RegisterFarmUpdatableObject(penaltyGiver);
        farmClock.RegisterFarmUpdatableObject(pestGiver);
        farmClock.AddPauseCondition(() => penaltyGiver.IsAnimationPlaying);
        farmClock.AddPauseCondition(() => harvestTutorialGiver.IsPlayingTutorial);
        farmClock.AddPauseCondition(() => farmDebugUI.IsPaused);
        farmDebugUI.Init(farmClock.SetDaytime, () => farmClock.RemainingDaytime);
        farmClock.AddPauseCondition(() => weatherGiver.IsWeatherOnGoing);
        farmClock.AddPauseCondition(() => pestGiver.IsWarningShowing);

        quotaContext.Init(GetProductEntry, OnQuotaFilled, OnQuotaAssigned);

        farm.Init(
            () => pestGiver.IsPestRunning,
            entry => quotaContext.TryGetQuota(entry.ProductName, out var quota) ? quota : 0,
            OnFarmQuotaFilledHandler,
            farmUI.ToggleCropGuide);

        farmUI.Init(farmInput,
            productDatabase,
            OpenDefenceScene,
            () =>
            { 
                SerializeToSaveFile();
                SceneManager.LoadScene("Main Scene");
            },
            (productEntry, duration, screenFrom, screenTo) => harvestAnimationPlayer.PlayScreenAnimation(productEntry, duration, screenFrom, screenTo, ()=>{}),
            () => CanGoDefence);

        wateringCan.Init(() => !farmClock.Paused, 
            
            wateringPosition =>
            {
                farm.WateringAction(wateringPosition);
            });
        
        penaltyGiver.Init(farm);

        weatherShopUI.Init(OnWeatherShopItemBought);
        weatherShopUI.AddItem(new SunItem(20 + MapManager.Instance.MaximumUnlockedMapIndex*10));
        weatherShopUI.AddItem(new RainItem(10, 10));
        weatherShopUI.AddItem(new RainItem(30, 30));
        weatherShopUI.AddItem(new RainItem(50, 50));

        goDefenceUI.Init(OpenDefenceScene);

		timerUI.Init(MapManager.Instance.MaximumUnlockedMapIndex, MapManager.Instance.MaximumUnlockedStageIndex, () => farmClock.RemainingDaytime / farmClock.LengthOfDaytime);

        weatherGiver.Init(farm.ApplyCropCommand);

        pestGiver.Init(
            productName =>IsProductAvailableNow(GetProductEntry(productName)),
            GetProductEntry,
            () => farmClock.CurrentDaytime,
            () => farmClock.Paused,
            EarnGold,
            (productEntry, duration, worldFrom, worldTo) => harvestAnimationPlayer.PlayWorldAnimation(productEntry, duration, worldFrom, worldTo, () => {}));
    }

    private void OnQuotaFilled()
    {
        UpdateHarvestInventory();
        farm.UpdateAvailability(IsProductAvailableNow);

        if (quotaContext.IsAllQuotaFilled)
        {
            quotaContext.AssignQuotas(MapManager.Instance.MaximumUnlockedMapIndex, MapManager.Instance.MaximumUnlockedStageIndex);
        }
    }

    private void OnQuotaAssigned()
    {
        farmUI.PlayQuotaAssignAnimation(
            IsProductAvailableNow,
            productEntry => quotaContext.TryGetQuota(productEntry.ProductName, out var quota) ? quota : 0);
    }

    private bool IsProductAvailableNow(ProductEntry productEntry) => quotaContext.IsProductAvailable(productEntry,
        MapManager.Instance.MaximumUnlockedMapIndex, MapManager.Instance.MaximumUnlockedStageIndex);
    
    private void DeserializeFromSaveFile()
    {
        var saveJson = SaveManager.Instance.LoadedSave;

        harvestTutorialGiver.Deserialize(saveJson["HarvestTutorialGiver"] as JObject ?? new JObject());
        quotaContext.Deserialize(saveJson["QuotaContext"] as JObject ?? new JObject());
        penaltyGiver.Deserialize(saveJson["PenaltyGiver"] as JObject ?? new JObject());
        farm.Deserialize(saveJson["Farm"] as JObject ?? new JObject());
        pestGiver.Deserialize(saveJson["PestGiver"] as JObject ?? new JObject());
        farmClock.Deserialize(saveJson["FarmClock"] as JObject ?? new JObject());
    }

    private void SerializeToSaveFile()
    {
        var saveJson = new JObject();

        saveJson.Add("FarmClock", farmClock.Serialize());
        saveJson.Add("PestGiver", pestGiver.Serialize());
        saveJson.Add("QuotaContext", quotaContext.Serialize());
        saveJson.Add("PenaltyGiver", penaltyGiver.Serialize());
        saveJson.Add("Farm", farm.Serialize());
        saveJson.Add("HarvestTutorialGiver", harvestTutorialGiver.Serialize());
        saveJson.Add("MapManager", MapManager.Instance.Serialize());
        saveJson.Add("ResourceManager", ResourceManager.Instance.Serialize());

        SaveManager.Instance.WriteSave(saveJson);
    }

    private void OpenDefenceScene()
    {
        if (!CanGoDefence)
        {
            return;
        }
        
        MapManager.Instance.CurrentMapIndex = MapManager.Instance.MaximumUnlockedMapIndex;
        MapManager.Instance.CurrentStageIndex = MapManager.Instance.MaximumUnlockedStageIndex;

        var defenceSceneOpenContextObject = new GameObject("DefenceSceneOpenContext");
        defenceSceneOpenContextObject.AddComponent<DefenceSceneOpenContext>();
        DontDestroyOnLoad(defenceSceneOpenContextObject);

        SerializeToSaveFile();
        SceneManager.LoadScene("Defence Scene");
    }

    private void OnFarmQuotaFilledHandler(ProductEntry entry, Vector2 cropWorldPosition, int quota)
    {
        var quotaAfterPestsEat = pestGiver.LetPestsEat(entry.ProductName, cropWorldPosition, quota);

        if (quotaAfterPestsEat > 0)
        {
            var goldEarnedFromSelling = quotaContext.FillQuota(entry.ProductName, quotaAfterPestsEat, MapManager.Instance.MaximumUnlockedMapIndex);
            farmUI.PlayProductFillAnimation(entry, cropWorldPosition, quotaAfterPestsEat);
            UpdateHarvestInventory();

            EarnGold(goldEarnedFromSelling);
        }
    }

    private void UpdateHarvestInventory()
    {
        farmUI.UpdateHarvestInventory(
            IsProductAvailableNow,
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

    private ProductEntry GetProductEntry(string productName) => productDatabase.Products.FirstOrDefault(p => p.ProductName == productName);

    private void EarnGold(int gold)
    {
        ResourceManager.Instance.Gold += gold;
        farmUI.PlayCoinAnimation();
        SoundManager.Instance.PlaySfx("SFX_T_coin", SoundManager.Instance.coinVolume);
    }

    private bool SaveOnQuit()
    {
        SerializeToSaveFile();
        return true;
    }

    private void OnDestroy()
    {
        Application.wantsToQuit -= SaveOnQuit;
    }

    private void OnApplicationPause(bool pauseStatus) => SerializeToSaveFile();
}
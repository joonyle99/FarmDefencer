using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using System.Collections;
using Sirenix.OdinInspector;

/// <summary>
/// 타이쿤의 최초 로드 동작을 정의하고, 타이쿤을 구성하는 여러 시스템들간의 중재자 역할을 하는 컴포넌트.
/// </summary>
public sealed class FarmManager : MonoBehaviour
{
    [Header("디버그용 세이브 무시")] [SerializeField]
    private bool ignoreSaveFile;

    [Header("의존성 오브젝트 설정")]
    [Tooltip("여기에서는 프로그래밍 파트 외에서 조정할 것은 없음.")]
    [SerializeField] private Farm farm;
    [SerializeField] private HarvestInventory harvestInventory;
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
    [SerializeField] private GoldEarnEffectPlayer goldEarnEffectPlayer;
    
    private bool CanGoDefence => !weatherGiver.IsWeatherOnGoing && !pestGiver.IsWarningShowing && !harvestTutorialGiver.IsPlayingTutorial && ResourceManager.Instance.Coin > 0;

    private void Start()
    {
        InitializeDependencies();
#if DEBUG
        if (ignoreSaveFile)
        {
            // 디버그용 할당 코드 등 여기에...
            harvestInventory.AssignAllQuotas(MapManager.Instance.MaximumUnlockedMapIndex, MapManager.Instance.MaximumUnlockedStageIndex);
        }
        else
#endif
        {
            DeserializeFromSaveFile();
        }

        penaltyGiver.SpawnMonsters(MapManager.Instance.CurrentMapIndex, ResourceManager.Instance.SurvivedMonsters);
        ResourceManager.Instance.SurvivedMonsters.Clear();

        if (farmClock.CurrentDaytime == 0.0f)
        {
            farmInput.FullZoomOut();
            pestGiver.ReserveRandomPestSpawn();
        }

        Application.wantsToQuit += SaveOnQuit;
        
        harvestInventory.AssignIfJustUnlocked(MapManager.Instance.MaximumUnlockedMapIndex, MapManager.Instance.MaximumUnlockedStageIndex);
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

        if (!pestGiver.IsPestRunning)
        {
            SoundManager.Instance.PlayAmb("AMB_T_main", SoundManager.Instance.ambVolume);
        }
        
        ProcessBgm();
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

        harvestInventory.Init(GetProductEntry);

        farm.Init(
            () => pestGiver.IsPestRunning,
            productName => harvestInventory.IsProductAvailable(productName, MapManager.Instance.MaximumUnlockedMapIndex, MapManager.Instance.MaximumUnlockedStageIndex),
            cropWorldPosition =>
            {
                EarnGold(1);
                goldEarnEffectPlayer.PlayEffectAt(cropWorldPosition, 1);
            },
            OnHarvestedCropSold,
            farmUI.ToggleCropGuide);

        farmUI.Init(farmInput,
            OpenDefenceScene,
            () =>
            { 
                SerializeToSaveFile();
                SceneManager.LoadScene("Main Scene");
            },
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
            (productEntry, duration, worldFrom, worldTo) => harvestAnimationPlayer.PlayAnimation(productEntry, duration, worldFrom, worldTo, () => {}));
    }

    private bool IsProductAvailableNow(ProductEntry productEntry) => harvestInventory.IsProductAvailable(productEntry.ProductName,
        MapManager.Instance.MaximumUnlockedMapIndex, MapManager.Instance.MaximumUnlockedStageIndex);
    
    private void DeserializeFromSaveFile()
    {
        var saveJson = SaveManager.Instance.LoadedSave;

        harvestTutorialGiver.Deserialize(saveJson["HarvestTutorialGiver"] as JObject ?? new JObject());
        penaltyGiver.Deserialize(saveJson["PenaltyGiver"] as JObject ?? new JObject());
        farm.Deserialize(saveJson["Farm"] as JObject ?? new JObject());
        harvestInventory.Deserialize(saveJson["HarvestInventory"] as JObject ?? new JObject());
        pestGiver.Deserialize(saveJson["PestGiver"] as JObject ?? new JObject());
        farmClock.Deserialize(saveJson["FarmClock"] as JObject ?? new JObject());
    }

    private void SerializeToSaveFile()
    {
        var saveJson = new JObject();

        saveJson.Add("FarmClock", farmClock.Serialize());
        saveJson.Add("PestGiver", pestGiver.Serialize());
        saveJson.Add("HarvestInventory", harvestInventory.Serialize());
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

        // 로딩 씬 설정
        SceneLoadContext.NextSceneName = "Defence Scene";
        //SceneLoadContext.OnSceneChanged = null;
        SceneLoadContext.OnSceneChanged += () =>
        {
            SceneLoadContext.OnSceneChanged = null;

            var defenceSceneOpenContext = new GameObject("DefenceSceneOpenContext");
            defenceSceneOpenContext.AddComponent<DefenceSceneOpenContext>();
            DontDestroyOnLoad(defenceSceneOpenContext);
        };

        SceneManager.LoadScene("Loading Scene");

        // 데이터 저장
        SerializeToSaveFile();
    }

    private void OnHarvestedCropSold(ProductEntry entry, Vector2 cropWorldPosition, int count)
    {
        var countAfterPestsEat = pestGiver.LetPestsEat(entry.ProductName, cropWorldPosition, count);

        if (countAfterPestsEat <= 0)
        {
            return;
        }
        
        harvestInventory.SellProduct(
            entry.ProductName, 
            cropWorldPosition,
            count, 
            MapManager.Instance.MaximumUnlockedMapIndex,
            MapManager.Instance.MaximumUnlockedStageIndex,
            gold =>
            {
                ResourceManager.Instance.Coin += gold;
                farmUI.PlayCoinAnimation();
            },
            (productEntry, duration, worldFrom, worldTo) => harvestAnimationPlayer.PlayAnimation(productEntry, duration, worldFrom, worldTo,
                () => { }),
            goldEarnEffectPlayer.GetEffectObject(cropWorldPosition));
    }

    private bool OnWeatherShopItemBought(WeatherShopItem item)
    {
        if (ResourceManager.Instance.Coin < item.Price)
        {
            return false;
        }

        var isWeatherGiven = weatherGiver.SetWeather(item);
        if (isWeatherGiven)
        {
            ResourceManager.Instance.Coin -= item.Price;
        }

        return isWeatherGiven;
    }

    private ProductEntry GetProductEntry(string productName) => productDatabase.Products.FirstOrDefault(p => p.ProductName == productName);

    private void EarnGold(int gold)
    {
        ResourceManager.Instance.Coin += gold;
        farmUI.PlayCoinAnimation();
        SoundManager.Instance.PlaySfx("SFX_T_coin", SoundManager.Instance.coinVolume);
    }
    
    private bool SaveOnQuit()
    {
        SerializeToSaveFile();
        return true;
    }

    private void ProcessBgm()
    {
        // BGM 교체
        var shouldPlayFastBgm = farmClock.CurrentDaytime / farmClock.LengthOfDaytime > 0.5f;
        if (shouldPlayFastBgm)
        {
            var fastSpeedBgmTime = 0.0f;
            if (SoundManager.Instance.CurrentBgmName is not null &&
                SoundManager.Instance.CurrentBgmName.Equals("BGM_T_main_origin_song"))
            {
                var normalSpeedBgmTime = SoundManager.Instance.CurrentBgmTime;
                fastSpeedBgmTime = normalSpeedBgmTime / 1.7f;
            }
            SoundManager.Instance.PlayBgm("BGM_T_main_fast", SoundManager.Instance.songVolume, fastSpeedBgmTime);
        }
        else
        {
            var normalSpeedBgmTime = 0.0f;
            if (SoundManager.Instance.CurrentBgmName is not null &&
                SoundManager.Instance.CurrentBgmName.Equals("BGM_T_main_fast_song"))
            {
                var fastSpeedBgmTime = SoundManager.Instance.CurrentBgmTime;
                normalSpeedBgmTime = fastSpeedBgmTime * 1.7f;
            }
            SoundManager.Instance.PlayBgm("BGM_T_main_origin", SoundManager.Instance.songVolume, normalSpeedBgmTime);
        }
        
        // 리버브 적용
        var currentBgmTime = SoundManager.Instance.CurrentBgmTime;
        if (currentBgmTime - Time.deltaTime < 0.0f)
        {
            var reverbSfxName = shouldPlayFastBgm ? "SFX_T_main_fast_reverb" : "SFX_T_main_origin_reverb";
            SoundManager.Instance.PlaySfx(reverbSfxName, SoundManager.Instance.songVolume);
        }
    }

    private void OnDestroy()
    {
        Application.wantsToQuit -= SaveOnQuit;
    }

    private void OnApplicationPause(bool pauseStatus) => SerializeToSaveFile();
}
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using TMPro;

[InfoBox("이 오브젝트를 배치한 씬의 이름은 MapStageSelectScene_{MapCode}로 둘 것.")]
public sealed class MapStageSelectSceneUI : MonoBehaviour
{
    public MapEntry SceneMapEntry { get; private set; }

    [InfoBox("StageButton_{MapIndex}_{StageIndex} 이름을 가진 Button들을 자식으로 갖는 오브젝트를 등록하면 됨.")]
    [SerializeField] private GameObject stageButtonsRootObject;

    private Button _goPreviousMapButton;
    private Button _goNextMapButton;
    private CoinsUI _gold;
    private Button _infiniteModeButton;
    private Button _settingButton;
    private TMP_Text _debugCurrentMapCode;

    private int _selectedStageIndex;

    private void Awake()
    {
        var mapCode = SceneManager.GetActiveScene().name.Split("_")[1];
        SceneMapEntry = MapManager.Instance.GetMapEntryOf(mapCode);

        // Current 정보 초기화
        MapManager.Instance.CurrentMapIndex = MapManager.Instance.MaximumUnlockedMapIndex;
        MapManager.Instance.CurrentStageIndex = MapManager.Instance.MaximumUnlockedStageIndex;

        _goPreviousMapButton = transform.Find("GoPreviousMapButton").GetComponent<Button>();
        _goPreviousMapButton.gameObject.SetActive(SceneMapEntry.MapId > 1);
        _goPreviousMapButton.onClick.AddListener(() => MoveToAnotherSelectSceneFor(SceneMapEntry.MapId - 1));

        _goNextMapButton = transform.Find("GoNextMapButton").GetComponent<Button>();
        _goNextMapButton.gameObject.SetActive(SceneMapEntry.MapId < 3);
        _goNextMapButton.interactable = SceneMapEntry.MapId + 1 <= MapManager.Instance.MaximumUnlockedMapIndex;
        _goNextMapButton.onClick.AddListener(() => MoveToAnotherSelectSceneFor(SceneMapEntry.MapId + 1));

        _gold = transform.Find("Gold").GetComponent<CoinsUI>();
        _gold.SetCoin(ResourceManager.Instance.Gold);

        _infiniteModeButton = transform.Find("InfiniteModeButton").GetComponent<Button>();
        _infiniteModeButton.interactable = false;

        _settingButton = transform.Find("SettingButton").GetComponent<Button>();

        // TODO 실제 설정창 구현하기
        _settingButton.onClick.AddListener(() => SceneManager.LoadScene("Main Scene"));

        _debugCurrentMapCode = transform.Find("Debug_CurrentMapCode").GetComponent<TMP_Text>();
        _debugCurrentMapCode.text = SceneMapEntry.MapCode;
    }

    private void Start()
    {
        var maximumUnlockedMapIndex = MapManager.Instance.MaximumUnlockedMapIndex;
        var maximumUnlockedStageIndex = MapManager.Instance.MaximumUnlockedStageIndex;

        for (var childIndex = 0; childIndex < stageButtonsRootObject.transform.childCount; ++childIndex)
        {
            var stageButton = stageButtonsRootObject.transform.GetChild(childIndex).GetComponent<StageButton>();
            stageButton.OnClicked += OnStageButtonClicked;
            stageButton.SetStageButtonEnabled(stageButton.MapIndex < maximumUnlockedMapIndex || stageButton.MapIndex == maximumUnlockedMapIndex && stageButton.StageIndex <= maximumUnlockedStageIndex);
        }
    }

    private void OnStageButtonClicked(int mapIndex, int stageIndex)
    {
        MapManager.Instance.CurrentMapIndex = mapIndex;
        MapManager.Instance.CurrentStageIndex = stageIndex;

        var defenceSceneOpenContext = new GameObject("DefenceSceneOpenContext");
        defenceSceneOpenContext.AddComponent<DefenceSceneOpenContext>();
        DontDestroyOnLoad(defenceSceneOpenContext);

        SceneManager.LoadScene("Defence Scene");
    }

    private void MoveToAnotherSelectSceneFor(int anotherMapId)
    {
        var anotherMap = MapManager.Instance.GetMapEntryOf(anotherMapId);
        SceneManager.LoadScene($"MapStageSelectScene_{anotherMap.MapCode}");
    }
}

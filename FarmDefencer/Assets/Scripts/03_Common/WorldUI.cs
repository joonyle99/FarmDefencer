using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class WorldUI : MonoBehaviour
{
    [SerializeField] private CoinUI _coinUI;
    [SerializeField] private TMP_Text _mapNameText;

    private MapEntry _travelMap;
    public MapEntry TravelMap => _travelMap;

    private void Awake()
    {
        _travelMap = MapManager.Instance.CurrentMap;
    }
    private void Start()
    {
        _coinUI.SetCoin(ResourceManager.Instance.Coin);

        ChangeTravelMap(TravelMap);
    }

    public void GoToMain()
    {
        SceneManager.LoadScene("Main Scene");
    }

    public void ChangeTravelMap(int mapIdx)
    {
        var map = MapManager.Instance.GetMapEntryOf(mapIdx);
        _travelMap = map;
        _mapNameText.text = map.MapName;
    }
    public void ChangeTravelMap(MapEntry map)
    {
        _travelMap = map;
        _mapNameText.text = map.MapName;
    }
}

using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

public sealed class HarvestTutorialGiver : MonoBehaviour, IFarmInputLayer
{
    [SerializeField] private GameObject Prefab_Field_Carrot;
    [SerializeField] private GameObject Prefab_Field_Potato;
    [SerializeField] private GameObject Prefab_Field_Corn;
    [SerializeField] private GameObject Prefab_Field_Cabbage;
    [SerializeField] private GameObject Prefab_Field_Cucumber;
    [SerializeField] private GameObject Prefab_Field_Eggplant;
    [SerializeField] private GameObject Prefab_Field_Sweetpotato;
    [SerializeField] private GameObject Prefab_Field_Mushroom;
    public int InputPriority => IFarmInputLayer.Priority_HarvestTutorialGiver; 
    
    private List<string> _tutorials; // Product Name 저장

    public bool IsPlayingTutorial => _currentTutorial is not null;

    private HarvestTutorial _currentTutorial;
    private SpriteRenderer _background;
    
    private WateringCan _tutorialWateringCan;

    public void AddTutorial(string product)
    {
        _tutorials.Add(product);
        enabled = true;
        _background.gameObject.SetActive(true);
    }
    
    public bool OnSingleTap(Vector2 worldPosition)
    {
        if (!gameObject.activeSelf
            ||_currentTutorial is null)
        {
            return false;
        }

        if (_currentTutorial.TargetCrop.AABB(worldPosition))
        {
            _currentTutorial.TargetCrop.OnSingleTap(worldPosition);
            return true;
        }

        return _tutorialWateringCan.Using;
    }

    public bool OnSingleHolding(Vector2 initialWorldPosition, Vector2 offset, bool isEnd, float deltaHoldTime)
    {
        if (!gameObject.activeSelf
            || _currentTutorial is null)
        {
            return false;
        }
        
        if (_currentTutorial.TargetCrop.AABB(initialWorldPosition))
        {
            _currentTutorial.Field.OnSingleHolding(initialWorldPosition, offset, isEnd, deltaHoldTime);
            return true;
        }
        
        return _tutorialWateringCan.Using;
    }

    private void Awake()
    {
        _tutorials = new();
        _background = transform.Find("Background").GetComponent<SpriteRenderer>();
        _tutorialWateringCan = transform.GetComponentInChildren<WateringCan>();
        _tutorialWateringCan.Init(() => true, wateringPosition => { if (_currentTutorial is not null && _currentTutorial.TargetCrop.AABB(wateringPosition)) _currentTutorial.TargetCrop.OnWatering(); });
    }
    
    private void Update()
    {
        if (!_tutorials.Any())
        {
            _background.gameObject.SetActive(false);
            enabled = false;
            return;
        }

        var currentTutorialProductName = _tutorials[0];
        if (_currentTutorial is null ||
            !_currentTutorial.ProductEntry.ProductName.Equals(currentTutorialProductName))
        {
            var newTutorial = InstantiateTutorial(currentTutorialProductName);
            if (newTutorial is null)
            {
                _tutorials.RemoveAt(0);
                return;
            }

            newTutorial.transform.parent = transform;
            AssignTutorial(newTutorial);
        }

        if (_currentTutorial.Done)
        {
            AssignTutorial(null);
            _tutorials.RemoveAt(0);
        }
    }

    private void AssignTutorial(HarvestTutorial tutorial)
    {
        if (_currentTutorial is not null)
        {
            Destroy(_currentTutorial.gameObject);
            _currentTutorial = null;
        }

        _currentTutorial = tutorial;
    }
    
    private HarvestTutorial InstantiateTutorial(string productName)
    {
        var prefab = productName switch
        {
            "product_carrot" => Prefab_Field_Carrot,
            "product_potato" => Prefab_Field_Potato,
            "product_corn" => Prefab_Field_Corn,
            "product_cabbage" => Prefab_Field_Cabbage,
            "product_cucumber" => Prefab_Field_Cucumber,
            "product_eggplant" => Prefab_Field_Eggplant,
            "product_sweetpotato" => Prefab_Field_Sweetpotato,
            "product_mushroom" => Prefab_Field_Mushroom,
            _ => null
        };
        
        var tutorialType = productName switch
        {
            "product_carrot" => typeof(CarrotTutorial),
            "product_potato" => typeof(PotatoTutorial),
            "product_corn" => typeof(CornTutorial),
            "product_cabbage" => typeof(CabbageTutorial),
            "product_cucumber" => typeof(CucumberTutorial),
            "product_eggplant" => typeof(EggplantTutorial),
            "product_sweetpotato" => typeof(SweetpotatoTutorial),
            "product_mushroom" => typeof(MushroomTutorial),
            _ => null
        };
        
        if (prefab is null || tutorialType is null)
        {
            Debug.LogError($"{productName}에 해당하는 HarvestTutorial 컴포넌트를 생성할 수 없습니다.");
            return null;
        }

        var instantiated = Instantiate(prefab);
        var harvestTutorial = instantiated.AddComponent(tutorialType) as HarvestTutorial;

        return harvestTutorial;
    }
}

using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public sealed class HarvestTutorialGiver : MonoBehaviour, IFarmInputLayer
{
    private const float TimeToShowHandFromLastInput = 1.0f;
    
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

    public bool IsPlayingTutorial => _currentTutorialField is not null;

    private HarvestTutorialField _currentTutorialField;
    private SpriteRenderer _background;
    
    private WateringCan _tutorialWateringCan;
    private TutorialHand _tutorialHand;

    private float _lastInputTime;

    public void AddTutorial(string product)
    {
        _tutorials.Add(product);
        enabled = true;
        _background.gameObject.SetActive(true);
    }
    
    public bool OnSingleTap(Vector2 worldPosition)
    {
        if (!gameObject.activeSelf
            ||_currentTutorialField is null)
        {
            return false;
        }

        if (_currentTutorialField.TargetCrop.AABB(worldPosition) || _tutorialWateringCan.Using)
        {
            if (_currentTutorialField.TargetCrop.AABB(worldPosition))
            {
                _currentTutorialField.TargetCrop.OnSingleTap(worldPosition);
            }
            
            _lastInputTime = Time.time;
            _tutorialHand.gameObject.SetActive(false);
            return true;
        }
        
        return false;
    }

    public bool OnSingleHolding(Vector2 initialWorldPosition, Vector2 offset, bool isEnd, float deltaHoldTime)
    {
        if (!gameObject.activeSelf
            || _currentTutorialField is null)
        {
            return false;
        }
        
        if (_currentTutorialField.TargetCrop.AABB(initialWorldPosition) || _tutorialWateringCan.Using)
        {
            if (_currentTutorialField.TargetCrop.AABB(initialWorldPosition))
            {
                _currentTutorialField.Field.OnSingleHolding(initialWorldPosition, offset, isEnd, deltaHoldTime);
            }
            
            _lastInputTime = Time.time;
            _tutorialHand.gameObject.SetActive(false);
            return true;
        }
        
        return false;
    }

    private void Awake()
    {
        _tutorials = new();
        _background = transform.Find("Background").GetComponent<SpriteRenderer>();
        
        _tutorialWateringCan = transform.GetComponentInChildren<WateringCan>();
        _tutorialWateringCan.Init(() => true, wateringPosition => { if (_currentTutorialField is not null && _currentTutorialField.TargetCrop.AABB(wateringPosition)) _currentTutorialField.TargetCrop.OnWatering(); });
        
        _tutorialHand = transform.GetComponentInChildren<TutorialHand>();
    }
    
    private void Update()
    {
        if (!_tutorials.Any())
        {
            gameObject.SetActive(false);
            return;
        }

        var currentTutorialProductName = _tutorials[0];
        if (_currentTutorialField is null ||
            !_currentTutorialField.ProductEntry.ProductName.Equals(currentTutorialProductName))
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

        var currentTime = Time.time;
        _tutorialHand.gameObject.SetActive(currentTime > _lastInputTime + TimeToShowHandFromLastInput);
        _tutorialHand.transform.position = _currentTutorialField.TargetCrop.transform.position;
        _tutorialHand.CurrentAction = _currentTutorialField.TargetCrop.RequiredCropAction;

        if (_currentTutorialField.Done)
        {
            AssignTutorial(null);
            _tutorials.RemoveAt(0);
        }
    }

    private void AssignTutorial(HarvestTutorialField tutorialField)
    {
        if (_currentTutorialField is not null)
        {
            Destroy(_currentTutorialField.gameObject);
            _currentTutorialField = null;
        }

        _currentTutorialField = tutorialField;
    }
    
    private HarvestTutorialField InstantiateTutorial(string productName)
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
        
        if (prefab is null)
        {
            Debug.LogError($"{productName}에 해당하는 HarvestTutorial 컴포넌트를 생성할 수 없습니다.");
            return null;
        }

        var instantiated = Instantiate(prefab);
        var harvestTutorial = instantiated.AddComponent<HarvestTutorialField>();

        return harvestTutorial;
    }
}

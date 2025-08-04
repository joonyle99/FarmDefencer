using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Newtonsoft.Json.Linq;
using Sirenix.OdinInspector;
using System;
using Newtonsoft.Json;
using UnityEngine.UI;

public sealed class HarvestTutorialGiver : MonoBehaviour, IFarmInputLayer, IFarmSerializable
{
    [InfoBox("입력한 이후 잠시 튜토리얼 손이 안 보이는 시간을 정함.")]
    [SerializeField] private float hideTimeAfterInput = 1.0f;
    
    [SerializeField] private GameObject Prefab_Field_Carrot;
    [SerializeField] private GameObject Prefab_Field_Potato;
    [SerializeField] private GameObject Prefab_Field_Corn;
    [SerializeField] private GameObject Prefab_Field_Cabbage;
    [SerializeField] private GameObject Prefab_Field_Cucumber;
    [SerializeField] private GameObject Prefab_Field_Eggplant;
    [SerializeField] private GameObject Prefab_Field_Sweetpotato;
    [SerializeField] private GameObject Prefab_Field_Mushroom;
    public int InputPriority => IFarmInputLayer.Priority_HarvestTutorialGiver; 
    
    private List<string> _onGoingTutorials; // Product Name 저장

    public bool IsPlayingTutorial => _currentTutorialField is not null;

    private HarvestTutorialField _currentTutorialField;
    private SpriteRenderer _background;
    
    private WateringCan _tutorialWateringCan;
    private TutorialHand _tutorialHand;
    private RectTransform _signRectTransform;
    private Button _skipTutorialButton;

    private float _lastInputTime;
    private Action<ProductEntry> _showCropGuide;
    private Func<bool> _isCropGuideShowing;
    
    // 저장되는 값
    private List<string> _finishedTutorials;

    public void Init(Action<ProductEntry> showCropGuide, Func<bool> isCropGuideShowing)
    {
        _showCropGuide = showCropGuide;
        _isCropGuideShowing = isCropGuideShowing;
    }
    
    public void PlayTutorialsToDo(
        IReadOnlyList<ProductEntry> productEntries, 
        Func<ProductEntry, bool> isCropUnlocked)
    {
        foreach (var productEntry in productEntries)
        {
            if (isCropUnlocked(productEntry) && !_finishedTutorials.Contains(productEntry.ProductName))
            {
                _onGoingTutorials.Add(productEntry.ProductName);
            }
        }

        if (_onGoingTutorials.Count > 0)
        {
            enabled = true;
            _background.gameObject.SetActive(true);
        }
    }
    
    public bool OnTap(Vector2 worldPosition)
    {
        if (!gameObject.activeSelf
            ||_currentTutorialField is null)
        {
            return false;
        }

        if (_currentTutorialField.State == HarvestTutorialField.TutorialState.ShouldClickSign)
        {
            var cropSignPosition = _currentTutorialField.CropSignWorldPosition;
            if (Mathf.Abs(worldPosition.x - cropSignPosition.x) < 0.5f &&
                Mathf.Abs(worldPosition.y - cropSignPosition.y) < 0.5f)
            {
                _currentTutorialField.State = HarvestTutorialField.TutorialState.ShouldCloseGuide;
                _showCropGuide(_currentTutorialField.ProductEntry);
                return true;
            }
            return false;
        }

        if (_currentTutorialField.State == HarvestTutorialField.TutorialState.ShouldCloseGuide)
        {
            return false;
        }

        if (_currentTutorialField.TargetCrop.AABB(worldPosition) || _tutorialWateringCan.Using)
        {
            if (_currentTutorialField.TargetCrop.AABB(worldPosition))
            {
                _currentTutorialField.TargetCrop.OnTap(worldPosition);
                _tutorialHand.HideForSeconds(hideTimeAfterInput);
            }
            return true;
        }
        
        return false;
    }

    public bool OnHold(Vector2 initialWorldPosition, Vector2 offset, bool isEnd, float deltaHoldTime)
    {
        if (!gameObject.activeSelf
            || _currentTutorialField is null)
        {
            return false;
        }
        
        if (_currentTutorialField.TargetCrop.AABB(initialWorldPosition))
        {
            if (_currentTutorialField.TargetCrop.AABB(initialWorldPosition))
            {
                _currentTutorialField.Field.OnHold(initialWorldPosition, offset, isEnd, deltaHoldTime);
                _tutorialHand.HideForSeconds(hideTimeAfterInput);
            }
            return true;
        }

        if (_tutorialWateringCan.Using)
        {
            _tutorialHand.HideForSeconds(hideTimeAfterInput);
            return true;
        }
        
        return false;
    }
    
    public JObject Serialize()
    {
        var jsonObject = new JObject();
        var jsonFinishedTutorials = new JArray();
        foreach (var finishedTutorial in _finishedTutorials)
        {
            jsonFinishedTutorials.Add(finishedTutorial);
        }
        jsonObject.Add("FinishedTutorials", jsonFinishedTutorials);
        return jsonObject;
    }

    public void Deserialize(JObject json)
    {
        if (json["FinishedTutorials"] is JArray jsonFinishedTutorials)
        {
            foreach (var jsonFinishedTutorial in jsonFinishedTutorials)
            {
                var finishedTutorial = jsonFinishedTutorial.Value<string>();
                if (finishedTutorial is not null)
                {
                    _finishedTutorials.Add(finishedTutorial);
                }
            }
        }
    }
    
    private void Awake()
    {
        _finishedTutorials = new();
        _onGoingTutorials = new();
        _background = transform.Find("Background").GetComponent<SpriteRenderer>();
        
        _tutorialWateringCan = transform.Find("TutorialWateringCan").GetComponent<WateringCan>();
        _tutorialWateringCan.Init(
            () => true,
            wateringPosition =>
            {
                if (_currentTutorialField is not null && _currentTutorialField.TargetCrop.AABB(wateringPosition))
                {
                    _currentTutorialField.TargetCrop.OnWatering();
                }
            });
        
        _tutorialHand = transform.GetComponentInChildren<TutorialHand>();

        _signRectTransform = transform.Find("SignCanvas/Sign").GetComponent<RectTransform>();
        _signRectTransform.gameObject.SetActive(false);
        
        _skipTutorialButton = transform.Find("DebugCanvas/SkipTutorialButton").GetComponent<Button>();
        _skipTutorialButton.onClick.AddListener(OnSkipTutorialButtonClicked);
    }
    
    private void Update()
    {
        if (!_onGoingTutorials.Any())
        {
            gameObject.SetActive(false);
            return;
        }

        var currentTutorialProductName = _onGoingTutorials[0];
        if (_currentTutorialField is null ||
            !_currentTutorialField.ProductEntry.ProductName.Equals(currentTutorialProductName))
        {
            var newTutorial = InstantiateTutorial(currentTutorialProductName);
            if (newTutorial is null)
            {
                _onGoingTutorials.RemoveAt(0);
                return;
            }

            newTutorial.transform.parent = transform;
            AssignTutorial(newTutorial);

            if (newTutorial.TargetCrop is not CropCarrot
                && newTutorial.TargetCrop is not CropPotato
                && newTutorial.TargetCrop is not CropCorn)
            {
                _signRectTransform.anchoredPosition = new Vector2(0, Screen.height);
                _signRectTransform.gameObject.SetActive(true);
                SoundManager.Instance.PlaySfx("SFX_T_unlock");

                var sequence = DOTween.Sequence();

                sequence.Append(_signRectTransform.DOAnchorPosY(0.0f, 1.0f).SetEase(Ease.OutBounce))
                    .AppendInterval(2.0f)
                    .AppendCallback(() => _signRectTransform.gameObject.SetActive(false));
            }
        }

        if (_currentTutorialField.State == HarvestTutorialField.TutorialState.ShouldClickSign)
        {
            _tutorialHand.transform.position = _currentTutorialField.CropSignWorldPosition;
            _tutorialHand.SetTutorialHandMotion(RequiredCropAction.SingleTap);
        }
        else if (_currentTutorialField.State == HarvestTutorialField.TutorialState.ShouldCloseGuide)
        {
            var emptyScreenPosition = new Vector2(Screen.width, Screen.height / 2.0f) + new Vector2(-200.0f, -100.0f);
            var emptyWorldPosition = Camera.main.ScreenToWorldPoint(emptyScreenPosition);
            emptyWorldPosition.z = 0.0f;
            
            _tutorialHand.transform.position = emptyWorldPosition;
            _tutorialHand.SetTutorialHandMotion(RequiredCropAction.SingleTap);

            if (!_isCropGuideShowing())
            {
                _currentTutorialField.State = HarvestTutorialField.TutorialState.Crop;
            }
        }
        else
        {
            _tutorialHand.transform.position = _currentTutorialField.TargetCrop.transform.position;
            _tutorialHand.SetTutorialHandMotion(_currentTutorialField.TargetCrop.RequiredCropAction);
        }

        if (_currentTutorialField.Done)
        {
            AssignTutorial(null);
            _finishedTutorials.Add(currentTutorialProductName);
            _onGoingTutorials.RemoveAt(0);
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

        var originFieldPosition = GameObject.Find($"Farm/{prefab.name}").transform.position;
        var instantiated = Instantiate(prefab);
        instantiated.transform.position = originFieldPosition;
        var harvestTutorial = instantiated.AddComponent<HarvestTutorialField>();

        return harvestTutorial;
    }

    private void OnSkipTutorialButtonClicked()
    {
        if (_currentTutorialField is not null)
        {
            _currentTutorialField.Done = true;
        }
    }
}

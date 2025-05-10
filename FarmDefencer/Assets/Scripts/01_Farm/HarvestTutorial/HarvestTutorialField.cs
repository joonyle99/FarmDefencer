using UnityEngine;

public sealed class HarvestTutorialField : MonoBehaviour
{
    private const float TimeMultiplierWhenWaiting = 7.0f;
    
    public ProductEntry ProductEntry => _field.ProductEntry;

    public bool Done { get; private set; }

    public Field Field => _field;
    private Field _field;
    
    public Crop TargetCrop => _field.TopLeftCrop;

    private void Awake()
    {
        _field = GetComponent<Field>();
    }

    private void Start()
    {
        _field.Reset();
        _field.IsAvailable = true;
        foreach (var crop in _field.GetComponentsInChildren<Crop>())
        {
            crop.gameObject.SetActive(true);
        }
        _field.transform.Find("FieldLockedDisplay").gameObject.SetActive(false);
        
        _field.Init(_ => 9999, (_, _, _) => { Done = true; SoundManager.PlaySfxStatic("SFX_T_coin"); }, _ => { });

        foreach (var spriteRendererComponent in _field.GetComponentsInChildren(typeof(SpriteRenderer)))
        {
            var spriteRenderer = spriteRendererComponent as SpriteRenderer;
            var sortingOrder = spriteRenderer.sortingOrder;
            spriteRenderer.sortingOrder = 10000 +  SortingLayer.GetLayerValueFromID(spriteRenderer.sortingLayerID) * 1000 + sortingOrder;
            spriteRenderer.sortingLayerName = "Mask";
        }

        if (TargetCrop is CropSweetpotato cropSweetpotato)
        {
            cropSweetpotato.ForceHarvestOne = true;
        }
        else if (TargetCrop is CropMushroom cropMushroom)
        {
            cropMushroom.ForceHarvestOne = true;
        }
    }
    
    private void Update()
    {
        var multiplier = TargetCrop.RequiredCropAction == RequiredCropAction.None ? TimeMultiplierWhenWaiting : 1.0f; 
        _field.OnFarmUpdate(Time.deltaTime * multiplier);
        
    }
}

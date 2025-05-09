using UnityEngine;

public abstract class HarvestTutorial : MonoBehaviour
{
    public ProductEntry ProductEntry => _field.ProductEntry;

    public bool Done => false;

    public Field Field => _field;
    private Field _field;
    
    public Crop TargetCrop => _field.TopLeftCrop;

    protected virtual void Awake()
    {
        _field = GetComponent<Field>();
    }

    protected virtual void Start()
    {
        _field.Reset();
        _field.IsAvailable = true;
        foreach (var crop in _field.GetComponentsInChildren<Crop>())
        {
            crop.gameObject.SetActive(true);
        }
        _field.transform.Find("FieldLockedDisplay").gameObject.SetActive(false);
        
        _field.Init(_ => 1, (_, _, _) => { }, _ => { });

        foreach (var spriteRendererComponent in _field.GetComponentsInChildren(typeof(SpriteRenderer)))
        {
            var spriteRenderer = spriteRendererComponent as SpriteRenderer;
            var sortingOrder = spriteRenderer.sortingOrder;
            spriteRenderer.sortingOrder = 10000 +  SortingLayer.GetLayerValueFromID(spriteRenderer.sortingLayerID) * 1000 + sortingOrder;
            spriteRenderer.sortingLayerName = "Mask";
        }
    }
    
    protected virtual void Update()
    {
        _field.OnFarmUpdate(Time.deltaTime);
        
        
    }
}

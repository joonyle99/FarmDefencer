using UnityEngine;

[CreateAssetMenu(fileName = "ProductEntry", menuName = "Scriptable Objects/Farm/CropEntry")]
public sealed class ProductEntry : ScriptableObject
{
    [SerializeField] private string name;
    public string Name => name;
    [SerializeField] private Sprite productSprite;
    public Sprite ProductSprite => productSprite;
    [SerializeField] private int price;
    public int Price => price;
}

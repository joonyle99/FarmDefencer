using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "ProductEntry", menuName = "Scriptable Objects/Farm/CropEntry")]
public sealed class ProductEntry : ScriptableObject
{
    [FormerlySerializedAs("productProductName")] [SerializeField] private string productName;
    public string ProductName => productName;
    [SerializeField] private Sprite productSprite;
    public Sprite ProductSprite => productSprite;
    
    [SerializeField] private int price;
    public int Price => price;
}

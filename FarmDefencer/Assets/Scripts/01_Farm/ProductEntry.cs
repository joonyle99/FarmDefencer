using UnityEngine;

[CreateAssetMenu(fileName = "ProductEntry", menuName = "Scriptable Objects/Farm/CropEntry")]
public class ProductEntry : ScriptableObject
{
    /// <summary>
    /// 고유한 이름입니다. 여러 클래스에서 이 아이템을 조회할 때 사용됩니다.
    /// carrot, potato처럼 사용합니다.
    /// </summary>
    public string UniqueId;
}

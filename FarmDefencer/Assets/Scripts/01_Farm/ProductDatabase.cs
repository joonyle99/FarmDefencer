using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ProductDatabase", menuName = "Scriptable Objects/Farm/ProductDatabase")]
public sealed class ProductDatabase : ScriptableObject
{
    public List<ProductEntry> Products;
    private Dictionary<string, ProductEntry> _lookupDictionary;

	public bool TryGetProduct(string productUniqueId, out ProductEntry entry) => _lookupDictionary.TryGetValue(productUniqueId, out entry);
	public bool TryGetProduct(ProductEntry keyEntry, out ProductEntry foundEntry) => _lookupDictionary.TryGetValue(keyEntry.ProductName, out foundEntry);

	private void Awake()
	{
		_lookupDictionary = new Dictionary<string, ProductEntry>();
		foreach (var entry in Products)
		{
			if (entry == null)
			{
				continue;
			}

			if (_lookupDictionary.ContainsKey(entry.ProductName))
			{
				Debug.LogError($"ProductDatabase({name})에 중복된 UniqueId {entry.ProductName}(을)를 갖는 Entry가 존재합니다.");
				continue;
			}

			_lookupDictionary.Add(entry.ProductName, entry);
		}
	}
}

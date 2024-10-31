using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ProductDatabase", menuName = "Scriptable Objects/Farm/ProductDatabase")]
public class ProductDatabase : ScriptableObject
{
    public List<ProductEntry> Products;
    private Dictionary<string, ProductEntry> _lookupDictionary;

	public bool TryGetProduct(string productUniqueId, out ProductEntry entry) => _lookupDictionary.TryGetValue(productUniqueId, out entry);
	public bool TryGetProduct(ProductEntry keyEntry, out ProductEntry foundEntry) => _lookupDictionary.TryGetValue(keyEntry.UniqueId, out foundEntry);

	private void Awake()
	{
		foreach (var entry in Products)
		{
			if (entry == null)
			{
				continue;
			}

			if (_lookupDictionary.ContainsKey(entry.UniqueId))
			{
				Debug.LogError($"ProductDatabase({name})�� �ߺ��� UniqueId {entry.UniqueId}(��)�� ���� Entry�� �����մϴ�.");
				continue;
			}

			_lookupDictionary.Add(entry.UniqueId, entry);
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "CropDictionary", menuName = "Scriptable Objects/Farm/CropDictionary")]
public class CropDictionary : ScriptableObject
{
	[Serializable]
	public struct CropInfo
    {
        public GameObject CropPrefab;
        public string CropName;
    }
    
    public List<CropInfo> CropInfos;

    public bool TryGetCropName(Crop crop, out string cropName)
    {
        var cropQuery = CropInfos.FirstOrDefault(
            cropInfo =>
            cropInfo.CropPrefab != null &&
            cropInfo.CropPrefab.GetComponent<Crop>()?.GetType() == crop.GetType());

        if (cropQuery.CropPrefab == null)
        {
            cropName = string.Empty;
            return false;
        }

        cropName = cropQuery.CropName;
        return true;
    }
}

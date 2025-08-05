using UnityEngine;
using Newtonsoft.Json.Linq;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using System;

public sealed class PestEatingPoint : MonoBehaviour, IFarmSerializable
{
    [SerializeField] private ProductEntry targetProduct;
    public ProductEntry TargetProduct => targetProduct;

    private List<Pest> _pests;
    public IReadOnlyList<Pest> Pests => _pests;
        
    private TMP_Text _countText;

    private Func<PestSize, Pest> _pestFactory;

    public void Init(Func<PestSize, Pest> pestFactory)
    {
        _pestFactory = pestFactory;
    }
    
    public void AddArrivedPest(Pest pest)
    {
        _pests.Insert(0, pest);
        Refresh();
    }

    public int LetPestsEat(Vector2 cropWorldPosition, int count)
    {
        var remainder = count;
        while (_pests.Count > 0 && remainder > 0)
        {
            var frontPest = _pests[0];
            remainder = frontPest.Eat(cropWorldPosition, remainder);

            if (frontPest.RemainingCropEatCount <= 0)
            {
                _pests.RemoveAt(0);
            }
        }

        Refresh();
        return remainder;
    }
    
    public JObject Serialize()
    {
        var jsonObject = new JObject();
        var arrivedPests = _pests;
        var jsonArrivedPests = new JArray();
        foreach (var arrivedPest in arrivedPests)
        {
            jsonArrivedPests.Add(arrivedPest.Serialize());
        }
        jsonObject.Add("ArrivedPests", jsonArrivedPests);

        return jsonObject;
    }

    public void Deserialize(JObject json)
    {
        if (json["ArrivedPests"] is JArray jsonArrivedPests)
        {
            foreach (var jsonArrivedPestToken in jsonArrivedPests)
            {
                if (jsonArrivedPestToken is not JObject jsonArrivedPest
                    || jsonArrivedPest["PestSize"]?.Value<int>() is null)
                {
                    continue;
                }

                var pestSize = (PestSize)jsonArrivedPest["PestSize"].Value<int>();

                var pest = _pestFactory(pestSize);
                pest.transform.parent = transform;
                pest.transform.localPosition = Vector3.zero;
                pest.Deserialize(jsonArrivedPest);
                _pests.Add(pest);
            }
        }
        Refresh();
    }

    private void Awake()
    {
        _pests = new List<Pest>();
        _countText = transform.Find("Count").GetComponent<TMP_Text>();
        Refresh();
        var meshRenderer = transform.Find("Count").GetComponent<MeshRenderer>();
        meshRenderer.sortingLayerName = "UI";
        meshRenderer.sortingOrder = 100;
    }

    private void Refresh()
    {
        var count = _pests.Sum(p => p.RemainingCropEatCount);

        _countText.text = count.ToString();
        _countText.gameObject.SetActive(count > 0);

        if (_pests.Count > 0)
        {
            _pests.ForEach(p => p.gameObject.SetActive(false));
            _pests.First().gameObject.SetActive(true);
        }
    }
}

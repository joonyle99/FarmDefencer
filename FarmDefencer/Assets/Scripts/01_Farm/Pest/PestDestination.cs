using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using TMPro;

public sealed class PestDestination : MonoBehaviour
{
    [SerializeField] private ProductEntry targetProduct;
    public ProductEntry TargetProduct => targetProduct;

    private List<Pest> _pests;
    public IReadOnlyList<Pest> Pests => _pests;
        
    private float _distanceBetweenPests;

    private TMP_Text _countText;

    public void Init(float distanceBetweenPests) => _distanceBetweenPests = distanceBetweenPests;

    public void AddPest(Pest pest)
    {
        _pests.Insert(0, pest);
        AdjustPestPositions();
        RefreshCountText();
    }

    public int LetPestsEat(int count)
    {
        var remainder = count;
        while (_pests.Count > 0 && remainder > 0)
        {
            var frontPest = _pests[0];
            remainder = frontPest.Eat(remainder);

            if (frontPest.RemainingCropEatCount <= 0)
            {
                _pests.RemoveAt(0);
                Destroy(frontPest.gameObject);
            }
        }

        AdjustPestPositions();
        RefreshCountText();
        return remainder;
    }

    private void Awake()
    {
        _pests = new List<Pest>();
        _countText = transform.Find("Count").GetComponent<TMP_Text>();
        AdjustPestPositions();
        RefreshCountText();
    }

    private void AdjustPestPositions()
    {
        for (var i = 0; i < _pests.Count; ++i)
        {
            var pest = _pests[i];
            pest.transform.position = transform.position + new Vector3(0.0f, _distanceBetweenPests * i, 0.0f);
        }
    }

    private void RefreshCountText()
    {
        var count = _pests.Sum(p => p.RemainingCropEatCount);

        _countText.text = count.ToString();
        _countText.gameObject.SetActive(count > 0);
    }
}

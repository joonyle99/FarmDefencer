using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 자식으로 HarvestAnimationPlayer 오브젝트를 가져야 함.
/// </summary>
public sealed class HarvestInventory : MonoBehaviour
{
    private ProductDatabase _productDatabase;
    private HarvestAnimationPlayer _harvestAnimationPlayer;
    private Image _drawer;
    private Dictionary<ProductEntry, HarvestBox> _harvestBoxes;
    private Animator _quotaAssignAnimator;

    public void UpdateInventory(Func<ProductEntry, bool> isProductAvailable, Func<ProductEntry, int> getQuota)
    {
        foreach (var productEntry in _productDatabase.Products)
        {
            var isAvailable = isProductAvailable(productEntry);
            _harvestBoxes[productEntry].IsAvailable = isAvailable;
            if (!isAvailable)
            {
                _harvestBoxes[productEntry].Quota = 0;
                continue;
            }

            _harvestBoxes[productEntry].Quota = getQuota(productEntry);
        }
    }

    public void PlayQuotaAssignAnimation(Func<ProductEntry, bool> isAvailable, Func<ProductEntry, int> getQuota) => StartCoroutine(DoQuotaAssignAnimation(isAvailable, getQuota));
    
    public void PlayProductFillAnimation(ProductEntry productEntry, Vector2 cropWorldPosition, int count,
        Func<ProductEntry, bool> isAvailable, Func<ProductEntry, int> getQuota)
    {
        var cropScreenPosition = Camera.main.WorldToScreenPoint(cropWorldPosition);
        StartCoroutine(HarvestAnimationCoroutine(productEntry, cropScreenPosition, count, isAvailable, getQuota));
    }

    public void Init(ProductDatabase database)
    {
        _productDatabase = database;
        foreach (var entry in _productDatabase.Products)
        {
            var harvestBoxTransform = transform.Find($"BoxArea/HarvestBox_{entry.ProductName}");
            if (harvestBoxTransform is null)
            {
                Debug.LogError($"HarvestInventory/BoxArea의 자식중에 HarvestBox_{entry.ProductName}가 필요합니다.");
                continue;
            }

            if (!harvestBoxTransform.TryGetComponent<HarvestBox>(out var harvestBox))
            {
                Debug.LogError($"{harvestBoxTransform.gameObject.name}(은)는 HarvestBox 컴포넌트를 갖지 않습니다.");
                continue;
            }

            _harvestBoxes.Add(entry, harvestBox);
        }
    }

    private void Awake()
    {
        _harvestAnimationPlayer = GetComponentInChildren<HarvestAnimationPlayer>();
        _harvestBoxes = new();
        _quotaAssignAnimator = GetComponentInChildren<Animator>();
        _quotaAssignAnimator.gameObject.SetActive(false);
        _drawer = transform.Find("Drawer").GetComponent<Image>();
    }

    private IEnumerator HarvestAnimationCoroutine(ProductEntry productEntry, Vector2 cropScreenPosition, int count,
        Func<ProductEntry, bool> isAvailable, Func<ProductEntry, int> getQuota)
    {
        var harvestBox = _harvestBoxes[productEntry];
        var toPosition = harvestBox.ScreenPosition;

        for (var i = 0; i < count; i++)
        {
            _harvestAnimationPlayer.PlayAnimation(productEntry, cropScreenPosition, toPosition, () => { });
            yield return new WaitForSeconds(0.1f);
        }

        UpdateInventory(isAvailable, getQuota);
        yield return null;
    }

    private IEnumerator DoQuotaAssignAnimation(Func<ProductEntry, bool> isAvailable, Func<ProductEntry, int> getQuota)
    {
        _quotaAssignAnimator.gameObject.SetActive(true);
        _quotaAssignAnimator.Play("HarvestInventoryGlow", 0, 0.0f);

        var glowAnimationLength = _quotaAssignAnimator.runtimeAnimatorController.animationClips[0].length;
        
        var elapsed = 0.0f;
        foreach (var (productEntry, harvestBox) in _harvestBoxes)
        {
            if (isAvailable(productEntry))
            {
                harvestBox.Blink(glowAnimationLength);
            }
        }

        while (elapsed < glowAnimationLength)
        {
            elapsed += Time.deltaTime;

            var x = elapsed / glowAnimationLength;
            var countAlpha = -(x - 1.0f) * (x - 1.0f) + 1.0f; // 0->1 체감 곡선

            foreach (var (productEntry, harvestBox) in _harvestBoxes)
            {
                if (!isAvailable(productEntry))
                {
                    continue;
                }
                
                harvestBox.Quota = Mathf.RoundToInt(getQuota(productEntry) * countAlpha);
            }

            var color = Color.white;
            _drawer.color = color;
            yield return null;
        }

        UpdateInventory(isAvailable, getQuota);
        _quotaAssignAnimator.gameObject.SetActive(false);
    }
}
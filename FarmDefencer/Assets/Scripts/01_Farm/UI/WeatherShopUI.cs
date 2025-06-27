using UnityEngine;
using UnityEngine.UI;
using System;

public abstract class WeatherShopItem
{
    public int Price { get; }
    protected WeatherShopItem(int price) => Price = price;
}

public sealed class RainItem : WeatherShopItem
{
    public int Percentage { get; }
    
    public RainItem(int price, int percentage) : base(price) => Percentage = percentage;
}

public sealed class SunItem : WeatherShopItem
{
    public SunItem(int price) : base(price){}
}

public sealed class WeatherShopUI : MonoBehaviour
{
    private GameObject _shopRootObject;
    private Func<WeatherShopItem, bool> _onWeatherShopItemBought;
    
    public void Init(Func<WeatherShopItem, bool> onWeatherShopItemBought) => _onWeatherShopItemBought = onWeatherShopItemBought;
    
    public void AddItem(WeatherShopItem item)
    {
        if (item is SunItem sunItem)
        {
            var sunButton = transform.Find("Shop/SunButton").GetComponent<Button>();
            sunButton.onClick.AddListener(() => OnWeatherShopItemBought(sunItem));
        }
        else if (item is RainItem rainItem)
        {
            var rainButton = transform.Find($"Shop/RainButton_{rainItem.Percentage}").GetComponent<Button>();
            rainButton.onClick.AddListener(() => OnWeatherShopItemBought(rainItem));
        }
        else
        {
            Debug.LogError($"처리되지 않은 WeatherShopItem Type: {item.GetType()} ");
        }
    }

    private void Awake()
    {
        _shopRootObject = transform.Find("Shop").gameObject;
        _shopRootObject.SetActive(false);
        var openButton = transform.Find("OpenButton").GetComponent<Button>();
        openButton.onClick.AddListener(() => _shopRootObject.SetActive(true));       
        
        var closeButton = transform.Find("Shop/CloseButton").GetComponent<Button>();
        closeButton.onClick.AddListener(() => _shopRootObject.SetActive(false));
    }

    private void OnWeatherShopItemBought(WeatherShopItem item)
    {
        if (_onWeatherShopItemBought(item))
        {
            _shopRootObject.SetActive(false);
        }
    }
}

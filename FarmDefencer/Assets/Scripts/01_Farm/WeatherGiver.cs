using UnityEngine;
using System;
using System.Collections;
using TMPro;
using Random = UnityEngine.Random;

public class WeatherGiver : MonoBehaviour
{
    public bool IsWeatherOnGoing => _onGoingWeather != null;

    private Coroutine _onGoingWeather;
    private Action<CropCommand> _applyCropCommand;
    private TMP_Text _weatherText; // Test
    private bool _wasSunBought;
    
    public void Init(Action<CropCommand> applyCropCommand) => _applyCropCommand = applyCropCommand;

    public bool SetWeather(WeatherShopItem item)
    {
        if (IsWeatherOnGoing || item is SunItem && _wasSunBought)
        {
            return false;
        }

        _onGoingWeather = StartCoroutine(DoWeather(item));

        return true;
    }

    private IEnumerator DoWeather(WeatherShopItem item)
    {
        _weatherText.enabled = true;
        
        switch (item)
        {
            case SunItem:
            {
                _wasSunBought = true;
                _weatherText.SetText("*SUN*");
                yield return new WaitForSeconds(1.0f);
                _applyCropCommand(new GrowCommand());
                break;
            }
            case RainItem rainItem:
            {
                var random = Random.Range(0, 100);
                if (random > rainItem.Percentage)
                {
                    break;
                }

                _weatherText.SetText("*RAIN*");
                yield return new WaitForSeconds(2.0f);
                _applyCropCommand(new WaterCommand());
                break;
            }
        }

        _weatherText.SetText("");
        _weatherText.enabled = false;
        _onGoingWeather = null;
    }

    private void Awake()
    {
        _weatherText = GetComponent<TMP_Text>();
        _weatherText.enabled = false;
    }
}

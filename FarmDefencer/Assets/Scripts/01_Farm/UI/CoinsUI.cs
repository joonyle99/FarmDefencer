using TMPro;
using UnityEngine;

public sealed class CoinsUI : MonoBehaviour
{
	private TMP_Text _coinsText;

	public void UpdateCoinText(int coin)
	{
		_coinsText.text = coin.ToString("N0");
	}

	private void Awake()
	{
		_coinsText = GetComponentInChildren<TMP_Text>();
	}
}

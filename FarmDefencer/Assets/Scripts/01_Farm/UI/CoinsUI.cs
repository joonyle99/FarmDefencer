using Spine.Unity;
using TMPro;
using UnityEngine;

public sealed class CoinsUI : MonoBehaviour
{
	private TMP_Text _text;
	private SkeletonGraphic _animation;

	public void SetCoin(int coin)
	{
		_text.text = coin.ToString("N0");
	}

	public void PlayAnimation()
	{
		_animation.AnimationState.AddAnimation(0, "coin_rotation", false, 0.0f);
	}

	private void Awake()
	{
		_text = GetComponentInChildren<TMP_Text>();
		_animation = GetComponentInChildren<SkeletonGraphic>();
	}
}

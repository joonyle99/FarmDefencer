using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 월드에 EffectPlayer를 가진 오브젝트를 배치하여 Singleton 접근할 것.
/// </summary>
public class EffectPlayer : MonoBehaviour
{
	private static EffectPlayer s_singleton;
	
	private Animator _interactEffectAnimator;
	private Animator _vfxAnimator;

	private bool _isHolding;
	private Dictionary<string, RuntimeAnimatorController> _vfxControllers;

	public static void PlayTabEffect(Vector2 worldPosition)
	{
		s_singleton._interactEffectAnimator.transform.position = worldPosition;
		s_singleton._interactEffectAnimator.SetTrigger("Play");
	}

	/// <summary>
	/// Hold 이펙트를 재생하려는 동안 매 프레임 호출할 것.
	/// <seealso cref="StopHoldEffect"/> 참조
	/// </summary>
	/// <param name="worldPosition"></param>
	public static void PlayHoldEffect(Vector2 worldPosition)
	{
		s_singleton._interactEffectAnimator.transform.position = worldPosition;
		s_singleton._interactEffectAnimator.SetTrigger("Play");
		s_singleton._interactEffectAnimator.SetBool("Looping", true);
		s_singleton._isHolding = true;
	}

	/// <summary>
	/// Hold()를 임의로 해제. Hold()를 매 프레임 호출하다가 특정 순간에 호출하지 않으면 자동으로 호출되는 메소드지만,
	/// 명시적으로 종료하고자 할 때 호출하는 메소드.
	/// </summary>
	public static void StopHoldEffect()
	{
		if (s_singleton._interactEffectAnimator.GetBool("Looping"))
		{
			s_singleton._interactEffectAnimator.ResetTrigger("Play");
			s_singleton._interactEffectAnimator.SetBool("Looping", false);
		}
	}

	/// <summary>
	/// </summary>
	/// <param name="name">Resources/Vfx에 위치한 Animation Controller의 이름.</param>
	/// <param name="worldPosition"></param>
	public static void PlayVfx(string name, Vector2 worldPosition)
	{
		s_singleton._vfxAnimator.runtimeAnimatorController = null;
		if (!s_singleton._vfxControllers.ContainsKey(name))
		{
			var loadedController = Resources.Load<RuntimeAnimatorController>($"Vfx/{name}");
			s_singleton._vfxControllers.Add(name, loadedController);
		}

		var controller = s_singleton._vfxControllers[name];
		if (controller == null)
		{
			Debug.LogError($"존재하지 않는 AnimationController for VFX: {name}");
			return;
		}

		s_singleton._vfxAnimator.transform.position = worldPosition;
		s_singleton._vfxAnimator.runtimeAnimatorController = controller;
	}

	private void Awake()
	{
		if (s_singleton != null)
		{
			throw new InvalidOperationException("EffectPlayer의 싱글톤 객체가 유효한 상황에서 Awake()가 다시 호출되었습니다.");
		}

		s_singleton = this;
		_vfxControllers = new Dictionary<string, RuntimeAnimatorController>();
		_interactEffectAnimator = transform.GetChild(0).GetComponent<Animator>();
		_vfxAnimator = transform.GetChild(1).GetComponent<Animator>();
	}

	private void Update()
	{
		if (!_isHolding)
		{
			StopHoldEffect();
		}
		_isHolding = false;
	}
}

using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// 월드에 EffectPlayer를 가진 오브젝트를 배치하여 Singleton 접근할 것.
/// </summary>
public sealed class EffectPlayer : MonoBehaviour
{
	[InfoBox("해당 씬에서 SceneGlobalInstance로 접근 가능한 인스턴스인지")]
	[SerializeField] private bool isSceneGlobalInstance;
	
	private Animator _interactEffectAnimator;
	private Animator _vfxAnimator;
	private SpriteRenderer _effectSpriteRenderer;
	private SpriteRenderer _vfxSpriteRenderer;
	private float _lastVfxDoneTime;

	private bool _isHolding;
	private Dictionary<string, RuntimeAnimatorController> _vfxControllers;
	private static readonly int Looping = Animator.StringToHash("Looping");
	private static readonly int Play = Animator.StringToHash("Play");
	private static readonly int Enter = Animator.StringToHash("Enter");
	
	public static EffectPlayer SceneGlobalInstance { get; private set; }

	public void PlayTapEffect(Vector2 worldPosition)
	{
		if (!gameObject.activeSelf)
		{
			return;
		}
		_interactEffectAnimator.transform.position = worldPosition;
		_interactEffectAnimator.Play(Enter, 0, 0.0f);
	}

	/// <summary>
	/// Hold 이펙트를 재생하려는 동안 매 프레임 호출할 것.
	/// <seealso cref="StopHoldEffect"/> 참조
	/// </summary>
	/// <param name="worldPosition"></param>
	public void PlayHoldEffect(Vector2 worldPosition)
	{
		if (!gameObject.activeSelf)
		{
			return;
		}
		_interactEffectAnimator.transform.position = worldPosition;
		_interactEffectAnimator.SetTrigger(Play);
		_interactEffectAnimator.SetBool(Looping, true);
		_isHolding = true;
	}

	/// <summary>
	/// Hold()를 임의로 해제. Hold()를 매 프레임 호출하다가 특정 순간에 호출하지 않으면 자동으로 호출되는 메소드지만,
	/// 명시적으로 종료하고자 할 때 호출하는 메소드.
	/// </summary>
	public void StopHoldEffect()
	{
		if (!gameObject.activeSelf)
		{
			return;
		}
		if (_interactEffectAnimator.GetBool(Looping))
		{
			_interactEffectAnimator.ResetTrigger(Play);
			_interactEffectAnimator.SetBool(Looping, false);
		}
	}
	
	/// <summary>
	/// </summary>
	/// <param name="name">Resources/Vfx에 위치한 Animation Controller의 이름.</param>
	/// <param name="worldPosition"></param>
	/// <param name="overwriteEvenIfSame">true일 경우, 동일한 VFX를 요청해도 다시 처음부터 재생.</param>
	public void PlayVfx(string name, Vector2 worldPosition, bool overwriteEvenIfSame = true)
	{
		if (!gameObject.activeSelf)
		{
			return;
		}
		if (!_vfxControllers.ContainsKey(name))
		{
			var loadedController = Resources.Load<RuntimeAnimatorController>($"_Vfx/{name}");
			_vfxControllers.Add(name, loadedController);
		}

		var controller = _vfxControllers[name];
		if (controller == null)
		{
			Debug.LogError($"존재하지 않는 AnimationController for VFX: {name}");
			return;
		}

		var currentTime = Time.time;

		if (currentTime >= _lastVfxDoneTime || overwriteEvenIfSame || controller != _vfxAnimator.runtimeAnimatorController)
		{
			_vfxAnimator.runtimeAnimatorController = null;
			_vfxAnimator.transform.position = worldPosition;
			_vfxAnimator.runtimeAnimatorController = controller;
			_lastVfxDoneTime = currentTime;
			if (controller.animationClips.Length > 0)
			{
				_lastVfxDoneTime += controller.animationClips[0].length;
			}
		}
	}

	/// <summary>
	/// 현재 재생중인 VFX가 있다면 중지.
	/// </summary>
	public void StopVfx()
	{
		if (!gameObject.activeSelf)
		{
			return;
		}
		_vfxAnimator.runtimeAnimatorController = null;
	}

	private void OnDisable()
	{
		_effectSpriteRenderer.sprite = null;
		_vfxSpriteRenderer.sprite = null;
	}

	private void Awake()
	{
		if (isSceneGlobalInstance)
		{
			SceneGlobalInstance = this;
		}
		
		_vfxControllers = new Dictionary<string, RuntimeAnimatorController>();
		_effectSpriteRenderer = transform.Find("InteractEffect").GetComponent<SpriteRenderer>();
		_interactEffectAnimator = transform.Find("InteractEffect").GetComponent<Animator>();
		_vfxSpriteRenderer = transform.Find("VFX").GetComponent<SpriteRenderer>();
		_vfxAnimator = transform.Find("VFX").GetComponent<Animator>();
	}

	private void FixedUpdate()
	{
		if (!_isHolding)
		{
			StopHoldEffect();
		}
		_isHolding = false;
	}
}

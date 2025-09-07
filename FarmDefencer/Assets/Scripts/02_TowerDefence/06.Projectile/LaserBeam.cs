using UnityEngine;
using Sirenix.OdinInspector;

/// <summary>
/// 일반적인 레이저 빔
/// 지속적인 데미지를 줍니다
/// </summary>
public sealed class LaserBeam : BeamBase
{
    //[Header("──────── Laser Beam ────────")]
    //[Space]

    private ColorEffect _colorEffect;
    private bool _isColorEffectApplied = false;

    private SlowEffector _slowEffector;
    private bool _isSlowEffectApplied = false;

    private AudioSource _audioSource;
    [VolumeControl("Defence")][BoxGroup("볼륨 조절")][Range(0f, 1f)] public float laserStayVolume = 0.5f;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }
    private void Start()
    {
        _audioSource.volume = laserStayVolume;
        _audioSource.loop = true;
        _audioSource.Play();
    }
    protected override void DealDamage()
    {
        damager.DealDamage(target, DamageType.Laser);
    }
    protected override void DealEffect()
    {
        //if (_isSlowEffectApplied == false)
        //{
        //    // 슬로우 효과 적용 (중복 적용 가능)
        //    _slowEffector = target.gameObject.AddComponent<SlowEffector>();
        //    _slowEffector.Activate(target, caster.CurrentLevelData.SlowRate, caster.CurrentLevelData.SlowDuration);

        //    _isSlowEffectApplied = true;
        //}

        if (_isColorEffectApplied == false)
        {
            // 컬러 이펙트 적용
            _colorEffect = new ColorEffect(ConstantConfig.PINK, 0f, true);
            target.SpineController.AddColorEffect(_colorEffect);

            _isColorEffectApplied = true;
        }
    }

    protected override void OnEndFunc()
    {
        //_isSlowEffectApplied = false;
        //_slowEffector.DeActivate();

        _isColorEffectApplied = false;
        target.SpineController.RemoveColorEffect(_colorEffect);
    }
}
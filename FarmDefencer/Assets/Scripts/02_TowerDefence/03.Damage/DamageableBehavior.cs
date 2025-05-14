using Spine;
using Spine.Unity;
using System.Collections;
using UnityEngine;

public enum Status
{
    NONE,
    BURN,
    SLOW,
    STUN,
    POISON,
}

/// <summary>
///
/// </summary>
public abstract class DamageableBehavior : MonoBehaviour
{
    [Header("──────── DamageableBehavior ────────")]
    [Space]

    [SerializeField] private ProgressBar _healthBar;
    public ProgressBar HealthBar => _healthBar;

    [Space]

    // status
    [SerializeField] private int _maxHp = 100;
    public int MaxHp => _maxHp;
    private int _hp;
    public int HP
    {
        get => _hp;
        set
        {
            _hp = value;

            //debug
            //var debugLabel = new GameObject("DebugLabel").AddComponent<DebugLabel>();
            //debugLabel.SetLabel(HP.ToString(), 1.0f, transform.position, Color.white);

            if (_hp <= 0)
            {
                //Debug.Log("hp is 0");
                _hp = 0;
                Kill();
            }

            if (_hp <= _maxHp / 2)
            {
                _healthBar.ChangeToDanger();
            }

            if (_healthBar != null)
            {
                _healthBar.UpdateProgressBar((float)_hp, (float)startHp);
                //Debug.Log("_healthBar.UpdateProgressBar");
            }
        }
    }
    [SerializeField] private float _stunDuration = 0.03f;
    public float StunDuration => _stunDuration;

    [Space]

    // state
    [SerializeField] private bool _isDead;
    public bool IsDead
    {
        get => _isDead;
        set
        {
            _isDead = value;
            spineController?.ResetColor();
        }
    }
    [SerializeField] private bool _isStun;
    public bool IsStun { get { return _isStun; } protected set { _isStun = value; } }

    // etc
    protected int startHp;
    public int StartHp => startHp;

    protected SpineController spineController;
    public SpineController SpineController => spineController;

    protected GridMovement gridMovement;
    public GridMovement GridMovement => gridMovement;

    //protected Dictionary<Status, Coroutine> activeEffects = new();
    protected Coroutine tickDamageCo;

    protected virtual void Awake()
    {
        var damageZone = GetComponent<DamageableZone>();
        if (damageZone == null)
        {
            throw new System.NullReferenceException($"You should add DamageZone component");
        }

        spineController = GetComponentInChildren<SpineController>();
        gridMovement = GetComponent<GridMovement>();

        startHp = MaxHp;
    }
    protected virtual void OnEnable()
    {
        IsDead = false;

        HP = MaxHp;
    }
    protected virtual void OnDisable()
    {
        if (tickDamageCo != null)
        {
            StopCoroutine(tickDamageCo);
            tickDamageCo = null;
        }
    }
    protected virtual void Start()
    {
        if (_healthBar != null)
        {
            _healthBar.UpdateProgressBar((float)_hp, (float)startHp);
            _healthBar.SetDangerousThreshold(_hp / _maxHp);
        }
    }

    public abstract void TakeDamage(int damage, ProjectileType type);
    public abstract void Kill();

    // stun
    public IEnumerator StunCo(float duration)
    {
        IsStun = true;
        yield return new WaitForSeconds(duration);
        IsStun = false;
    }

    // tick
    public void TakeTickDamage(int count, float interval, int damage, ProjectileType type)
    {
        // TODO: 중복이 될지 안될지를 확인해야 한다
        if (tickDamageCo != null)
        {
            StopCoroutine(tickDamageCo);
            tickDamageCo = null;
        }

        var newCo = StartCoroutine(TickDamageCo(count, interval, damage, type));
        tickDamageCo = newCo;
    }
    public IEnumerator TickDamageCo(int count, float interval, int damage, ProjectileType type)
    {
        spineController.SetColor(new Color(0.7f, 0f, 0f, 1f));

        for (int i = 0; i < count; i++)
        {
            if (IsDead)
            {
                tickDamageCo = null;
                yield break;
            }

            TakeDamage(damage, type);
            yield return new WaitForSeconds(interval);
        }

        spineController.ResetColor();
    }

    // effect
    // TODO: 일단 슬로우 전용 함수로 생성
    public void TakeEffect(float slowRate, float duration, ProjectileType type)
    {
        // do nothing
    }
}

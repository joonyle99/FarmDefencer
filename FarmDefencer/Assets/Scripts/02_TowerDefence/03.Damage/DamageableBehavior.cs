using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

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

    [SerializeField] private FloatingHealthBar _healthBar;

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
                _healthBar.ChangeToRedBar();
            }

            if (_healthBar != null)
            {
                _healthBar.UpdateHealthBar((float)_hp, (float)startHp);
                //Debug.Log("_healthBar.UpdateHealthBar");
            }
        }
    }
    [SerializeField] private float _stunDuration = 0.03f;
    public float StunDuration => _stunDuration;

    [Space]

    // state
    [SerializeField] private bool _isDead;
    public bool IsDead { get { return _isDead; } protected set { _isDead = value; } }
    [SerializeField] private bool _isStun;
    public bool IsStun { get { return _isStun; } protected set { _isStun = value; } }

    // etc
    protected int startHp;
    public int StartHp => startHp;

    protected SpineController spineController;
    public SpineController SpineController => spineController;

    protected Dictionary<Status, Coroutine> activeEffects = new();

    protected virtual void Awake()
    {
        var damageZone = GetComponent<DamageableZone>();
        if (damageZone == null)
        {
            throw new System.NullReferenceException($"You should add DamageZone component");
        }

        spineController = GetComponent<SpineController>();

        startHp = MaxHp;
    }
    protected virtual void OnEnable()
    {
        IsDead = false;

        HP = MaxHp;
    }
    protected virtual void Start()
    {
        if (_healthBar != null)
        {
            _healthBar.UpdateHealthBar((float)_hp, (float)startHp);
        }
    }

    public abstract void TakeDamage(int damage);
    public abstract void Kill();

    // stun
    public IEnumerator StunCo(float duration)
    {
        IsStun = true;
        yield return new WaitForSeconds(duration);
        IsStun = false;
    }

    // tick
    public void TakeTickDamage(Status status, int tickCount, float interval, int tickDamage)
    {
        // TODO: 중복이 될지 않될지를 확인해야 한다
        if (activeEffects.TryGetValue(status, out Coroutine co))
        {
            StopCoroutine(co);
            activeEffects[status] = null;
        }

        var newCo = StartCoroutine(TickDamageCo(status, tickCount, interval, tickDamage));
        activeEffects[status] = newCo;
    }
    public IEnumerator TickDamageCo(Status status, int tickCount, float interval, int tickDamage)
    {
        spineController.SetColor(Color.red);

        for (int i = 0; i < tickCount; i++)
        {
            if (IsDead)
            {
                spineController.ResetColor();
                activeEffects[status] = null;
                yield break;
            }

            TakeDamage(tickDamage);
            yield return new WaitForSeconds(interval);
        }

        spineController.ResetColor();
        activeEffects[status] = null;
    }
}

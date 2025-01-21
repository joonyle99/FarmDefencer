using System.Collections;
using UnityEngine;

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
    [SerializeField] private int _hp = 100;
    public int HP
    {
        get => _hp;
        set
        {
            _hp = value;

            if (_hp < 0)
            {
                _hp = 0;
                Kill();
            }

            if (_hp < 50)
            {
                _healthBar.ChangeToRedBar();
            }

            if (_healthBar != null)
            {
                _healthBar.UpdateHealthBar((float)_hp, (float)_startHp);
            }
        }
    }
    [SerializeField] private float _stunDuration = 0.1f;
    public float StunDuration => _stunDuration;

    [Space]

    // state
    [SerializeField] private bool _isDead;
    public bool IsDead { get { return _isDead; } protected set { _isDead = value; } }
    [SerializeField] private bool _isStun;
    public bool IsStun { get { return _isStun; } protected set { _isStun = value; } }

    // etc
    private int _startHp;
    public int StartHp => _startHp;

    protected virtual void Awake()
    {
        var damageZone = GetComponent<DamageableZone>();

        if (damageZone == null)
        {
            throw new System.NullReferenceException($"You should add DamageZone component");
        }

        _startHp = _hp;
    }
    protected virtual void OnEnable()
    {
        IsDead = false;
        HP = StartHp;
    }
    protected virtual void Start()
    {
        if (_healthBar != null)
        {
            _healthBar.UpdateHealthBar((float)_hp, (float)_startHp);
        }
    }

    public abstract void TakeDamage(int damage);
    public abstract void Kill();

    // status
    public IEnumerator StunRoutine(float duration)
    {
        IsStun = true;
        yield return new WaitForSeconds(duration);
        IsStun = false;
    }
}

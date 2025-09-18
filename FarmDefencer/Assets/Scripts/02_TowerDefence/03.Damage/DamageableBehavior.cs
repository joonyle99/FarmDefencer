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

    protected ProgressBar healthBar;
    public ProgressBar HealthBar => healthBar;

    // status
    protected int maxHp = 100;
    public int MaxHp => maxHp;
    private int _hp = 0;
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

            if (_hp <= maxHp / 2)
            {
                healthBar?.ChangeToDanger();
            }

            healthBar?.UpdateProgressBar((float)_hp, (float)MaxHp);
        }
    }

    // state
    private bool _isDead;
    public bool IsDead
    {
        get => _isDead;
        set
        {
            _isDead = value;
            spineController?.ResetColor();
        }
    }

    protected SpineController spineController;
    public SpineController SpineController => spineController;

    protected GridMovement gridMovement;
    public GridMovement GridMovement => gridMovement;

    private CircleCollider2D _damagableCollider;
    public CircleCollider2D DamagableColiider => _damagableCollider;

    protected Coroutine tickDamageCo;

    protected bool isActivated = false;
    public bool IsActivated => isActivated;

    protected virtual void Awake()
    {
        var damageZone = GetComponent<DamageableZone>();
        if (damageZone == null)
        {
            throw new System.NullReferenceException($"You should add DamageZone component");
        }

        healthBar = GetComponentInChildren<ProgressBar>();

        spineController = GetComponentInChildren<SpineController>();
        gridMovement = GetComponent<GridMovement>();
        _damagableCollider = GetComponent<CircleCollider2D>();
    }
    protected virtual void OnEnable()
    {
        IsDead = false;

        _hp = MaxHp;
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
        healthBar?.UpdateProgressBar((float)HP, (float)MaxHp);
        healthBar?.SetDangerousThreshold(0.5f);
    }

    // normal
    public abstract void TakeDamage(int damage, DamageType type);
    public abstract void Kill();

    // activate
    public void Activate()
    {
        isActivated = true;
    }
    public void Deactivate()
    {
        isActivated = false;
    }
}

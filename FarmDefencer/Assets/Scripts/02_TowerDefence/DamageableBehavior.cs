using UnityEngine;

/// <summary>
/// 
/// </summary>
public abstract class DamageableBehavior : MonoBehaviour
{
    [Header("式式式式式式式式 DamageableBehavior 式式式式式式式式")]
    [Space]

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
        }
    }

    private int _startHp;
    public int StartHp => _startHp;

    protected virtual void Awake()
    {
        var damageZone = GetComponent<DamageZone>();

        if (damageZone == null)
        {
            throw new System.NullReferenceException($"You should add DamageZone component");
        }

        _startHp = _hp;
    }
    protected virtual void Start()
    {

    }

    public abstract void TakeDamage(float damage);
    public abstract void Kill();
}

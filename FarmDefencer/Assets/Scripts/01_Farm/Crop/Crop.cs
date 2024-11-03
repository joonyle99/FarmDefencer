using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// �۹� �� ������ ������ ����ϴ� �߻� Ŭ�����Դϴ�. ���� ���۰�, ��ġ, ����, ���� ������ ���� �׸��� �⺻���� ���������� �����մϴ�.
/// �� �۹��� �� Ŭ������ ��ӹ޾� �ð��� ǥ��, ���� ����, ��ġ ���� ��ȣ �ۿ��� �����ؾ� �մϴ�.
/// <br/><br/>
/// <b>��ü���� ����:</b> �� Ŭ������ OnGrow()�� true�� ��ȯ�ϰ�, CropState�� Growing�̸�, WaterStored > 0.0f�� ��Ȳ�̶�� MatureAgeSeconds���� GrowthAgeSeconds�� ������Ű�� ������ �մϴ�. <br/><br/>
/// ����ϴ� Ŭ������ ������ �����ؾ� �� ���Դϴ�: <br/>
/// <list type="bullet">���� ������ ���� ������ ���� ��� OnGrow()�� �������̵�</list>
/// <list type="bullet">Update()���� �۹��� �ð��� ǥ���� ���¿� ���� ������Ʈ</list>
/// <list type="bullet">�۹��� ���� �����̶�� State�� Seed�� ����(Crop�� �۹��� State�� �����Ű�� ����)</list>
/// <list type="bullet">OnTap(), OnHoldAndUp()�� �������̵��� �۹� �ɱ�, ��Ȯ ���� ���� ����</list>
/// </summary>
public abstract class Crop : MonoBehaviour
{
	public FarmClock FarmClock;
	public ProductEntry ProductEntry;
	public UnityAction OnHarvest;
	protected CropState State
	{
		get
		{
			return _state;
		}
		set
		{
			if (value == CropState.Seed
				|| value == CropState.Planted)
			{
				_growthAgeSeconds = 0.0f;
				_waterStored = 0.0f;
				_waterWaitingSeconds = 0.0f;
			}
			else // Harvested
			{
				_growthAgeSeconds = MatureAgeSeconds;
			}
			_state = value;
		}
	}

	[SerializeField]
	private CropState _state;
	/// <summary>
	/// �۹��� ��Ȯ ������ ���±��� �ʿ��� ���� �ð� (GrowthAgeSeconds�� �� �̻��̾�� ��)
	/// </summary>
	public float MatureAgeSeconds;
	/// <summary>
	/// �۹��� ���������� ������ �� ���� (���� ���� ���� ����� �ð��� ������ ����)
	/// </summary>
	public float GrowthAgeSeconds
	{
		get
		{
			return _growthAgeSeconds;
		}
		private set
		{
			_growthAgeSeconds = value;
			if (_growthAgeSeconds >= MatureAgeSeconds)
			{
				_growthAgeSeconds = MatureAgeSeconds;
			}
		}
	}
	[SerializeField]
	private float _growthAgeSeconds;
	/// <summary>
	/// �� ���差 (��� �۹��� 1.0�ʴ� 1.0�� ���� �Һ�)
	/// �� �ִ� Ƚ�� ���� �����Ϸ��� ���� �ִ� ���� �ٸ��� �ϸ� �� ��
	/// </summary>
	public float WaterStored
	{
		get
		{
			return _waterStored;
		}
		set
		{
			_waterStored = value;
			if (_waterStored < 0.0f)
			{
				_waterStored = 0.0f;
			}
		}
	}
	[SerializeField]
	private float _waterStored;

	/// <summary>
	/// ���� ��� ��ٸ��� �־��� �� �ð�.
	/// ���� �ְ� ����(WaterStored += amount) 0.0���� �ʱ�ȭ�˴ϴ�.
	/// </summary>
	public float WaterWaitingSeconds => _waterWaitingSeconds;
	[SerializeField]
	private float _waterWaitingSeconds;
	/// <summary>
	/// �۹��� ���嵵�� ������� ��Ÿ���ϴ�.
	/// �� 10�� �� 5�ʰ� �����ٸ� 50.0f�Դϴ�.
	/// </summary>
	public float GrowthPercentage => MatureAgeSeconds <= 0.0f ? 100.0f : GrowthAgeSeconds / MatureAgeSeconds * 100.0f;
	public bool IsHarvestable => GrowthPercentage == 100.0f && State == CropState.Planted;
	public bool IsLocatedAt(Vector2 position) => Mathf.Abs(position.x - transform.position.x) < 0.5f && Mathf.Abs(position.y - transform.position.y) < 0.5f;
	/// <summary>
	/// �� Crop�� ���� XY ��ǥ�� ��ȯ�մϴ�. Ÿ���̱� ������ Vector2Int�� ��ȯ�մϴ�.
	/// </summary>
	public Vector2Int Position => new Vector2Int((int)transform.position.x, (int)transform.position.y);

	/// <summary>
	/// �� �۹��� ª�� ��ġ�Ǿ��� ���� ������ �����մϴ�.
	/// </summary>
	public virtual void OnTap() {}

	/// <summary>
	/// �� �۹��� ��� ������ ���� ���� ������ �����մϴ�.
	/// </summary>
	/// <param name="holdTime"></param>
	public virtual void OnHolding(float holdTime) {}
	
	/// <summary>
	/// �� �۹��� ���Ѹ����� �� �ֱ� ������ �������� ���� ������ �����մϴ�.
	/// </summary>
	public virtual void OnWatering() {}


	/// <summary>
	/// �۹��� ���� �� �����Ӵ� ȣ��(Crop.Update()����)�Ǹ�, Ư���� ���� ������ �˻��ϴ� �޼ҵ��Դϴ�.
	/// �۹��� ���� ������ ���� ������ ��� �ٸ� �� �ִµ�, �̸� �����ϱ� ���� �޼ҵ��Դϴ�.
	/// <br/><br/><b>�� �������� ���⼭ �˻����� ������.</b>
	/// <br/><br/>
	/// State�� Growing�� �ƴ϶�� ȣ����� �ʽ��ϴ�.
	/// </summary>
	/// <returns>������ �����Ǿ��ٸ� true</returns>
	protected virtual bool OnGrow(float deltaTime) => true;
	public abstract bool IsDead();
	public abstract bool IsGrowing();
	public override string ToString() => $"{this.GetType()} at {Position}, age {GrowthAgeSeconds}, water waiting {WaterWaitingSeconds}, state {State}";

	protected enum CropState
	{
		Seed, // ��ġ�Ͽ� Planted ���°� �Ǳ� ��ٸ��� ����
		Planted, // �������̰ų�, �׾��ų� �ϴ� ����
		Harvested // 100% ������ �۹��� ��ġ�� ����, �ٽ� ��ġ�Ͽ� ���ڷ� �Ű����� ��ٸ��� ����
	}

	protected virtual void Awake() { }
	protected virtual void Start() { }
	protected virtual void Update()
	{
		if (FarmClock.Paused
			|| State == CropState.Seed
			|| State == CropState.Harvested
			|| _growthAgeSeconds >= MatureAgeSeconds)
		{
			return;
		}

		var deltaTime = Time.deltaTime;

		if (_waterStored < deltaTime)
		{
			_waterStored = 0.0f;
			_waterWaitingSeconds += deltaTime;
			return;
		}
		_waterWaitingSeconds = 0.0f;
		if (OnGrow(deltaTime))
		{
			GrowthAgeSeconds += deltaTime;
			WaterStored -= deltaTime;
		}
	}
}
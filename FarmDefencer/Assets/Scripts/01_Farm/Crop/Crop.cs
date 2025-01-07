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
public abstract class Crop : MonoBehaviour, IFarmUpdatable
{
	public ProductEntry ProductEntry;

	private UnityAction<UnityAction<bool>> _onTryItemifyAction;
	public CropState State
	{
		get
		{
			return _state;
		}
		protected set
		{
			if (_state == value)
			{
				return;
			}
			if (value == CropState.Seed
				|| value == CropState.Planted)
			{
				_growthAgeSeconds = 0.0f;
				_waterStored = 0.0f;
				_waterWaitingSeconds = 0.0f;
				_lockRemainingSeconds = 0.0f;
			}
			else if (value == CropState.Harvested)
			{
				_lockRemainingSeconds = 0.0f;
				_growthAgeSeconds = MatureAgeSeconds;
			}
			else if (value == CropState.Locked)
			{
				_lockRemainingSeconds = LockSeconds;
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
	/// �۹��� �г�Ƽ �ð� ������
	/// </summary>
	public float LockSeconds = 60.0f;
	/// <summary>
	/// �۹��� ���� �г�Ƽ �ð�
	/// </summary>
	public float LockRemainingSeconds
	{
		get
		{
			return _lockRemainingSeconds;
		}
		private set
		{
			_lockRemainingSeconds = value;
			if (_lockRemainingSeconds < 0.0f)
			{
				_lockRemainingSeconds = 0.0f;
			}
		}
	}
	[SerializeField]
	private float _lockRemainingSeconds;
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
	/// ������ OnFarmUpdate()�� 0.0�̾����� true, �̿��� ��� false�� ���õ˴ϴ�.
	/// </summary>
	protected bool Paused { get; private set; }

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
	private CropLockedDisplay _cropLockedDisplay;

	/// <summary>
	/// �� �۹��� ª�� ��ġ�Ǿ��� ���� ������ �����մϴ�.
	/// </summary>
	public virtual void OnSingleTap() {}

	/// <summary>
	/// �� �۹��� ���� Single Holding ������ �����մϴ�.
	/// </summary>
	/// <param name="deltaPosition">ù Ȧ�� ��ġ�� ���� Ȧ�� ��ġ�� �����Դϴ�.</param>
	/// <param name="isEnd">Ȧ�尡 ����Ǿ� ������ �׼��� ��� true�Դϴ�.</param>
	/// <param name="holdTime"></param>
	public virtual void OnSingleHolding(Vector2 deltaPosition, bool isEnd, float deltaHoldTime) {}
	
	/// <summary>
	/// �� �۹��� ���Ѹ����� �� �ֱ� ������ �������� ���� ������ �����մϴ�.
	/// </summary>
	public virtual void OnWatering() {}

	/// <summary>
	/// �۹��� ������ȭ �õ��մϴ�. State�� Harvested�� �ƴ϶�� �ƹ� �ϵ� ���� �ʽ��ϴ�.
	/// OnTryItemify&lt;afterItemify&lt;bool&gt;&gt; �̺�Ʈ�� �߻���Ű��, afterItemify �ݹ����δ� bool�� true�� ��� �۹��� ���� ���·� �ǵ����� �۾��� ���޵˴ϴ�.
	/// </summary>
	protected void Itemify()
	{
		if (State != CropState.Harvested)
		{
			return;
		}

		_onTryItemifyAction(
		(isItemified) =>
		{
			if (isItemified)
			{
				State = CropState.Seed;
			}
		});
	}

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

	public void Init(GameObject cropLockedDisplayPrefab, UnityAction<UnityAction<bool>> onTryItemifyAction)
	{
		var cropLockedDisplayObject = Instantiate(cropLockedDisplayPrefab);
		cropLockedDisplayObject.transform.SetParent(transform, false);
		cropLockedDisplayObject.transform.localPosition = new Vector2(0.0f, 0.0f);

		_cropLockedDisplay = cropLockedDisplayObject.GetComponent<CropLockedDisplay>();
		_cropLockedDisplay.Crop = this;
		_onTryItemifyAction = onTryItemifyAction;
	}

	public enum CropState
	{
		Seed, // ��ġ�Ͽ� Planted ���°� �Ǳ� ��ٸ��� ����
		Planted, // �������̰ų�, �׾��ų� �ϴ� ����
		Harvested, // 100% ������ �۹��� ��ġ�� ����, �ٽ� ��ġ�Ͽ� ���ڷ� �Ű����� ��ٸ��� ����
		Locked // �۹� ���� �г�Ƽ ������ �ɱ� ������ ��� ����
	}

	protected virtual void Awake() {}
	protected virtual void Start() {}
	
	public virtual void OnFarmUpdate(float deltaTime)
	{
		Paused = deltaTime == 0.0f;
		if (_cropLockedDisplay != null)
		{
			_cropLockedDisplay.OnFarmUpdate(deltaTime);
		}

		if (State == CropState.Locked)
		{
			LockRemainingSeconds -= deltaTime;
			return;
		}

		if (_waterStored < deltaTime)
		{
			_waterStored = 0.0f;
			_waterWaitingSeconds += deltaTime;
			return;
		}

		if (OnGrow(deltaTime))
		{
			GrowthAgeSeconds += deltaTime;
			WaterStored -= deltaTime;
		}
	}
}
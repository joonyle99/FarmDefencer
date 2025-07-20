using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ChainLightning : MonoBehaviour
{
    private LineRenderer _lineRenderer;

    private Tower _caster;
    private List<DamageableBehavior> _targetsToApply;

    private int _damage;

    private List<DamageableBehavior> _appliedTargets = new List<DamageableBehavior>();
    public List<DamageableBehavior> AppliedTargets => _appliedTargets;

    public int MaxCount = 3;

    private void Awake()
    {
        _lineRenderer = GetComponent<LineRenderer>();
    }
    private void Update()
    {
        // 유효한 타겟에 대해서 라인 렌더러의 각 포인트 위치를 업데이트한다
        for (int i = 0; i < _appliedTargets.Count; i++)
        {
           var appliedTarget = _appliedTargets[i];
           if (appliedTarget == null || appliedTarget.gameObject.activeInHierarchy == false || appliedTarget.IsDead == true)
           {
                continue;
           }

            // index: 0은 caster의 위치이므로 제외한다
            _lineRenderer.SetPosition(i + 1, appliedTarget.transform.position);
        }
    }

    public void Initialize(Tower caster, List<DamageableBehavior> targets, int damage)
    {
        _lineRenderer.positionCount = 1;
        _lineRenderer.SetPosition(0, caster.transform.position);

        _caster = caster;
        _targetsToApply = targets;

        _damage = damage;

        StartCoroutine(ChainLightningRoutine());
    }

    private IEnumerator ChainLightningRoutine()
    {
        foreach (var target in _targetsToApply)
        {
            if (target == null || target.gameObject.activeInHierarchy == false || target.IsDead == true)
            {
                continue;
            }

            if (_appliedTargets.Contains(target))
            {
                continue;
            }

            // 데미지 적용
            target.TakeDamage(_damage, DamageType.Lightning);

            // 라이트닝 효과 적용
            var lightningEffector = target.gameObject.AddComponent<LightningEffector>();
            lightningEffector.Activate(target);

            // 라인 렌더러에 타겟 추가
            _appliedTargets.Add(target);
            _lineRenderer.positionCount += 1;
            _lineRenderer.SetPosition(_appliedTargets.Count, target.transform.position);

            // 다음 전이까지 0.1초 대기
            yield return new WaitForSeconds(0.1f);
        }

        // TODO: 모든 타겟에 적용 후, 잠시 후 오브젝트 파괴(잔상 효과 등 필요시 시간 조절)
        yield return new WaitForSeconds(0.5f);
        Destroy(gameObject);
    }
}

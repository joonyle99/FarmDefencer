using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;

public class ChainLightning : MonoBehaviour
{
    private LineRenderer _lineRenderer;

    private Tower _caster;
    private List<TargetableBehavior> _targetsToApply;

    private List<int> _damageList;

    private List<TargetableBehavior> _appliedTargets = new List<TargetableBehavior>();
    public List<TargetableBehavior> AppliedTargets => _appliedTargets;

    private AudioSource _audioSource;
    [SerializeField] private List<AudioClip> _audioClips;
    [VolumeControl("Defence")][BoxGroup("볼륨 조절")][Range(0f, 1f)] public float lightningVolume = 0.5f;

    private void Awake()
    {
        _lineRenderer = GetComponent<LineRenderer>();
        _audioSource = GetComponent<AudioSource>();
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
            _lineRenderer.SetPosition(i + 1, appliedTarget.TargetPoint.position);
        }
    }

    public void Initialize(Tower caster, List<TargetableBehavior> targets, int damage)
    {
        _lineRenderer.positionCount = 1;
        _lineRenderer.SetPosition(0, caster.transform.position);

        _caster = caster;
        _targetsToApply = targets;
        _damageList = new List<int>(targets.Count);

        float rate = 0.6f;

        for (int i = 0; i < targets.Count; i++)
        {
            var _damage = 0;

            if (i == 0)
            {
                _damage = Mathf.FloorToInt(damage * 0.5f);
            }
            else
            {
                _damage = Mathf.Max(Mathf.FloorToInt(_damageList[i - 1] * rate), 1);
                rate = Mathf.Max(rate - 0.2f, 0f);
            }

            _damageList.Add(_damage);
        }

        StartCoroutine(ChainLightningRoutine());
    }

    private IEnumerator ChainLightningRoutine()
    {
        for (int i = 0; i < _targetsToApply.Count; i++)
        {
            var target = _targetsToApply[i];

            if (target == null || target.gameObject.activeInHierarchy == false || target.IsDead == true)
            {
                continue;
            }

            if (_appliedTargets.Contains(target))
            {
                continue;
            }

            // 데미지 적용
            var damageIdx = Mathf.Clamp(i, 0, _damageList.Count);
            var damageValue = _damageList[damageIdx];
            target.TakeDamage(damageValue, DamageType.Lightning);

            // 라이트닝 효과 적용
            var lightningEffector = target.gameObject.AddComponent<LightningEffector>();
            lightningEffector.Activate(target);

            // 사운드 재생
            // 5-1, 5-2, 5-3, 5-4를 출력하고 그 이후에는 항상 5-4출력
            if (_audioClips != null && _audioClips.Count > 0)
            {
                var audioIdx = Mathf.Clamp(i, 0, _audioClips.Count - 1);
                var audioClip = _audioClips[audioIdx];
                _audioSource.PlayOneShot(audioClip, lightningVolume);
            }

            // 라인 렌더러에 타겟 추가
            _appliedTargets.Add(target);
            _lineRenderer.positionCount += 1;
            _lineRenderer.SetPosition(_appliedTargets.Count, target.TargetPoint.position);

            // 다음 전이까지 0.1초 대기
            yield return new WaitForSeconds(0.1f);
        }

        // TODO: 모든 타겟에 적용 후, 잠시 후 오브젝트 파괴(잔상 효과 등 필요시 시간 조절)
        yield return new WaitForSeconds(0.5f);
        Destroy(gameObject);
    }
}

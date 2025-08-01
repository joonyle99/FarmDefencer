using Spine;
using Spine.Unity;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using TMPro;

public class ColorEffect
{
    public Color color = Color.white;
    public float duration = 0f;
    public float eTime = 0f;
    public bool isContinuous = false;

    public ColorEffect(Color color, float duration, bool isContinuous = false)
    {
        this.color = color;
        this.duration = duration;
        this.isContinuous = isContinuous;
    }
}

public class SpineController : MonoBehaviour
{
    [Header("──────── SpineController ────────")]
    [Space]

    // attribute
    private SkeletonAnimation _skeletonAnimation;
    private Spine.AnimationState _spineAnimationState;
    private Spine.Skeleton _skeleton;

    // property
    public SkeletonAnimation SkeletonAnimation => _skeletonAnimation;
    public Spine.AnimationState SpineAnimationState => _spineAnimationState;
    public Spine.Skeleton Skeleton => _skeleton;

    private Color _originalColor;
    private Dictionary<int, Bone> _cachedShootingBones;

    // color effect
    private List<ColorEffect> _colorEffectList = new List<ColorEffect>();
    private ColorEffect _curColorEffect;

    private DamageableBehavior _damagableBehavior;

    private void Awake()
    {
        //
        _skeletonAnimation = GetComponent<SkeletonAnimation>();
        _spineAnimationState = _skeletonAnimation.AnimationState;
        _skeleton = _skeletonAnimation.Skeleton;

        //
        _originalColor = _skeletonAnimation.Skeleton.GetColor();

        //
        InitShootingBone();

        //
        _damagableBehavior = GetComponent<DamageableBehavior>() ?? GetComponentInParent<DamageableBehavior>();
    }
    private void Update()
    {
        // 비활성화 되어었지 않으면 종료
        if (_damagableBehavior != null && _damagableBehavior.IsActivated == false)
        {
            return;
        }

        // 적용할 컬러 이펙트가 없다면 원래 색상으로 복구
        if (_colorEffectList.Count == 0)
        {
            if (!IsOriginalColor())
            {
                ResetColor();
            }

            _curColorEffect = null;

            return;
        }

        // 가장 최근에 추가된 컬러 이펙트를 사용
        var topColorEffect = _colorEffectList[_colorEffectList.Count - 1];
        if (topColorEffect != _curColorEffect)
        {
            _curColorEffect = topColorEffect;

            // 색상이 같을 경우 중복 처리를 방지
            if (IsDifferentColor(topColorEffect.color))
            {
                SetColor(topColorEffect.color);
            }
        }

        // 모든 컬러 이펙트 시간을 업데이트
        // 컬러 이펙트 시간이 종료되면 리스트에서 제거
        for (int index = 0; index < _colorEffectList.Count; index++)
        {
            var colorEffect = _colorEffectList[index];

            // 시간 업데이트를 하지 않는 경우 (e.g 타워 4의 레이저)
            if (colorEffect.isContinuous)
            {
                continue;
            }

            colorEffect.eTime += Time.deltaTime;
            if (colorEffect.eTime >= colorEffect.duration)
            {
                _colorEffectList.RemoveAt(index);

                // 처음, 중간, 마지막 중 어떤걸 제거하든 유효한 인덱스 조정
                index--;
            }
        }
    }

    public void SetAnimation(string animationName, bool loop)
    {
        // 현재 애니메이션이 이미 실행 중이라면 중복 실행하지 않는다
        if (_skeletonAnimation.AnimationName == animationName) return;
        _spineAnimationState.SetAnimation(0, animationName, loop);
    }
    public void AddAnimation(string animationName, bool loop, float delay = 0f)
    {
        _spineAnimationState.AddAnimation(0, animationName, loop, delay);
    }

    public Color GetColor()
    {
        if (!_skeletonAnimation)
        {
            return Color.white;
        }

        return _skeletonAnimation.Skeleton.GetColor();
    }
    public void SetColor(Color color)
    {
        if (!_skeletonAnimation)
        {
            return;
        }
        _skeletonAnimation.Skeleton.SetColor(color);
    }
    public void ResetColor()
    {
        if (!_skeletonAnimation)
        {
            return;
        }

        _skeletonAnimation.Skeleton.SetColor(_originalColor);
    }
    public bool IsOriginalColor()
    {
        return GetColor() == _originalColor;
    }
    public bool IsDifferentColor(Color color)
    {
        return GetColor() != color;
    }
    public void AddColorEffect(ColorEffect colorEffect)
    {
        _colorEffectList.Add(colorEffect);
    }
    public void RemoveColorEffect(ColorEffect colorEffect)
    {
        _colorEffectList.Remove(colorEffect);
    }

    public void InitShootingBone()
    {
        _cachedShootingBones = new Dictionary<int, Bone>();

        // 기본 shoot
        Bone defaultBone = GetBone("shoot");
        if (defaultBone != null)
        {
            _cachedShootingBones[0] = defaultBone;
        }

        // 레벨별 shoot
        for (int i = 1; i <= 3; i++)
        {
            Bone levelBone = GetBone($"shoot_Lv{i}");
            if (levelBone != null)
            {
                _cachedShootingBones[i] = levelBone;
            }
        }
    }
    public Bone GetBone(string boneName)
    {
        return _skeleton.FindBone(boneName);
    }
    public Bone GetShootingBone(int towerLevel)
    {
        Bone bone;

        // 우선 기본 shoot 본 사용
        if (_cachedShootingBones.TryGetValue(0, out bone))
        {
            return bone;
        }

        // towerLevel에 해당하는 shoot_Lv 본이 있으면 사용
        if (_cachedShootingBones.TryGetValue(towerLevel, out bone))
        {
            return bone;
        }

        Debug.LogError($"타워 레벨 {towerLevel}에 해당하는 shoot bone이 없습니다");
        return null; // fallback
    }
    public Vector3 GetShootingBonePos(int towerLevel)
    {
        Bone bone;

        // 우선 기본 shoot 본 사용
        if (_cachedShootingBones.TryGetValue(0, out bone))
        {
            return bone.GetWorldPosition(transform);
        }

        // towerLevel에 해당하는 shoot_Lv 본이 있으면 사용
        if (_cachedShootingBones.TryGetValue(towerLevel, out bone))
        {
            return bone.GetWorldPosition(transform);
        }

        Debug.LogError($"타워 레벨 {towerLevel}에 해당하는 shoot bone이 없습니다");
        return transform.position; // fallback
    }
}

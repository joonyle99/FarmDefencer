using Spine;
using Spine.Unity;
using System.Collections.Generic;
using UnityEngine;

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
        return _skeletonAnimation.Skeleton.GetColor();
    }
    public void SetColor(Color color)
    {
        _skeletonAnimation.Skeleton.SetColor(color);
    }
    public void ResetColor()
    {
        _skeletonAnimation.Skeleton.SetColor(_originalColor);
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
        return null; // fallback`
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

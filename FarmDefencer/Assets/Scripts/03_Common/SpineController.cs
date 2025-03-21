using Spine;
using Spine.Unity;
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

    private void Awake()
    {
        _skeletonAnimation = GetComponent<SkeletonAnimation>();

        _spineAnimationState = _skeletonAnimation.AnimationState;
        _skeleton = _skeletonAnimation.Skeleton;

        _originalColor = _skeletonAnimation.Skeleton.GetColor();
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

    public Bone GetBone(string boneName)
    {
        return _skeletonAnimation.Skeleton.FindBone(boneName);
    }
}

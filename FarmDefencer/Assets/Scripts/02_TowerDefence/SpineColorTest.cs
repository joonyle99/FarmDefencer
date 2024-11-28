using Spine;
using Spine.Unity;
using UnityEngine;

public class SpineColorTest : MonoBehaviour
{
    private SkeletonAnimation _skeletonAnimation;
    private Spine.Skeleton _skeleton;

    private void Awake()
    {
        _skeletonAnimation = GetComponent<SkeletonAnimation>();
        _skeleton = _skeletonAnimation.Skeleton;
    }
    private void Start()
    {
        _skeleton.A = 0f;
    }
}

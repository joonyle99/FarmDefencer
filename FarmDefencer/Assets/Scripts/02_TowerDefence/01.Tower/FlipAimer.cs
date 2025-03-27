using UnityEngine;

public class FlipAimer : MonoBehaviour
{
    public void FlipAim(Vector3 targetPosition)
    {
        var dirVec = (targetPosition - transform.position).normalized;

        // flip aim
        var dir = Mathf.Sign(dirVec.x);
        var localScale = transform.localScale;
        var targetScaleX = dir * Mathf.Abs(localScale.x);
        localScale.x = targetScaleX;
        transform.localScale = localScale;
    }
}

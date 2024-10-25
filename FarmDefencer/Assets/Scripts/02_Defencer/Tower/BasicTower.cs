using System.Linq;
using UnityEngine;

public sealed class BasicTower : Tower
{
    protected override void Attack()
    {
        var allTargets = TargetDetector.Targets.ToArray<Target>();

        // + TargetingStrategy를 이용해 타겟을 선택하는 로직 추가

        foreach (var target in allTargets)
        {
            var diffVec = target.transform.position - FirePoint.position;
            var dirVec = diffVec.normalized;

            Debug.DrawRay(FirePoint.position, diffVec, Color.red, 0.1f);

            float angle = Mathf.Atan2(dirVec.y, dirVec.x) * Mathf.Rad2Deg;
            Quaternion rotation = Quaternion.Euler(0f, 0f, angle);

            var bullet = Instantiate(Bullet, FirePoint.position, rotation);

            bullet.SetDamage(10);
            bullet.SetSpeed(15f);
            bullet.SetTarget(target);   // should set target before shoot

            bullet.Shoot();
        }

        Debug.Log("BasicTower Attack");
    }
}

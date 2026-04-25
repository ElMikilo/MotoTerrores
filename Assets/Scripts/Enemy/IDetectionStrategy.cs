using UnityEngine;

public interface IDetectionStrategy
{
    bool Detect(Transform target, out Vector3 targetPos);
    void Initialize(EnemyMonster owner);
}

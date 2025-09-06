using System;
using UnityEngine;

public partial class PenaltyGiver
{
    private class EatingMonster
    {
        private Monster _monster;
        private float _eatenTime;

        public bool IsDone => _eatenTime >= AnimationEndTime;

        public Vector2 EatingPosition => _monster.transform.position;
        
        public EatingMonster(Monster monster)
        {
            _monster = monster;
            _eatenTime = 0.0f;
        }

        public void UpdateAnimation(float deltaTime)
        {
            _eatenTime += deltaTime;
            if (_eatenTime >= AnimationEndTime)
            {
                _monster.SpineController.Skeleton.A = 0.0f;
            }
            else if (_eatenTime >= AnimationFadeOutBeginTime)
            {
                var alpha = Mathf.Clamp((AnimationEndTime - _eatenTime) / (AnimationEndTime - AnimationFadeOutBeginTime), 0.0f, 1.0f);
                _monster.SpineController.Skeleton.A = alpha;
            }
        }

        public void DestroyMonster()
        {
            Destroy(_monster.gameObject);
        }
    }

    private class CropLocker
    {
        public float RemainingTime { get; private set; }

        public bool IsDone => RemainingTime <= 0.0f;

        public Vector2 LockPosition { get; }
        private Action<Vector2, float> _updateGauge;

        public CropLocker(Vector2 cropWorldPosition, float lockTime, Action<Vector2, float> updateGauge)
        {
            LockPosition = cropWorldPosition;
            RemainingTime = lockTime;
            _updateGauge = updateGauge;
        }

        public void UpdateLock(float deltaTime)
        {
            RemainingTime -= deltaTime;
            _updateGauge(LockPosition, Mathf.Clamp01(RemainingTime / CropLockTime));
        }
    }
}
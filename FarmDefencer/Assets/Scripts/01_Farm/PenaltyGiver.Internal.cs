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
        private CropLock _cropLock;
        private float _remainingTime;

        public bool IsDone => _remainingTime <= 0.0f;

        public Vector2 LockPosition => _cropLock.transform.position;

        public CropLocker(CropLock cropLock)
        {
            _cropLock = cropLock;
            _remainingTime = CropLockTime;
        }

        public void UpdateLock(float deltaTime)
        {
            _remainingTime -= deltaTime;
            _cropLock.UpdateGauge(Mathf.Clamp01(_remainingTime / CropLockTime));
        }

        public void DestroyLock()
        {
            Destroy(_cropLock.gameObject);
        }
    }
}
using UnityEngine;

namespace DungeonCrawler.Core.Enemies
{
    public sealed class EnemyAttackAnimationEvents : MonoBehaviour
    {
        private EnemyBase _enemy;

        private void Awake()
        {
            _enemy = GetComponentInParent<EnemyBase>();
        }

        public void ExecuteAttackAnimationEvent()
        {
            _enemy?.ExecuteAttackAnimationEvent();
        }

        public void FinishAttackAnimation()
        {
            _enemy?.FinishAttackAnimation();
        }
    }
}

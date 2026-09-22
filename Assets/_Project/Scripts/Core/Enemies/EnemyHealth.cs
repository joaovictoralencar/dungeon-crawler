using UnityEngine;
using DungeonCrawler.Core.Combat;

namespace DungeonCrawler.Core.Enemies
{
    public sealed class EnemyHealth : Health
    {
        private EnemyBase _enemy;
        private Collider _collider;

        protected override void Awake()
        {
            base.Awake();
            _enemy = GetComponent<EnemyBase>();
            _collider = GetComponent<Collider>();
        }

        protected override void Die()
        {
            base.Die();

            _enemy.enabled = false;
            _collider.enabled = false;

            if (_enemy.TryGetComponent(out UnityEngine.AI.NavMeshAgent agent))
                agent.enabled = false;
        }
    }
}

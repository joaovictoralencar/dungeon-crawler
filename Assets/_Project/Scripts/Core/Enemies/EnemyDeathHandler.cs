using UnityEngine;
using UnityEngine.AI;

namespace DungeonCrawler.Core.Enemies
{
    public sealed class EnemyDeathHandler : MonoBehaviour
    {
        private const float DeathDespawnDelay = 3f;

        private EnemyHealth _health;
        private EnemyBase _enemy;
        private Collider _collider;
        private NavMeshAgent _agent;

        private void Awake()
        {
            _health = GetComponent<EnemyHealth>();
            _enemy = GetComponent<EnemyBase>();
            _collider = GetComponent<Collider>();
            _agent = GetComponent<NavMeshAgent>();
        }

        private void OnEnable()
        {
            _health.Died += OnDied;
        }

        private void OnDisable()
        {
            _health.Died -= OnDied;
        }

        private void OnDied()
        {
            _enemy.enabled = false;
            _collider.enabled = false;
            _agent.enabled = false;
            Destroy(gameObject, DeathDespawnDelay);
        }
    }
}

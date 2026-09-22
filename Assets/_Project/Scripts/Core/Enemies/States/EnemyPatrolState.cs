using UnityEngine;
using UnityEngine.AI;

namespace DungeonCrawler.Core.Enemies
{
    public sealed class EnemyPatrolState : IEnemyState
    {
        private readonly EnemyBase _enemy;
        private float _waitTimer;

        public EnemyPatrolState(EnemyBase enemy) => _enemy = enemy;

        public void Enter()
        {
            _waitTimer = 0f;
            SetNextDestination();
        }

        public void Tick()
        {
            if (_enemy.HasTarget())
            {
                _enemy.ChangeState(new EnemyChaseState(_enemy));
                return;
            }

            if (!_enemy.HasReachedDestination())
                return;

            _waitTimer += Time.deltaTime;
            if (_waitTimer >= _enemy.PatrolWait)
                SetNextDestination();
        }

        public void Exit() => _enemy.StopMoving();

        private void SetNextDestination()
        {
            _waitTimer = 0f;
            Vector3 randomOffset = Random.insideUnitSphere * _enemy.PatrolRadius;
            randomOffset.y = 0f;

            if (NavMesh.SamplePosition(_enemy.transform.position + randomOffset, out NavMeshHit hit,
                    _enemy.PatrolRadius, NavMesh.AllAreas))
                _enemy.MoveTo(hit.position);
        }
    }
}

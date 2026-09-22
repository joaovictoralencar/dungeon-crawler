using UnityEngine;

namespace DungeonCrawler.Core.Enemies
{
    public sealed class EnemyIdleState : IEnemyState
    {
        private readonly EnemyBase _enemy;

        public EnemyIdleState(EnemyBase enemy) => _enemy = enemy;

        public void Enter() => _enemy.StopMoving();

        public void Tick()
        {
            if (_enemy.HasTarget())
            {
                _enemy.ChangeState(new EnemyChaseState(_enemy));
                return;
            }

            if (_enemy.ShouldPatrol)
                _enemy.ChangeState(new EnemyPatrolState(_enemy));
        }

        public void Exit() { }
    }
}

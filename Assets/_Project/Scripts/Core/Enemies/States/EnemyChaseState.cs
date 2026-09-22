namespace DungeonCrawler.Core.Enemies
{
    public sealed class EnemyChaseState : IEnemyState
    {
        private readonly EnemyBase _enemy;

        public EnemyChaseState(EnemyBase enemy) => _enemy = enemy;

        public void Enter() { }

        public void Tick()
        {
            if (!_enemy.HasTarget())
            {
                _enemy.ChangeState(_enemy.ShouldPatrol
                    ? new EnemyPatrolState(_enemy)
                    : new EnemyIdleState(_enemy));
                return;
            }

            if (_enemy.IsInAttackRange())
            {
                _enemy.ChangeState(new EnemyAttackState(_enemy));
                return;
            }

            _enemy.MoveTo(_enemy.Target.position);
        }

        public void Exit() => _enemy.StopMoving();
    }
}

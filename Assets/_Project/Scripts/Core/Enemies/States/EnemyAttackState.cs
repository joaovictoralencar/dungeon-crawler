namespace DungeonCrawler.Core.Enemies
{
    public sealed class EnemyAttackState : IEnemyState
    {
        private readonly EnemyBase _enemy;

        public EnemyAttackState(EnemyBase enemy) => _enemy = enemy;

        public void Enter() => _enemy.StopMoving();

        public void Tick()
        {
            if (!_enemy.HasTarget() || !_enemy.IsInAttackRange())
            {
                _enemy.ChangeState(new EnemyChaseState(_enemy));
                return;
            }

            _enemy.FaceTarget();
            if (!_enemy.IsAttacking)
                _enemy.BeginAttack();
        }

        public void Exit() => _enemy.StopMoving();
    }
}

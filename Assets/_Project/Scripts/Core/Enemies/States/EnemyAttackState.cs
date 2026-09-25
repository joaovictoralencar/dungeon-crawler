namespace DungeonCrawler.Core.Enemies
{
    public sealed class EnemyAttackState : IEnemyState
    {
        private readonly EnemyBase _enemy;
        private float _anticipationTimer;

        public EnemyAttackState(EnemyBase enemy) => _enemy = enemy;

        public void Enter()
        {
            _anticipationTimer = _enemy.AttackAnticipation;
            _enemy.AttackFinished += OnAttackFinished;
            _enemy.Health.Damaged.AddListener(OnDamaged);
            _enemy.StopMoving();
        }

        public void Tick()
        {
            if (_enemy.IsAttacking)
            {
                _enemy.FaceTarget();
                return;
            }

            if (!_enemy.HasTarget() || !_enemy.IsInAttackRange())
            {
                _enemy.ChangeState(new EnemyChaseState(_enemy));
                return;
            }

            _enemy.FaceTarget();
            if (_anticipationTimer > 0f)
            {
                _anticipationTimer -= UnityEngine.Time.deltaTime;
                return;
            }

            if (_enemy.CanAttack)
                _enemy.BeginAttack();
        }

        public void Exit()
        {
            _enemy.AttackFinished -= OnAttackFinished;
            _enemy.Health.Damaged.RemoveListener(OnDamaged);
            _enemy.StopMoving();
        }

        private void OnAttackFinished()
        {
            _anticipationTimer = _enemy.AttackAnticipation;
        }

        private void OnDamaged(float damage)
        {
            if (!_enemy.IsAttacking)
                _anticipationTimer = _enemy.AttackAnticipation;
        }
    }
}

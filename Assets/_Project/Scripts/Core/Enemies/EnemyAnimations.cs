using UnityEngine;
using DungeonCrawler.Core.Combat;

namespace DungeonCrawler.Core.Enemies
{
    public sealed class EnemyAnimations : MonoBehaviour
    {
        private static readonly int SpeedHash = Animator.StringToHash("Speed");
        private static readonly int AttackHash = Animator.StringToHash("Attack");
        private static readonly int HitHash = Animator.StringToHash("Hit");
        private static readonly int DeathHash = Animator.StringToHash("Death");
        private static readonly int IsDeadHash = Animator.StringToHash("IsDead");

        [SerializeField] private Animator animator;
        [SerializeField] private float speedDampTime = 0.1f;

        private EnemyBase _enemy;
        private EnemyHealth _health;

        private void Awake()
        {
            _enemy = GetComponent<EnemyBase>();
            _health = GetComponent<EnemyHealth>();
        }

        private void OnEnable()
        {
            if (_enemy != null)
            {
                _enemy.AttackStarted += OnAttackStarted;
                _enemy.AttackFinished += OnAttackFinished;
            }

            if (_health != null)
            {
                _health.Damaged.AddListener(OnDamaged);
                _health.Died.AddListener(OnDied);
            }
        }

        private void OnDisable()
        {
            if (_enemy != null)
            {
                _enemy.AttackStarted -= OnAttackStarted;
                _enemy.AttackFinished -= OnAttackFinished;
            }

            if (_health != null)
            {
                _health.Damaged.RemoveListener(OnDamaged);
                _health.Died.RemoveListener(OnDied);
            }
        }

        private void Update()
        {
            if (animator == null || _enemy == null)
                return;

            animator.SetFloat(SpeedHash, _enemy.MovementSpeed01, speedDampTime, Time.deltaTime);
            animator.SetBool(IsDeadHash, _health != null && _health.IsDead);
        }

        private void OnAttackStarted()
        {
            if (animator == null)
                return;

            animator.SetTrigger(AttackHash);
        }

        private void OnAttackFinished()
        {
            if (animator != null)
                animator.ResetTrigger(AttackHash);
        }

        private void OnDamaged(float damage)
        {
            if (animator != null && !_health.IsDead)
                animator.SetTrigger(HitHash);
        }

        private void OnDied()
        {
            if (animator == null)
                return;

            animator.SetBool(IsDeadHash, true);
            animator.SetTrigger(DeathHash);
        }
    }
}

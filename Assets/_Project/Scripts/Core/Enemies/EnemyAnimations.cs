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
        private static readonly int HitShaderPropertyId = Shader.PropertyToID("_Hit");

        [SerializeField] private Animator animator;
        [SerializeField] private float speedDampTime = 0.1f;
        [SerializeField, Min(0f)] private float hitFlashDuration = .5f;

        private EnemyBase _enemy;
        private EnemyHealth _health;
        private Renderer[] _renderers;
        private MaterialPropertyBlock[] _propertyBlocks;
        private float _hitFlashTimer;

        private void Awake()
        {
            _enemy = GetComponent<EnemyBase>();
            _health = GetComponent<EnemyHealth>();
            _renderers = GetComponentsInChildren<Renderer>(true);
            _propertyBlocks = new MaterialPropertyBlock[_renderers.Length];

            for (int i = 0; i < _propertyBlocks.Length; i++)
                _propertyBlocks[i] = new MaterialPropertyBlock();
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
            UpdateHitFlash();

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
            if (_health.IsDead)
                return;

            _enemy?.CancelAttack();
            _hitFlashTimer = hitFlashDuration;
            SetHitShaderValue(true);

            if (animator != null)
            {
                animator.ResetTrigger(AttackHash);
                animator.SetTrigger(HitHash);
            }
        }

        private void UpdateHitFlash()
        {
            if (_hitFlashTimer <= 0f)
                return;

            _hitFlashTimer -= Time.deltaTime;
            if (_hitFlashTimer <= 0f)
                SetHitShaderValue(false);
        }

        private void SetHitShaderValue(bool value)
        {
            if (_renderers == null)
                return;

            for (int i = 0; i < _renderers.Length; i++)
            {
                Renderer renderer = _renderers[i];
                if (renderer == null)
                    continue;

                MaterialPropertyBlock propertyBlock = _propertyBlocks[i];
                renderer.GetPropertyBlock(propertyBlock);
                propertyBlock.SetFloat(HitShaderPropertyId, value ? 1f : 0f);
                renderer.SetPropertyBlock(propertyBlock);
            }
        }

        private void OnDestroy()
        {
            SetHitShaderValue(false);
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

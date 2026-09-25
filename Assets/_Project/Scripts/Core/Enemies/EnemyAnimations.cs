using UnityEngine;

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

        private Animator _animator;
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
            _animator = GetComponentInChildren<Animator>();
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
                _health.Damaged += OnDamaged;
                _health.Died += OnDied;
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
                _health.Damaged -= OnDamaged;
                _health.Died -= OnDied;
            }
        }

        private void Update()
        {
            UpdateHitFlash();

            if (_animator == null || _enemy == null)
                return;

            _animator.SetFloat(SpeedHash, _enemy.MovementSpeed01, speedDampTime, Time.deltaTime);
            _animator.SetBool(IsDeadHash, _health != null && _health.IsDead);
        }

        private void OnAttackStarted()
        {
            if (_animator == null)
                return;

            _animator.SetTrigger(AttackHash);
        }

        private void OnAttackFinished()
        {
            if (_animator != null)
                _animator.ResetTrigger(AttackHash);
        }

        private void OnDamaged(float damage)
        {
            if (_health.IsDead)
                return;

            _hitFlashTimer = hitFlashDuration;
            SetHitShaderValue(true);

            if (_animator != null)
            {
                _animator.ResetTrigger(AttackHash);
                _animator.SetTrigger(HitHash);
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
            if (_animator == null)
                return;

            _animator.SetBool(IsDeadHash, true);
            _animator.SetTrigger(DeathHash);
        }
    }
}

using System.Collections;
using UnityEngine;

namespace DungeonCrawler.Core.Player
{
    public class PlayerAnimations : MonoBehaviour
    {
        private static readonly int SpeedHash = Animator.StringToHash("Speed");
        private static readonly int AttackHash = Animator.StringToHash("Attack");
        private static readonly int ComboStepHash = Animator.StringToHash("ComboStep");
        private static readonly int DodgeHash = Animator.StringToHash("Dodge");
        private static readonly int IsDeadHash = Animator.StringToHash("IsDead");

        private Animator _animator;
        private PlayerMovement _playerMovement;
        private PlayerAttack _playerAttack;
        private PlayerHealth _playerHealth;
        private PlayerRootMotion _rootMotion;
        private bool _isDead;
        [SerializeField] private float speedDampTime = 0.1f;
        [SerializeField, Min(0f)] private float hitBlinkDuration = 0.5f;
        [SerializeField, Min(0.01f)] private float hitBlinkInterval = 0.08f;

        private float _targetSpeed;
        private Renderer[] _renderers;
        private bool[] _originalRendererStates;
        private Coroutine _hitBlinkRoutine;

        private void Awake()
        {
            _animator = GetComponentInChildren<Animator>();
            _playerMovement = GetComponent<PlayerMovement>();
            _playerAttack = GetComponent<PlayerAttack>();
            _playerHealth = GetComponent<PlayerHealth>();
            _renderers = GetComponentsInChildren<Renderer>(true);
            _originalRendererStates = new bool[_renderers.Length];
            for (int i = 0; i < _renderers.Length; i++)
                _originalRendererStates[i] = _renderers[i] != null && _renderers[i].enabled;

            if (_animator != null)
            {
                _rootMotion = _animator.GetComponent<PlayerRootMotion>();
                if (_rootMotion == null)
                    _rootMotion = _animator.gameObject.AddComponent<PlayerRootMotion>();

                _rootMotion.Initialize(
                    _animator,
                    GetComponent<CharacterController>(),
                    _playerAttack);
            }
        }

        private void OnEnable()
        {
            _playerMovement.MovementSpeedChanged += OnMovementSpeedChanged;
            _playerAttack.AttackStarted += OnAttacked;
            _playerMovement.DodgeStarted += OnDodged;
            _playerHealth.Damaged += OnDamaged;
            _playerHealth.Died += OnDied;
        }

        private void OnDisable()
        {
            _playerMovement.MovementSpeedChanged -= OnMovementSpeedChanged;
            _playerAttack.AttackStarted -= OnAttacked;
            _playerMovement.DodgeStarted -= OnDodged;
            _playerHealth.Damaged -= OnDamaged;
            _playerHealth.Died -= OnDied;

            StopHitBlinkAndRestore();
        }

        private void Update()
        {
            _animator.SetFloat(SpeedHash, _targetSpeed, speedDampTime, Time.deltaTime);
            _animator.SetBool(IsDeadHash, _isDead);
        }

        private void OnMovementSpeedChanged(float speed)
        {
            _targetSpeed = speed;
        }

        private void OnAttacked(int step)
        {
            // Set the index BEFORE the trigger so the transition sees the right value.
            _animator.SetInteger(ComboStepHash, step);
            _animator.SetTrigger(AttackHash);
        }

        private void OnDodged()
        {
            _animator.ResetTrigger(AttackHash);
            _animator.SetTrigger(DodgeHash);
        }

        private void OnDied()
        {
            StopHitBlinkAndRestore();
            _isDead = true;
            _animator.SetBool(IsDeadHash, true);
        }

        private void OnDamaged(float damage)
        {
            if (_playerHealth.IsDead)
                return;

            _animator.ResetTrigger(AttackHash);

            if (_hitBlinkRoutine != null)
            {
                StopCoroutine(_hitBlinkRoutine);
                _hitBlinkRoutine = null;
            }

            _hitBlinkRoutine = StartCoroutine(BlinkOnHit());
        }

        private IEnumerator BlinkOnHit()
        {
            float elapsed = 0f;
            bool visible = true;

            while (elapsed < hitBlinkDuration)
            {
                visible = !visible;
                SetRenderersVisible(visible);
                yield return new WaitForSeconds(hitBlinkInterval);
                elapsed += hitBlinkInterval;
            }

            RestoreRendererStates();
            _hitBlinkRoutine = null;
        }

        private void SetRenderersVisible(bool visible)
        {
            if (_renderers == null || _originalRendererStates == null)
                return;

            for (int i = 0; i < _renderers.Length; i++)
            {
                if (_renderers[i] != null)
                    _renderers[i].enabled = _originalRendererStates[i] && visible;
            }
        }

        private void StopHitBlinkAndRestore()
        {
            if (_hitBlinkRoutine != null)
            {
                StopCoroutine(_hitBlinkRoutine);
                _hitBlinkRoutine = null;
            }

            RestoreRendererStates();
        }

        private void RestoreRendererStates()
        {
            if (_renderers == null || _originalRendererStates == null)
                return;

            for (int i = 0; i < _renderers.Length; i++)
            {
                if (_renderers[i] != null)
                    _renderers[i].enabled = _originalRendererStates[i];
            }
        }
    }
}
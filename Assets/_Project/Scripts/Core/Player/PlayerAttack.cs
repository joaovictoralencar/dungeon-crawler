using System;
using UnityEngine;
using DungeonCrawler.Core.Combat;

namespace DungeonCrawler.Core.Player
{
    public class PlayerAttack : MonoBehaviour
    {
        private PlayerInputController _playerInputController;
        private PlayerMovement _movement;

        [Header("VFX")]
        [SerializeField] private GameObject swordTrailEffect;

        [Header("Damage")]
        [SerializeField] private DamageHitbox attackHitbox;
        [SerializeField, Min(0f)] private float damage = 1f;
        [SerializeField] private LayerMask damageMask;

        [Header("Combo")]
        [SerializeField] private PlayerAttackStep[] comboSteps =
        {
            new PlayerAttackStep(),
            new PlayerAttackStep(),
            new PlayerAttackStep()
        };

        public bool CanAttack = true;
        public bool IsAttacking => _isAttacking;
        public bool IsRecovering => _isRecovering;
        public int ComboStep => _comboStep;
        public event Action<int> AttackStarted;
        public event Action AttackFinished;
        public event Action AttackCancelled;

        private bool _isAttacking;
        private bool _isRecovering;
        private bool _comboWindowOpen;
        private int _comboStep;
        private float _recoveryTimer;
        private float _recoveryDuration;
        private AttackInputType _bufferedInput;

        private void Awake()
        {
            _playerInputController = GetComponent<PlayerInputController>();
            _movement = GetComponent<PlayerMovement>();
        }

        private void OnEnable()
        {
            _playerInputController.AttackRequested += OnAttackRequested;
            _movement.DodgeStarted += OnDodged;
        }

        private void OnDisable()
        {
            _playerInputController.AttackRequested -= OnAttackRequested;
            _movement.DodgeStarted -= OnDodged;

            if (_isAttacking || _isRecovering)
                EndAttack(true);
        }

        private void Update()
        {
            if (_isRecovering)
            {
                _recoveryTimer += Time.deltaTime;
                if (_recoveryTimer >= _recoveryDuration)
                {
                    _isRecovering = false;
                    TryStartBufferedAttack();
                }
            }

            // Attack-step timing is owned by animation events. This update only
            // handles post-combo recovery.
        }

        private void OnAttackRequested(AttackInputType input)
        {
            if (!CanAttack || _movement.IsDodging)
                return;

            if (_isAttacking && !_comboWindowOpen)
                return;

            _bufferedInput = input;

            if (!_isAttacking && !_isRecovering)
                TryStartBufferedAttack();
        }

        private void TryStartBufferedAttack()
        {
            if (_bufferedInput == AttackInputType.None || !_movement.CanMove)
                return;

            AttackInputType input = _bufferedInput;
            _bufferedInput = AttackInputType.None;
            StartStep(0, input);
        }

        private void OnDodged()
        {
            if (_isAttacking || _isRecovering)
                EndAttack(true);
        }

        private void StartStep(int step, AttackInputType input)
        {
            if (comboSteps == null || step < 0 || step >= comboSteps.Length)
            {
                EndAttack(false);
                return;
            }

            PlayerAttackStep definition = comboSteps[step];
            if (definition == null || definition.input != input)
            {
                EndAttack(false);
                return;
            }

            _isAttacking = true;
            _isRecovering = false;
            _comboStep = step;
            _comboWindowOpen = false;
            _movement.LockWalking();

            attackHitbox?.Configure(
                transform,
                damage * Mathf.Max(0f, definition.damageMultiplier),
                damageMask,
                definition.knockbackForce,
                definition.knockbackDuration);

            AttackStarted?.Invoke(step);
        }

        public void EnableAttackHitbox(int index)
        {
            if (!_isAttacking)
                return;

            attackHitbox?.Activate(index);
        }

        public void EnableAttackHitbox()
        {
            EnableAttackHitbox(0);
        }

        public void DisableAttackHitbox()
        {
            attackHitbox?.Deactivate();
        }

        public void EnableSwordTrail()
        {
            if (_isAttacking)
                swordTrailEffect?.SetActive(true);
        }

        public void DisableSwordTrail()
        {
            swordTrailEffect?.SetActive(false);
        }

        public void OpenComboWindow()
        {
            if (_isAttacking)
                _comboWindowOpen = true;
        }

        public void FinishAttackAnimation()
        {
            if (!_isAttacking)
                return;

            _comboWindowOpen = false;
            DisableAttackHitbox();
            DisableSwordTrail();

            int nextStep = _comboStep + 1;
            if (_bufferedInput != AttackInputType.None
                && nextStep < comboSteps.Length
                && comboSteps[nextStep] != null
                && comboSteps[nextStep].input == _bufferedInput)
            {
                AttackInputType nextInput = _bufferedInput;
                _bufferedInput = AttackInputType.None;
                StartStep(nextStep, nextInput);
                return;
            }

            _isAttacking = false;
            _comboWindowOpen = false;
            _recoveryTimer = 0f;
            PlayerAttackStep definition = comboSteps[_comboStep];
            _recoveryDuration = Mathf.Max(0f, definition.recoveryDuration);
            _isRecovering = _recoveryDuration > 0f;
            _movement.UnlockWalking();
            AttackFinished?.Invoke();
        }

        public void CancelAttack()
        {
            if (!_isAttacking && !_isRecovering && _movement.CanWalk)
                return;

            EndAttack(true);
        }

        private void EndAttack(bool cancelled)
        {
            _isAttacking = false;
            _isRecovering = false;
            _comboWindowOpen = false;
            _comboStep = 0;
            _recoveryTimer = 0f;
            _recoveryDuration = 0f;
            _bufferedInput = AttackInputType.None;
            _movement.UnlockWalking();
            DisableAttackHitbox();
            DisableSwordTrail();

            if (cancelled)
                AttackCancelled?.Invoke();
        }
    }
}

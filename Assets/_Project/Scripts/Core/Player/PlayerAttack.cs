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
        [SerializeField, Min(0f)] private float knockbackForce = 4f;
        [SerializeField, Min(0f)] private float knockbackDuration = 0.15f;
        [SerializeField] private bool debugCombat;

        [Header("Combo")]
        [Tooltip("Duration in seconds of each hit. One entry per animation (3 entries = 3-hit combo).")]
        [SerializeField] private float[] stepDurations = { 0.4f, 0.4f, 0.6f };

        [Tooltip("Fraction of a hit (0-1) after which pressing attack queues the next hit.")]
        [SerializeField, Range(0f, 1f)] private float comboWindowStart = 0.5f;

        public bool CanAttack = true;
        public bool IsAttacking => _isAttacking;
        public int ComboStep => _comboStep;
        public event Action<int> AttackStarted;
        public event Action AttackFinished;
        public event Action AttackCancelled;

        private bool _isAttacking;
        private int _comboStep;
        private float _stepDuration;
        private float _stepTimer;
        private bool _nextStepQueued;

        private void Awake()
        {
            _playerInputController = GetComponent<PlayerInputController>();
            _movement = GetComponent<PlayerMovement>();
        }

        private void OnEnable()
        {
            _playerInputController.AttackRequested += OnInputAttacked;
            _movement.DodgeStarted += OnDodged;
        }

        private void OnDisable()
        {
            _playerInputController.AttackRequested -= OnInputAttacked;
            _movement.DodgeStarted -= OnDodged;

            // Never leave the player stuck if this component is disabled mid-combo.
            if (_isAttacking)
                EndAttack(true);
        }

        private void Update()
        {
            if (!_isAttacking)
                return;

            _stepTimer += Time.deltaTime;
        }

        private void OnInputAttacked()
        {
            if (!CanAttack || !_movement.CanMove || _movement.IsDodging)
                return;

            if (stepDurations == null || stepDurations.Length == 0)
                return;

            // Not attacking: start a fresh combo at hit 1.
            if (!_isAttacking)
            {
                StartStep(0);
                return;
            }

            // Already attacking: queue the next hit, but only if there is one and the window is open.
            bool hasNextStep = _comboStep < stepDurations.Length - 1;
            float elapsed = _stepTimer;
            bool windowOpen = elapsed >= _stepDuration * comboWindowStart;

            if (hasNextStep && windowOpen)
                _nextStepQueued = true;
        }

        private void OnDodged()
        {
            // Dodge interrupts the combo and resets it.
            if (_isAttacking)
                EndAttack(true);
        }

        private void StartStep(int step)
        {
            _isAttacking = true;
            _comboStep = step;
            _stepDuration = Mathf.Max(0.01f, stepDurations[step]);
            _stepTimer = 0f;
            _nextStepQueued = false;
            _movement.LockWalking();

            attackHitbox?.Configure(transform, damage, damageMask, knockbackForce, knockbackDuration);
            AttackStarted?.Invoke(step);
        }

        public void EnableAttackHitbox()
        {
            attackHitbox?.Activate();
            swordTrailEffect?.SetActive(true);
        }

        public void DisableAttackHitbox()
        {
            attackHitbox?.Deactivate();
            swordTrailEffect?.SetActive(false);
        }

        public void FinishAttackAnimation()
        {
            DisableAttackHitbox();

            if (_nextStepQueued && _comboStep < stepDurations.Length - 1)
                StartStep(_comboStep + 1);
            else
                EndAttack(false);
        }

        public void CancelAttack()
        {
            if (!_isAttacking && _movement.CanWalk)
                return;

            EndAttack(true);
        }

        private void EndAttack(bool cancelled)
        {
            _isAttacking = false;
            _comboStep = 0;
            _stepTimer = 0f;
            _nextStepQueued = false;
            _movement.UnlockWalking();

            DisableAttackHitbox();
            if (cancelled)
                AttackCancelled?.Invoke();
            else
                AttackFinished?.Invoke();
        }
    }
}
using UnityEngine;
using UnityEngine.Events;

namespace DungeonCrawler
{
    public class PlayerAttack : MonoBehaviour
    {
        [SerializeField] private PlayerInputController playerInputController;
        [SerializeField] private PlayerMovement playerMovement;

        [Header("Combo")]
        [Tooltip("Duration in seconds of each hit. One entry per animation (3 entries = 3-hit combo).")]
        [SerializeField] private float[] stepDurations = { 0.4f, 0.4f, 0.6f };

        [Tooltip("Fraction of a hit (0-1) after which pressing attack queues the next hit.")]
        [SerializeField, Range(0f, 1f)] private float comboWindowStart = 0.5f;

        /// <summary>Fires at the start of every hit. Parameter = hit index (0, 1, 2).</summary>
        public UnityEvent<int> Attacked { get; } = new();

        /// <summary>Fires once when the whole combo is over (or cancelled) and the player regains control.</summary>
        public UnityEvent AttackEnded { get; } = new();

        public bool CanAttack = true;
        public bool IsAttacking => _isAttacking;
        public int ComboStep => _comboStep;

        private bool _isAttacking;
        private int _comboStep;
        private float _stepDuration;
        private float _stepTimer;
        private bool _nextStepQueued;

        private void OnEnable()
        {
            playerInputController.InputAttacked.AddListener(OnInputAttacked);
            playerMovement.Dodged.AddListener(OnDodged);
        }

        private void OnDisable()
        {
            playerInputController.InputAttacked.RemoveListener(OnInputAttacked);
            playerMovement.Dodged.RemoveListener(OnDodged);

            // Never leave the player stuck if this component is disabled mid-combo.
            if (_isAttacking)
                EndAttack();
        }

        private void Update()
        {
            if (!_isAttacking)
                return;

            _stepTimer -= Time.deltaTime;

            if (_stepTimer > 0f)
                return;

            if (_nextStepQueued)
                StartStep(_comboStep + 1); // chain straight into the next hit, walking stays locked
            else
                EndAttack();
        }

        private void OnInputAttacked()
        {
            if (!CanAttack || !playerMovement.CanMove || playerMovement.IsDodging)
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
            float elapsed = _stepDuration - _stepTimer;
            bool windowOpen = elapsed >= _stepDuration * comboWindowStart;

            if (hasNextStep && windowOpen)
                _nextStepQueued = true;
        }

        private void OnDodged()
        {
            // Dodge interrupts the combo and resets it.
            if (_isAttacking)
                EndAttack();
        }

        private void StartStep(int step)
        {
            _isAttacking = true;
            _comboStep = step;
            _stepDuration = Mathf.Max(0.01f, stepDurations[step]);
            _stepTimer = _stepDuration;
            _nextStepQueued = false;
            playerMovement.CanWalk = false;

            Attacked.Invoke(step);
        }

        private void EndAttack()
        {
            _isAttacking = false;
            _comboStep = 0;
            _stepTimer = 0f;
            _nextStepQueued = false;
            playerMovement.CanWalk = true;

            AttackEnded.Invoke();
        }
    }
}
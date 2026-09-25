using System.Collections;
using UnityEngine;
using DungeonCrawler.Player;

namespace DungeonCrawler
{
    public class PlayerAnimations : MonoBehaviour
    {
        private static readonly int SpeedHash = Animator.StringToHash("Speed");
        private static readonly int AttackHash = Animator.StringToHash("Attack");
        private static readonly int AttackIndexHash = Animator.StringToHash("AttackIndex");
        private static readonly int DodgeHash = Animator.StringToHash("Dodge");
        private static readonly int IsDeadHash = Animator.StringToHash("IsDead");

        [SerializeField] private Animator animator;
        [SerializeField] private PlayerInputController playerInputController;
        [SerializeField] private PlayerMovement playerMovement;
        [SerializeField] private PlayerAttack playerAttack;
        [SerializeField] private PlayerHealth playerHealth;
        [SerializeField] private float speedDampTime = 0.1f;
        [SerializeField, Min(0f)] private float hitBlinkDuration = 0.5f;
        [SerializeField, Min(0.01f)] private float hitBlinkInterval = 0.08f;

        private float _targetSpeed;
        private Renderer[] _renderers;
        private Coroutine _hitBlinkRoutine;

        private void Awake()
        {
            if (playerHealth == null)
                playerHealth = GetComponent<PlayerHealth>();

            _renderers = GetComponentsInChildren<Renderer>(true);
        }

        private void OnEnable()
        {
            if (playerHealth == null)
                playerHealth = GetComponent<PlayerHealth>();

            playerInputController.InputMoved.AddListener(OnInputMoved);
            playerAttack.Attacked.AddListener(OnAttacked);
            playerMovement.Dodged.AddListener(OnDodged);
            playerHealth.Damaged.AddListener(OnDamaged);
            playerHealth.Died.AddListener(OnDied);
        }

        private void OnDisable()
        {
            playerInputController.InputMoved.RemoveListener(OnInputMoved);
            playerAttack.Attacked.RemoveListener(OnAttacked);
            playerMovement.Dodged.RemoveListener(OnDodged);
            playerHealth.Damaged.RemoveListener(OnDamaged);
            playerHealth.Died.RemoveListener(OnDied);

            if (_hitBlinkRoutine != null)
            {
                StopCoroutine(_hitBlinkRoutine);
                _hitBlinkRoutine = null;
                SetRenderersEnabled(true);
            }
        }

        private void Update()
        {
            bool isWalking = playerMovement.CanMove && playerMovement.CanWalk;
            float speed = isWalking ? _targetSpeed : 0f;

            animator.SetFloat(SpeedHash, speed, speedDampTime, Time.deltaTime);
            animator.SetBool(IsDeadHash, playerHealth.IsDead);
        }

        private void OnInputMoved(Vector2 input)
        {
            _targetSpeed = Mathf.Clamp01(input.magnitude);
        }

        private void OnAttacked(int step)
        {
            // Set the index BEFORE the trigger so the transition sees the right value.
            animator.SetInteger(AttackIndexHash, step);
            animator.SetTrigger(AttackHash);
        }

        private void OnDodged()
        {
            animator.ResetTrigger(AttackHash);
            animator.SetTrigger(DodgeHash);
        }

        private void OnDied()
        {
            animator.SetBool(IsDeadHash, true);
        }

        private void OnDamaged(float damage)
        {
            if (playerHealth.IsDead)
                return;

            animator.ResetTrigger(AttackHash);

            if (_hitBlinkRoutine != null)
                StopCoroutine(_hitBlinkRoutine);

            _hitBlinkRoutine = StartCoroutine(BlinkOnHit());
        }

        private IEnumerator BlinkOnHit()
        {
            bool[] originalStates = new bool[_renderers.Length];
            for (int i = 0; i < _renderers.Length; i++)
                originalStates[i] = _renderers[i] != null && _renderers[i].enabled;

            float elapsed = 0f;
            bool visible = true;

            while (elapsed < hitBlinkDuration)
            {
                visible = !visible;
                SetRenderersEnabled(visible);
                yield return new WaitForSeconds(hitBlinkInterval);
                elapsed += hitBlinkInterval;
            }

            for (int i = 0; i < _renderers.Length; i++)
            {
                if (_renderers[i] != null)
                    _renderers[i].enabled = originalStates[i];
            }

            _hitBlinkRoutine = null;
        }

        private void SetRenderersEnabled(bool enabled)
        {
            if (_renderers == null)
                return;

            foreach (Renderer renderer in _renderers)
            {
                if (renderer != null)
                    renderer.enabled = enabled;
            }
        }
    }
}
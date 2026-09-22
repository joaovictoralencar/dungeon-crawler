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

        private float _targetSpeed;

        private void Awake()
        {
            if (playerHealth == null)
                playerHealth = GetComponent<PlayerHealth>();
        }

        private void OnEnable()
        {
            if (playerHealth == null)
                playerHealth = GetComponent<PlayerHealth>();

            playerInputController.InputMoved.AddListener(OnInputMoved);
            playerAttack.Attacked.AddListener(OnAttacked);
            playerMovement.Dodged.AddListener(OnDodged);
            playerHealth.Died.AddListener(OnDied);
        }

        private void OnDisable()
        {
            playerInputController.InputMoved.RemoveListener(OnInputMoved);
            playerAttack.Attacked.RemoveListener(OnAttacked);
            playerMovement.Dodged.RemoveListener(OnDodged);
            playerHealth.Died.RemoveListener(OnDied);
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
    }
}
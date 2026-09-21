using UnityEngine;

namespace DungeonCrawler
{
    public class PlayerAnimations : MonoBehaviour
    {
        private static readonly int SpeedHash = Animator.StringToHash("Speed");
        private static readonly int AttackHash = Animator.StringToHash("Attack");
        private static readonly int AttackIndexHash = Animator.StringToHash("AttackIndex");
        private static readonly int DodgeHash = Animator.StringToHash("Dodge");

        [SerializeField] private Animator animator;
        [SerializeField] private PlayerInputController playerInputController;
        [SerializeField] private PlayerMovement playerMovement;
        [SerializeField] private PlayerAttack playerAttack;
        [SerializeField] private float speedDampTime = 0.1f;

        private float _targetSpeed;

        private void OnEnable()
        {
            playerInputController.InputMoved.AddListener(OnInputMoved);
            playerAttack.Attacked.AddListener(OnAttacked);
            playerMovement.Dodged.AddListener(OnDodged);
        }

        private void OnDisable()
        {
            playerInputController.InputMoved.RemoveListener(OnInputMoved);
            playerAttack.Attacked.RemoveListener(OnAttacked);
            playerMovement.Dodged.RemoveListener(OnDodged);
        }

        private void Update()
        {
            bool isWalking = playerMovement.CanMove && playerMovement.CanWalk;
            float speed = isWalking ? _targetSpeed : 0f;

            animator.SetFloat(SpeedHash, speed, speedDampTime, Time.deltaTime);
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
    }
}
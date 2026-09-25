using UnityEngine;

namespace DungeonCrawler.Core.Player
{
    [DisallowMultipleComponent]
    public sealed class PlayerRootMotion : MonoBehaviour
    {
        private Animator _animator;
        private CharacterController _characterController;
        private PlayerAttack _playerAttack;

        public void Initialize(Animator animator, CharacterController characterController, PlayerAttack playerAttack)
        {
            _animator = animator;
            _characterController = characterController;
            _playerAttack = playerAttack;

            _animator.applyRootMotion = true;
        }

        private void OnAnimatorMove()
        {
            if (_animator == null || _characterController == null || _playerAttack == null)
                return;

            if (!_playerAttack.IsAttacking)
                return;

            _characterController.Move(_animator.deltaPosition);
        }
    }
}

using UnityEngine;

namespace DungeonCrawler.Core.Player
{
    public sealed class PlayerAttackAnimationEvents : MonoBehaviour
    {
        private PlayerAttack _playerAttack;

        private void Awake()
        {
            _playerAttack = GetComponentInParent<PlayerAttack>();
        }

        public void EnableAttackHitbox(int index)
        {
            _playerAttack?.EnableAttackHitbox(index);
        }

        public void EnableAttackHitbox()
        {
            _playerAttack?.EnableAttackHitbox(0);
        }

        public void DisableAttackHitbox()
        {
            _playerAttack?.DisableAttackHitbox();
        }

        public void EnableSwordTrail()
        {
            _playerAttack?.EnableSwordTrail();
        }

        public void DisableSwordTrail()
        {
            _playerAttack?.DisableSwordTrail();
        }

        public void FinishAttackAnimation()
        {
            _playerAttack?.FinishAttackAnimation();
        }

        public void OpenComboWindow()
        {
            _playerAttack?.OpenComboWindow();
        }

    }
}

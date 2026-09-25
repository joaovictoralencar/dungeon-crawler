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

        public void EnableAttackHitbox()
        {
            _playerAttack?.EnableAttackHitbox();
        }

        public void DisableAttackHitbox()
        {
            _playerAttack?.DisableAttackHitbox();
        }

        public void FinishAttackAnimation()
        {
            _playerAttack?.FinishAttackAnimation();
        }
    }
}

using UnityEngine;
using DungeonCrawler.Core.Combat;

namespace DungeonCrawler.Player
{
    public sealed class PlayerHealth : Health
    {
        private PlayerMovement _movement;
        private PlayerAttack _attack;

        public bool IsInvulnerable { get; private set; }

        private void OnEnable()
        {
            _movement = GetComponent<PlayerMovement>();
            _attack = GetComponent<PlayerAttack>();
            if (_movement != null)
            {
                _movement.Dodged.AddListener(OnDodgeStarted);
                _movement.DodgeEnded.AddListener(OnDodgeEnded);
            }
        }

        private void OnDisable()
        {
            if (_movement != null)
            {
                _movement.Dodged.RemoveListener(OnDodgeStarted);
                _movement.DodgeEnded.RemoveListener(OnDodgeEnded);
            }
        }

        public override void TakeDamage(float damage)
        {
            if (IsInvulnerable)
            {
                if (DebugDamage)
                    Debug.Log($"[{name}] Ignored {damage} damage while dodging.", this);
                return;
            }

            _attack?.CancelAttack();
            base.TakeDamage(damage);
        }

        private void OnDodgeStarted()
        {
            IsInvulnerable = true;
            if (DebugDamage)
                Debug.Log($"[{name}] Dodge invulnerability started.", this);
        }

        private void OnDodgeEnded()
        {
            IsInvulnerable = false;
            if (DebugDamage)
                Debug.Log($"[{name}] Dodge invulnerability ended.", this);
        }

        protected override void Die()
        {
            base.Die();

            PlayerMovement movement = GetComponent<PlayerMovement>();
            if (movement != null)
                movement.enabled = false;

            PlayerAttack attack = GetComponent<PlayerAttack>();
            if (attack != null)
                attack.enabled = false;
        }
    }
}

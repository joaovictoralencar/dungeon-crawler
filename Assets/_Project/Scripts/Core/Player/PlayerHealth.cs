using UnityEngine;
using DungeonCrawler.Core.Combat;

namespace DungeonCrawler.Core.Player
{
    public sealed class PlayerHealth : Health
    {
        private PlayerMovement _movement;

        public bool IsInvulnerable { get; private set; }

        private void Awake()
        {
            _movement = GetComponent<PlayerMovement>();
        }

        private void OnEnable()
        {
            if (_movement != null)
            {
                _movement.DodgeStarted += OnDodgeStarted;
                _movement.DodgeFinished += OnDodgeEnded;
            }
        }

        private void OnDisable()
        {
            if (_movement != null)
            {
                _movement.DodgeStarted -= OnDodgeStarted;
                _movement.DodgeFinished -= OnDodgeEnded;
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
        }
    }
}

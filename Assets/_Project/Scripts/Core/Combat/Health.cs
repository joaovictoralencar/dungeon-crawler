using System;
using UnityEngine;

namespace DungeonCrawler.Core.Combat
{
    public abstract class Health : MonoBehaviour, IDamageable
    {
        [SerializeField, Min(1f)] private float maxHealth = 10f;
        [SerializeField] private bool debugDamage;

        public float CurrentHealth { get; private set; }
        public float MaxHealth => maxHealth;
        public bool IsDead { get; private set; }
        protected bool DebugDamage => debugDamage;

        public event Action<float> Damaged;
        public event Action Died;

        protected virtual void Awake()
        {
            CurrentHealth = maxHealth;
        }

        public virtual void TakeDamage(float damage)
        {
            if (IsDead || damage <= 0f)
            {
                if (debugDamage)
                    Debug.Log($"[{name}] Damage ignored. IsDead={IsDead}, Damage={damage}.", this);
                return;
            }

            float previousHealth = CurrentHealth;
            CurrentHealth = Mathf.Max(0f, CurrentHealth - damage);
            if (debugDamage)
                Debug.Log($"[{name}] Took {damage} damage. Health {previousHealth} -> {CurrentHealth}.", this);

            if (CurrentHealth <= 0f)
                Die();

            Damaged?.Invoke(damage);
        }

        protected virtual void Die()
        {
            if (IsDead)
                return;

            IsDead = true;
            if (debugDamage)
                Debug.Log($"[{name}] Died.", this);
            Died?.Invoke();
        }
    }
}

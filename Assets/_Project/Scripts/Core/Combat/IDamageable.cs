using UnityEngine;

namespace DungeonCrawler.Core.Combat
{
    public interface IDamageable
    {
        bool IsDead { get; }
        void TakeDamage(float damage);
    }

    public interface IKnockbackable
    {
        void ApplyKnockback(Vector3 direction, float force, float duration);
    }
}

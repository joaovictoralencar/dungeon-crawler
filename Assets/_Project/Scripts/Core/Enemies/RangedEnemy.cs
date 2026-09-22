using UnityEngine;

namespace DungeonCrawler.Core.Enemies
{
    public sealed class RangedEnemy : EnemyBase
    {
        [SerializeField] private EnemyProjectile projectilePrefab;
        [SerializeField] private float projectileSpeed = 8f;

        protected override void PerformAttack()
        {
            if (projectilePrefab == null || Target == null)
                return;

            Vector3 direction = Target.position - AttackOrigin.position;
            direction.y = 0f;
            if (direction.sqrMagnitude <= 0.001f)
                return;

            EnemyProjectile projectile = Instantiate(
                projectilePrefab, AttackOrigin.position, Quaternion.LookRotation(direction));
            projectile.Initialize(direction.normalized, projectileSpeed, Config.Damage);
        }
    }
}

using UnityEngine;

namespace DungeonCrawler.Core.Enemies
{
    public sealed class MeleeEnemy : EnemyBase
    {
        [SerializeField] private float hitRadius = 1.25f;
        [SerializeField] private LayerMask damageMask;

        protected override void PerformAttack()
        {
            Collider[] hits = Physics.OverlapSphere(AttackOrigin.position, hitRadius, damageMask);
            foreach (Collider hit in hits)
                ApplyDamage(hit.transform, Config.Damage);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(AttackOrigin.position, hitRadius);
        }
    }
}

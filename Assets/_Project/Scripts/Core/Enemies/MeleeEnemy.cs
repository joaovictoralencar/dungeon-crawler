using UnityEngine;
using DungeonCrawler.Core.Combat;

namespace DungeonCrawler.Core.Enemies
{
    public sealed class MeleeEnemy : EnemyBase
    {
        [SerializeField] private DamageHitbox attackHitbox;
        [SerializeField] private LayerMask damageMask;

        protected override void PerformAttack()
        {
            attackHitbox?.Configure(transform, Config.Damage, damageMask);
            attackHitbox?.Activate();
        }

        protected override void OnDisable()
        {
            attackHitbox?.Deactivate();
            base.OnDisable();
        }

        protected override void EndAttackHitbox()
        {
            attackHitbox?.Deactivate();
        }
    }
}

using System;
using UnityEngine;

namespace DungeonCrawler.Core.Player
{
    [Serializable]
    public sealed class PlayerAttackStep
    {
        [Tooltip("Input required to select this step. The current input map produces Light attacks.")]
        public AttackInputType input = AttackInputType.Light;

        [Min(0f)]
        public float damageMultiplier = 1f;

        [Header("Knockback")]
        [Min(0f)]
        public float knockbackForce = 4f;

        [Min(0f)]
        public float knockbackDuration = 0.15f;

        [Header("Recovery")]
        [Min(0f)]
        public float recoveryDuration = 0.1f;
    }
}

using UnityEngine;

namespace DungeonCrawler.Core.Enemies
{
    [CreateAssetMenu(menuName = "Dungeon Crawler/Enemies/Enemy Config")]
    public class EnemyConfig : ScriptableObject
    {
        [Header("Movement")]
        [Min(0f)] public float MovementSpeed = 3f;
        [Min(0f)] public float AngularSpeed = 720f;
        [Min(0f)] public float Acceleration = 20f;
        [Min(0f)] public float StoppingDistance = 1.5f;

        [Header("Awareness")]
        [Min(0f)] public float DetectionRange = 8f;
        [Min(0f)] public float LoseTargetRange = 12f;
        public bool RequireLineOfSight;
        public LayerMask ObstacleMask;

        [Header("Patrol")]
        public bool Patrol;
        [Min(0f)] public float PatrolRadius = 5f;
        [Min(0f)] public float PatrolWait = 1f;

        [Header("Attack")]
        [Min(0f)] public float AttackRange = 2f;
        [Min(0f)] public float AttackAnticipation = 0.2f;
        [Min(0.01f)] public float AttackInterval = 1.5f;
        [Min(0.01f)] public float AttackDuration = 0.6f;
        [Min(0f)] public float Damage = 1f;
    }
}

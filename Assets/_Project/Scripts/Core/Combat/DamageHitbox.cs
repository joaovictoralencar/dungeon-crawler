 using System.Collections.Generic;
 using DamageNumbersPro;
 using UnityEngine;

namespace DungeonCrawler.Core.Combat
{
    public sealed class DamageHitbox : MonoBehaviour
    {
         [Tooltip("Colliders used by this attack. Indexes are selected by animation events.")]
         [SerializeField] private Collider[] hitboxes;
         [SerializeField] private GameObject hitEffectPrefab;
         [SerializeField, Min(0f)] private float hitEffectSurfaceOffset = 0.02f;
         [SerializeField, Min(0.1f)] private float hitEffectFallbackLifetime = 2f;
         [SerializeField] private DamageNumberMesh damageNumberPrefab;
         [SerializeField] private Vector3 damageNumberOffset = new(0f, 0.5f, 0f);

         private Collider _activeCollider;
         private Transform _owner;
         private float _damage;
         private LayerMask _damageMask;
         private float _knockbackForce;
         private float _knockbackDuration;
         private readonly List<IDamageable> _affectedTargets = new();
         private readonly HashSet<IDamageable> _affectedTargetSet = new();

         public IReadOnlyList<IDamageable> AffectedTargets => _affectedTargets;

         private void Awake()
         {
             if (hitboxes == null || hitboxes.Length == 0)
             {
                 Collider ownCollider = GetComponent<Collider>();
                 if (ownCollider == null || ownCollider is CharacterController)
                 {
                     Debug.LogError(
                         $"[{nameof(DamageHitbox)}] Configure at least one hitbox collider on '{name}'.",
                         this);
                     hitboxes = System.Array.Empty<Collider>();
                     return;
                 }

                 hitboxes = new[] { ownCollider };
             }

             for (int i = 0; i < hitboxes.Length; i++)
             {
                 if (hitboxes[i] == null)
                     continue;

                 hitboxes[i].isTrigger = true;
                 hitboxes[i].enabled = false;
             }
         }

         public void Configure(Transform owner, float damage, LayerMask damageMask,
             float knockbackForce = 0f, float knockbackDuration = 0f)
         {
             Deactivate();
             _affectedTargets.Clear();
             _affectedTargetSet.Clear();
             _owner = owner;
             _damage = damage;
             _damageMask = damageMask;
             _knockbackForce = knockbackForce;
             _knockbackDuration = knockbackDuration;
         }

         public void Activate(int index)
         {
             if (hitboxes == null || index < 0 || index >= hitboxes.Length || hitboxes[index] == null)
             {
                 Debug.LogError($"[{nameof(DamageHitbox)}] Cannot activate collider index {index} on '{name}'.", this);
                 return;
             }

             Deactivate();
             _activeCollider = hitboxes[index];
             _activeCollider.enabled = true;
         }

         public void Activate()
         {
             Activate(0);
         }

         public void Deactivate()
         {
             if (_activeCollider != null)
                 _activeCollider.enabled = false;

             _activeCollider = null;

             if (hitboxes == null)
                 return;

             for (int i = 0; i < hitboxes.Length; i++)
             {
                 if (hitboxes[i] != null)
                     hitboxes[i].enabled = false;
             }
         }

        private void OnTriggerEnter(Collider other)
        {
            TryDamage(other);
        }

        private void TryDamage(Collider other)
        {
            if (_owner == null || other.transform.root == _owner.root)
                return;

            int layerBit = 1 << other.gameObject.layer;
            if (_damageMask.value != 0 && (_damageMask.value & layerBit) == 0)
                return;

            IDamageable damageable = other.GetComponentInParent<IDamageable>();
            if (damageable == null || damageable.IsDead || !_affectedTargetSet.Add(damageable))
                return;

            _affectedTargets.Add(damageable);
            Health health = damageable as Health;
            float healthBeforeDamage = health != null ? health.CurrentHealth : 0f;
            damageable.TakeDamage(_damage);

            float damageDealt = health != null
                ? healthBeforeDamage - health.CurrentHealth
                : _damage;
            if (damageDealt <= 0f)
                return;

            Vector3 hitPoint = GetHitPoint(other);
            SpawnHitEffect(other, hitPoint);
            damageNumberPrefab?.Spawn(hitPoint + damageNumberOffset, damageDealt);

            IKnockbackable knockbackable = other.GetComponentInParent<IKnockbackable>();
            if (knockbackable == null || _knockbackForce <= 0f)
                return;

            Vector3 direction = other.transform.position - _owner.position;
            knockbackable.ApplyKnockback(direction, _knockbackForce, _knockbackDuration);
        }

        private Vector3 GetHitPoint(Collider targetCollider)
        {
            Vector3 hitboxCenter = _activeCollider != null
                ? _activeCollider.bounds.center
                : transform.position;

            return targetCollider.ClosestPoint(hitboxCenter);
        }

        private void SpawnHitEffect(Collider targetCollider, Vector3 hitPoint)
        {
            if (hitEffectPrefab == null)
                return;

            Vector3 normal = hitPoint - targetCollider.bounds.center;

            if (normal.sqrMagnitude <= 0.0001f)
                normal = _owner.position - hitPoint;

            normal = normal.sqrMagnitude > 0.0001f ? normal.normalized : Vector3.up;
            hitPoint += normal * hitEffectSurfaceOffset;

            GameObject effect = Instantiate(hitEffectPrefab, hitPoint, Quaternion.LookRotation(normal));

            float lifetime = GetParticleEffectLifetime(effect);
            Destroy(effect, lifetime);
        }

        private float GetParticleEffectLifetime(GameObject effect)
        {
            ParticleSystem[] particleSystems = effect.GetComponentsInChildren<ParticleSystem>();
            float longestLifetime = 0f;

            foreach (ParticleSystem particleSystem in particleSystems)
            {
                ParticleSystem.MainModule main = particleSystem.main;
                float lifetime = main.duration
                                 + main.startDelay.constantMax
                                 + main.startLifetime.constantMax;
                longestLifetime = Mathf.Max(longestLifetime, lifetime);
            }

            return longestLifetime > 0f
                ? longestLifetime + 0.25f
                : hitEffectFallbackLifetime;
        }
    }
}

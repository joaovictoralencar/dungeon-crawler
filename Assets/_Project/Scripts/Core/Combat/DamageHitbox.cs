 using System.Collections.Generic;
using UnityEngine;

namespace DungeonCrawler.Core.Combat
{
    public sealed class DamageHitbox : MonoBehaviour
    {
         [Tooltip("Colliders used by this attack. Indexes are selected by animation events.")]
         [SerializeField] private Collider[] hitboxes;

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
            damageable.TakeDamage(_damage);

            IKnockbackable knockbackable = other.GetComponentInParent<IKnockbackable>();
            if (knockbackable == null || _knockbackForce <= 0f)
                return;

            Vector3 direction = other.transform.position - _owner.position;
            knockbackable.ApplyKnockback(direction, _knockbackForce, _knockbackDuration);
        }
    }
}

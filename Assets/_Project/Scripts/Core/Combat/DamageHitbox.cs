 using System.Collections.Generic;
using UnityEngine;

namespace DungeonCrawler.Core.Combat
{
    [RequireComponent(typeof(Collider))]
    public sealed class DamageHitbox : MonoBehaviour
    {
        private Collider _collider;
        private Transform _owner;
        private float _damage;
        private LayerMask _damageMask;
        private float _knockbackForce;
        private float _knockbackDuration;
        private readonly HashSet<IDamageable> _damagedTargets = new();

        private void Awake()
        {
            _collider = GetComponent<Collider>();
            _collider.isTrigger = true;
            _collider.enabled = false;
        }

        public void Configure(Transform owner, float damage, LayerMask damageMask,
            float knockbackForce = 0f, float knockbackDuration = 0f)
        {
            _owner = owner;
            _damage = damage;
            _damageMask = damageMask;
            _knockbackForce = knockbackForce;
            _knockbackDuration = knockbackDuration;
        }

        public void Activate()
        {
            _damagedTargets.Clear();
            _collider.enabled = true;
        }

        public void Deactivate()
        {
            if (_collider != null)
                _collider.enabled = false;
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
            if (damageable == null || damageable.IsDead || !_damagedTargets.Add(damageable))
                return;

            damageable.TakeDamage(_damage);

            IKnockbackable knockbackable = other.GetComponentInParent<IKnockbackable>();
            if (knockbackable == null || _knockbackForce <= 0f)
                return;

            Vector3 direction = other.transform.position - _owner.position;
            knockbackable.ApplyKnockback(direction, _knockbackForce, _knockbackDuration);
        }
    }
}

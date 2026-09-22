using UnityEngine;
using DungeonCrawler.Core.Combat;

namespace DungeonCrawler.Core.Enemies
{
    public sealed class EnemyProjectile : MonoBehaviour
    {
        [SerializeField] private float lifetime = 5f;
        [SerializeField] private LayerMask damageMask;

        private Vector3 _direction;
        private float _speed;
        private float _damage;
        private bool _initialized;

        public void Initialize(Vector3 direction, float speed, float damage)
        {
            _direction = direction.normalized;
            _speed = speed;
            _damage = damage;
            _initialized = true;
            Destroy(gameObject, lifetime);
        }

        private void Update()
        {
            if (_initialized)
                transform.position += _direction * (_speed * Time.deltaTime);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (damageMask.value != 0 && (damageMask.value & (1 << other.gameObject.layer)) == 0)
                return;

            IDamageable damageable = other.GetComponentInParent<IDamageable>();
            damageable?.TakeDamage(_damage);
            Destroy(gameObject);
        }
    }
}

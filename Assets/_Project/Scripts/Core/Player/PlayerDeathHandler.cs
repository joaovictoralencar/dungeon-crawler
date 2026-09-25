using UnityEngine;

namespace DungeonCrawler.Core.Player
{
    public sealed class PlayerDeathHandler : MonoBehaviour
    {
        private PlayerHealth _health;
        private PlayerMovement _movement;
        private PlayerAttack _attack;
        private PlayerInputController _input;

        private void Awake()
        {
            _health = GetComponent<PlayerHealth>();
            _movement = GetComponent<PlayerMovement>();
            _attack = GetComponent<PlayerAttack>();
            _input = GetComponent<PlayerInputController>();
        }

        private void OnEnable()
        {
            _health.Died += OnDied;
        }

        private void OnDisable()
        {
            _health.Died -= OnDied;
        }

        private void OnDied()
        {
            _movement.enabled = false;
            _attack.enabled = false;
            _input.enabled = false;
        }
    }
}

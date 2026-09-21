using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace DungeonCrawler
{
    public class PlayerInputController : MonoBehaviour
    {
        [SerializeField] private PlayerInput playerInput;

        public UnityEvent<Vector2> InputMoved { get; } = new();
        public UnityEvent InputDodged { get; } = new();
        public UnityEvent InputAttacked { get; } = new();

        private void OnMove(InputValue inputValue)
        {
            InputMoved.Invoke(inputValue.Get<Vector2>());
        }

        private void OnAttack(InputValue inputValue)
        {
            InputAttacked.Invoke();
        }

        private void OnDodge(InputValue inputValue)
        {
            InputDodged.Invoke();
        }
    }
}
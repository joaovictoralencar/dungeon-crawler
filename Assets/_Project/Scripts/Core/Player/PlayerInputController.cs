using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DungeonCrawler.Core.Player
{
    public class PlayerInputController : MonoBehaviour
    {
        public event Action<Vector2> MoveRequested;
        public event Action DodgeRequested;
        public event Action<AttackInputType> AttackRequested;

        private void OnMove(InputValue inputValue)
        {
            MoveRequested?.Invoke(inputValue.Get<Vector2>());
        }

        private void OnAttack(InputValue inputValue)
        {
            AttackRequested?.Invoke(AttackInputType.Light);
        }

        private void OnDodge(InputValue inputValue)
        {
            DodgeRequested?.Invoke();
        }
    }
}
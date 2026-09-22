using UnityEngine;

namespace DungeonCrawler.Player
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(PlayerInputController))]
    [RequireComponent(typeof(PlayerMovement))]
    [RequireComponent(typeof(PlayerAttack))]
    [RequireComponent(typeof(PlayerHealth))]
    [RequireComponent(typeof(PlayerAnimations))]
    public class Player : MonoBehaviour
    {
        public float GameSpeed = 1f;

        private void Update()
        {
            Time.timeScale = GameSpeed;
        }
    }
}
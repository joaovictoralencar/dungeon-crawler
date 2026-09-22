using UnityEngine;

namespace DungeonCrawler.Player
{
    public class Player : MonoBehaviour
    {
        public float GameSpeed = 1f;

        private void Update()
        {
            Time.timeScale = GameSpeed;
        }
    }
}
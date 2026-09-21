using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace DungeonCrawler.Player
{
    public class Player : MonoBehaviour
    {
        [MinMaxSlider(0, 1)] public float GameSpeed = 1f;

        private void Update()
        {
            Time.timeScale = GameSpeed;
        }
    }
}
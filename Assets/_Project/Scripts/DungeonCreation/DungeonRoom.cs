using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace DungeonCrawler
{
    public class DungeonRoom : MonoBehaviour
    {
        [SerializeField] private DungeonDoor[] Doors;
        [SerializeField] private float Width;
        [SerializeField] private float Depth;

        public Vector2Int Position { get; private set; }
        private Dictionary<Directions, DungeonRoom> NeighborsByDirection = new();

        public void SetPosition(Vector2Int position)
        {
            Position = position;
            transform.localPosition = new Vector3(Position.x * Width, 0, Position.y * Depth);
        }

        public void SetNeighbor(Directions direction, DungeonRoom room)
        {
            NeighborsByDirection[direction] = room;
        }

        public void ClearNeighbors()
        {
            NeighborsByDirection.Clear();
        }

        public void UpdateDoorBasedOnNeighbors()
        {
            if (Doors == null) return;

            foreach (DungeonDoor doorData in Doors)
            {
                bool hasNeighbor = NeighborsByDirection.ContainsKey(doorData.Direction);

                if (doorData.Door != null)
                {
                    doorData.Door.SetActive(hasNeighbor);
                }

                if (doorData.Wall != null)
                {
                    doorData.Wall.SetActive(!hasNeighbor);
                }
            }
        }
    }

    [Serializable]
    public struct DungeonDoor
    {
        public Directions Direction;
        public GameObject Door;
        public GameObject Wall;
    }
}
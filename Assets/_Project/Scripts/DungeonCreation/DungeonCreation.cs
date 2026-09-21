using System;
using System.Collections.Generic;
using HelloDev.Utils;
using UnityEngine;
using Sirenix.OdinInspector;

namespace DungeonCrawler
{
    public class DungeonCreation : MonoBehaviour
    {
        [SerializeField] private Transform _dungeonParent;
        [SerializeField] private DungeonRoom _startRoom;
        [SerializeField] private DungeonRoom _emptyRoom;
        [SerializeField] private DungeonRoom _endRoom;
        [SerializeField] private int maxRooms = 20;

        [Header("Branching Control")]
        [SerializeField, Range(0f, 1f), Tooltip("0 = Pure linear corridor (DFS), 1 = Branchy/bushy dungeon (BFS/Random)")]
        private float branchChance = 0.4f;

        private readonly Dictionary<Vector2Int, DungeonRoom> Rooms = new();

        private void Start()
        {
            GenerateDungeon();
        }

        [Button]
        private void GenerateDungeon()
        {
            ResetDungeonState();

            if (!ValidatePrefabs()) return;

            SpawnStartRoom();
            BuildDungeonLayout();
            PlaceEndRoomAtFurthestRoom();
            ConfigureRoomConnections();

            Debug.Log($"<color=cyan>=== DUNGEON GENERATION COMPLETE. Total Rooms: {Rooms.Count} ===</color>");
        }

        private void ResetDungeonState()
        {
            Rooms.Clear();
            _dungeonParent.DestroyAllChildren();
            Debug.Log("<color=cyan>=== STARTING DUNGEON GENERATION ===</color>");
        }

        private bool ValidatePrefabs()
        {
            bool isValid = true;
            if (_startRoom == null) { Debug.LogError("[ERROR] StartRoom Prefab missing!"); isValid = false; }
            if (_emptyRoom == null) { Debug.LogError("[ERROR] EmptyRoom Prefab missing!"); isValid = false; }
            if (_endRoom == null) { Debug.LogError("[ERROR] EndRoom Prefab missing!"); isValid = false; }
            return isValid;
        }

        private void SpawnStartRoom()
        {
            DungeonRoom startRoom = SpawnRoomPrefab(_startRoom, Vector2Int.zero);
            Debug.Log($"<color=green>[START ROOM]</color> Grid Pos: {startRoom.Position} | World Pos: {startRoom.transform.localPosition}");
        }

        private void BuildDungeonLayout()
        {
            List<Vector2Int> activePositions = new List<Vector2Int> { Vector2Int.zero };

            while (Rooms.Count < maxRooms && activePositions.Count > 0)
            {
                int index = SelectActivePositionIndex(activePositions.Count);
                Vector2Int currentPos = activePositions[index];
                List<Directions> freeDirections = GetFreeDirections(currentPos);

                if (freeDirections.Count > 0)
                {
                    Directions chosenDir = freeDirections[UnityEngine.Random.Range(0, freeDirections.Count)];
                    Vector2Int newPos = currentPos + DirectionToVector(chosenDir);

                    SpawnRoomPrefab(_emptyRoom, newPos);
                    activePositions.Add(newPos);
                }
                else
                {
                    activePositions.RemoveAt(index);
                }
            }
        }

        private int SelectActivePositionIndex(int count)
        {
            return (UnityEngine.Random.value < branchChance)
                ? UnityEngine.Random.Range(0, count)
                : count - 1;
        }

        private DungeonRoom SpawnRoomPrefab(DungeonRoom prefab, Vector2Int gridPos)
        {
            DungeonRoom newRoom = Instantiate(prefab, _dungeonParent);
            newRoom.SetPosition(gridPos);
            Rooms.Add(gridPos, newRoom);
            return newRoom;
        }

        private void PlaceEndRoomAtFurthestRoom()
        {
            if (Rooms.Count <= 1) return;

            Vector2Int furthestPos = FindFurthestGridPosition();

            if (Rooms.TryGetValue(furthestPos, out DungeonRoom roomToReplace))
            {
                DestroyRoomObject(roomToReplace.gameObject);
                Rooms.Remove(furthestPos);

                DungeonRoom endRoom = SpawnRoomPrefab(_endRoom, furthestPos);
                Debug.Log($"<color=magenta>[END ROOM PLACED]</color> Grid Pos: {furthestPos} | World Pos: {endRoom.transform.localPosition}");
            }
        }

        private Vector2Int FindFurthestGridPosition()
        {
            Vector2Int furthestPos = Vector2Int.zero;
            int maxDistance = -1;

            foreach (var kvp in Rooms)
            {
                Vector2Int pos = kvp.Key;
                if (pos == Vector2Int.zero) continue;

                int distance = Mathf.Abs(pos.x) + Mathf.Abs(pos.y);
                if (distance > maxDistance)
                {
                    maxDistance = distance;
                    furthestPos = pos;
                }
            }

            return furthestPos;
        }

        private void DestroyRoomObject(GameObject obj)
        {
#if UNITY_EDITOR
            DestroyImmediate(obj);
#else
            Destroy(obj);
#endif
        }

        private void ConfigureRoomConnections()
        {
            foreach (var kvp in Rooms)
            {
                Vector2Int pos = kvp.Key;
                DungeonRoom room = kvp.Value;

                room.ClearNeighbors();

                foreach (Directions dir in Enum.GetValues(typeof(Directions)))
                {
                    Vector2Int neighborPos = pos + DirectionToVector(dir);
                    if (Rooms.TryGetValue(neighborPos, out DungeonRoom neighborRoom))
                    {
                        room.SetNeighbor(dir, neighborRoom);
                    }
                }

                room.UpdateDoorBasedOnNeighbors();
            }
        }

        private List<Directions> GetFreeDirections(Vector2Int origin)
        {
            List<Directions> freeDirs = new();
            foreach (Directions dir in Enum.GetValues(typeof(Directions)))
            {
                if (!Rooms.ContainsKey(origin + DirectionToVector(dir)))
                {
                    freeDirs.Add(dir);
                }
            }
            return freeDirs;
        }

        private Vector2Int DirectionToVector(Directions direction)
        {
            switch (direction)
            {
                case Directions.Up:    return new Vector2Int(0, 1);
                case Directions.Right: return new Vector2Int(1, 0);
                case Directions.Down:  return new Vector2Int(0, -1);
                case Directions.Left:  return new Vector2Int(-1, 0);
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }
        }
    }

    public enum Directions
    {
        Up,
        Right,
        Down,
        Left
    }
}
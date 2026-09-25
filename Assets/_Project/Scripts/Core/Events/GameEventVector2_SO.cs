using UnityEngine;
using HelloDev.Events;

namespace DungeonCrawler.Core.Events
{
    [CreateAssetMenu(fileName = "GameEventVector2", menuName = "HelloDev/Events/Vector2 Game Event")]
    public sealed class GameEventVector2_SO : GameEvent_SO<Vector2>
    {
    }
}

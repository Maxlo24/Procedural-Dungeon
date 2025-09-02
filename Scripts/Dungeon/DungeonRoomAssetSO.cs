using System.Collections.Generic;
using UnityEngine;

namespace Generator.Dungeon
{
    [CreateAssetMenu(fileName = "DungeonRoomPreset", menuName = "ScriptableObjects/Dungeon/RoomPreset", order = 3)]
    public class DungeonRoomAssetSO : ScriptableObject
    {
        public List<Room> fillRooms = new List<Room>();

        public List<Room> requiredRooms = new List<Room>();

    }
}
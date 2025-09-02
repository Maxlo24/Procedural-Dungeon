using System.Collections.Generic;
using UnityEngine;

namespace Generator.Dungeon
{
    public enum DecorationTag
    {
        Barrel,
        Bed,
        Bench,
        Bones,
        Book,
        Bottle,
        Carpet,
        Chair,
        Candle,
        Chest,
        Decoration,
        Fence,
        Fireplace,
        Food,
        Fork,
        Goblet,
        Key,
        knife,
        Machine,
        Mess,
        Pillar,
        Plate,
        Rock,
        Shelf,
        Storage,
        Table,
        Treasure,
        Vegetation,
        Weapon,
        Wood,
    }

    public class Decoration : MonoBehaviour
    {
        [SerializeField] private bool m_networkObject;
        [SerializeField] private RoomType m_validRooms;
        [SerializeField] private SurfaceTag m_validSurfaces;
        [SerializeField] private bool m_tagCombinationRequired;
        [SerializeField] private DecorationTag m_tag;
        [SerializeField] private bool m_checkForCollision;

        public bool IsNetworkeObject { get { return m_networkObject; } }
        public List<RoomType> Rooms { get { return VoxelGrid.TypeFlagToList(m_validRooms); } }
        public List<SurfaceTag> Surfaces { get { return VoxelGrid.TypeFlagToList(m_validSurfaces); } }
        public SurfaceTag SurfaceTag { get { return m_validSurfaces; } }
        public bool CombinationRequired { get { return m_tagCombinationRequired; } }
        public DecorationTag Tag { get { return m_tag; } }
        public bool CheckForCollision { get { return m_checkForCollision; } }
    }
}
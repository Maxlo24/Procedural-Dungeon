using System.Collections.Generic;
using UnityEngine;

namespace Generator.Dungeon
{
    public enum TileType
    {
        Wall,
        Ceiling,
        Floor,
        Oppening,
        Door,
        Stairs,
        DeadEnd,
        Separation,
    }

    public enum TileStyle
    {
        Stone,
        Wood,
        Cell,
        Window,
    }

    public class Tile : MonoBehaviour
    {
        [SerializeField] private bool m_networkObject;
        [SerializeField] private RoomType m_validRooms;
        [SerializeField] private TileType m_type;
        [SerializeField] private TileStyle m_style;
        [SerializeField] private Room m_parentRoom;

        public bool IsNetworkeObject { get { return m_networkObject; } }
        public List<RoomType> Rooms { get { return VoxelGrid.TypeFlagToList(m_validRooms); } }
        public TileType Type { get { return m_type; } }
        public TileStyle Style { get { return m_style; } }
        public Room Parent {  get { return m_parentRoom; } set {  m_parentRoom = value; } }

        public void TurnIntoTileSpawner()
        {
            GameObject _newTileSpawnerObj = new GameObject("New TileSpawner " + m_type.ToString());
            _newTileSpawnerObj.AddComponent<TileSpawner>().Type = m_type;

            Vector3 _finalPos = this.transform.position;

            if (m_type == TileType.Wall || m_type == TileType.Oppening || m_type == TileType.Door)
            {
                _finalPos += 0.5f * Vector3.up * VoxelGrid.VOXEL_SIZE;
            }

            _newTileSpawnerObj.transform.position = _finalPos;
            _newTileSpawnerObj.transform.rotation = this.transform.rotation;
            m_parentRoom.AddTileSpawner(_newTileSpawnerObj.GetComponent<TileSpawner>());

            DestroyImmediate(this.gameObject);
        }

        public void TurnIntoTileSpawnerWithActualStyleAsPrefered()
        {
            GameObject _newTileSpawnerObj = new GameObject("New TileSpawner");
            _newTileSpawnerObj.AddComponent<TileSpawner>().Type = m_type;
            _newTileSpawnerObj.GetComponent<TileSpawner>().SetPreferedStyle(m_style);

            Vector3 _finalPos = this.transform.position;

            if (m_type == TileType.Wall || m_type == TileType.Oppening || m_type == TileType.Door)
            {
                _finalPos += 0.5f * Vector3.up * VoxelGrid.VOXEL_SIZE;
            }

            _newTileSpawnerObj.transform.position = _finalPos;
            _newTileSpawnerObj.transform.rotation = this.transform.rotation;
            m_parentRoom.AddTileSpawner(_newTileSpawnerObj.GetComponent<TileSpawner>());

            DestroyImmediate(this.gameObject);
        }
    }
}
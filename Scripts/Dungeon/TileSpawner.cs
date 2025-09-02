using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

namespace Generator.Dungeon
{
    public class TileSpawner : MonoBehaviour
    {
        [SerializeField] private TileType m_tileType;
        [SerializeField] private int m_spawnProbability = 100;
        [SerializeField] private bool m_spawnDecoration = false;
        [SerializeField] private List<TileStyle> m_preferedStyle = new List<TileStyle>();

        public TileType Type { get { return m_tileType; } set { m_tileType = value; } }
        public int Probability { get { return m_spawnProbability; } set { m_spawnProbability = value; } }
        public bool SpawnDecoration { get { return m_spawnDecoration; } set { m_spawnDecoration = value; } }
        public List<TileStyle> PreferedDecoration { get { return m_preferedStyle; } }

        private Room m_parentRoom;
        private TileBankManager m_tileBankManager;

        public void SetPreferedStyle(TileStyle _style)
        {
                m_preferedStyle.Add(_style);
        }

        public void SpawnTile(Room _room, DRandom _random, TileBankManager _tileBank, GameObject _container)
        {
            m_parentRoom = _room;
            m_tileBankManager = _tileBank;

            if (_random.range(0, 100) <= m_spawnProbability)
            {
                List<Tile> _tilePool = new List<Tile>();

                if (m_preferedStyle.Count > 0)
                    _tilePool = _tileBank.GetTilesWithStyles(_room.Type, m_tileType, m_preferedStyle);
                else
                    _tilePool = _tileBank.GetTileByRoomAndTileType(_room.Type, m_tileType);

                if (_tilePool.Count > 0)
                {
                    Tile _tileToSpawn = _tilePool[_random.range(0, _tilePool.Count - 1)];
                    SpawnTile(_random, _tileToSpawn, _container);
                }
            }
        }

        private void SpawnTile(DRandom _random, Tile _tileToSpawn, GameObject _container)
        {
            Vector3 _finalPos = this.transform.position;
            Quaternion _finalRotation = this.transform.rotation;

            if (m_tileType == TileType.Wall || m_tileType == TileType.Oppening || m_tileType == TileType.Door || m_tileType == TileType.DeadEnd)
            {
                _finalPos += 0.5f * Vector3.down * VoxelGrid.VOXEL_SIZE;
            }

            //bool _networkIsActive = NetworkObjectSpawner.Instance != null;
            //if (_networkIsActive && _tileToSpawn.IsNetworkeObject)
            //{
            //    SpawnOnNetwork(_tileToSpawn.gameObject, _finalPos, _finalRotation.eulerAngles);
            //}
            //else
            //{
                GameObject _instance = Instantiate(_tileToSpawn.gameObject, _finalPos, _finalRotation);
                _instance.transform.parent = _container.transform;

                DecorationSpawner _instanceSpawner;
                _instance.gameObject.TryGetComponent<DecorationSpawner>(out _instanceSpawner);
                if (_instanceSpawner != null && m_spawnDecoration)
                {
                    m_parentRoom.AddDecorationSpawwner(_instanceSpawner);
                }
            //}
        }

        //private void SpawnOnNetwork(GameObject _objectToSpawn, Vector3 _position, Vector3 _rotation)
        //{
        //    if (NetworkObjectSpawner.Instance.IsServer)
        //    {
        //        int _prefabIndex = NetworkObjectSpawner.Instance.GetIndexOf(_objectToSpawn);
        //        if (_prefabIndex == -1)
        //        {
        //            Debug.Log("DecorationSpawner :: Cant spawn " + _objectToSpawn.name);
        //            return;
        //        }
        //        NetworkObjectSpawner.Instance.InstantiateNetworkObjectServerRpc(_prefabIndex, _position, _rotation);
        //    }
        //}

        public void OnDrawGizmos()
        {
            if (VoxelGrid.DRAW_SPAWNPOINTS)
            {
                Color _col = new Color(0, 1, 0, 0.5f);

                Gizmos.color = _col;
                Gizmos.DrawSphere(transform.position, 0.2f);
                DrawArrow.ForGizmo(transform.position, transform.forward * 1);
            }
        }
    }
}
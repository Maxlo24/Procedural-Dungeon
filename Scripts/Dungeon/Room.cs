using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Generator.Dungeon
{
    [System.Flags]
    public enum RoomType
    {
        None = 0,
        Hall = 1,
        Kitchen = 2,
        Bedroom = 4,
        Torture = 8,
        Treasure = 16,
        Office = 32,
        Diningroom = 64,
        Basement = 128,
        Attic = 256,
        Cave = 512,
        All = ~0,
    }

    [System.Flags]
    public enum SurfaceTag
    {
        None = 0,
        Floor = 1,
        Wall = 2,
        Ceiling = 4,
        Table = 8,
        Shelf = 16,
        Balcony = 32,
        All = ~0,
    }

    [RequireComponent(typeof(Volume),typeof(RoomRenderer))]
    public class Room: MonoBehaviour
    {
        [Range(0, 1)]
        [SerializeField] private float m_generationStage; // How deep it sould be in the dungeon
        [SerializeField] private RoomType m_possibleRoomType = RoomType.All;
        [SerializeField] private bool m_generateTilesRuntime = true;
        [SerializeField] private List<TileSpawner> m_linkTileSpawners = new List<TileSpawner>();
        [SerializeField] private List<TileSpawner> m_tileSpawners = new List<TileSpawner>();
        [SerializeField] private List<DecorationSpawner> m_decorationSpawners = new List<DecorationSpawner>();

        private Volume m_volume;
        private RoomRenderer m_roomRenderer;
        private ProceduralDungeon m_proceduralDungeon;
        [SerializeField] private RoomType m_roomType;
        private RoomRenderer[] m_neighborRoomRenderes;
        private bool m_inRoom;

        //Containers
        private GameObject m_basicTileContainer;
        private GameObject m_tileSpawnerContainer;
        private GameObject m_spawnedLinkTilesContainer;
        private GameObject m_spawnedTilesContainer;
        private GameObject m_decorationContainer;
        private GameObject m_monsterSpawnPointContainer;

        [Header("Editor")]
        [SerializeField] DungeonAssetSO d_dungeonAssetSO;

        public float GenerationStage { get { return m_generationStage; } }
        public RoomType Type { get { return m_roomType; } }
        public Bounds Bounds { get { return m_volume.Bounds; } }

        public void EnterRoom()
        {
            if (m_inRoom) return;
            m_proceduralDungeon.SwitchActiveRoom(this);
            m_inRoom = true;
        }
        public void ExitRoom()
        {
            m_inRoom = false;
        }

        public List<RoomRenderer> GetRoomsRenderer()
        {
            List<RoomRenderer> _out = m_neighborRoomRenderes.ToList();
            _out.Add(m_roomRenderer);
            return _out;
        }

        public void AddLinkTileSpawner(TileSpawner _tileSpawner)
        {
            m_linkTileSpawners.Add(_tileSpawner);
        }

        public void AddTileSpawner(TileSpawner _tileSpawner)
        {
            _tileSpawner.transform.parent = this.TilesSpawnerContainer.transform;
            m_tileSpawners.Add(_tileSpawner);
        }

        public void AddDecorationSpawwner(DecorationSpawner decorationSpawner)
        {
            m_decorationSpawners.Add(decorationSpawner);
        }

        public void FillRoom(ProceduralDungeon proceduralDungeon, DRandom _random, DecorationBankManager _decorationBank, TileBankManager _tileBank, bool _defaultVisibility = false)
        {
            m_proceduralDungeon = proceduralDungeon;
            SetRoomType(_random);

            GetNeighborRooms();
            GenerateTiles(_random, _tileBank);
            GenerateDecoration(_random, _decorationBank);
            RoomRenderer.BatchSpawnedObjects(SpawnedTilesContainer, DecorationContainer);
            RoomRenderer.FindAllMeshRenderers();
            RoomRenderer.SwitchRoomVisibility(_defaultVisibility);
        }

        private void GetNeighborRooms()
        {
            List<RoomRenderer> _neighbors = new List<RoomRenderer>();
            Volume[] _neighboreVolumes = m_volume.GetNeighbors(2).ToArray();
            foreach(Volume _neighborsVolume in _neighboreVolumes)
                _neighbors.Add(_neighborsVolume.GetComponent<RoomRenderer>());

            m_neighborRoomRenderes = _neighbors.ToArray();
        }

        private void SetRoomType(DRandom _random)
        {
            m_volume = gameObject.GetComponent<Volume>();

            List<RoomType> _possibleRoomType = VoxelGrid.TypeFlagToList(m_possibleRoomType);
            m_roomType = _possibleRoomType[_random.range(0, _possibleRoomType.Count - 1)];
        }

        private void GenerateTiles(DRandom _random, TileBankManager _tileBank)
        {
            DestroyImmediate(this.SpawnedLinkTilesContainer);

            foreach (VolumeHardLink _hardLink in m_volume.HardLinks)
                _hardLink.UpdateTileStatus(_random);

            foreach (VolumeSoftLink _hardLink in m_volume.SoftLinks)
                _hardLink.UpdateTileStatus(_random);

            foreach (TileSpawner _tileSpawner in m_linkTileSpawners)
                if(_tileSpawner != null)
                    _tileSpawner.SpawnTile(this, _random, _tileBank, SpawnedLinkTilesContainer);


            if (!m_generateTilesRuntime) return;

            foreach (TileSpawner _tileSpawner in m_tileSpawners)
                if (_tileSpawner != null)
                    _tileSpawner.SpawnTile(this, _random, _tileBank, SpawnedTilesContainer);
        }

        private void GenerateDecoration(DRandom _random, DecorationBankManager _decorationBank)
        {
            DecorationSpawner _thisDecoSpawner;
            gameObject.TryGetComponent<DecorationSpawner>(out _thisDecoSpawner);
            if (_thisDecoSpawner != null)
                _thisDecoSpawner.SpawnObjects(this, _decorationBank, _random);
            foreach (DecorationSpawner _decoSpawner in m_decorationSpawners)
            {
                _decoSpawner.SpawnObjects(this, _decorationBank, _random);
            }
        }
#if UNITY_EDITOR

        public void TestGeneration()
        {
            Clear();

            DRandom _random = new DRandom(UnityEngine.Random.Range(0, int.MaxValue));
            TileBankManager _tileBank = new TileBankManager();
            DecorationBankManager _decorationBank = new DecorationBankManager();

            _tileBank.SortTiles(d_dungeonAssetSO.tilesPresets);
            _decorationBank.SortDecorations(d_dungeonAssetSO.decorationPresets);

            SetRoomType(_random);
            GenerateTiles(_random, _tileBank);
            GenerateDecoration(_random, _decorationBank);
        }

        public void GenerateBasicTilesEditor()
        {
            ClearBasicTiles();
            DRandom _random = new DRandom(UnityEngine.Random.Range(0, int.MaxValue));
            TileBankManager _tileBank = new TileBankManager();
            _tileBank.SortTiles(d_dungeonAssetSO.tilesPresets);

            SetRoomType(_random);

            m_volume.DetermineOutsideFacesAndRecalculateBounds();
            VolumeFaceData _volumeFaceData = m_volume.FaceData;

            List<Tile> _tilePool = new List<Tile>();
            foreach (Vector3 _direction in _volumeFaceData.faces.Keys)
            {
                TileType _tyleType = TileType.Wall;
                Quaternion _rotation = Quaternion.identity;
                Vector3 _offset = Vector3.zero;

                if (_direction == Vector3.down)
                    _tyleType = TileType.Floor;
                else if (_direction == Vector3.up)
                    _tyleType = TileType.Ceiling;
                else if (VoxelGrid.CheckIfFaceIsWall(_direction))
                {
                    _tyleType = TileType.Wall;
                    _rotation = Quaternion.LookRotation(-1.0f * _direction, Vector3.up);
                    _offset = 0.5f * Vector3.down * VoxelGrid.VOXEL_SIZE;
                }

                _tilePool = _tileBank.GetTileByRoomAndTileType(m_roomType, _tyleType);

                foreach (Vector3 _facePos in _volumeFaceData.faces[_direction])
                {
                    Tile _selectedTile = _tilePool[_random.range(0, _tilePool.Count - 1)];
                    GameObject _newTile = PrefabUtility.InstantiatePrefab(_selectedTile.gameObject) as GameObject;

                    _newTile.transform.position = _facePos + _offset;
                    _newTile.transform.rotation = _rotation;
                    _newTile.transform.parent = BasicTileContainer.transform;
                    _newTile.GetComponent<Tile>().Parent = this;
                }
            }
        }

        public void AddMonsterSpawnPoint()
        {
            GameObject _newSpawnPoint = new GameObject("New MonsterSpawnPoint");
            _newSpawnPoint.transform.position = transform.position;
            _newSpawnPoint.transform.parent = MonsterSpawnPointContainer.transform;
            _newSpawnPoint.AddComponent<MonsterSpawnPoint>();
        }

#endif

        public void TurnAllBasicTilesInTileSpawner()
        {
            int _childNbr = BasicTileContainer.transform.childCount;
            for (int i = 0; i< _childNbr; i++)
            {
                BasicTileContainer.transform.GetChild(0).GetComponent<Tile>().TurnIntoTileSpawner();
            }
        }
        public RoomRenderer RoomRenderer
        {
            get
            {
                if(m_roomRenderer == null)
                    m_roomRenderer = GetComponent<RoomRenderer>();
                return m_roomRenderer;
            }
        }

        public GameObject BasicTileContainer
        {
            get
            {
                if (m_basicTileContainer == null)
                    m_basicTileContainer = VoxelGrid.CheckContainerExistance(transform, "BasicTiles");
                return m_basicTileContainer;

            }
        }
        public GameObject TilesSpawnerContainer
        {
            get
            {
                if (m_tileSpawnerContainer == null)
                    m_tileSpawnerContainer = VoxelGrid.CheckContainerExistance(transform, "TileSpawners");
                return m_tileSpawnerContainer;
            }
        }
        public GameObject SpawnedTilesContainer
        {
            get
            {
                if (m_spawnedTilesContainer == null)
                    m_spawnedTilesContainer = VoxelGrid.CheckContainerExistance(transform, "SpawnedTiles");
                return m_spawnedTilesContainer;
            }
        }
        public GameObject SpawnedLinkTilesContainer
        {
            get
            {
                if (m_spawnedLinkTilesContainer == null)
                    m_spawnedLinkTilesContainer = VoxelGrid.CheckContainerExistance(transform, "SpawnedLinkTiles");
                return m_spawnedLinkTilesContainer;
            }
        }
        public GameObject DecorationContainer
        {
            get
            {
                if (m_decorationContainer == null)
                    m_decorationContainer = VoxelGrid.CheckContainerExistance(transform, "SpawnedDecorations");
                return m_decorationContainer;
            }
        }
        public GameObject MonsterSpawnPointContainer
        {
            get
            {
                if (m_monsterSpawnPointContainer == null)
                    m_monsterSpawnPointContainer = VoxelGrid.CheckContainerExistance(transform, "MonsterSpawnPoints");
                return m_monsterSpawnPointContainer;
            }
        }

        public void Clear()
        {
            m_decorationSpawners.Clear();
            DestroyImmediate(this.DecorationContainer);
            DestroyImmediate(this.SpawnedTilesContainer);
            DestroyImmediate(this.SpawnedLinkTilesContainer);
        }
        public void ClearBasicTiles()
        {
            DestroyImmediate(this.BasicTileContainer);
        }

        public void ToggleSpawnPointView()
        {
            VoxelGrid.DRAW_SPAWNPOINTS = !VoxelGrid.DRAW_SPAWNPOINTS;
        }
    }


}
using System.Collections.Generic;
using UnityEngine;
using Unity.AI.Navigation;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif


namespace Generator.Dungeon
{
    public class ProceduralDungeon : MonoBehaviour
    {

        [Header("Generation Settings")]
        [SerializeField] private Vector3 m_dungeonMaxSize = new Vector3(100, 50, 100);
        [SerializeField] private DungeonAssetSO m_dungeonAsset;
        [SerializeField] private int m_targetRoomNumber;
        [SerializeField] private int m_minRoomNumber;
        [SerializeField] private int m_maxRoomNumber;
        [SerializeField] private int m_seed = 0;
        [SerializeField] private bool m_useRandomSeed;
        [SerializeField] private bool m_tryEveryPossibility;
        [SerializeField] private bool m_roomVisible;
        [SerializeField] private bool m_buildNavMesh;

        [Header("Generation Speed")]
        [SerializeField] private bool m_useCoroutine;
        [SerializeField] private float m_generationDelay;

        [Header("Result")]
        [SerializeField] private int m_maxIterations = 5000;
        [SerializeField] private int m_iterations;
        [SerializeField] private int m_generatedRoomCount;


        //List
        private List<Volume> m_volumes;
        private HashSet<Vector3> m_usedVoxels;
        [SerializeField] private List<Room> m_requiredRooms;

        //Dictionary
        private Dictionary<Vector3, VolumeHardLink> m_dicUnconnectedHardLinks;
        private Dictionary<Vector3, VolumeSoftLink> m_dicExposedSoftLinks;

        //Queue
        private Queue<Volume> m_volumsToWorkOn;

        //Variables
        [SerializeField] private List<NavMeshSurface> m_navMeshSurfaces;
        private DRandom m_random;
        private VolumeBankManager m_volumeBankManager;
        private TileBankManager m_tileBankManager;
        private DecorationBankManager m_decorationBankManager;
        private Bounds m_dungeonAreaBound;
        private bool m_generatingEndVolumes;

        //Runtime
        private Room m_activeRoom;
        private GameObject m_spawnedVolumesContainer;
        private float m_origineAltitude;

        //Getters setters
        public int Seed { get { return m_seed; } set { m_seed = value; } }
        public bool UseRandomeSeed { get { return m_useRandomSeed; } set { m_useRandomSeed = value; } }

        public GameObject SpawnedVolumeContainer
        {
            get
            {
                if (m_spawnedVolumesContainer == null)
                    m_spawnedVolumesContainer = VoxelGrid.CheckContainerExistance(transform, "SpawnedVolumes");
                return m_spawnedVolumesContainer;
            }
        }

        private void Init()
        {
            //Initiate variables
            m_generatingEndVolumes = false;
            m_iterations = 0;
            m_generatedRoomCount = 0;
            m_dungeonAreaBound.center = transform.position;
            m_dungeonAreaBound.size = m_dungeonMaxSize;
            m_origineAltitude = transform.position.z;

            //Initiate collections
            m_random = new DRandom(m_seed);
            m_volumes = new List<Volume>();
            m_usedVoxels = new HashSet<Vector3>();
            m_dicUnconnectedHardLinks = new Dictionary<Vector3, VolumeHardLink>();
            m_dicExposedSoftLinks = new Dictionary<Vector3, VolumeSoftLink>();
            m_volumsToWorkOn = new Queue<Volume>();
            m_requiredRooms = new List<Room>(m_dungeonAsset.roomPreset.requiredRooms);

            //Generate assets banks
            m_volumeBankManager = new VolumeBankManager(m_origineAltitude);
            m_volumeBankManager.GenerateVolumeBank(m_dungeonAsset);
            m_tileBankManager = new TileBankManager();
            m_tileBankManager.SortTiles(m_dungeonAsset.tilesPresets);
            m_decorationBankManager = new DecorationBankManager();
            m_decorationBankManager.SortDecorations(m_dungeonAsset.decorationPresets);
        }


        /* Runtime */
        public void SwitchActiveRoom(Room _newActiveRoom)
        {
            List<RoomRenderer> _oldRoomRenderers = m_activeRoom.GetRoomsRenderer();
            List<RoomRenderer> _newRoomRenderers = _newActiveRoom.GetRoomsRenderer();
            List<RoomRenderer> _roomToHide = new List<RoomRenderer>();
            List<RoomRenderer> _roomToShow = new List<RoomRenderer>();

            foreach (RoomRenderer _room in _newRoomRenderers)
                if (!_oldRoomRenderers.Contains(_room))
                    _roomToShow.Add(_room);

            foreach (RoomRenderer _room in _oldRoomRenderers)
                if (!_newRoomRenderers.Contains(_room))
                    _roomToHide.Add(_room);

            foreach (RoomRenderer _room in _roomToShow)
                _room.SwitchRoomVisibility(true);

            foreach (RoomRenderer _room in _roomToHide)
                _room.SwitchRoomVisibility(false);

            m_activeRoom.ExitRoom();
            m_activeRoom = _newActiveRoom;
        }


        /* Generation */
        public void GenerateDungeon()
        {
            if (m_useRandomSeed) SetRandomSeed();
            Init();

            Debug.Log("Starting dungeon generation");
            Debug.Log("Using seed :" + m_random.seed);
            DDebugTimer.Start();

            try
            {
                TryGeneration();
            }
            catch (System.Exception ex)
            {
                // This will catch the exception thrown by AddIteration() or any other unexpected error.
                Debug.LogError("An exception occurred during dungeon generation, and the process has been stopped. See the message below for details:");
                Debug.LogException(ex, this); // This logs the exception with its full stack trace for easier debugging.

                // Stop everything and clean up to prevent Unity from freezing
                StopAllCoroutines();
                Clear();
            }
        }

        private void TryGeneration()
        {
            Clear();
            SetStartVolume();
            if (m_useCoroutine)
            {
                StartCoroutine(GenerateVolumesCoroutine());
            }
            else
            {
                GenerateVolumes();
                GenerateEndVolumes();
                if (m_generatedRoomCount < m_minRoomNumber || m_requiredRooms.Count > 0)
                {
                    TryGeneration();
                }
                else
                {
                    TagAllUnusedHardLinkAsUnconnected();
                    FillVolumes();
                    if (m_buildNavMesh) BakeNavmeshSurfaces();
                    GenerationDone();
                }
            }
        }

        private void SetStartVolume()
        {
            m_generatingEndVolumes = false;
            List<Volume> _spawnPool = m_volumeBankManager.GetVolumeOfType(VolumeType.Spawn);

            int _index = m_random.range(0, _spawnPool.Count - 1);
            GameObject _volume = Instantiate(_spawnPool[_index].gameObject);
            Volume _newVolume = _volume.GetComponent<Volume>();
            m_activeRoom = _volume.GetComponent<Room>();

            _volume.transform.position = transform.position;
            _volume.transform.parent = SpawnedVolumeContainer.transform;

            _newVolume.Voxels = VoxelGrid.TranslateVoxels(_newVolume.Voxels, transform.position);
            AddVolume(_newVolume);

            AddIteration();
        }

        private void GenerateVolumes()
        {
            while ((m_generatedRoomCount < m_targetRoomNumber || m_requiredRooms.Count > 0) && m_volumsToWorkOn.Count > 0)
            {
                Volume _VolumeToWorkOn = PickNextVolumeToWorkOn();
                if (_VolumeToWorkOn == null) break; //We worked on all the generated volumes

                Queue<VolumeHardLink> _doorsToWorkOn = GetShuffledAvailableLinks(_VolumeToWorkOn);
                while (_doorsToWorkOn.Count > 0)
                {
                    VolumeHardLink _doorToWorkOn = _doorsToWorkOn.Dequeue();
                    GenerateNextVolume(_doorToWorkOn);
                }
            }
        }

        private void GenerateNextVolume(VolumeHardLink _connectingLink)
        {
            List<Volume> _preferedVolume = _connectingLink.PeferedVolumePool;
            Queue<Volume> _volumePool = new Queue<Volume>();

            if (_preferedVolume.Count > 0)
            {
                _volumePool = m_volumeBankManager.GetShuffledVolumes(_preferedVolume, m_random);
                TryVolumePool(_connectingLink, _volumePool);
            }
            else
            {

                bool _success = TryRequiredRoom(_connectingLink);

                if (!_success)
                {
                    if (m_tryEveryPossibility)
                        _volumePool = m_volumeBankManager.GetShuffledVolumesOrderedByType(m_random);
                    else
                        _volumePool = m_volumeBankManager.GetShuffledVolumesOfOneRandomType(m_random);
                    TryVolumePool(_connectingLink, _volumePool);
                }
            }
        }

        private void GenerateEndVolumes()
        {
            m_generatingEndVolumes = true;
            List<VolumeHardLink> _availableLinks = new List<VolumeHardLink>(m_dicUnconnectedHardLinks.Values);

            foreach (VolumeHardLink _volumeLinks in _availableLinks)
            {
                TryVolumePool(_volumeLinks, m_volumeBankManager.GetShuffledVolumes(m_volumeBankManager.GetVolumeOfType(VolumeType.End), m_random));
            }
        }

        private bool TryRequiredRoom(VolumeHardLink _connectingLink)
        {
            bool _success = false;

            if (m_requiredRooms.Count > 0)
            {
                float _generationStage = (float)m_generatedRoomCount / m_targetRoomNumber;

                //Debug.Log(m_generatedRoomCount);
                //Debug.Log(m_targetRoomNumber);

                Debug.Log(_generationStage);

                List<Room> _unusedRoom = new List<Room>(m_requiredRooms);

                foreach (Room _room in m_requiredRooms)
                {
                    bool _roomPlaced = false;
                    if (_room.GenerationStage < _generationStage)
                    {
                        Volume _volume = _room.GetComponent<Volume>();
                        _roomPlaced = TryVolumePool(_connectingLink, new Queue<Volume>(new[] { _volume }));

                        Debug.Log("Room placed :" + _roomPlaced);
                    }

                    if (_roomPlaced)
                    {
                        _success = true;
                        _unusedRoom.Remove(_room);
                        break;
                    }

                }
                m_requiredRooms = _unusedRoom;
                Debug.Log("Room left to place :" + m_requiredRooms.Count);

            }

            return _success;

        }

        private bool TryVolumePool(VolumeHardLink _connectingLink, Queue<Volume> _volumePool)
        {
            if (_connectingLink == null)
            {
                Debug.LogWarning("TryVolumePool called with a NULL connecting link. Skipping.");
                return false;
            }

            bool _success = false;
            bool _volumeTestFinished = false;

            while (!_volumeTestFinished && _volumePool.Count > 0)
            {
                Volume _volumeToTest = _volumePool.Dequeue();
                if (_volumeToTest == null)
                {
                    Debug.LogWarning("TryVolumePool: Encountered a NULL volume prefab. Skipping.");
                    continue;
                }

                Queue<VolumeHardLink> _linksPool = GetShuffledAvailableLinksOfSameType(_volumeToTest, _connectingLink);
                if (_linksPool == null || _linksPool.Count == 0)
                {
                    // no valid links → skip to next volume
                    continue;
                }

                bool _linkTestFinished = false;
                while (!_linkTestFinished && _linksPool.Count > 0)
                {
                    VolumeHardLink _linkToTest = _linksPool.Dequeue();
                    if (_linkToTest == null)
                    {
                        Debug.LogWarning($"TryVolumePool: Skipping null link on volume {_volumeToTest.name}");
                        continue;
                    }

                    if (!m_generatingEndVolumes && m_generatedRoomCount >= m_targetRoomNumber && m_requiredRooms.Count == 0)
                    {
                        return false; // Stop adding rooms if we've met our goal
                    }
                    else
                    {
                        if (m_generatingEndVolumes && m_generatedRoomCount == m_maxRoomNumber)
                        {
                            return false;
                        }
                    }

                    List<Vector3> _voxelsToTest = VoxelGrid.ConnectLink(_volumeToTest.Voxels, _linkToTest, _connectingLink);
                    if (_voxelsToTest != null && CheckVoxelOverlap(_voxelsToTest))
                    {
                        Volume _newVolume = InstantiateVolume(_volumeToTest, _linkToTest, _connectingLink, _voxelsToTest);
                        if (_newVolume != null)
                        {
                            AddVolume(_newVolume);
                            _success = true;
                            _linkTestFinished = true;
                            _volumeTestFinished = true;
                        }
                    }

                    AddIteration();
                }
            }

            return _success;
        }


        private void TagAllUnusedHardLinkAsUnconnected()
        {


            foreach (VolumeHardLink _hardLink in m_dicUnconnectedHardLinks.Values)
            {
                _hardLink.Connected = false;
            }
        }

        private void FillVolumes()
        {
            foreach (Volume _volume in m_volumes)
            {
                Room _room = _volume.gameObject.GetComponent<Room>();
                _room.FillRoom(this, m_random, m_decorationBankManager, m_tileBankManager, m_roomVisible);
            }
        }

        private void GenerationDone()
        {
            DLogger.Log("Generation completed in " + DDebugTimer.Lap() + "ms (" + m_iterations + " iterations)", this);

            foreach (RoomRenderer _room in m_activeRoom.GetRoomsRenderer())
                _room.SwitchRoomVisibility(true);
        }


        /* Coroutines */
        IEnumerator GenerateVolumesCoroutine()
        {
            while (m_generatedRoomCount < m_targetRoomNumber && m_volumsToWorkOn.Count > 0)
            {
                Volume _VolumeToWorkOn = PickNextVolumeToWorkOn();
                if (_VolumeToWorkOn == null) break; //We worked on all the generated volumes

                Queue<VolumeHardLink> _doorsToWorkOn = GetShuffledAvailableLinks(_VolumeToWorkOn);
                while (_doorsToWorkOn.Count > 0)
                {
                    VolumeHardLink _doorToWorkOn = _doorsToWorkOn.Dequeue();
                    GenerateNextVolume(_doorToWorkOn);
                }
                yield return new WaitForSeconds(m_generationDelay);
            }

            StartCoroutine(GenerateEndVolumesCoroutine());
        }

        IEnumerator GenerateEndVolumesCoroutine()
        {
            m_generatingEndVolumes = true;
            List<VolumeHardLink> _availableLinks = new List<VolumeHardLink>(m_dicUnconnectedHardLinks.Values);

            foreach (VolumeHardLink _volumeLinks in _availableLinks)
            {
                TryVolumePool(_volumeLinks, m_volumeBankManager.GetShuffledVolumes(m_volumeBankManager.GetVolumeOfType(VolumeType.End), m_random));
                yield return new WaitForSeconds(m_generationDelay);
            }
            if (m_generatedRoomCount < m_minRoomNumber || m_requiredRooms.Count > 0)
                TryGeneration();
            else
                StartCoroutine(FillVolumesCoroutine());
        }

        IEnumerator FillVolumesCoroutine()
        {
            TagAllUnusedHardLinkAsUnconnected();

            foreach (Volume _volume in m_volumes)
            {
                Room _room = _volume.gameObject.GetComponent<Room>();
                _room.FillRoom(this, m_random, m_decorationBankManager, m_tileBankManager, m_roomVisible);
                yield return new WaitForSeconds(m_generationDelay);
            }

            if (m_buildNavMesh) BakeNavmeshSurfaces();
            GenerationDone();
        }

        /* Utility */

        public void SetRandomSeed()
        {
            m_seed = Random.Range(0, int.MaxValue);
        }

        private Volume PickNextVolumeToWorkOn()
        {
            if (m_volumsToWorkOn.Count == 0) return null;

            Volume room = m_volumsToWorkOn.Dequeue();

            return room;
        }

        private Volume InstantiateVolume(Volume _volume, VolumeHardLink _volumeLink, VolumeHardLink _previousLink, List<Vector3> _newVoxelsPos)
        {
            int linkIndex = _volume.HardLinks.IndexOf(_volumeLink);
            GameObject _volumeInstance = Instantiate(_volume.gameObject);
            _volumeInstance.transform.parent = this.SpawnedVolumeContainer.transform;
            _volumeInstance.GetComponent<Volume>().Voxels = _newVoxelsPos;

            GameObject _connectedVolumes = ConnectVolumes(_volumeInstance, linkIndex, _previousLink);
            return _connectedVolumes.GetComponent<Volume>();
        }

        private GameObject ConnectVolumes(GameObject _volumeInstance, int _hardLinkToConnectIndex, VolumeHardLink _existingHardLink)
        {
            _volumeInstance.name = _volumeInstance.name + m_iterations.ToString();
            VolumeHardLink _hardLinkToConnect = _volumeInstance.GetComponent<Volume>().HardLinks[_hardLinkToConnectIndex];

            _volumeInstance.transform.rotation = Quaternion.AngleAxis((_existingHardLink.transform.eulerAngles.y - _hardLinkToConnect.transform.eulerAngles.y) + 180f, Vector3.up);
            Vector3 _translate = _existingHardLink.transform.position - _hardLinkToConnect.transform.position;
            _volumeInstance.transform.position += _translate;

            //Update links variables
            UpdateHardLinkVariables(_hardLinkToConnect, _existingHardLink);

            m_dicUnconnectedHardLinks.Remove(VoxelGrid.RoundVec3(_existingHardLink.transform.position));

            return _volumeInstance;
        }

        private void UpdateHardLinkVariables(VolumeLink _existingHardLink, VolumeLink _hardLinkToConnect)
        {
            _hardLinkToConnect.Connected = true;
            _existingHardLink.Connected = true;

            _hardLinkToConnect.Parent.AddNeighbor(_existingHardLink.Parent);
            _existingHardLink.Parent.AddNeighbor(_hardLinkToConnect.Parent);

            if (_existingHardLink.Priority >= _hardLinkToConnect.Priority) //Priority check
                _existingHardLink.Used = true;
            else
                _hardLinkToConnect.Used = true;
        }

        private bool CheckVoxelOverlap(List<Vector3> _newVoxelsPos)
        {
            foreach (Vector3 _voxel in _newVoxelsPos)
            {
                Vector3 _position = VoxelGrid.RoundVec3(_voxel);
                if (m_usedVoxels.Contains(_position) || CheckIfVoxelOutOfBounds(_position))
                {
                    return false;
                }
            }
            return true;
        }

        private bool CheckIfVoxelOutOfBounds(Vector3 _testPosition)
        {
            return !m_dungeonAreaBound.Contains(_testPosition);
        }

        private Queue<VolumeHardLink> GetShuffledAvailableLinks(Volume _volume)
        {
            List<VolumeHardLink> _availableLinks = _volume.GetUnconnectedLinks();
            //List<VolumeHardLink> _links = new List<VolumeHardLink>(_availableLinks);
            _availableLinks.Shuffle(m_random.random);

            Queue<VolumeHardLink> _linkQueue = new Queue<VolumeHardLink>(_availableLinks);
            return _linkQueue;
        }

        private Queue<VolumeHardLink> GetShuffledAvailableLinksOfSameType(Volume _volume, VolumeHardLink _volumeLink)
        {
            List<VolumeHardLink> _availableLinks = _volume.GetUnconnectedLinksOfType(_volumeLink);
            //List<VolumeHardLink> _links = new List<VolumeHardLink>(_availableLinks);
            _availableLinks.Shuffle(m_random.random);

            Queue<VolumeHardLink> _linkQueue = new Queue<VolumeHardLink>(_availableLinks);
            return _linkQueue;
        }

        /* Adds */
        private void AddVolume(Volume _newVolume)
        {
            _newVolume.DetermineOutsideFacesAndRecalculateBounds();
            AddUsedVoxels(_newVolume.BoundsVoxels);

            AddNewVolumeUnconectedHardLinkToDic(_newVolume);
            AddNewVolumeSoftLinks(_newVolume);

            m_volumes.Add(_newVolume);
            m_volumsToWorkOn.Enqueue(_newVolume);
            m_generatedRoomCount++;
            if (m_generatedRoomCount > m_maxRoomNumber)
            {
                Debug.LogWarning($"Generation exceeded max room count ({m_maxRoomNumber}). Stopping generation and will attempt a restart.");
                m_volumsToWorkOn.Clear(); // Empty the queue to stop the 'GenerateVolumes' while-loop.
            }
        }

        private void AddNewVolumeUnconectedHardLinkToDic(Volume _newVolume)
        {
            foreach (VolumeHardLink _volumUnconnectedHardLink in _newVolume.GetUnconnectedLinks())
            {
                Vector3 _key = VoxelGrid.RoundVec3(_volumUnconnectedHardLink.transform.position);
                if (!m_dicUnconnectedHardLinks.ContainsKey(_key))
                    m_dicUnconnectedHardLinks.Add(_key, _volumUnconnectedHardLink);
                else
                {
                    VolumeHardLink _otherLink = m_dicUnconnectedHardLinks[_key];
                    UpdateHardLinkVariables(_otherLink, _volumUnconnectedHardLink);
                    m_dicUnconnectedHardLinks.Remove(_key);
                }
            }
        }

        private void AddNewVolumeSoftLinks(Volume _newVolume)
        {
            foreach (VolumeSoftLink _volumeSoftLink in _newVolume.SoftLinks)
            {
                Vector3 _key = VoxelGrid.RoundVec3(_volumeSoftLink.transform.position);
                if (m_dicExposedSoftLinks.TryGetValue(_key, out VolumeSoftLink _otherSoftLink)) //Check for softLinkConnection
                {
                    UpdateHardLinkVariables(_otherSoftLink, _volumeSoftLink);
                    m_dicExposedSoftLinks.Remove(_key);
                }
                else if (m_dicUnconnectedHardLinks.TryGetValue(_key, out VolumeHardLink _otherHardLink)) //Check for hardLinkConnection
                {
                    _volumeSoftLink.Connected = true;
                    _otherHardLink.Connected = true;
                    _otherHardLink.Used = true;
                    _otherHardLink.Parent.AddNeighbor(_volumeSoftLink.Parent);
                    _volumeSoftLink.Parent.AddNeighbor(_otherHardLink.Parent);
                    m_dicUnconnectedHardLinks.Remove(_key);
                }
                else
                    m_dicExposedSoftLinks.Add(_key, _volumeSoftLink);
            }
        }

        private void AddUsedVoxels(List<Vector3> _voxels)
        {
            foreach (Vector3 _voxel in _voxels)
            {
                //Vector3 _position = VoxelGrid.RoundVec3(_voxel);
                if (!m_usedVoxels.Add(VoxelGrid.RoundVec3(_voxel)))
                    Debug.Log("Voxel we're trying to add to usedVoxels is already defined..");
                //else
                //    m_usedVoxels.Add(_position);
            }
        }

        private void BakeNavmeshSurfaces()
        {
            StartCoroutine(BakeNavmeshSurfacesCoroutine());
        }

        IEnumerator BakeNavmeshSurfacesCoroutine()
        {
            foreach (NavMeshSurface _navMeshSurface in m_navMeshSurfaces)
            {
                _navMeshSurface.center = Vector3.zero;
                _navMeshSurface.size = m_dungeonAreaBound.size * 1.1f;
                yield return new WaitForSeconds(m_generationDelay);
                _navMeshSurface.BuildNavMesh();
            }
        }

        private void AddIteration()
        {
            m_iterations++;
            if (m_iterations > m_maxIterations)
            {
                string errorMessage = $"Generation HALTED: Maximum iterations ({m_maxIterations}) exceeded. " +
                    "This commonly indicates an impossible generation constraint " +
                    "(e.g., trying to place increase dungeon max size, allow more flexibility for min and max rooms.";
                throw new System.InvalidOperationException(errorMessage);
            }
        }

        public void Clear()
        {
            //Debug.Log("ClearData");
            StopAllCoroutines();

            //m_iterations = 0;
            m_generatedRoomCount = 0;

            m_volumes.Clear();
            m_usedVoxels.Clear();
            m_dicUnconnectedHardLinks.Clear();
            m_dicExposedSoftLinks.Clear();
            m_volumsToWorkOn.Clear();

            for (int i = 0; i < m_navMeshSurfaces.Count; ++i)
            {
                NavMeshSurface _navMeshSurface = m_navMeshSurfaces[i];
                _navMeshSurface.RemoveData();
            }

            DestroyImmediate(SpawnedVolumeContainer);

#if UNITY_EDITOR
            SceneView.RepaintAll();
#endif
        }

        public void ToggleGridView()
        {
            m_dungeonAreaBound.center = transform.position;
            m_dungeonAreaBound.size = m_dungeonMaxSize;
            VoxelGrid.DRAW_GRID = !VoxelGrid.DRAW_GRID;
        }

        private void OnDrawGizmos()
        {
            if (VoxelGrid.DRAW_GRID)
            {
                Gizmos.color = Color.gray;
                Gizmos.DrawWireCube(transform.position, m_dungeonAreaBound.size);
            }
        }
    }
}
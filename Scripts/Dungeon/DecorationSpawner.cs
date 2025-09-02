using System.Collections.Generic;
using UnityEngine;

namespace Generator.Dungeon
{
    public class DecorationSpawner : MonoBehaviour
    {

        [SerializeField] List<DecorationSpawnPoint> m_spawnPoints = new List<DecorationSpawnPoint>();

        private Room m_parentRoom;
        private DecorationBankManager m_decorationBankManager;
        private GameObject m_spawnPointContainer;
        private int m_collisionMaxTest = 20;

        public void SpawnObjects(Room _room, DecorationBankManager _decorationBank, DRandom _random )
        {
            m_parentRoom = _room;
            m_decorationBankManager = _decorationBank;

            foreach (DecorationSpawnPoint _spawnPoint in m_spawnPoints)
            {
                if (_spawnPoint != null && _random.range(0, 100) <= _spawnPoint.Probability)
                {
                    List<Decoration> _decorationPool = new List<Decoration>();

                    if (_spawnPoint.PreferedDecoration.Count > 0)
                        _decorationPool = _decorationBank.GetDecorationsWithTags(_spawnPoint.PreferedDecoration, _room.Type, _spawnPoint.SurfaceType);
                    else
                        _decorationPool = _decorationBank.GetDecorationByRoomAndSurfaceType(_room.Type, _spawnPoint.SurfaceType);

                    if (_decorationPool.Count > 0)
                    {
                        Decoration _decorationToSpawn = _decorationPool[_random.range(0, _decorationPool.Count-1)];
                        SpawnDecoration(_decorationToSpawn, _spawnPoint, _random);
                    }
                }
            }
        }

        private void SpawnDecoration(Decoration _decoration, DecorationSpawnPoint _spawnPoint, DRandom _random )
        {
            Vector3 _finalPos = _spawnPoint.transform.position;
            Quaternion _finalRotation = _spawnPoint.transform.rotation;


            if (_spawnPoint.TranslationAllowed)
            {
                _finalPos += _random.range(-100, 100) * 0.001f * _spawnPoint.TranslationRange * Vector3.left +
                            _random.range(-100, 100) * 0.001f * _spawnPoint.TranslationRange * Vector3.forward;
            }

            if (_spawnPoint.RotationAllowed)
            {
                _finalRotation = Quaternion.Euler(_finalRotation.eulerAngles.x, _finalRotation.eulerAngles.y + _random.range(-20, 20), _finalRotation.eulerAngles.z);
            }

            if (_decoration.CheckForCollision)
            {
                Collider[] _hitColliders = CheckCollision(_decoration, _finalPos, _finalRotation);
                if ((_hitColliders.Length > 0 || !CheckInRoomBunds(_finalPos)) && (!_spawnPoint.TranslationAllowed || !_spawnPoint.RotationAllowed))
                    return;

                int _collisionTestCount = 0;
                bool _success = false;
                while(_collisionTestCount < m_collisionMaxTest)
                {
                    if (_hitColliders.Length == 0 && CheckInRoomBunds(_finalPos)) {
                        _success = true;
                        break;
                    }
                    else
                    {
                        _finalRotation = Quaternion.Euler(_finalRotation.eulerAngles.x, _random.range(0, 360), _finalRotation.eulerAngles.z);
                        _finalPos += _random.range(-100, 100) * 0.001f * _spawnPoint.TranslationRange * Vector3.left +
                                    _random.range(-100, 100) * 0.001f * _spawnPoint.TranslationRange * Vector3.forward;
                    }

                    _hitColliders = CheckCollision(_decoration, _finalPos, _finalRotation);
                    _collisionTestCount++;
                }
                if (!_success) return;
            }

            //bool _networkIsActive = NetworkObjectSpawner.Instance != null;

            //if( _networkIsActive && _decoration.IsNetworkeObject)
            //{
            //    SpawnOnNetwork(_decoration.gameObject, _finalPos, _finalRotation.eulerAngles);
            //}
            //else
            //{
                GameObject _instance = Instantiate(_decoration.gameObject, _finalPos, _finalRotation);
                _instance.transform.parent = m_parentRoom.DecorationContainer.transform;

                DecorationSpawner _instanceSpawner;
                _instance.gameObject.TryGetComponent<DecorationSpawner>(out _instanceSpawner);
                if( _instanceSpawner != null )
                {
                    _instanceSpawner.SpawnObjects(m_parentRoom, m_decorationBankManager, _random);
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

        private Collider[] CheckCollision(Decoration _decoration, Vector3 _position, Quaternion _rotation)
        {
            BoxCollider _collider;
            _decoration.gameObject.TryGetComponent<BoxCollider>(out _collider);
            if ( _collider == null )
            {
                Debug.LogErrorFormat("Missing collider on " + _decoration.gameObject.name);
            }

            LayerMask _layerMask = LayerMask.GetMask("Wall","Decoration","Door");

            Collider[] hitColliders = Physics.OverlapBox(_position + _collider.center, 0.5f*_collider.size, _rotation, _layerMask);
            return hitColliders;
        }

        private bool CheckInRoomBunds(Vector3 _pos)
        {
            return m_parentRoom.Bounds.Contains(_pos);
        }

        public void AddSpawnPoint()
        {
            if(m_spawnPointContainer == null)
            {
                m_spawnPointContainer = new GameObject("SpawnPoints");
                m_spawnPointContainer.transform.parent = this.transform;
            }

            GameObject _newSpawnPoint = new GameObject("New SpawnPoint");
            _newSpawnPoint.transform.parent = m_spawnPointContainer.transform;
            _newSpawnPoint.AddComponent<DecorationSpawnPoint>();

            m_spawnPoints.Add(_newSpawnPoint.GetComponent<DecorationSpawnPoint>());
        }

        public void ToggleSpawnPointView()
        {
            VoxelGrid.DRAW_SPAWNPOINTS = !VoxelGrid.DRAW_SPAWNPOINTS;
        }
    }
}
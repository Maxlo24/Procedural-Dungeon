using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Generator.Dungeon
{
    public class RoomRenderer : MonoBehaviour
    {
        [SerializeField] private MeshRenderer[] m_meshRenderersArray;
        [SerializeField] private Light[] m_lightsArray;


        public void FindAllMeshRenderers()
        {
            m_meshRenderersArray = gameObject.GetComponentsInChildren<MeshRenderer>();
            m_lightsArray = gameObject.GetComponentsInChildren<Light>();
        }

        public void SwitchRoomVisibility (bool _status)
        {
            foreach(MeshRenderer _renderer in m_meshRenderersArray)
                _renderer.enabled = _status;

            foreach(Light _light in m_lightsArray)
                _light.enabled = _status;
        }

        public void BatchSpawnedObjects(GameObject _tiles, GameObject _decorations)
        {
            MeshFilter[] _tilesMeshFilters = _tiles.GetComponentsInChildren<MeshFilter>();
            MeshFilter[] _decoMeshFilters = _decorations.GetComponentsInChildren<MeshFilter>();

            GameObject[] _tileGameObjects = new GameObject[_tilesMeshFilters.Length];
            GameObject[] _decoGameObjects = new GameObject[_decoMeshFilters.Length];

            for(int i = 0; i < _tilesMeshFilters.Length; i++)
                _tileGameObjects[i] = _tilesMeshFilters[i].gameObject;
            
            for(int i = 0; i < _decoMeshFilters.Length; i++)
                _decoGameObjects[i] = _decoMeshFilters[i].gameObject;

            StaticBatchingUtility.Combine(_tileGameObjects, _tiles);
            StaticBatchingUtility.Combine(_decoGameObjects, _decorations);
        }


    }
}
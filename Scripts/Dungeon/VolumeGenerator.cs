using System.Collections.Generic;
using UnityEngine;



namespace Generator.Dungeon
{
    public class VolumeGenerator : MonoBehaviour
    {
        private class Cursor
        {
            public Vector3 Position = VoxelGrid.VOXEL_SIZE * 0.5f * Vector3.one;
            public Quaternion Rotation = Quaternion.identity;
            public Vector3 forward { get {  return Rotation * Vector3.forward; } }
        }

        private enum LinkType
        {
            Hard,
            Soft,
        }

        [SerializeField] Vector3 m_volumeSize = new Vector3(1f, 1f, 1f);

        private GameObject m_voxelsContainer;
        private Volume m_volume;
        private Room m_room;

        public void GenerateVoxelGrid()
        {
            Clear();
            m_voxelsContainer = new GameObject("Voxels");
            m_voxelsContainer.transform.parent = this.transform;

            for (int i = 0; i < m_volumeSize.x; i++)
            {
                for (int j = 0; j < m_volumeSize.y; j++)
                {
                    for (int k = 0; k < m_volumeSize.z; k++)
                    {
                        GenerateVoxel(new Vector3(i,j,k) * VoxelGrid.VOXEL_SIZE + VoxelGrid.VOXEL_SIZE * 0.5f * Vector3.one);
                    }
                }
            }
        }

        public void GenerateVoxel(Vector3 position)
        {
            GameObject voxel = GameObject.CreatePrimitive(PrimitiveType.Cube);
            voxel.name = (string.Format("Voxel - ({0}, {1}, {2})", position.x, position.y, position.z));
            voxel.transform.localScale = 0.2f * Vector3.one;
            voxel.transform.position = position;
            voxel.transform.parent = m_voxelsContainer.transform;

            DestroyImmediate(voxel.GetComponent<BoxCollider>());
            voxel.AddComponent<VoxelVisual>();
        }


        private void UpdateVolume()
        {
            gameObject.TryGetComponent<Volume>(out m_volume);
            gameObject.TryGetComponent<Room>(out m_room);

            if (m_volume == null || m_room ==null)
            {
                m_volume = gameObject.AddComponent<Volume>();
                m_room = gameObject.AddComponent<Room>();
            }
        }

        public void Clear()
        {
            if (m_voxelsContainer != null)
            {
                DestroyImmediate(m_voxelsContainer);
            }
        }

        public void AssignVoxelsToVolume()
        {
            if (m_voxelsContainer == null) return;
            UpdateVolume();

            List<Vector3> _voxelList = new List<Vector3>();

            for (int i=0; i < m_voxelsContainer.transform.childCount; i++)
            {
                _voxelList.Add(m_voxelsContainer.transform.GetChild(i).position);
            }
            m_volume.Voxels = _voxelList;
        }

        public void Done()
        {
            AssignVoxelsToVolume();
            if (transform.Find("Geometry") == null)
            {
                GameObject geometryRoot = new GameObject("Geometry");
                geometryRoot.transform.parent = transform;
            }
            Clear();
            DestroyImmediate(gameObject.GetComponent<VolumeGenerator>());
        }

        public void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(transform.position + VoxelGrid.VOXEL_SIZE * m_volumeSize * 0.5f, VoxelGrid.VOXEL_SIZE * m_volumeSize);
        }
    }
}

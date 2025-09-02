using UnityEngine;

namespace Generator.Dungeon
{
    public class VolumeLinkFace : MonoBehaviour
    {

        [SerializeField] private Volume m_parent;

        public Volume Parent { set { m_parent = value; } }

        public void TurnInHardLink()
        {
            if (m_parent != null)
            {
                GameObject _newLink = new GameObject("New HardLink");
                _newLink.transform.parent = m_parent.HardLinkContainer.transform;
                _newLink.transform.position = VoxelGrid.RoundVec3(transform.position);
                _newLink.transform.rotation = transform.rotation;
                _newLink.AddComponent<TileSpawner>();
                _newLink.AddComponent<VolumeHardLink>().Parent = m_parent;

                m_parent.HardLinks.Add(_newLink.GetComponent<VolumeHardLink>());
                m_parent.GetComponent<Room>().AddLinkTileSpawner(_newLink.GetComponent<TileSpawner>());
                DestroyImmediate(gameObject);
            }
        }
        
        public void TurnInSoftLink()
        {
            if (m_parent != null)
            {
                GameObject _newLink = new GameObject("New SoftLink");
                _newLink.transform.parent = m_parent.SoftLinkContainer.transform;
                _newLink.transform.position = VoxelGrid.RoundVec3(transform.position);
                _newLink.transform.rotation = transform.rotation;
                _newLink.AddComponent<TileSpawner>();
                _newLink.AddComponent<VolumeSoftLink>().Parent = m_parent;

                m_parent.SoftLinks.Add(_newLink.GetComponent<VolumeSoftLink>());
                m_parent.GetComponent<Room>().AddLinkTileSpawner(_newLink.GetComponent<TileSpawner>());
                DestroyImmediate(gameObject);
            }
        }

        public void OnDrawGizmos()
        {
            Gizmos.color = Color.white;
            DrawArrow.ForGizmo(transform.position, transform.forward * 1);
        }
    }
}
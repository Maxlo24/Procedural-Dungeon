using System.Collections.Generic;
using UnityEngine;

namespace Generator.Dungeon
{
    public enum VolumeType
    {
        Basic,
        Hall,
        Stairs,
        Special,
        Objective,
        Spawn,
        End,
    }
    public class VolumeFaceData
    {
        public Dictionary<Vector3, List<Vector3>> faces;
        public VolumeFaceData()
        {
            faces = new Dictionary<Vector3, List<Vector3>>();

            faces[Vector3.left] = new List<Vector3>();
            faces[Vector3.right] = new List<Vector3>();
            faces[Vector3.up] = new List<Vector3>();
            faces[Vector3.down] = new List<Vector3>();
            faces[Vector3.forward] = new List<Vector3>();
            faces[Vector3.back] = new List<Vector3>();
        }
    }
    public class Volume : MonoBehaviour
    {
        [SerializeField] private VolumeType m_volumeType;
        [SerializeField] private List<VolumeHardLink> m_hardLinks = new List<VolumeHardLink>();
        [SerializeField] private List<VolumeSoftLink> m_softLinks = new List<VolumeSoftLink>();
        [SerializeField] private List<Vector3> m_voxels = new List<Vector3>();
        [SerializeField] private List<Volume> m_neighborVolumes = new List<Volume>();

        private List<Vector3> m_linkPositions = new List<Vector3>();
        private List<Vector3> m_boundsVoxels = new List<Vector3>();
        private VolumeFaceData m_volumeFaceData = new VolumeFaceData();
        private Bounds bounds;
        private bool m_showVoxels = false;


        //Containers
        private GameObject m_hardLinkContainer;
        private GameObject m_softLinkContainer;
        private GameObject m_linkFaceContainer;

        public VolumeType Type { get { return m_volumeType; } } 
        public List<VolumeHardLink> HardLinks { get { return m_hardLinks; } set { m_hardLinks = value; } }
        public List<VolumeSoftLink> SoftLinks { get { return m_softLinks; } set { m_softLinks = value; } }
        public List<Vector3> Voxels { get { return m_voxels; } set { m_voxels = value; RecalculateBounds(); } }
        public List<Volume> Neighbors { get { return m_neighborVolumes; } }
        public VolumeFaceData FaceData { get { return m_volumeFaceData; } }
        public List<Vector3> BoundsVoxels { get { return m_boundsVoxels; } }
        public Bounds Bounds { get { return  bounds; } }

        public void AddNeighbor(Volume _neighbor)
        {
            if (m_neighborVolumes.Contains(_neighbor)) return;
            m_neighborVolumes.Add(_neighbor);
        }

        public List<Volume> GetNeighbors(int _targetDegree, int _degree = 0, List<Volume> _neighborList = null)
        {
            if(_neighborList == null)
                _neighborList = new List<Volume>(m_neighborVolumes);

            if (_degree < _targetDegree)
            {
                foreach (Volume _neighbor in m_neighborVolumes)
                {
                    if (!_neighborList.Contains(_neighbor))
                        _neighborList.Add(_neighbor);
                    List<Volume> _neighbors2 = _neighbor.GetNeighbors(_targetDegree, _degree + 1, _neighborList);
                    foreach (Volume _neighbor2 in _neighbors2)
                    {
                        if (!_neighborList.Contains(_neighbor2))
                            _neighborList.Add(_neighbor2);
                    }
                }
                return _neighborList;
            }
            else
                return m_neighborVolumes;
        }

        public List<VolumeHardLink> GetUnconnectedLinks()
        {
            List<VolumeHardLink> _unconnectedLinks = new List<VolumeHardLink>();
            for (int i = 0; i < m_hardLinks.Count; i++)
            {
                if (!m_hardLinks[i].Connected) _unconnectedLinks.Add(m_hardLinks[i]);
            }
            return _unconnectedLinks;
        }

        public List<VolumeHardLink> GetUnconnectedLinksOfType(VolumeHardLink _link)
        {
            List<VolumeHardLink> _unconnectedLinks = new List<VolumeHardLink>();
            for (int i = 0; i < m_hardLinks.Count; i++)
            {
                if (!m_hardLinks[i].Connected && _link.CompatibleType.HasFlag(m_hardLinks[i].Type))
                    _unconnectedLinks.Add(m_hardLinks[i]);
            }
            return _unconnectedLinks;
        }

        public void RecalculateBounds()
        {
            Vector3 min = new Vector3(m_voxels[0].x,
                                      m_voxels[0].y,
                                      m_voxels[0].z);
            Vector3 max = min;

            for (int i = 0; i < m_voxels.Count; i++)
            {
                Vector3 pos = m_voxels[i];

                if (pos.x < min.x) min.x = pos.x;
                if (pos.y < min.y) min.y = pos.y;
                if (pos.z < min.z) min.z = pos.z;

                if (pos.x > max.x) max.x = pos.x;
                if (pos.y > max.y) max.y = pos.y;
                if (pos.z > max.z) max.z = pos.z;
            }

            Vector3 size = new Vector3(0.5f * VoxelGrid.VOXEL_SIZE, 0.5f * VoxelGrid.VOXEL_SIZE, 0.5f * VoxelGrid.VOXEL_SIZE);
            bounds = new Bounds((min + max) / 2f, ((max + size) - (min - size)));
        }

        private void UpdateLinksPosition()
        {
            m_linkPositions.Clear();
            foreach (VolumeHardLink _hardLink in m_hardLinks)
            {
                m_linkPositions.Add(VoxelGrid.RoundVec3(_hardLink.transform.position));
            }
            foreach (VolumeSoftLink _softLink in m_softLinks)
            {
                m_linkPositions.Add(VoxelGrid.RoundVec3(_softLink.transform.position));
            }
        }

        public void DetermineOutsideFacesAndRecalculateBounds()
        {
            RecalculateBounds();
            UpdateLinksPosition();

            m_volumeFaceData = new VolumeFaceData();

            List<Vector3> _newBoundsVoxels = new List<Vector3>(m_voxels);

            foreach (Vector3 _voxelToTest in m_voxels)
            {
                bool _removeVoxel = true;

                _removeVoxel = CheckBoundAndLinkCollision(_voxelToTest, Vector3.left, _removeVoxel);
                _removeVoxel = CheckBoundAndLinkCollision(_voxelToTest, Vector3.right, _removeVoxel);
                _removeVoxel = CheckBoundAndLinkCollision(_voxelToTest, Vector3.forward, _removeVoxel);
                _removeVoxel = CheckBoundAndLinkCollision(_voxelToTest, Vector3.back, _removeVoxel);
                _removeVoxel = CheckBoundCollision(_voxelToTest, Vector3.up, _removeVoxel);
                _removeVoxel = CheckBoundCollision(_voxelToTest, Vector3.down, _removeVoxel);

                if (_removeVoxel)
                    _newBoundsVoxels.Remove(_voxelToTest);
            }
            m_boundsVoxels = _newBoundsVoxels;
        }

        private bool CheckBoundAndLinkCollision(Vector3 _voxelToTest, Vector3 _direction, bool _previousResult)
        {
            Vector3 _posLinkTest = _voxelToTest + VoxelGrid.VOXEL_SIZE * 0.5f * _direction;

            if (!m_linkPositions.Contains(_posLinkTest))
            {
                return CheckBoundCollision(_voxelToTest, _direction, _previousResult);
            }
            return _previousResult;
        }
        
        private bool CheckBoundCollision(Vector3 _voxelToTest, Vector3 _direction, bool _previousResult)
        {
            Vector3 _posVoxelTest = _voxelToTest + _direction * VoxelGrid.VOXEL_SIZE;

            if (!m_voxels.Contains(_posVoxelTest))
            {
                m_volumeFaceData.faces[_direction].Add(_voxelToTest + VoxelGrid.VOXEL_SIZE * 0.5f * _direction);
                return false;
            }
            return _previousResult;
        }


        public void GenerateLinkFaces()
        {
            DetermineOutsideFacesAndRecalculateBounds();

            foreach(Vector3 _direction in m_volumeFaceData.faces.Keys)
            {

                if (VoxelGrid.CheckIfFaceIsWall(_direction))
                {
                    foreach (Vector3 _pos in m_volumeFaceData.faces[_direction])
                    {
                        GenerateLinkFace(_pos, Quaternion.LookRotation(-_direction));
                    }
                }
            }
        }

        public void ClearLinkFaces()
        {
            DestroyImmediate(LinkFaceContainer);
        }

        public void GenerateLinkFace(Vector3 _position, Quaternion _rotation)
        {
            GameObject _face = GameObject.CreatePrimitive(PrimitiveType.Cube);
            _face.name = (string.Format("Face - ({0}, {1}, {2})", _position.x, _position.y, _position.z));
            _face.transform.localScale = 0.2f * Vector3.one;
            _face.transform.position = _position;
            _face.transform.rotation = _rotation;
            _face.transform.parent = LinkFaceContainer.transform;

            DestroyImmediate(_face.GetComponent<BoxCollider>());
            _face.AddComponent<VolumeLinkFace>();
            _face.GetComponent<VolumeLinkFace>().Parent = this;
        }


        public GameObject HardLinkContainer
        {
            get
            {
                if (m_hardLinkContainer == null)
                    m_hardLinkContainer = VoxelGrid.CheckContainerExistance(transform, "HardLinks");
                return m_hardLinkContainer;

            }
        }
        public GameObject SoftLinkContainer
        {
            get
            {
                if (m_softLinkContainer == null)
                    m_softLinkContainer = VoxelGrid.CheckContainerExistance(transform, "SoftLinks");
                return m_softLinkContainer;
            }
        }
        public GameObject LinkFaceContainer
        {
            get
            {
                if (m_linkFaceContainer == null)
                    m_linkFaceContainer = VoxelGrid.CheckContainerExistance(transform, "LinkFaces");
                return m_linkFaceContainer;
            }
        }


        public void ToggleGizmoToDraw()
        {
            VoxelGrid.DRAW_GRID = !VoxelGrid.DRAW_GRID;
            RecalculateBounds();
        }
        
        public void ToggleLinkView()
        {
            VoxelGrid.DRAW_LINK = !VoxelGrid.DRAW_LINK;
        }

        public void ToggleVoxelView()
        {
            m_showVoxels = !m_showVoxels;
        }

        public void OnDrawGizmos()
        {
            if (VoxelGrid.DRAW_GRID)
            {
                switch (m_volumeType)
                {
                    case VolumeType.Basic:
                        Gizmos.color = Color.white;
                        break;
                    case VolumeType.Hall:
                        Gizmos.color = Color.black;
                        break;
                    case VolumeType.Stairs:
                        Gizmos.color = Color.blue;
                        break;
                    case VolumeType.Special:
                        Gizmos.color = Color.yellow;
                        break;
                    case VolumeType.Objective:
                        Gizmos.color = Color.cyan;
                        break;
                    case VolumeType.Spawn:
                        Gizmos.color = Color.magenta;
                        break;
                    case VolumeType.End:
                        Gizmos.color = Color.red;
                        break;
                }

                Gizmos.DrawWireCube(bounds.center, bounds.size);

                Gizmos.color = new Color(0.5f,0.5f,0.5f,0.3f);
                if (m_showVoxels)
                {
                    foreach (Vector3 voxel in m_voxels)
                    {
                        Gizmos.DrawCube(voxel, VoxelGrid.VOXEL_SIZE * Vector3.one);
                    }
                }
                else if (VoxelGrid.DRAW_FACES)
                {
                    foreach (Vector3 _direction in m_volumeFaceData.faces.Keys)
                    {
                        foreach(Vector3 _pos in m_volumeFaceData.faces[_direction])
                        {
                            Gizmos.DrawSphere(_pos, 0.2f);
                        }
                    }
                }
            }
        }
    }
}
using UnityEngine;

namespace Generator.Dungeon
{
    public class VoxelVisual : MonoBehaviour
    {
        private float m_VisualvoxelSize = 2.9f;
        public void OnDrawGizmos()
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireCube(transform.position, m_VisualvoxelSize * Vector3.one);
        }
    }
}
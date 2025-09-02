using System.Collections.Generic;
using UnityEngine;

namespace Generator.Dungeon
{
    public class DecorationSpawnPoint : MonoBehaviour
    {
        [SerializeField] private SurfaceTag m_surfaceTag;
        [SerializeField] private int m_spawnProbability = 100;
        [SerializeField] private bool m_randomRotation = true;
        [SerializeField] private bool m_randomOffset = true;
        [SerializeField] private float m_offsetRange = 0.1f;
        [SerializeField] private List<DecorationTag> m_preferedDecorations = new List<DecorationTag>();

        public SurfaceTag SurfaceType { get { return m_surfaceTag; } set { m_surfaceTag = value; } }
        public int Probability { get { return m_spawnProbability; } }
        public bool RotationAllowed { get { return m_randomRotation; } }
        public bool TranslationAllowed { get { return m_randomOffset; } }
        public float TranslationRange {  get { return m_offsetRange; } }
        public List<DecorationTag> PreferedDecoration { get { return m_preferedDecorations; } }

        public void OnDrawGizmos()
        {
            if (VoxelGrid.DRAW_SPAWNPOINTS)
            {
                Color _col = new Color(0, 0, 1, 0.5f);

                Gizmos.color = _col;
                Gizmos.DrawSphere(transform.position, m_offsetRange);
                DrawArrow.ForGizmo(transform.position, transform.forward * 1);
            }
        }
    }
}
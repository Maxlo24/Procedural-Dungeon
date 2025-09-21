using System.Collections.Generic;
using UnityEngine;

namespace Generator.Dungeon
{
    [System.Flags]
    public enum VolumeLinkType
    {
        None = 0,
        Normal = 1,
        StairUp = 2,
        StairDown = 4,
        Special = 8,
        All = ~0,
    }
    [RequireComponent(typeof(TileSpawner))]
    public class VolumeHardLink : VolumeLink
    {
        [SerializeField] private VolumeLinkType m_linkType = VolumeLinkType.Normal; //Can only connect same link type
        [SerializeField] private VolumeLinkType m_compatibleLinkType = VolumeLinkType.All; //Can only connect same link type
        [SerializeField] private int m_probabilityToGenerate = 100; //If used, probability to generate a door
        [SerializeField] private List<Volume> m_peferedVolumePool; //Volume to spawn in priority

        public VolumeLinkType Type { get { return m_linkType; } }
        public VolumeLinkType CompatibleType { get { return m_compatibleLinkType; } }
        public List<Volume> PeferedVolumePool { get { return m_peferedVolumePool; } }

        public override void UpdateTileStatus(DRandom _random)
        {
            base.UpdateTileStatus(_random);

            if (m_used)
            {
                if (_random.range(0, 100) <= m_probabilityToGenerate)
                    m_activeLink.Type = TileType.Door;
                else
                    m_activeLink.Type = TileType.Oppening;
            }
        }

        private void OnDrawGizmos()
        {
            if (VoxelGrid.DRAW_LINK)
            {
                float _radius = 0.2f;
                Gizmos.color = Color.red;

                Gizmos.DrawSphere(transform.position, _radius);

                DrawArrow.ForGizmo(transform.position, transform.forward * 1, Color.blue);
            }
        }
    }
}
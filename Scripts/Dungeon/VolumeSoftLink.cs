using UnityEngine;

namespace Generator.Dungeon
{
    [RequireComponent(typeof(TileSpawner))]
    public class VolumeSoftLink : VolumeLink
    {
        public override void UpdateTileStatus(DRandom _random)
        {
            base.UpdateTileStatus(_random);

            if (m_used)
                m_activeLink.Type = TileType.Oppening;
        }

        private void OnDrawGizmos()
        {
            if (VoxelGrid.DRAW_LINK)
            {
                Gizmos.color = Color.gray;
                Gizmos.DrawSphere(transform.position, 0.2f);
                DrawArrow.ForGizmo(transform.position, transform.forward * 1, Color.yellow);
            }
        }
    }
}
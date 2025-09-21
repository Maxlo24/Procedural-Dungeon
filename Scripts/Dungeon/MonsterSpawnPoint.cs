using UnityEngine;

namespace Generator.Dungeon
{
    public class MonsterSpawnPoint : MonoBehaviour
    {
        [SerializeField] private SurfaceTag m_surfaceTag;
        [SerializeField] private int m_spawnProbability = 100;
        [SerializeField] private bool m_randomRotation = true;
        //[SerializeField] private List<Monster> m_preferedMonsters = new List<Monster>();

        public SurfaceTag SurfaceType { get { return m_surfaceTag; } set { m_surfaceTag = value; } }
        public int Probability { get { return m_spawnProbability; } }
        public bool RotationAllowed { get { return m_randomRotation; } }
        //public List<Monster> PreferedMonsters {  get { return m_preferedMonsters;} }


        private void Start()
        {
            //if(EclipsedGameManager.Instance != null)
            //{
            //    EclipsedGameManager.Instance.MonsterSpawnPoints.Add(this);
            //}
        }


        public void OnDrawGizmos()
        {
            if (VoxelGrid.DRAW_SPAWNPOINTS)
            {
                Color _col = new Color(1, 0, 0, 0.5f);

                Gizmos.color = _col;
                Gizmos.DrawSphere(transform.position, 0.2f);
                DrawArrow.ForGizmo(transform.position, transform.forward * 1);
            }
        }
    }
}
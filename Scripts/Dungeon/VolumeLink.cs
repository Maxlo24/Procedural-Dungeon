using System.Collections.Generic;
using UnityEngine;

namespace Generator.Dungeon
{
    public class VolumeLink : MonoBehaviour
    {
        [SerializeField] protected Volume m_parent;
        [SerializeField] protected int m_priority = 1; //Link with highest priority take the lead when overlapping with an other one
        [SerializeField] protected bool m_connected = false;
        [SerializeField] protected bool m_used = false;
        [SerializeField] protected List<GameObject> m_destroyIfConnected = new List<GameObject>();
        [SerializeField] protected List<GameObject> m_destroyIfNotConnected = new List<GameObject>();

        protected TileSpawner m_activeLink;
        protected BoxCollider m_collider;

        public int Priority { get { return m_priority; } }
        public bool Connected { get { return m_connected; } set { m_connected = value; } }
        public bool Used { get { return m_used; } set { m_used = value; } }
        public Volume Parent { get { return m_parent; } set { m_parent = value; } }

        public virtual void UpdateTileStatus(DRandom _random) 
        {
            m_activeLink = gameObject.GetComponent<TileSpawner>();
            if (m_connected)
            {
                AddCollider();
                if (!m_used)
                {
                    m_activeLink.Probability = -1;
                }
                foreach (GameObject _go in m_destroyIfConnected)
                    DestroyImmediate(_go);
            }
            else
            {
                foreach (GameObject _go in m_destroyIfNotConnected)
                    DestroyImmediate(_go);
            }
        }

        protected void AddCollider()
        {
            m_collider = gameObject.AddComponent<BoxCollider>();
            m_collider.size = new Vector3(3, 3, 2f);
            m_collider.center = new Vector3(0, 0, 1f);
            m_collider.isTrigger = true;
            m_collider.enabled = true;
            gameObject.layer = LayerMask.NameToLayer("Door");
        }

        protected void OnTriggerEnter(Collider other)
        {
            Debug.Log(other.tag);

            if(other.tag == "Player")
            {
                m_parent.GetComponent<Room>().EnterRoom();
            }
            //if (other.tag == "Player" && other.GetComponent<PlayerStateMachine>().IsOwner)
            //{
            //    m_parent.GetComponent<Room>().EnterRoom();
            //}
        }
    }
}
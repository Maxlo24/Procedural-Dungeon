using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.AI;

public class MobTest : MonoBehaviour
{

    private Camera cam;
    private NavMeshAgent agent;

    private void Awake()
    {
        cam = Camera.main;
        agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        //if(StarterAssetsInputs.Instance != null)
        //{
        //    if (StarterAssetsInputs.Instance.shoot)
        //    {
        //        StarterAssetsInputs.Instance.shoot = false;
        //        Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
        //        RaycastHit hit;

        //        if( Physics.Raycast(ray, out hit))
        //        {
        //            agent.SetDestination(hit.point);
        //        }
        //    }
        //}
    }
}

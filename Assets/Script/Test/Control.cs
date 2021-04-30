using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Control : MonoBehaviour
{
    private NavMeshAgent agent;
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
       
        if(Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if(Physics.Raycast(ray, out hit))
            {
                if(!hit.collider.tag.Equals("Terrain"))
                {
                    return;
                }
            }
            Vector3 point = hit.point;
            transform.LookAt(new Vector3(point.x, transform.position.y, point.z));
            agent.SetDestination(point);
        }
        if (agent.remainingDistance > 0)
        {
            Debug.Log("seeking");
        }
    }
}

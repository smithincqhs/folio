using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AgentManager : MonoBehaviour {

    public GameObject destination;
    List<NavMeshAgent> agents = new List<NavMeshAgent>();

    void Start() {

        GameObject[] a = GameObject.FindGameObjectsWithTag("AI");
        foreach (GameObject go in a) {

            agents.Add(go.GetComponent<NavMeshAgent>());
        }
    }


    void Update() {

        if (Input.GetMouseButtonDown(0)) {

            RaycastHit hit;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 100)) {

                Vector3 targetPosition = hit.point;
                destination.transform.position = targetPosition;
                foreach (NavMeshAgent a in agents) {

                    a.SetDestination(hit.point);
                }
            }
        }
    }
}

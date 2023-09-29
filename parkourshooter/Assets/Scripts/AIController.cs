using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIController : MonoBehaviour {

    public NavMeshAgent agent;
    public GameObject agentBody;
    public GameObject target;
    public float vertSpeed;

    void Start() {
        agent = GetComponent<NavMeshAgent>();
    }

    void Update() {
        agent.SetDestination(target.transform.position);
        float y = Vector3.Lerp(agentBody.transform.position, target.transform.position, Time.deltaTime * vertSpeed).y;
        agentBody.transform.SetPositionAndRotation(new Vector3(agentBody.transform.position.x, y, agentBody.transform.position.z), new Quaternion(0, 0, 0, 0));
    }
}

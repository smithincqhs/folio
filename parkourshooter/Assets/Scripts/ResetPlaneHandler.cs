using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetPlaneHandler : MonoBehaviour
{
    public GameObject playerObj; // player game object
    public Camera cam; // main camera

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject == playerObj)
        {
            playerObj.GetComponent<PlayerControl>().DeathFunc();
        }
    }
}

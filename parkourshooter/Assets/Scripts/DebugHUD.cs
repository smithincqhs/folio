using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class DebugHUD : MonoBehaviour
{
    public bool debugEnabled = false;
    public GameObject player;
    PlayerControl playerControl;
    Rigidbody playerRB;
    public TextMeshProUGUI boxTopLeft, boxBottomLeft, boxTopRight, boxBottomRight;
    public CameraControl cameraControl;
    public Camera mainCam;
    void Start()
    {
        playerRB = player.GetComponent<Rigidbody>();
        playerControl = player.GetComponent<PlayerControl>();
        cameraControl = mainCam.GetComponent<CameraControl>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Backslash))
        {
            debugEnabled = debugEnabled ? false : true;
            ToggleDebugMode(debugEnabled);
        }

        if (debugEnabled)
        {
            Vector3 playerVFlat = new Vector3(playerRB.velocity.x, 0, playerRB.velocity.z);
            //speed, speed horizontal, speed vertical, isgrounded, player position
            boxTopLeft.text = "V: " + playerVFlat.magnitude + "<br>V.Horz: " + Mathf.Round((Mathf.Abs(playerRB.velocity.x) + Mathf.Abs(playerRB.velocity.y) / 2) * 100)/100
                + "<br>V.Vert: " + Mathf.Round(Mathf.Abs(playerRB.velocity.z)*100)/100 + "<br>Jump Ready: " + playerControl.jumpReady + "<br>isGrounded: " + (playerControl.isGrounded ? "true<br>POS: " : "false<br>POS: ")
                + Mathf.Round(player.transform.position.x*100)/100 + " " + Mathf.Round(player.transform.position.y*100)/100 + " " + Mathf.Round(player.transform.position.z*100)/100;
            //current player state
            boxBottomLeft.text = playerControl.state.ToString();
            //camera yaw, camera pitch
            boxTopRight.text = "Yaw: " + Mathf.Round(mainCam.transform.rotation.y*100)/100 + "<br>Pitch: " + Mathf.Round((mainCam.transform.rotation.x + mainCam.transform.rotation.z / 2)*100)/100;
            // dash ready, cooldown
            boxTopRight.text += "<br>Dash Ready: " + playerControl.dashReady;
        }
    }

    void ToggleDebugMode(bool isEnabled)
    {
        if (isEnabled)
        {
            Debug.Log("bonk");
            boxBottomLeft.enabled = true;
            boxTopLeft.enabled = true;
            boxTopRight.enabled = true;
        }
        else
        {
            Debug.Log("zonk");
            boxBottomLeft.enabled = false;
            boxTopLeft.enabled = false;
            boxTopRight.enabled = false;
        }

    }
}

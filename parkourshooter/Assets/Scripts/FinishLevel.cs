using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FinishLevel : MonoBehaviour
{
    public TextMeshProUGUI endPromptText;
    public ScoreCounter scoreHandler;
    public PlayerControl pcControl;
    public GameObject levelCompleteUI;
    public TextMeshProUGUI scoreCard;
    public GameObject normalHUD;
    public GameObject playerGO;
    public bool inEndZone = false;

    private void Start()
    {
        endPromptText.enabled = false;
        levelCompleteUI.SetActive(false);
    }

    private void Update()
    {
        if (inEndZone && Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("gowka");
            pcControl.state = PlayerControl.PlayerState.freeze;
            scoreHandler.TimeToScore();
            DisplayLevelComplete();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("w0oooo");
        if(other.gameObject == playerGO)
        {

            Debug.Log("w0oooot");
            endPromptText.enabled = true;
            inEndZone = true;
        }

    }

    private void OnTriggerExit(Collider other)
    {
        endPromptText.enabled = false;
        inEndZone = false;
    }

    private void DisplayLevelComplete()
    {
        normalHUD.SetActive(false);
        levelCompleteUI.SetActive(true);
        scoreCard.text = scoreHandler.timeUI;
        scoreCard.text += "<br>Score: " + scoreHandler.totalScore;
    }
}

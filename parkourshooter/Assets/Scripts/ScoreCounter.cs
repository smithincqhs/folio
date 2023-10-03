using System.Collections;
using System.Diagnostics;
using System.Collections.Generic;
using UnityEngine;

public class ScoreCounter : MonoBehaviour
{
    private bool timerActive;
    float time;
    public string timeUI;
    bool levelStarted = false;
    public int totalScore;
    public float levelDifficulty = 10;

    private void Start()
    {
        time = 0;
    }

    private void Update()
    {
        if(timerActive == true)
        {
            time = time + Time.deltaTime;
        } 
        else if (levelStarted == false)
        {
            if (Input.anyKeyDown)
            {
                StartTimer();
            }
        }
        timeUI = (Mathf.FloorToInt(time / 60)).ToString() + " : " + (Mathf.FloorToInt(time % 60)).ToString() + "." + Mathf.Abs(Mathf.FloorToInt((Mathf.FloorToInt(time % 60)-(time % 60))*1000)).ToString();
    }

    public void StartTimer()
    {
        timerActive = true;
    }

    public void StopTimer()
    {
        timerActive = false;
    }

    public void TimeToScore()
    {
        StopTimer();
        float _t = time * levelDifficulty - Mathf.FloorToInt(time / 60);
        AddScore(Mathf.RoundToInt(_t));
    }

    public void AddScore(int points)
    {
        totalScore += points;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadSceneA : MonoBehaviour
{
    public string sceneIndex = "Level1";
    public void LoadSceneAA()
    {
        SceneManager.LoadScene(sceneIndex);
    }
}

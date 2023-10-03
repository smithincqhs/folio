using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadSceneA : MonoBehaviour
{
    public void LoadSceneAA(string sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex);
    }
}

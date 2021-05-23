using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainSceneManager : MonoBehaviour
{
    private static MainSceneManager instance;
    public static MainSceneManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = GameObject.FindGameObjectWithTag("Scene Manager").GetComponent<MainSceneManager>();
            }
            return instance;
        }
    }

    public void LoadScene(int sceneindex)
    {
        SceneManager.LoadScene(sceneindex);
    }
}

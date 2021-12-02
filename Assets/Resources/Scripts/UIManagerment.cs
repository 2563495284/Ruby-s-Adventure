using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManagerment : MonoBehaviour
{
    public static UIManagerment Instance;
    public void StartGame()
    {
        SceneManager.LoadScene(1);

    }
    public void GameOver()
    {
        Invoke("loadStartScene", 2f);
    }
    public void loadStartScene()
    {
        SceneManager.LoadScene(0);
    }
}

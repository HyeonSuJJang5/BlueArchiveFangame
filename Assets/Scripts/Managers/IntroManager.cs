using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class IntroManager : MonoBehaviour
{


    public void GameStart()
    {
        SceneManager.LoadScene("First_GameScene");
    }

    public void Quit()
    {
        Application.Quit();
    }
}

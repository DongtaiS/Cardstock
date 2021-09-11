using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuScript : MonoBehaviour
{
    [SerializeField] Scene main;
    public void OnPlayButtonClick()
    {
        SceneManager.LoadScene("Main");
    }
    public void OnOptionsButtonClick()
    {

    }
    public void OnCreditsButtonClick()
    {

    }
    public void OnQuitButtonClick()
    {
        Application.Quit();
    }
}

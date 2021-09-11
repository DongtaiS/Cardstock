using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class PauseMenu : MenuScript
{
    [SerializeField] SettingsMenuScript settingsMenu;
    private protected override Vector3 closePos { get { return new Vector3(0, 1080); } }
    public override void OnEscape()
    {
        if (!IsOpen)
        {
            Pause();
        }
        else
        {
            Resume();
        }
    }
    public void Resume()
    {
        Globals.Unpause(1);
        Close();
        UIManagerScript.GetDarkBG().color = Globals.ChangeColorAlpha(UIManagerScript.GetDarkBG().color, 0);
    }
    public void Settings()
    {
        settingsMenu.Open();
        Close();
    }
    public void Quit()
    {
        SceneManager.LoadScene("MainMenu");
    }
    private void Pause()
    {
        Globals.Pause(1);
        Open();
        UIManagerScript.GetDarkBG().color = Globals.ChangeColorAlpha(UIManagerScript.GetDarkBG().color, 0.5f);
    }
}

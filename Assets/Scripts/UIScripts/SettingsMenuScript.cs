using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsMenuScript : MenuScript
{
    [SerializeField] private PauseMenu pauseMenu;
    private protected override Vector3 closePos { get { return new Vector3(0, -1080); } }
    public override void OnEscape()
    {
        Back();
    }
    public void Back()
    {
        Close();
        pauseMenu.Open();
    }
}
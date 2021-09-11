using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class EnergyScript : TextScript
{
    public void SetEnergy(int newEnergy)
    {
        SetText(newEnergy.ToString());
    }
    public override void SetAlpha(float alpha)
    {
        base.SetAlpha(alpha);
        GetComponent<Image>().color = Globals.ChangeColorAlpha(GetComponent<Image>().color, alpha);
    }
}

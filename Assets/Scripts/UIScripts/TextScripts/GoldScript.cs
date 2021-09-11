using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoldScript : TextScript
{
    public void SetGold(int inGold)
    {
        SetText("Gold: " + inGold);
    }
}

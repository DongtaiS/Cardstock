using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoldMenuItem : BattleEndMenuItem
{
    [SerializeField] private int goldAmount;
    private void Start()
    {
        Setup();
    }
    public override void OnClick()
    {
        Globals.PlayerCombat.AddGold(goldAmount);
        CheckHighlightAnim(FadeOut(1f));
    }
}

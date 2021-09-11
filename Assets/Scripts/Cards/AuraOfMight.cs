using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AuraOfMight : AbilityCard
{
    public override bool Check(Vector3Int targetCell)
    {
        return true;
    }

    private protected override IEnumerator PlayEffect()
    {
        foreach (CombatScript c in playerCombat.CurrentRoom.GetAllCombatables())
        {
            c.Buffs.Strength.IncrementValue(2);
        }
        yield return base.PlayEffect();
    }
    private protected override void CreateHighlights()
    {
        SpawnHighlight(playerCombat.CellCoord);
    }
    public override void OnMouseOverHighlight(Vector3Int coord)
    {
    }
}

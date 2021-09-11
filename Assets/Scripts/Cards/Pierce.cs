using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pierce : MeleeAttackCard
{
    private AnimationScript spear;
    private Vector3Int cellPlayed;
    public override bool Check(Vector3Int targetCell)
    {
        if (base.Check(targetCell) && CheckDirection(targetCell) && CheckPerpRange(targetCell, cardData.Range) && targetCell != playerCombat.CellCoord)
        {
            cellPlayed = targetCell;
            return CheckHits(Globals.GetCellsInLine(playerCombat.CellCoord, targetCell, false, true));
        }
        return false;
    }
    public override void UpdateDescription(bool successfulCheck)
    {
        base.UpdateDescription(successfulCheck);
        if (successfulCheck)
        {
            CardGameObjects.description.text = "Deal " + Keywords.DamageNum(cardData.HitEnemies[0].damage, baseDamage, attackType) + " " + Keywords.damage + ".\n" + Keywords.Piercing + ".";
        }
        else
        {
            CardGameObjects.description.text = "Deal " + Keywords.DamageNum(damage, baseDamage, attackType) + " " + Keywords.damage + ".\n" + Keywords.Piercing + ".";
        }
    }
    private protected override void CreatePreviews(Vector3Int targetCell)
    {
        base.CreatePreviews(targetCell);
        spear = Globals.PrefabManager.SpawnAnimationObject(AnimationObjectType.Spear, playerCombat.transform.position, cardData.Direction, playerCombat.CurrentRoom.transform);
        previews.Add(spear);
        spear.animator.enabled = false;
    }
    private protected override IEnumerator PlayEffect()
    {
        yield return playerCombat.CheckRotate(cardData.Direction);
        Coroutine spearAnim = spear.StartCoroutine(WeaponAnimations.ExtendSpear(spear, Globals.PerpDist(playerCombat.CellCoord, cellPlayed)));
        yield return playerCombat.StartCoroutine(DamageEnemies(DamageSFXType.Stab));
        yield return spearAnim;
        yield return base.PlayEffect();
        DestroyPreviews(true);
    }
    public override void OnMouseOverHighlight(Vector3Int coord)
    {
        base.OnMouseOverHighlight(coord);
        SpawnHighlightsLine(playerCombat.CellCoord, coord, true, false);
    }
    private protected override void CreateHighlights()
    {
        SpawnHighlightsPerp(cardData.Range, true, false);
    }
}

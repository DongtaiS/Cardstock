using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GreatCleave : MeleeAttackCard
{
    AnimationScript broadsword;
    public override bool Check(Vector3Int targetCell)
    {
        if (base.Check(targetCell) && CheckDirection(targetCell) && CheckPerpRange(targetCell, cardData.Range) && IsPathClear(targetCell, false))
        {
            Vector3Int tempCoord = (Vector3Int)Globals.IntDirectionToVector2(1 - cardData.Direction % 2);
            return CheckHits(new List<Vector3Int> { targetCell, targetCell + tempCoord, targetCell - tempCoord });
        }
        return false;
    }
    private protected override void CreatePreviews(Vector3Int targetCell)
    {
        base.CreatePreviews(targetCell);
        broadsword = Globals.PrefabManager.SpawnAnimationObject(AnimationObjectType.Broadsword, playerCombat.transform.position, cardData.Direction, playerCombat.CurrentRoom.transform);
        broadsword.animator.Play(WeaponAnimations.Broadsword.GreatCleaveSlow.ToString());
        previews.Add(broadsword);
    }
    public override void OnMouseOverHighlight(Vector3Int coord)
    {
        base.OnMouseOverHighlight(coord);
        Vector3Int temp = (Vector3Int)Globals.IntDirectionToVector2(1 - Globals.Vector3ToDir(playerCombat.CellCoord, coord) % 2);
        Color tempColor = defaultHighlightColor;
        tempColor.a /= 2;
        SpawnHighlight(coord);
        SpawnHighlight(coord + temp, tempColor);
        SpawnHighlight(coord - temp, tempColor);
    }
    public override void UpdateDescription(bool successfulCheck)
    {
        base.UpdateDescription(successfulCheck);
        if (successfulCheck)
        {
            CardGameObjects.description.text = "Deal " + Keywords.DamageNum(cardData.HitEnemies[0].damage, baseDamage, attackType) + " " + Keywords.damage + " to enemies " + cardData.Range + " tile(s) in front of you. \n 3 units wide.";
        }
        else
        {
            CardGameObjects.description.text = "Deal " + Keywords.DamageNum(damage, baseDamage, attackType) + " " + Keywords.damage + " to enemies " + cardData.Range + " tile(s) in front of you. \n 3 units wide.";
        }
    }
    private protected override IEnumerator PlayEffect()
    {
        yield return playerCombat.CheckRotate(cardData.Direction);
        yield return broadsword.PlayAndWaitForAnim(WeaponAnimations.Broadsword.GreatCleave.ToString());
        yield return DamageEnemies(DamageSFXType.Blade);
        yield return base.PlayEffect();
    }
    private protected override void CreateHighlights()
    {
        SpawnHighlightsPerp(cardData.Range, true, true);
    }
}
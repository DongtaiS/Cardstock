using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PikeThrust : MeleeAttackCard
{
    AnimationScript spear;
    public override bool Check(Vector3Int targetCell)
    {
        if (base.Check(targetCell) && CheckDirection(targetCell) && CheckPerpRange(targetCell, cardData.Range) && IsPathClear(targetCell, false) && targetCell != playerCombat.CellCoord)
        {
            return CheckHits(new List<Vector3Int> { targetCell });
        }
        return false;
    }
    public override void UpdateDescription(bool successfulCheck)
    {
        base.UpdateDescription(successfulCheck);
        if (successfulCheck)
        {
            CardGameObjects.description.text = "Deal " + Keywords.DamageNum(cardData.HitEnemies[0].damage, baseDamage, attackType) + " " + Keywords.damage + ".";
        }
        else
        {
            CardGameObjects.description.text = "Deal " + Keywords.DamageNum(damage, baseDamage, attackType) + " " + Keywords.damage + ".";
        }
    }
    public override void OnMouseOverHighlight(Vector3Int coord)
    {
        base.OnMouseOverHighlight(coord);
        SpawnHighlight(coord);
    }
    private protected override void CreatePreviews(Vector3Int targetCell)
    {
        base.CreatePreviews(targetCell);
        Vector3 position = playerCombat.CurrentRoom.GetFloor().GetCellCenterWorld(targetCell - (Vector3Int)Globals.IntDirectionToVector2(cardData.Direction));
        spear = Globals.PrefabManager.SpawnAnimationObject(AnimationObjectType.Spear, position, cardData.Direction, playerCombat.CurrentRoom.transform);
        spear.animator.Play(WeaponAnimations.Spear.ThrustSlow.ToString());
        previews.Add(spear);
    }
    private protected override IEnumerator PlayEffect()
    {
        yield return playerCombat.CheckRotate(cardData.Direction);
        Coroutine thrust = spear.StartCoroutine(spear.PlayAndWaitForAnim(WeaponAnimations.Spear.Thrust.ToString()));
        yield return DamageEnemies(DamageSFXType.Stab);
        yield return thrust;
        yield return base.PlayEffect();
        DestroyPreviews(true);
    }
    private protected override void CreateHighlights()
    {
        SpawnHighlightsPerp(cardData.Range, true, true);
    }
}

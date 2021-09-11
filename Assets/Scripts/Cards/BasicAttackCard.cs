using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicAttackCard : MeleeAttackCard
{
    private AnimationScript sword;
    public override bool Check(Vector3Int targetCell)
    {
        if (base.Check(targetCell) && CheckDirection(targetCell) && CheckPerpRange(targetCell, cardData.Range) && IsPathClear(targetCell, false))
        {
            return CheckHits(new List<Vector3Int> { targetCell });
        }
        return false;
    }
    private protected override void CreatePreviews(Vector3Int targetCell)
    {
        base.CreatePreviews(targetCell);
        sword = Globals.PrefabManager.SpawnAnimationObject(AnimationObjectType.Sword, playerCombat.transform.position, Globals.Vector3ToDir(playerCombat.CellCoord, targetCell), playerCombat.transform);
        previews.Add(sword);
        sword.animator.Play(WeaponAnimations.Sword.SwordSwingSlow.ToString());
    }
    public override void OnMouseOverHighlight(Vector3Int coord)
    {
        base.OnMouseOverHighlight(coord);
        SpawnHighlight(coord);
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
    private protected override IEnumerator PlayEffect()
    {
        yield return playerCombat.CheckRotate(cardData.Direction);
        Coroutine dmgAnim = playerCombat.StartCoroutine(DamageEnemies(DamageSFXType.Blade));
        yield return sword.PlayAndWaitForAnim(WeaponAnimations.Sword.SwordSwing.ToString());
        yield return dmgAnim;
        yield return base.PlayEffect();
    }
    private protected override void CreateHighlights()
    {
        SpawnHighlightsPerp(cardData.Range, true, true);
    }
}

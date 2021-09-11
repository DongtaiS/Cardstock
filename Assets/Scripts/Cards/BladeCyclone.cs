using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BladeCyclone : MeleeAttackCard
{
    private AnimationScript sword;
    public override bool Check(Vector3Int targetCell)
    {
        if (base.Check(targetCell) && Globals.InCellRadius(playerCombat.CellCoord, targetCell, cardData.Range) && targetCell != playerCombat.CellCoord)
        {
            return CheckHits(Globals.GetCellsInRadius(playerCombat.CellCoord, cardData.Range));
        }
        return false;
    }
    public override void UpdateDescription(bool successfulCheck)
    {
        base.UpdateDescription(successfulCheck);
        if (successfulCheck)
        {
            CardGameObjects.description.text = "Deal " + Keywords.DamageNum(cardData.HitEnemies[0].damage, baseDamage, attackType) + " " + Keywords.damage + " to enemies in a " + cardData.Range + " unit radius";
        }
        else
        {
            CardGameObjects.description.text = "Deal " + Keywords.DamageNum(damage, baseDamage, attackType) + " " + Keywords.damage + " to enemies in a " + cardData.Range + " unit radius";
        }
    }
    private protected override void CreatePreviews(Vector3Int targetCell)
    {
        base.CreatePreviews(targetCell);
        if (previews.Count == 0)
        {
            sword = Globals.PrefabManager.SpawnAnimationObject(AnimationObjectType.Sword, playerCombat.transform.position, 0, playerCombat.transform);
            previews.Add(sword);
            sword.animator.Play(WeaponAnimations.Sword.SwordSpinSlow.ToString());
        }
    }
    private protected override IEnumerator PlayEffect()
    {
        Coroutine dmg = StartCoroutine(DamageEnemies(DamageSFXType.Blade));
        yield return sword.PlayAndWaitForAnim(WeaponAnimations.Sword.SwordSpin.ToString());
        yield return dmg;
        yield return base.PlayEffect();
    }
    public override void OnMouseOverHighlight(Vector3Int coord)
    {
        base.OnMouseOverHighlight(coord);
        foreach (CardAttackData e in cardData.HitEnemies)
        {
            SpawnHighlight(e.combatable.CellCoord);
        }
    }
    private protected override void CreateHighlights()
    {
        SpawnHighlightsCircle(playerCombat.CellCoord, cardData.Range, defaultHighlightColor);
    }
}

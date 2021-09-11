using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverheadSlam : MeleeAttackCard
{
    private AnimationScript broadsword;
    private int originalDir = -1;
    public override bool Check(Vector3Int targetCell)
    {
        if (base.Check(targetCell) && CheckDirection(targetCell) && CheckPerpRange(targetCell, cardData.Range) && IsPathClear(targetCell, false))
        {
            return CheckHits(new List<Vector3Int> { targetCell } );
        }
        return false;
    }
    private protected override void CreatePreviews(Vector3Int targetCell)
    {
        base.CreatePreviews(targetCell);
        originalDir = playerCombat.FacingDirection;
        playerCombat.StartCoroutine(playerCombat.CheckRotate(cardData.Direction));
        broadsword = Globals.PrefabManager.SpawnAnimationObject(AnimationObjectType.Broadsword, playerCombat.transform.position, cardData.Direction, playerCombat.CurrentRoom.transform);
        broadsword.animator.Play(WeaponAnimations.Broadsword.OverheadSwingSlow.ToString());
        previews.Add(broadsword);
    }
    private protected override void DestroyPreviews(bool fade)
    {
        base.DestroyPreviews(fade);
        if (originalDir != -1)
        {
            playerCombat.StartCoroutine(playerCombat.CheckRotate(originalDir));
        }
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
            CardGameObjects.description.text = "Deal " + Keywords.DamageNum(cardData.HitEnemies[0].damage, baseDamage, attackType) + " " + Keywords.damage + " and apply 1 " + Keywords.stun;
        }
        else
        {
            CardGameObjects.description.text = "Deal " + Keywords.DamageNum(damage, baseDamage, attackType) + " " + Keywords.damage + " and apply 1 " + Keywords.stun;
        }
    }
    private protected override IEnumerator PlayEffect()
    {
        originalDir = -1;
        yield return playerCombat.CheckRotate(cardData.Direction);
        yield return broadsword.PlayAndWaitForAnim(WeaponAnimations.Broadsword.OverheadSwing.ToString());
        cardData.HitEnemies[0].combatable.Buffs.Stunned.Activate(1);
        yield return DamageEnemies(DamageSFXType.Blade);
        yield return base.PlayEffect();
    }
    private protected override void CreateHighlights()
    {
        SpawnHighlightsPerp(cardData.Range, true, true);
    }
}
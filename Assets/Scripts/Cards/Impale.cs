using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Impale : MeleeAttackCard
{
    private int pullDist;
    private Vector3Int pullCell;
    private AnimationScript spear;
    public override bool Check(Vector3Int targetCell)
    {
        if (base.Check(targetCell) && CheckDirection(targetCell) && CheckPerpRange(targetCell, cardData.Range) && IsPathClear(targetCell, false) && targetCell != playerCombat.CellCoord)
        {
            pullDist = 0;
            pullCell = targetCell;
            for (int i = 1; i <= 2; i++)
            {
                Vector3Int tempCell = targetCell - i * (Vector3Int)Globals.IntDirectionToVector2(cardData.Direction);
                if (playerCombat.CurrentRoom.HasEmptyTile(tempCell))
                {
                    pullDist = i;
                    pullCell = tempCell;
                }
                else
                {
                    break;
                }
            }
            return CheckHits(new List<Vector3Int> { targetCell });
        }
        return false;
    }
    public override void UpdateDescription(bool successfulCheck)
    {
        base.UpdateDescription(successfulCheck);
        if (successfulCheck)
        {
            CardGameObjects.description.text = "Deal " + Keywords.DamageNum(cardData.HitEnemies[0].damage, baseDamage, attackType) + " " + Keywords.damage + ".\nPull the enemy 2 units forward.";
        }
        else
        {
            CardGameObjects.description.text = "Deal " + Keywords.DamageNum(damage, baseDamage, attackType) + " " + Keywords.damage + ".\nPull the enemy 2 units forward.";
        }
    }
    private protected override void CreatePreviews(Vector3Int targetCell)
    {
        base.CreatePreviews(targetCell);
        spear = Globals.PrefabManager.SpawnAnimationObject(AnimationObjectType.Spear, playerCombat.transform.position, cardData.Direction, playerCombat.CurrentRoom.transform);
        previews.Add(spear);
        spear.animator.enabled = false;
        CombatScript combatable = hitEnemies[0].combatable;
        AnimationScript temp = SpawnPreview(combatable, combatable.transform.position, Globals.OppositeDirection(cardData.Direction), 0.25f);
        temp.StartCoroutine(Globals.InterpVector3(combatable.transform.localPosition, combatable.CurrentRoom.GetFloor().GetCellCenterLocal(pullCell), 0.25f * pullDist, pos => temp.transform.localPosition = pos));
    }
    private protected override IEnumerator PlayEffect()
    {
        previews.Remove(spear);
        DestroyPreviews(true);
        yield return playerCombat.CheckRotate(cardData.Direction);
        CombatScript enemy = hitEnemies[0].combatable;
        Coroutine spearAnim = spear.StartCoroutine(WeaponAnimations.ExtendSpear(spear, (int)Vector3Int.Distance(playerCombat.CellCoord, enemy.CellCoord)));
        Coroutine dmg = playerCombat.StartCoroutine(DamageEnemies(DamageSFXType.Stab));
        yield return Globals.WaitForSeconds((int)Vector3Int.Distance(playerCombat.CellCoord, enemy.CellCoord) * 0.2f);
        enemy.SetDirection(Globals.OppositeDirection(cardData.Direction));
        yield return enemy.Knockback(Globals.OppositeDirection(cardData.Direction), pullDist);
        yield return spearAnim;
        yield return dmg;
        yield return FadeThenDestroyAnimObject(spear);
        yield return base.PlayEffect();
    }
    public override void OnMouseOverHighlight(Vector3Int coord)
    {
        base.OnMouseOverHighlight(coord);
        SpawnHighlight(coord, staticCardInfo.Attackhighlight);
        SpawnHighlight(pullCell, Globals.ChangeColorAlpha(staticCardInfo.Attackhighlight, staticCardInfo.Attackhighlight.a/2));
    }
    private protected override void CreateHighlights()
    {
        SpawnHighlightsPerp(cardData.Range, true, true);
    }
}

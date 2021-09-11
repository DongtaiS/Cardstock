using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForcefulStrike : MeleeAttackCard
{
    private int baseKBDist = 2;
    private int kbDist;
    private Vector3Int defaultCell = new Vector3Int(0, 0, -100);
    private Vector3Int kbCell;
    public override bool Check(Vector3Int targetCell)
    {
        if (base.Check(targetCell) && CheckDirection(targetCell) && CheckPerpRange(targetCell, cardData.Range) && IsPathClear(targetCell, false))
        {
            kbDist = 0;
            kbCell = defaultCell;
            if (CheckHits(new List<Vector3Int> { targetCell }))
            {
                for (int i = 1; i <= baseKBDist; i++)
                {
                    Vector3Int tempCoord = targetCell + i * (Vector3Int)Globals.IntDirectionToVector2(cardData.Direction);
                    if (playerCombat.CurrentRoom.HasEmptyTile(tempCoord))
                    {
                        kbDist = i;
                        kbCell = tempCoord;
                    }
                    else
                    {
                        break;
                    }
                }
                return true;
            }
        }
        return false;
    }
    private protected override void CreatePreviews(Vector3Int targetCell)
    {
        base.CreatePreviews(targetCell);
        if (kbCell != defaultCell && kbCell != cardData.HitEnemies[0].combatable.CellCoord)
        {
            CombatScript combatable = cardData.HitEnemies[0].combatable;
            AnimationScript temp = SpawnPreview(combatable, combatable.transform.position, combatable.FacingDirection, 0.25f);
            Vector3 target = playerCombat.CurrentRoom.GetFloor().GetCellCenterLocal(kbCell);
            temp.StartCoroutine(Globals.InterpVector3(temp.transform.localPosition, target, 0.15f, pos => temp.transform.localPosition = pos));
        }
    }
    public override void OnMouseOverHighlight(Vector3Int coord)
    {
        base.OnMouseOverHighlight(coord);
        SpawnHighlight(coord);
        SpawnHighlight(kbCell, Globals.ChangeColorAlpha(staticCardInfo.Attackhighlight, staticCardInfo.Attackhighlight.a/2));
    }
    public override void UpdateDescription(bool successfulCheck)
    {
        base.UpdateDescription(successfulCheck);
        if (successfulCheck)
        {
            CardGameObjects.description.text = "Deal " + Keywords.DamageNum(cardData.HitEnemies[0].damage, baseDamage, attackType) + " " + Keywords.damage + ".\nKnock back enemy up to 2 units.";
        }
        else
        {
            CardGameObjects.description.text = "Deal " + Keywords.DamageNum(damage, baseDamage, attackType) + " " + Keywords.damage +".\nKnock back enemy up to 2 units.";
        }
    }
    private protected override IEnumerator PlayEffect()
    {
        yield return playerCombat.CheckRotate(cardData.Direction);
        playerCombat.StartCoroutine(cardData.HitEnemies[0].combatable.Knockback(cardData.Direction, kbDist));
        DestroyPreviews(true);
        yield return DamageEnemies(DamageSFXType.Fist);
        yield return base.PlayEffect();
    }
    private protected override void CreateHighlights()
    {
        SpawnHighlightsPerp(cardData.Range, true, true);
    }
}

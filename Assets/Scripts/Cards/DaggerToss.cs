using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DaggerToss : AttackCard
{
    [SerializeField] private int moveRange;
    public AnimationCurve curve;
    Vector3Int moveCell;
    Vector3Int attackCell;
    public override CardType Type { get { return CardType.Movement; } }
    public override bool Check(Vector3Int targetCell)
    {
        if (base.Check(targetCell) && CheckDirection(targetCell) && CheckPerpRange(targetCell, cardData.Range) && IsPathClear(targetCell, false) && targetCell != playerCombat.CellCoord)
        {
            attackCell = targetCell;
            CheckHits(new List<Vector3Int> { attackCell });
            moveCell = playerCombat.CellCoord;
            for (int i = moveRange; i > 0; i--)
            {
                Vector3Int tempCell = playerCombat.CellCoord + i * (Vector3Int)Globals.IntDirectionToVector2(Globals.OppositeDirection(cardData.Direction));
                if (IsPathClear(tempCell, true) && playerBuffs.CanMoveTo(tempCell, false))
                {
                    moveCell = tempCell;
                    break;
                }
            }
            cardData.MoveDist = (int)Vector3Int.Distance(playerCombat.CellCoord, moveCell);
            return true;
        }
        return false;
    }
    public override void Setup()
    {
        base.Setup();
        attackType = AttackType.Ranged;
        defaultHighlightColor = staticCardInfo.Attackhighlight;
    }
    public override void UpdateValues()
    {
        base.UpdateValues();
        cardData.Range = baseRange + playerBuffs.Range.Value;
        damage = baseDamage + playerBuffs.SharpSight.GetValue();
    }
    public override void UpdateDescription(bool successfulCheck)
    {
        base.UpdateDescription(successfulCheck);
        if (successfulCheck && cardData.HitEnemies.Count > 1)
        {
            CardGameObjects.description.text = "Deal " + Keywords.DamageNum(cardData.HitEnemies[0].damage, baseDamage, attackType) + " " + Keywords.damage + ". Retreat " + cardData.Range + " units." ;
        }
        else
        {
            CardGameObjects.description.text = "Deal " + Keywords.DamageNum(damage, baseDamage, attackType) + " " + Keywords.damage + ". Retreat " + cardData.Range + " units.";
        }
    }
    private protected override IEnumerator PlayEffect()
    {
        camScript.ActivatePlayerCam();
        yield return playerCombat.CheckRotate(cardData.Direction);
        ProjectileScript dagger = Globals.PrefabManager.SpawnProjectile(ProjectileTypes.Dagger, playerCombat.CurrentRoom.transform);
        Coroutine dash = playerCombat.StartCoroutine(playerCombat.DashWithoutTurn(moveCell));
        yield return dagger.MoveTo(playerCombat.transform.localPosition, playerCombat.CurrentRoom.GetFloor().GetCellCenterLocal(attackCell), Vector3Int.Distance(playerCombat.CellCoord, attackCell) * 0.1f, cardData.Direction);
        Coroutine dmg = playerCombat.StartCoroutine(DamageEnemies(DamageSFXType.Blade));
        Destroy(dagger.gameObject);
        yield return dmg;
        yield return dash;
        yield return base.PlayEffect();
    }
    public override void OnMouseOverHighlight(Vector3Int coord)
    {
        base.OnMouseOverHighlight(coord);
        SpawnHighlight(moveCell, staticCardInfo.MovementHighlight);
        SpawnHighlight(coord, staticCardInfo.Attackhighlight);
        SpawnHighlightsLine(playerCombat.CellCoord, moveCell, false, true, staticCardInfo.MovementHighlight);
    }
    private protected override void CreateHighlights()
    {
        SpawnHighlightsPerp(cardData.Range, true, true);
    }
}

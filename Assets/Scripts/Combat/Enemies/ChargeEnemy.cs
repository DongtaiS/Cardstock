using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChargeEnemy : EnemyScript
{
    [SerializeField] private int damage;
    [SerializeField] private int baseAttackRange;
    [SerializeField] private int chargeDamage;
    [SerializeField] private int chargeRange;
    private int minChargeRange = 3;
    private EnemyAction charge;
    private EnemyAction attack;
    private enum ChargerAnims { Walk, WalkMid, WalkEnd, Charge, ChargeMid, ChargeEnd, Bite, BiteEnd, OnHit, Idle };
    private protected override void SetupActions()
    {
        base.SetupActions();
        charge = new EnemyAction(0, 0, chargeRange, chargeDamage, ChargeCondition, ChargeEffect);
        attack = new EnemyAction(0, 0, baseAttackRange, damage, AttackCondition, AttackEffect);
        actions.InsertRange(0, new List<EnemyAction> { charge, attack, });
    }
    private bool AttackCondition()
    {
        return CanAttack(playerCombat.CellCoord, baseAttackRange, true, true, false);
    }
    private protected IEnumerator AttackEffect()
    {
        yield return TurnToPlayer();
        yield return AnimObject.PlayAndWaitForAnim(ChargerAnims.Bite.ToString());
        Coroutine biteEnd = StartCoroutine(AnimObject.WaitForAnim(ChargerAnims.BiteEnd.ToString(), 0));
        yield return DamagePlayer(damage, true);
        yield return biteEnd;
    }
    private bool ChargeCondition()
    {
        return CheckPlayerPerp() && IsPathClear(CellCoord, playerCombat.CellCoord, false) && Buffs.CanMoveTo(playerCombat.CellCoord) && Globals.InRange(minChargeRange, chargeRange, Globals.PerpDist(CellCoord, playerCombat.CellCoord));
    }
    private protected IEnumerator ChargeEffect()
    {
        yield return TurnToPlayer();
        List<Vector3Int> path = GenPath(playerCombat.CellCoord, chargeRange, false);
        Premove();
        yield return AnimObject.PlayAndWaitForAnim(ChargerAnims.Charge.ToString());
        yield return Globals.InterpVector3(transform.localPosition, CurrentRoom.GetFloor().GetCellCenterLocal(path[path.Count - 2]), (moveDuration / 4f) * (path.Count - 2), newPos => transform.localPosition = newPos);
        AnimObject.animator.Play(ChargerAnims.ChargeEnd.ToString());
        yield return Globals.InterpVector3(transform.localPosition, CurrentRoom.GetFloor().GetCellCenterLocal(path[path.Count - 1]), moveDuration / 4f, newPos => transform.localPosition = newPos);
        if (path.Count > minChargeRange)
        {
            yield return DamagePlayer(chargeDamage, true);
        }
        Postmove();
    }
    private protected override void AnimateMove(int dir)
    {
        base.AnimateMove(dir);
        AnimObject.animator.Play(ChargerAnims.Walk.ToString());
    }
    private protected override void ResetAnimation(int dir)
    {
        base.ResetAnimation(dir);
        AnimObject.animator.Play(ChargerAnims.WalkEnd.ToString());
    }
    private protected override IEnumerator OnHitAnim(int dir)
    {
        AnimObject.animator.Play(ChargerAnims.OnHit.ToString());
        yield return base.OnHitAnim(dir);
    }
    private protected override void GenFinalPath()
    {
        savedPath = GenPath(playerCombat.CellCoord, false);
        List<Vector3Int> cellsToCheck = new List<Vector3Int>();
        if (Globals.PerpDist(CellCoord, playerCombat.CellCoord) >= minChargeRange)
        {
            for (int d = 0; d < 4; d++)
            {
                for (int i = Globals.PerpDist(CellCoord, playerCombat.CellCoord); i >= minChargeRange; i--)
                {
                    cellsToCheck.Add(playerCombat.CellCoord + i * (Vector3Int)Globals.IntDirectionToVector2(d));
                    savedPath = FindShortestPathToAnyCell(cellsToCheck, true);
                }
            }
        }
    }
}

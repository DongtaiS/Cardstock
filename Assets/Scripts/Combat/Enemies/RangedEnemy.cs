using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangedEnemy : EnemyScript
{
    [SerializeField] private int damage;
    [SerializeField] private protected int baseRange;
    private EnemyAction rangedAttack;
    private enum RangedEnemyAnims { Aim, AimMid, AimEnd, WalkLift, Walk, OnHit, Idle, EyeChargeStart, EyeChargeMid, EyeChargeEnd, EyeChargeRecover, EyeOnHit}
    private protected override void SetupActions()
    {
        base.SetupActions();
        rangedAttack = new EnemyAction(0, 0, baseRange, damage, RangedAttackCondition, RangedAttack);
        actions.Insert(0, rangedAttack);
    }
    private protected override void GenFinalPath()
    {
        List<Vector3Int> cells = new List<Vector3Int>();
        Vector3Int tempCell;
        for (int d = 0; d < 4; d++)
        {
            tempCell = playerCombat.CellCoord;
            for (int i = 1; i < baseRange; i++)
            {
                tempCell += (Vector3Int)Globals.IntDirectionToVector2(d);
                cells.Add(tempCell);
            }
        }
        savedPath = FindShortestPathToAnyCell(cells, true);
    }
    private protected override void AnimateMove(int dir)
    {
        base.AnimateMove(dir);
        StartCoroutine(AnimObject.PlayAndWaitForAnim(RangedEnemyAnims.Walk.ToString()));
    }
    private protected override void ResetAnimation(int dir)
    {
        base.ResetAnimation(dir);
        StartCoroutine(AnimObject.PlayAndWaitForAnim(RangedEnemyAnims.Idle.ToString()));
    }
    private bool RangedAttackCondition()
    {
        return CanAttack(playerCombat.CellCoord, baseRange, true, false, false);
    }
    private protected override IEnumerator OnHitAnim(int dir)
    {
        AnimObject.animator.Play(RangedEnemyAnims.OnHit.ToString());
        AnimObject.animator.Play(RangedEnemyAnims.EyeOnHit.ToString(), 1);
        yield return base.OnHitAnim(dir);
    }
    private protected IEnumerator RangedAttack()
    {
        yield return TurnToPlayer();
        Coroutine eye = StartCoroutine(AnimObject.PlayAndWaitForAnim(RangedEnemyAnims.EyeChargeStart.ToString(), 1));
        yield return AnimObject.PlayAndWaitForAnim(RangedEnemyAnims.Aim.ToString());
        yield return eye;
        yield return AnimObject.PlayAndWaitForAnim(RangedEnemyAnims.EyeChargeEnd.ToString(), 1);
        Coroutine dmg = StartCoroutine(DamagePlayer(damage, false));
        yield return AnimObject.PlayAndWaitForAnim(RangedEnemyAnims.AimEnd.ToString());
        yield return dmg;
    }
}

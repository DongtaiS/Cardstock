using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

public class KnightEnemy : EnemyScript
{
    private EnemyAction swingSword;
    private EnemyAction pray;
    private EnemyAction intimidate;
    private EnemyAction stab;
    private EnemyAction pommelStrike;
    private EnemyAction move;
    private enum KnightAnims { SwingSword, SwingSwordEnd, Pray, PrayEnd, Intimidate, Stab, StabEnd, PommelStrike, PommelStrikeEnd, Walk, OnHit, Idle }
    private protected override void SetupActions()
    {
        moveDuration = 1.5f;
        base.SetupActions();
        swingSword = new EnemyAction(0, 0, 2, 10, SwingSwordCondition, SwingSwordEffect);
        pray = new EnemyAction(3, 3, 0, 2, PrayCondition, PrayEffect);
        intimidate = new EnemyAction(5, 0, 3, 1, IntimidateCondition, IntimidateEffect);
        stab = new EnemyAction(0, 0, 1, 14, StabCondition, StabEffect);
        pommelStrike = new EnemyAction(4, 0, 1, 7, PommelStrikeCondition, PommelStrikeEffect);
        actions.InsertRange(0, new List<EnemyAction>() { pommelStrike, stab, swingSword, intimidate, pray });
    }
    private protected override void GenFinalPath()
    {
        savedPath = GenPath(playerCombat.CellCoord, false);
    }
    private protected override void AnimateMove(int dir)
    {
        base.AnimateMove(dir);
        AnimObject.animator.Play(KnightAnims.Walk.ToString());
    }
    private protected override void ResetAnimation(int dir)
    {
        base.ResetAnimation(dir);
        AnimObject.animator.Play(KnightAnims.Idle.ToString());
    }
    private protected override IEnumerator OnHitAnim(int dir)
    {
        AnimObject.animator.Play(KnightAnims.OnHit.ToString());
        yield return base.OnHitAnim(dir);
    }
    private bool SwingSwordCondition()
    {
        return CanAttack(playerCombat.CellCoord, (int)swingSword.range, true, true, false);
    }
    private IEnumerator SwingSwordEffect()
    {
        yield return TurnToPlayer();
        yield return AnimObject.PlayAndWaitForAnim(KnightAnims.SwingSword.ToString());
        Coroutine damage = StartCoroutine(DamagePlayer(swingSword.value, true));
        yield return AnimObject.PlayAndWaitForAnim(KnightAnims.SwingSwordEnd.ToString());
    }
    private bool PrayCondition()
    {
        return Globals.PerpDist(CellCoord, playerCombat.CellCoord) >= 3;
    }
    private IEnumerator PrayEffect()
    {
        yield return AnimObject.PlayAndWaitForAnim(KnightAnims.Pray.ToString());
        Buffs.Strength.IncrementValue(2);
        yield return AnimObject.PlayAndWaitForAnim(KnightAnims.PrayEnd.ToString());
    }
    private bool IntimidateCondition()
    {
        return Globals.CheckPerpRange(CellCoord, playerCombat.CellCoord, 0, 3) && IsPathClear(CellCoord, playerCombat.CellCoord, false);
    }
    private IEnumerator IntimidateEffect()
    {
        yield return TurnToPlayer();
        yield return AnimObject.PlayAndWaitForAnim(KnightAnims.Intimidate.ToString());
        playerCombat.Buffs.Intimidated.Activate(1);
        yield return Globals.WaitForSeconds(0.5f);
    }
    private bool StabCondition()
    {
        return CanAttack(playerCombat.CellCoord, (int)stab.range, true, true, false);
    }
    private IEnumerator StabEffect()
    {
        yield return TurnToPlayer();
        yield return AnimObject.PlayAndWaitForAnim(KnightAnims.Stab.ToString());
        Coroutine dmg = StartCoroutine(DamagePlayer(stab.value, true, DamageSFXType.Stab));
        yield return AnimObject.WaitForAnim(KnightAnims.StabEnd.ToString());
        yield return dmg;
    }
    private bool PommelStrikeCondition()
    {
        return CanAttack(playerCombat.CellCoord, (int)pommelStrike.range, true, true, false);
    }
    private IEnumerator PommelStrikeEffect()
    {
        yield return TurnToPlayer();
        yield return AnimObject.PlayAndWaitForAnim(KnightAnims.PommelStrike.ToString());
        playerCombat.Buffs.Stunned.Activate(1);
        yield return DamagePlayer(pommelStrike.value, true, DamageSFXType.Blunt);
    }
}

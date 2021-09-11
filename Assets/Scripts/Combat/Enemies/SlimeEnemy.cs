using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class SlimeEnemy : EnemyScript
{
    [SerializeField] private int damage;
    [SerializeField] private int baseAttackRange;
    private EnemyAction slimeSlow;
    private EnemyAction attack;
    private enum SlimeAnims { Bounce, Attack, OnHit, Idle};
    private protected override void SetupActions()
    {
        base.SetupActions();
        slimeSlow = new EnemyAction(3, 0, 2, 2, SlimeSlowCondition, SlimeSlowEffect);
        attack = new EnemyAction(0, 0, baseAttackRange, damage, AttackCondition, AttackEffect);
        actions.InsertRange(0, new List<EnemyAction> { attack, slimeSlow });
    }
    private protected override void AnimateMove(int dir)
    {
        base.AnimateMove(dir);
        AnimObject.animator.Play(SlimeAnims.Bounce.ToString());
    }
    private protected override void ResetAnimation(int dir)
    {
        base.ResetAnimation(dir);
        AnimObject.animator.Play(SlimeAnims.Idle.ToString());
    }
    private protected override void GenFinalPath()
    {
        savedPath = GenPath(playerCombat.CellCoord, false);
    }
    private bool SlimeSlowCondition()
    {
        return CheckPlayerPerp() && Globals.PerpDist(CellCoord, playerCombat.CellCoord) <= slimeSlow.range;
    }
    private protected IEnumerator SlimeSlowEffect()
    {
        yield return TurnToPlayer();
        playerCombat.Buffs.Quick.Activate(gameObject, false, 2);
        yield return Globals.WaitForSeconds(0.5f);
    }
    private bool AttackCondition()
    {
        return CanAttack(playerCombat.CellCoord, baseAttackRange, true, true, false);
    }
    private protected IEnumerator AttackEffect()
    {
        yield return TurnToPlayer();
        Coroutine anim = StartCoroutine(AnimObject.PlayAndWaitForAnim(SlimeAnims.Attack.ToString()));
        yield return DamagePlayer(damage, true);
        yield return anim;
    }
}

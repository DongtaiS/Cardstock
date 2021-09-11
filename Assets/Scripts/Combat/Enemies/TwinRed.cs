using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TwinRed : EnemyScript
{
    [System.NonSerialized] public TwinBlue twinBlue;
    int fireballRange = 4;
    int fireballDamage = 10;
    int baseFireWallCD = 4;
    int fireWallCD = 0;
    int fireWallDamage;
    private EnemyAction fireball;
    private EnemyAction heal;
    private EnemyAction moveToHeal;
    private enum TwinRedAnims { Aim, AimMid, AimEnd, Heal, HealEnd, Walk, OnHit, Idle, EyesOnHit, EyesIdle }
    private protected override void SetupActions()
    {
        base.SetupActions();
        fireball = new EnemyAction(0, 0, fireballRange, fireballDamage, FireballCondition, FireballEffect);
        heal = new EnemyAction(3, 0, 3, 20, HealCondition, HealEffect);
        moveToHeal = new EnemyAction(0, 0, 0, 0, MoveToHealCondition, MoveToHealEffect);
        actions.InsertRange(0, new List<EnemyAction> { heal, fireball, moveToHeal });
    }
    private protected override void AnimateMove(int dir)
    {
        base.AnimateMove(dir);
        AnimObject.animator.Play(TwinRedAnims.Walk.ToString());
    }
    private protected override void ResetAnimation(int dir)
    {
        base.ResetAnimation(dir);
        AnimObject.animator.Play("Idle");
    }
    private protected override void GenFinalPath()
    {
        savedPath = GenPath(playerCombat.CellCoord, false);
    }
    private bool FireballCondition()
    {
        return CanAttack(playerCombat.CellCoord, fireballRange, true, false, false);
    }
    private IEnumerator FireballEffect()
    {
        yield return TurnToPlayer();
        //make fireball projectile and shoot it
        yield return AnimObject.PlayAndWaitForAnim(TwinRedAnims.Aim.ToString());
        yield return AnimObject.WaitAnimStart(TwinRedAnims.AimEnd.ToString());
        yield return DamagePlayer(fireballDamage, false);
    }
    private IEnumerator Heal(EnemyScript target)
    {
        yield return AnimObject.PlayAndWaitForAnim(TwinRedAnims.Heal.ToString());
        Coroutine end = StartCoroutine(AnimObject.WaitForAnim(TwinRedAnims.HealEnd.ToString(), 0));
        if (target == this)
        {
            yield return HealHp(heal.value / 2);
        }
        else
        {
            yield return target.HealHp(heal.value);
        }
        yield return end;
    }
    private bool HealCondition()
    {
        return twinBlue != null && twinBlue.Hp < twinBlue.MaxHp / 2f && Globals.PerpDist(CellCoord, twinBlue.CellCoord) <= heal.range || Hp < MaxHp / 2;
    }
    private IEnumerator HealEffect()
    {
        if (twinBlue != null && twinBlue.Hp < twinBlue.MaxHp / 2f && Globals.PerpDist(CellCoord, twinBlue.CellCoord) <= heal.range)
        {
            yield return Heal(twinBlue);
        }
        else if (Hp < MaxHp / 2)
        {
            yield return Heal(this);
        }
    }
    private bool MoveToHealCondition()
    {
        return twinBlue != null && twinBlue.Hp < twinBlue.MaxHp / 2f && Buffs.CanMoveTo(twinBlue.CellCoord);
    }
    private IEnumerator MoveToHealEffect()
    {
        yield return Move(GenPath(twinBlue.CellCoord, false));
    }
    private protected override IEnumerator OnHitAnim(int dir)
    {
        StartCoroutine(AnimObject.PlayAndWaitForAnim(TwinRedAnims.OnHit.ToString()));
        StartCoroutine(AnimObject.PlayAndWaitForAnim(TwinRedAnims.EyesOnHit.ToString()));
        yield return base.OnHitAnim(dir);
    }
    //TODO Fire wall
}
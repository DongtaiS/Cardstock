using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
public class EyePriest : EnemyScript
{
    [SerializeField] private int damage;
    private int baseAttackRange = 2;
    private EnemyAction heal;
    private EnemyAction moveToHeal;
    private EnemyAction buffStrength;
    private EnemyAction debuffImprecise;
    private EnemyAction attack;
    private enum EyePriestAnims { Heal, HealEnd, Debuff, DebuffEnd, Buff, BuffEnd, Walk, OnHit, Idle };
    private protected override void SetupActions()
    {
        moveDuration = 1.5f;
        base.SetupActions();
        heal = new EnemyAction(4, 0, 2, 10, HealCondition, HealEffect);
        moveToHeal = new EnemyAction(0, 0, 0, 0, MoveToHealCondition, MoveToHealEffect);
        buffStrength = new EnemyAction(4, 0, 2, 2, BuffStrengthCondition, BuffStrengthEffect);
        debuffImprecise = new EnemyAction(4, 0, 3, 3, DebuffImpreciseCondition, DebuffImpreciseEffect);
        attack = new EnemyAction(0, 0, baseAttackRange, damage, AttackCondition, AttackEffect);
        actions.InsertRange(0, new List<EnemyAction> { heal, moveToHeal, buffStrength, debuffImprecise, attack });
    }
    private protected override IEnumerator OnHitAnim(int dir)
    {
        AnimObject.animator.Play(EyePriestAnims.OnHit.ToString());
        yield return base.OnHitAnim(dir);
    }
    private protected override void GenFinalPath()
    {
        savedPath = GenPath(playerCombat.CellCoord, false);
    }
    private protected override void AnimateMove(int dir)
    {
        base.AnimateMove(dir);
        AnimObject.animator.Play(EyePriestAnims.Walk.ToString());
    }
    private protected override void ResetAnimation(int dir)
    {
        base.ResetAnimation(dir);
        AnimObject.animator.Play(EyePriestAnims.Idle.ToString());
    }
    private bool AttackCondition()
    {
        return CanAttack(playerCombat.CellCoord, baseAttackRange, true, false, false);
    }
    private IEnumerator AttackEffect()
    {
        //play animation
        yield return TurnToPlayer();
        yield return DamagePlayer(damage, true);
    }
    private bool DebuffImpreciseCondition()
    {
        return CheckPlayerPerp() && Globals.PerpDist(CellCoord, playerCombat.CellCoord) <= debuffImprecise.range;
    }
    private IEnumerator DebuffImpreciseEffect()
    {
        yield return TurnToPlayer();
        yield return AnimObject.PlayAndWaitForAnim(EyePriestAnims.Debuff.ToString());
        playerCombat.Buffs.SharpSight.IncrementValue(-debuffImprecise.value);
        yield return AnimObject.WaitForAnim(EyePriestAnims.DebuffEnd.ToString(), 0);
    }
    private bool BuffStrengthCondition()
    {
        return CurrentRoom.GetEnemiesInRadius(CellCoord, buffStrength.range).Count > 0;
    }
    private IEnumerator BuffStrengthEffect()
    {
        yield return AnimObject.PlayAndWaitForAnim(EyePriestAnims.Buff.ToString());
        List<Vector3Int> buffCells = Globals.GetCellsInRadius(CellCoord, buffStrength.range);
        foreach (Vector3Int cell in buffCells)
        {
            if (CurrentRoom.TryGetEnemyAtCell(cell, out EnemyScript enemy))
            {
                enemy.Buffs.Strength.IncrementValue(buffStrength.value);
            }
        }
        yield return AnimObject.PlayAndWaitForAnim(EyePriestAnims.BuffEnd.ToString());
    }
    private bool MoveToHealCondition()
    {
        return AreEnemiesHurt(out EnemyScript closest) && !Globals.InCellRadius(CellCoord, closest.CellCoord, heal.range);
    }
    private IEnumerator MoveToHealEffect()
    {
        AreEnemiesHurt(out EnemyScript closest);
        yield return Move(GenPath(closest.CellCoord, false));
    }
    private bool HealCondition()
    {
        return AreEnemiesHurt(out EnemyScript closest) && Globals.InCellRadius(CellCoord, closest.CellCoord, heal.range);
    }
    private IEnumerator HealEffect()
    {
        yield return Heal();
    }
    private IEnumerator Heal()
    {
        yield return AnimObject.PlayAndWaitForAnim(EyePriestAnims.Heal.ToString());
        List<Vector3Int> healCells = Globals.GetCellsInRadius(CellCoord, heal.range, true);
        foreach (Vector3Int cell in healCells)
        {
            if (CurrentRoom.TryGetEnemyAtCell(cell, out EnemyScript enemy))
            {
                StartCoroutine(enemy.HealHp(heal.value));
            }
        }
        yield return AnimObject.PlayAndWaitForAnim(EyePriestAnims.HealEnd.ToString());
    }
    private bool AreEnemiesHurt(out EnemyScript closestHurtEnemy)
    {
        List<CombatScript> enemies = CurrentRoom.GetAllEnemies();
        int dist = int.MaxValue;
        EnemyScript e = null;
        foreach (EnemyScript enemy in enemies)
        {
            Debug.Log(enemy);
            if (enemy.Hp <= enemy.MaxHp * 0.5f)
            {
                if (Globals.PerpDist(CellCoord, enemy.CellCoord) < dist)
                {
                    dist = Globals.PerpDist(CellCoord, enemy.CellCoord);
                    e = enemy;
                }
            }
        }
        Debug.Log(e);
        closestHurtEnemy = e;
        return e != null;
    }
}

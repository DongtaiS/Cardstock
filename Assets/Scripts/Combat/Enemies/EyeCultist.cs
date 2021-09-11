using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class EyeCultist : EnemyScript
{
    [SerializeField] private int damage;
    [SerializeField] private int baseAttackRange;
    private EnemyAction attack;
    private enum EyeCultistAnims { Walk, Idle, Iris };
    private protected override void SetupActions()
    {
        base.SetupActions();
        attack = new EnemyAction(0, 0, baseAttackRange, damage, AttackCondition, AttackEffect);
        actions.Insert(0, attack);
    }
    private protected override void AnimateMove(int dir)
    {
        base.AnimateMove(dir);
        AnimObject.animator.Play(EyeCultistAnims.Walk.ToString(), 0);
        AnimObject.animator.Play(EyeCultistAnims.Iris.ToString(), 1, AnimObject.animator.GetCurrentAnimatorStateInfo(1).normalizedTime);
    }
    private protected override void ResetAnimation(int dir)
    {
        AnimObject.animator.Play(EyeCultistAnims.Idle.ToString(), 0);
    }
    private protected override void GenFinalPath()
    {
        savedPath = GenPath(playerCombat.CellCoord, false);
    }
    private bool AttackCondition()
    {
        return CanAttack(playerCombat.CellCoord, baseAttackRange, true, true, false);
    }
    private protected IEnumerator AttackEffect()
    {
        yield return TurnToPlayer();
        AnimationScript sword = Globals.PrefabManager.SpawnAnimationObject(AnimationObjectType.Sword, transform.position, DirectionToPlayer(), transform);
        Coroutine swing = StartCoroutine(sword.PlayAndWaitForAnim(WeaponAnimations.Sword.SwordSwing.ToString()));
        yield return DamagePlayer(damage, true);
        yield return swing;
        Destroy(sword.gameObject);
    }
}

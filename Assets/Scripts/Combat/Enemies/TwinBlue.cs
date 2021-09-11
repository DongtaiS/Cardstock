using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D.Animation;

public class TwinBlue : EnemyScript
{
    [System.NonSerialized] public TwinRed twinRed;
    [SerializeField] private AnimationCurve dashCurve;
    private Vector3Int teleportCell;
    private int attackRange = 2;
    private int attackDamage = 10;
    private int chargeRange = 5;
    private int chargeDamage = 8;
    private int chargeDamageReduced = 5;
    private float dashCellDuration = 0.2f;
    private EnemyAction attack;
    private EnemyAction charge;
    private EnemyAction teleport;
    private enum TwinBlueAnims { Dash, DashMid, DashEnd, Teleport, TeleportMid, TeleportEnd, CrossSwords, CrossSwordsEnd, Walk, OnHit, Idle, EyesOnHit, EyesIdle }
    private protected override void SetupActions()
    {
        base.SetupActions();
        attack = new EnemyAction(0, 0, attackRange, attackDamage, AttackCondition, AttackEffect);
        charge = new EnemyAction(0, 0, chargeRange, chargeDamage, ChargeCondition, ChargeEffect);
        teleport = new EnemyAction(3, 0, 0, 0, TeleportCondition, TeleportEffect);
        actions.InsertRange(0, new List<EnemyAction> { attack, charge, teleport });
    }
    private protected override void AnimateMove(int dir)
    {
        base.AnimateMove(dir);
        AnimObject.animator.Play(TwinBlueAnims.Walk.ToString());
    }
    private protected override void ResetAnimation(int dir)
    {
        base.ResetAnimation(dir);
        AnimObject.animator.Play(TwinBlueAnims.Idle.ToString());
    }
    private protected override void GenFinalPath()
    {
        savedPath = GenPath(playerCombat.CellCoord, false);
    }
    private protected bool TeleportCondition()
    {
        GenFinalPath();
        if (savedPath.Count <= moveRange)
        {
            return false;
        }
        List<Vector3Int> coordsToCheck = new List<Vector3Int>();
        coordsToCheck.Add(playerCombat.CellCoord - (Vector3Int)Globals.IntDirectionToVector2(playerCombat.FacingDirection));
        coordsToCheck.Add(playerCombat.CellCoord + (Vector3Int)Globals.IntDirectionToVector2(1 - playerCombat.FacingDirection % 2));
        coordsToCheck.Add(playerCombat.CellCoord + (Vector3Int)Globals.IntDirectionToVector2(1 - playerCombat.FacingDirection % 2 + 2));
        foreach (Vector3Int coord in coordsToCheck)
        {
            if (CurrentRoom.HasEmptyTile(coord))
            {
                teleportCell = coord;
                return true;
            }
        }
        return false;
    }
    private IEnumerator TeleportEffect()
    {
        yield return AnimObject.PlayAndWaitForAnim(TwinBlueAnims.Teleport.ToString());
        yield return Globals.WaitForSeconds(0.25f);
        Premove();
        transform.position = CurrentRoom.GetFloor().GetCellCenterWorld(teleportCell);
        Postmove();
        SetDirection(DirectionToPlayer());
        yield return Globals.WaitForSeconds(0.25f);
        yield return AnimObject.PlayAndWaitForAnim(TwinBlueAnims.TeleportEnd.ToString());
    }
    private bool AttackCondition()
    {
        return CanAttack(playerCombat.CellCoord, attackRange, true, true, false);
    }
    private IEnumerator AttackEffect()
    {
        yield return TurnToPlayer();
        yield return AnimObject.PlayAndWaitForAnim(TwinBlueAnims.CrossSwords.ToString());
        Coroutine end = StartCoroutine(AnimObject.PlayAndWaitForAnim(TwinBlueAnims.CrossSwordsEnd.ToString()));
        yield return DamagePlayer(attackDamage, true);
        yield return end;
    }
    private IEnumerator Dash(Vector3Int target)
    {
        yield return CheckRotate(Globals.Vector3ToDir(CellCoord, target), 0.1f);
        Premove();
        AnimObject.animator.Play(TwinBlueAnims.Dash.ToString());
        yield return Globals.InterpVector3(transform.localPosition, CurrentRoom.GetFloor().GetCellCenterLocal(target), dashCellDuration * Vector3Int.Distance(CellCoord, target), dashCurve, pos => transform.localPosition = pos);
        AnimObject.animator.Play(TwinBlueAnims.DashEnd.ToString());
        Postmove();
    }
    private bool ChargeCondition()
    {
        return CheckPlayerPerp() && CanAttack(playerCombat.CellCoord, chargeRange, true, true, false);
    }
    private IEnumerator ChargeEffect()
    {
        if (IsPathClear(CellCoord, playerCombat.CellCoord, false))
        {
            Vector3Int tempAdd = (Vector3Int)Globals.IntDirectionToVector2(Globals.Vector3ToDir(playerCombat.CellCoord, CellCoord));
            if (CurrentRoom.HasEmptyTile(playerCombat.CellCoord - tempAdd))
            {
                Coroutine dash = StartCoroutine(Dash(playerCombat.CellCoord - tempAdd));
                yield return new WaitUntil(() => Vector3.Distance(transform.position, playerCombat.transform.position) < 1f);
                yield return DamagePlayer(chargeDamage, true, DamageSFXType.Blade);
                yield return dash;
                yield return Globals.WaitForSeconds(0.5f);
            }
            else
            {
                yield return Dash(playerCombat.CellCoord + tempAdd);
                yield return DamagePlayer(chargeDamageReduced, true, DamageSFXType.Blade);
                yield return Globals.WaitForSeconds(0.5f);
            }
        }
    }
    private protected override IEnumerator OnHitAnim(int dir)
    {
        StartCoroutine(AnimObject.PlayAndWaitForAnim(TwinBlueAnims.OnHit.ToString()));
        StartCoroutine(AnimObject.PlayAndWaitForAnim(TwinBlueAnims.EyesOnHit.ToString()));
        yield return base.OnHitAnim(dir);
    }
    //TODO taunt?
}

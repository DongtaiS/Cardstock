using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class EnemyAction
{
    public delegate bool ConditionDelegate();
    public delegate IEnumerator EffectDelegate();
    public int baseCooldown;
    public int currentCooldown;
    public float range;
    public int value;
    private ConditionDelegate condition;
    private EffectDelegate effect;
    public EnemyAction(int baseCD, int startCD, float r, int val, ConditionDelegate cond, EffectDelegate eff)
    {
        baseCooldown = baseCD;
        currentCooldown = startCD;
        range = r;
        value = val;
        condition += cond;
        effect += eff;
    }
    public void DecrementCD()
    {
        if (currentCooldown > 0)
        {
            currentCooldown--;
        }
    }
    public bool CheckCondition()
    {
        return CheckCooldown() && condition != null && condition();
    }
    public IEnumerator Effect()
    {
        yield return effect?.Invoke();
        PutOnCooldown();
    }
    private bool CheckCooldown()
    {
        return currentCooldown == 0;
    }
    private void PutOnCooldown()
    {
        currentCooldown = baseCooldown;
    }
}
public abstract class EnemyScript : CombatScript
{
    private protected PlayerCombatScript playerCombat;
    private protected List<Vector3Int> savedPath;
    private protected Pathfinding pathfinding;

    [SerializeField] private protected int MaxActions;
    [SerializeField] private protected int moveRange;
    private protected int actionsUsed;
    private protected float moveDuration;
    private protected EnemyAction moveToPlayer;
    private protected List<EnemyAction> actions;
    public override void Setup(RoomScript currentRoom, AssetReference assetRef)
    {
        base.Setup(currentRoom, assetRef);
        playerCombat = Globals.PlayerCombat;
        moveDuration = 1f;
        pathfinding = CurrentRoom.pathfinding;
        UpdateCellCoord();
        SetupActions();
    }
    private protected virtual void SetupActions()
    {
        moveToPlayer = new EnemyAction(0, 0, moveRange, 0, MoveCondition, MoveToPlayerEffect);
        actions = new List<EnemyAction> { moveToPlayer };
    }
    private protected override IEnumerator Act()
    {
        for (; actionsUsed < MaxActions; actionsUsed++)
        {
            foreach (EnemyAction action in actions)
            {
                if (action.CheckCondition())
                {
                    yield return action.Effect();
                    break;
                }
            }
            yield return Globals.WaitForSeconds(0.5f);
        }
    }
    public virtual Vector3Int GenSpawnCoord(RoomScript room, int playerDir) //Convert to helper class and create multiple types of coord generation
    {
        return RoomManagerScript.GenSpawnCoord(this, RoomManagerScript.SpawnCoordRemovePlayerSide(CurrentRoom));
    }
    public virtual bool CheckCellToSpawn(Vector3Int cell)
    {
        return CurrentRoom.HasEmptyTile(cell);
    }
    private protected override IEnumerator StartOfTurn()
    {
        yield return base.StartOfTurn();
        actionsUsed = 0;
        foreach(EnemyAction action in actions)
        {
            action.DecrementCD();
        }
        yield return Globals.CameraManager.WaitForCameraBlend();
    }
    private protected override void Premove()
    {
        base.Premove();
    }
    private protected override void Postmove()
    {
        base.Postmove();
    }
    private protected bool MoveCondition()
    {
        return Buffs.CanMoveTo(playerCombat.CellCoord);
    }
    private protected IEnumerator MoveToPlayerEffect()
    {
        GenFinalPath();
        yield return Move(savedPath);
    }
    private protected virtual IEnumerator Move(List<Vector3Int> path)
    {
        if (path != null && path.Count > 0)
        {
            Premove();
            path.RemoveAt(0);
            int direction = Globals.Vector3ToDir(CellCoord, path[0]);
            yield return CheckRotate(direction);
            AnimateMove(direction);
            for (int i = 0; i < Mathf.Min(moveRange + Buffs.Quick.GetValue(), path.Count); i++)
            {
                Vector3 startPos = transform.localPosition;
                if (i != 0 && direction != Globals.Vector3ToDir(path[i - 1], path[i]))
                {
                    break;
                }
                yield return Globals.InterpVector3(startPos, CurrentRoom.GetFloor().GetCellCenterLocal(path[i]), moveDuration, newPos => transform.localPosition = newPos);
            }
            ResetAnimation(direction);
            Postmove();
        }
        else
        {
            Debug.Log("enemy path error");
        }
    }
    private protected virtual void AnimateMove(int dir)
    {
    }
    private protected virtual void ResetAnimation(int dir)
    {

    }
    private protected IEnumerator DamagePlayer(int damage, bool isMelee, DamageSFXType dmgType = DamageSFXType.Blade)
    {
        yield return playerCombat.LoseHp(CalculateDamage(damage, isMelee), dmgType, DirectionToPlayer());
    }
    private int CalculateDamage(int baseDamage, bool melee)
    {
        float multiplier = playerCombat.Buffs.Targeted.IsActive ? 1.25f : 1;
        if (melee)
        {
            return Globals.RoundToNearestInt(baseDamage + Buffs.Strength.GetValue() * multiplier);
        }
        else
        {
            return Globals.RoundToNearestInt(baseDamage + Buffs.SharpSight.GetValue() * multiplier);
        }
    }
    private protected override IEnumerator DieAnim()
    {
        CurrentRoom.RemoveAtCell(CellCoord);
        Globals.TurnManager.RemoveFromCombat(this);
        float alpha = 1f;
        StartCoroutine(Globals.InterpFloat(1, 0, 0.5f, a => alpha = a));
        while (alpha > 0)
        {
            SetAlpha(alpha);
            healthbar.SetAlphaAll(alpha);
            yield return Globals.FixedUpdate;
        }
        StartCoroutine(base.DieAnim());
        Destroy(gameObject);
    }
/*    private protected int GetBuffedValue(int baseVal, BuffType type)
    {
        if (Buffs.PersistentBuffs.ContainsKey(type))
        {
            return baseVal + Buffs.PersistentBuffs[type].Value;
        }
        Debug.Log("error calculatebuffedvalue");
        return baseVal;
    }*/
    private protected bool CanAttack(Vector3Int targetCell, int range, bool perp, bool melee, bool piercing)    //Checks for buffs, accounts for range buffs, checks for perpendicularity
    {                                                                                                           //and checks for path
        if ((melee && Buffs.Disarmed.IsActive) || (!melee && Buffs.Intimidated.IsActive))
        {
            return false; 
        }
        else
        {
            if (melee)
            {
                range += Buffs.Reach.Value;
            }
            else
            {
                range += Buffs.Range.Value;
            }
            if (perp && !Globals.CheckPerpendicular(CellCoord, targetCell))
            {
                return false;
            }
            if (Globals.PerpDist(CellCoord, targetCell) <= range)
            {
                if (piercing)
                {
                    return true;
                }
                return IsPathClear(CellCoord, targetCell, false);
            }
        }
        return false;
    }
    private protected bool IsPathClear(Vector3Int startCell, Vector3Int targetCell, bool includeStart)
    {
        Vector3Int increment = (Vector3Int)Globals.IntDirectionToVector2(Globals.Vector3ToDir(startCell, targetCell));
        int dist = (int)Vector3.Distance(startCell, targetCell);
        int i = 1;
        if (includeStart)
        {
            i = 0;
        }
        for (; i < dist; i++)
        {
            if (!CurrentRoom.HasEmptyTile(startCell + increment * i))
            {
                return false;
            }
        }
        return true;
    }
    private protected List<Vector3Int> FindShortestPathToAnyCell(List<Vector3Int> cells, bool lineOfSightToPlayer)
    {
        List<Vector3Int> result = new List<Vector3Int>();
        int lowestDist = int.MaxValue;
        foreach (Vector3Int cell in cells)
        {
            if (Globals.PerpDist(CellCoord, cell) < lowestDist && (!lineOfSightToPlayer || IsPathClear(cell, playerCombat.CellCoord, true)))
            {
                List<Vector3Int> tempList = GenPath(cell, true);
                if (tempList != null && tempList.Count < lowestDist)
                {
                    result = tempList;
                    lowestDist = tempList.Count;
                }
            }
        }
        return result;
    }
    private protected abstract void GenFinalPath();
    private protected List<Vector3Int> GenPath(Vector3Int targetCell, bool inclusiveEnd)
    {
        return GenPath(targetCell, moveRange, inclusiveEnd);
    }
    private protected List<Vector3Int> GenPath(Vector3Int targetCell, int range, bool inclusiveEnd)
    {
        return pathfinding.GeneratePath(CellCoord, targetCell, range + Buffs.Quick.GetValue(), inclusiveEnd);
    }
    private protected bool CheckPlayerPerp()
    {
        return Globals.CheckPerpendicular(CellCoord, playerCombat.CellCoord);
    }
    private protected int DirectionToPlayer()
    {
        return Globals.Vector3ToDir(CellCoord, playerCombat.CellCoord);
    }
    private protected IEnumerator TurnToPlayer(float waitTime = 0.1f)
    {
        yield return CheckRotate(DirectionToPlayer(), waitTime);
    }
    public override void EndTurn()
    {
        //implement this
    }
}

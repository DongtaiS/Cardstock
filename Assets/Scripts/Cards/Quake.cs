using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Quake : MeleeAttackCard
{
    private int baseKBDist = 1;
    private List<int[]> kbData = new List<int[]>();
    private float delay = 0.2f;
    public override bool Check(Vector3Int targetCell)
    {
        if (base.Check(targetCell) && Globals.InCellRadius(playerCombat.CellCoord, targetCell, cardData.Range))
        {
            if (CheckHits(Globals.GetCellsInRadius(playerCombat.CellCoord, cardData.Range)))
            {
                Dictionary<Vector3Int, GameObject> mapCopy = new Dictionary<Vector3Int, GameObject>(playerCombat.CurrentRoom.map);
                foreach (CardAttackData data in hitEnemies)
                {
                    int dir = Globals.RadialKnockbackDir(playerCombat.CellCoord, data.combatable.CellCoord, data.combatable.FacingDirection);
                    int dist = 0;
                    for (int i = 1; i <= baseKBDist; i++)
                    {
                        Vector3Int tempCoord = data.combatable.CellCoord + i * (Vector3Int)Globals.IntDirectionToVector2(dir);
                        if (playerCombat.CurrentRoom.GetFloor().HasTile(tempCoord) && (!mapCopy.ContainsKey(tempCoord) || mapCopy[tempCoord] == null))
                        {
                            dist = i;
                        }
                        else
                        {
                            break;
                        }
                    }
                    mapCopy[data.combatable.CellCoord] = null;
                    mapCopy[data.combatable.CellCoord + dist * (Vector3Int)Globals.IntDirectionToVector2(dir)] = data.combatable.gameObject;
                    kbData.Add(new int[] { dir, dist });
                }
                return true;
            }
        }
        return false;
    }
    private protected override void CreatePreviews(Vector3Int targetCell)
    {
        if (previews.Count == 0)
        {
            for (int i = 0; i < hitEnemies.Count; i++)
            {
                AnimationScript temp = SpawnPreview(hitEnemies[i].combatable, hitEnemies[i].combatable.transform.position, hitEnemies[i].combatable.FacingDirection, 0.25f);
                Vector3 target = playerCombat.CurrentRoom.GetFloor().GetCellCenterLocal(hitEnemies[i].combatable.CellCoord + kbData[i][1] * (Vector3Int)Globals.IntDirectionToVector2(kbData[i][0]));
                temp.StartCoroutine(Globals.InterpVector3(temp.transform.localPosition, target, 0.25f, pos => temp.transform.localPosition = pos));
            }
        }
    }
    public override void OnMouseOverHighlight(Vector3Int coord) //TODO Fix this (make it more visible)
    {
        base.OnMouseOverHighlight(coord);
/*        CreateHighlights(0, true);*/
        foreach (CardAttackData atk in hitEnemies)
        {
            SpawnHighlight(atk.combatable.CellCoord, staticCardInfo.Attackhighlight);
        }
    }
    public override void UpdateDescription(bool successfulCheck)
    {
        base.UpdateDescription(successfulCheck);
        if (successfulCheck)
        {
            CardGameObjects.description.text = "Deal " + Keywords.DamageNum(cardData.HitEnemies[0].damage, baseDamage, attackType) + " " + Keywords.damage + " to enemies in a 1.5 unit radius. \nKnock back enemies up to 1 unit.";
        }
        else
        {
            CardGameObjects.description.text = "Deal " + Keywords.DamageNum(damage, baseDamage, attackType) + " " + Keywords.damage + " to enemies in a 1.5 unit radius. \nKnock back enemies up to 1 unit.";
        }
    }
    private protected override IEnumerator PlayEffect()
    {
        List<Coroutine> coroutines = new List<Coroutine>();
        for (int i = 0; i < hitEnemies.Count; i++)
        {
            coroutines.Add(playerCombat.StartCoroutine(DelayedKnockback(hitEnemies[i], kbData[i][0], kbData[i][1])));
        }
        List<Vector3Int> cells = Globals.GetCellsInRadius(playerCombat.CellCoord, cardData.Range);
        int currentOrder = 1;
        for (int i = cells.Count-1; i >= 0; i--)
        {
            Vector3Int cellDist = cells[i] - playerCombat.CellCoord;
            int order = Mathf.Abs(cellDist.x) * 2 - 1 + Mathf.Abs(cellDist.y) * 2 - 1;
            if (order > currentOrder)
            {
                currentOrder = order;
                yield return Globals.WaitForSeconds(delay);
            }
            if (playerCombat.CurrentRoom.GetFloor().HasTile(cells[i])) // && playerCombat.CurrentRoom.map.TryGetValue(cells[i], out GameObject val) && (val == null || val.TryGetComponent(out CombatScript _))
            {
                previews.Add(Globals.PrefabManager.SpawnAnimationObject(AnimationObjectType.QuakeRock1, playerCombat.CurrentRoom.GetFloor().GetCellCenterWorld(cells[i]), 1, playerCombat.CurrentRoom.transform));
            }
        }
        for (int i = 0; i < coroutines.Count; i++)
        {
            yield return coroutines[i];
        }
        DestroyPreviews(true);
        yield return base.PlayEffect();
    }
    private IEnumerator DelayedKnockback(CardAttackData atkData, int dir, int dist)
    {
        int order = 0;
        Vector3Int cellDist = atkData.combatable.CellCoord - playerCombat.CellCoord;
        order += Mathf.Abs(cellDist.x) * 2 - 1;
        order += Mathf.Abs(cellDist.y) * 2 - 1;
        yield return Globals.WaitForSeconds(delay * order);
        Debug.DrawLine(Vector3.zero, playerCombat.CurrentRoom.GetFloor().GetCellCenterWorld(atkData.combatable.CellCoord), Color.blue, 20f);
        Coroutine knockback = playerCombat.StartCoroutine(atkData.combatable.Knockback(dir, dist));
        yield return atkData.combatable.LoseHp(atkData.damage, DamageSFXType.Blunt);
        yield return knockback;
    }
    private protected override void ClearLists()
    {
        base.ClearLists();
        kbData.Clear();
    }
    private protected override void CreateHighlights()
    {
        SpawnHighlightsCircle(playerCombat.CellCoord, cardData.Range, staticCardInfo.Attackhighlight);
    }
}
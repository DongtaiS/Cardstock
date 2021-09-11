using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Backtrack : MoveCard
{
    public override bool Check(Vector3Int targetCell)
    {
        for (int i = 0; i < playerCombat.CellsMovedToSinceTurnStart.Count-1; i++)
        {
            cardData.MoveDist += Globals.PerpDist(playerCombat.CellsMovedToSinceTurnStart[i], playerCombat.CellsMovedToSinceTurnStart[i + 1]);
        }
        return playerCombat.CellCoord != playerCombat.CellAtTurnStart && playerCombat.CellsMovedToSinceTurnStart.Count > 1;
    }
    private protected override void CreatePreviews(Vector3Int targetCell)
    {
        base.CreatePreviews(targetCell);
        if (previews.Count == 0)
        {
            SpawnPreview(playerCombat, playerCombat.CurrentRoom.GetFloor().GetCellCenterWorld(playerCombat.CellAtTurnStart), Globals.Vector3ToDir(playerCombat.CellAtTurnStart, playerCombat.CellsMovedToSinceTurnStart[1]), 0.5f);
        }
    }
    private protected override IEnumerator PlayEffect()
    {
        if (playerCombat.CellCoord != playerCombat.CellAtTurnStart && playerCombat.CellsMovedToSinceTurnStart.Count > 1)
        {
            playerCombat.SetAlpha(0.5f);
            for (int i = playerCombat.CellsMovedToSinceTurnStart.Count - 2; i >= 0; i--)
            {
                yield return playerCombat.CheckRotate(Globals.Vector3ToDir(playerCombat.CellsMovedToSinceTurnStart[i], playerCombat.CellCoord), 0f, 0.1f);
                yield return playerCombat.Move(playerCombat.CellsMovedToSinceTurnStart[i], Globals.AnimationCurves.IncLinear, PlayerCombatScript.WalkDuration/2f, false);
            }
            playerCombat.SetAlpha(1f);
        }
        yield return base.PlayEffect();
    }
    private protected override void CreateHighlights()
    {
        if (playerCombat.CellCoord != playerCombat.CellAtTurnStart)
        {
            for (int i = playerCombat.CellsMovedToSinceTurnStart.Count - 1; i >= 0; i--)
            {
                SpawnHighlight(playerCombat.CellsMovedToSinceTurnStart[i]);
            }
        }
    }
    public override void OnMouseOverHighlight(Vector3Int coord)
    {
    }
}

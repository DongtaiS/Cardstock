using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DashCard : MoveCard
{
    private Vector3Int savedCell;
    public override bool Check(Vector3Int targetCell)
    {
        if (CheckDirection(targetCell) && CheckPerpRange(targetCell, cardData.Range))
        {
            cardData.MoveDist = (int)Vector3Int.Distance(playerCombat.CellCoord, targetCell);
            savedCell = targetCell;
            return IsPathClear(targetCell, true);
        }
        return false;
    }
    private protected override void CreatePreviews(Vector3Int targetCell)
    {
        base.CreatePreviews(targetCell);
        DestroyPreviews(true);
        SpawnPreview(playerCombat, playerCombat.CurrentRoom.GetFloor().GetCellCenterWorld(targetCell), Globals.Vector3ToDir(playerCombat.CellCoord, targetCell), 0.5f);
    }
    private protected override IEnumerator PlayEffect()
    {
        camScript.ActivatePlayerCam();
        DestroyPreviews(true);
        yield return playerCombat.CheckRotate(cardData.Direction, 0);
        yield return playerCombat.Walk(savedCell);
        yield return base.PlayEffect();
    }
    private protected override void CreateHighlights()
    {
        SpawnHighlightsPerp(cardData.Range, false, true);
    }
    public override void UpdateDescription(bool successfulCheck)
    {
        base.UpdateDescription(successfulCheck);
        if (successfulCheck)
        {
            CardGameObjects.description.text = Keywords.Move + " up to " + cardData.Range + " tiles in any direction.";
        }
        else
        {
            CardGameObjects.description.text = Keywords.Move + " up to " + cardData.Range + " tiles in any direction.";
        }
    }
    public override void OnMouseOverHighlight(Vector3Int coord)
    {
        base.OnMouseOverHighlight(coord);
        SpawnHighlightsLine(playerCombat.CellCoord, 1, Vector3Int.Distance(playerCombat.CellCoord, coord), Globals.Vector3ToDir(playerCombat.CellCoord, coord), false, true, staticCardInfo.MovementHighlight);
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TacticalRetreat : MoveCard
{
    Vector3Int savedCell;
    [SerializeField] private int drawAmount;
    public override bool Check(Vector3Int targetCell)
    {
        bool IsPlayable = false;
        savedCell = playerCombat.CellCoord;
        for (int i = 1; i <= cardData.Range; i++)
        {
            Vector3Int cell = playerCombat.CellCoord - i * (Vector3Int)Globals.IntDirectionToVector2(playerCombat.FacingDirection);
            if (IsPathClear(cell, true) && playerBuffs.CanMoveTo(cell, false))
            {
                savedCell = cell;
                IsPlayable = true;
            }
        }
        return IsPlayable;
    }
    private protected override IEnumerator PlayEffect()
    {
        Globals.CameraManager.ActivatePlayerCam();
        yield return playerCombat.DashWithoutTurn(savedCell);
        yield return base.PlayEffect();
        yield return deck.DrawCard(drawAmount);
    }
    private protected override void CreateHighlights()
    {
        Check(Vector3Int.zero);
        SpawnHighlight(savedCell);
    }
    public override void OnMouseOverHighlight(Vector3Int coord)
    {
    }
}

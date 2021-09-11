using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class HighlightCard : CardScript        //Extension of CardScript that includes range and highlight spawning
{
    [SerializeField] private protected float baseRange;
    private protected Color defaultHighlightColor;
    private protected List<HighlightScript> highlightList = new List<HighlightScript>();    //List of highlights (so that you can remove them)
    private protected bool IsMouseOverHighlight;                                            //True if mouse is over a highlight (used for second layer of highlights)
    private protected List<AnimationScript> previews = new List<AnimationScript>();         //List of AnimationScripts that are spawned as visual previews of the card
    private Vector3Int lastHoveredCell;                                                     //Previous cell the mouse hovered over              
    private CombatScript lastHoveredCombatable;                                             //Previous unit the mouse hovered over
    private protected bool CheckDirection(Vector3Int targetCell)    //Returns true if the target cell is not the player's cell and the player can play a card 
    {                                                               //in that direction, also updates the direction in the cardData
        int dir = Globals.Vector3ToDir(playerCombat.CellCoord, targetCell);
        if (dir != -1 && playerBuffs.CheckDir(dir))
        {
            cardData.Direction = dir;
            return true;
        }
        return false;
    }
    private protected override IEnumerator PlayEffect()
    {
        DestroyPreviews(true);
        return base.PlayEffect();
    }
    public override bool HoverCheck(Vector3Int targetCell)          //Overridden method that shows/hides units in front of the unit at target cell
    {                                                               //Also calls CreatePreviews and OnHighlightHover, or DestroyPreviews and OnHighlightExit
        bool check = base.HoverCheck(targetCell);                   //and updates the description
        if (playerCombat.CurrentRoom.TryGetEnemyAtCell(targetCell, out EnemyScript combatable))
        {
            if (combatable != lastHoveredCombatable)
            {
                combatable.HideUnitsInFront();
                if (lastHoveredCombatable != null)
                {
                    lastHoveredCombatable.ShowUnitsInFront();
                }
                lastHoveredCombatable = combatable;
            }
        }
        else
        {
            if (lastHoveredCombatable != null)
            {
                lastHoveredCombatable.ShowUnitsInFront();
                lastHoveredCombatable = null;
            }
        }
        if (check)
        {
            CreatePreviews(targetCell);
            OnMouseOverHighlight(targetCell);
        }
        else
        {
            DestroyPreviews(true);
            OnHighlightExit(targetCell);
        }
        lastHoveredCell = targetCell;
        UpdateDescription(check);
        return check;
    }
    public override void OnPointerExit()    //Override, removes highlights when mouse leaves card
    {
        base.OnPointerExit();
        if (!isSelected)
        {
            RemoveHighlights();
        }
    }
    public override void OnPointerEnter()   //Override, creates highlights when mouse enters card
    {
        base.OnPointerEnter();
        if (!isSelected && playerCombat.CanSelectCard)
        {
            CreateHighlights();
        }
    }
    private protected virtual void CreatePreviews(Vector3Int targetCell)    //Overridable method that creates visual previews for the card's effect
    {

    }
    private protected AnimationScript SpawnPreview(CombatScript original, Vector3 position, int direction, float initAlpha) //Spawns a new preview of a unit using its sprite
    {
        AnimationScript preview = Globals.PrefabManager.CreatePreviewObj(original);
        preview.transform.position = position;
        preview.SetDirection(direction);
        preview.StartCoroutine(Globals.InterpFloat(0, initAlpha, 0.25f, a => preview.SetAlpha(a)));
        previews.Add(preview);
        return preview;
    }
    private protected AnimationScript SpawnPreview(AnimationScript original, Vector3 position, int direction, float initAlpha)  //Spawns a new AnimationScript (used for weapons/nonunits)
    {
        AnimationScript preview = Instantiate(original, playerCombat.CurrentRoom.transform);
        preview.transform.position = position;
        preview.SetDirection(direction);
        preview.StartCoroutine(Globals.InterpFloat(0, initAlpha, 0.25f, a => preview.SetAlpha(a)));
        previews.Add(preview);
        return preview;
    }
    private protected virtual void DestroyPreviews(bool fade)   //Destroys all previews
    {
        while (previews.Count > 0)
        {
            AnimationScript temp = previews[0];
            temp.StopAllCoroutines();
            if (fade && temp.isActiveAndEnabled)
            {
                temp.StartCoroutine(Globals.YieldThenDestroy(Globals.InterpFloat(temp.currentAlpha, 0, 0.1f, a => temp.SetAlpha(a)), temp.gameObject));
            }
            else
            {
                Destroy(temp.gameObject);
            }
            previews.RemoveAt(0);
        }
        previews.Clear();
    }
    private protected IEnumerator FadeThenDestroyAnimObject(BasicSpriteObject animObj)
    {
        yield return Globals.YieldThenDestroy(Globals.InterpFloat(animObj.currentAlpha, 0, 0.1f, a => animObj.SetAlpha(a)), animObj.gameObject);
    }
    private protected override void Deselect() //Override, removes highlights on deselect
    {
        if (!mouseOver)
        {
            RemoveHighlights();
        }
        base.Deselect();
        if (lastHoveredCombatable != null)
        {
            lastHoveredCombatable.ShowUnitsInFront();
            lastHoveredCombatable = null;
        }
        DestroyPreviews(true);
    }
    private protected override IEnumerator PlayCard()   //override removes highlights when card is played
    {
        RemoveHighlights();
        yield return base.PlayCard();
    }
    private protected override void OnPlayComplete()
    {
        if (lastHoveredCombatable != null)
        {
            lastHoveredCombatable.ShowUnitsInFront();
            lastHoveredCombatable = null;
        }
        base.OnPlayComplete();
    }
    private protected void RemoveHighlights()       //Removes all highlights
    {
        foreach (HighlightScript highlight in highlightList)
        {
            highlight.StartCoroutine(highlight.FadeOut(0.1f));
        }
        highlightList.Clear();
    }
    public virtual void OnMouseOverHighlight(Vector3Int coord)  //Called when mouse is over a highlight, overriden for secondary highlight effects (preview of card effect)
    {
        if (lastHoveredCell != coord)
        {
            IsMouseOverHighlight = false;
            RemoveHighlights();
        }
        IsMouseOverHighlight = true;
    }
    public virtual void OnHighlightExit(Vector3Int coord) //Called when mouse is no longer over a highlight, removes the extra highlights and calls the default createhighlights
    {
        if (IsMouseOverHighlight)
        {
            IsMouseOverHighlight = false;
            RemoveHighlights();
            CreateHighlights();
        }
    }
    private protected abstract void CreateHighlights();  //Abstract, creates the first set of highlights that show where the card can be played, is called on hover
    private protected void SpawnHighlightsCircle(Vector3Int baseCell, float radius, Color color) //Spawns highlights in a circle around the baseCell
    {
        foreach (Vector3Int coord in Globals.GetCellsInRadius(baseCell, radius))
        {
            CheckCell(coord, true, false, color);
        }
    }
    private protected void SpawnHighlightsPerp(int offset, bool[] dirs, float r, bool includeEnemies, bool stopOnNonEmpty, Color color) //Spawns highlights in all 4 directions
    {
        for (int dir = 0; dir < 4; dir++)
        {
            if (dirs[dir])
            {
                SpawnHighlightsLine(playerCombat.CellCoord, offset, r, dir, includeEnemies, stopOnNonEmpty, color);
            }
        }
    }
    private protected void SpawnHighlightsPerp(int offset, float r, bool includeEnemies, bool stopOnNonEmpty, Color color)
    {
        SpawnHighlightsPerp(offset, new bool[] { true, true, true, true }, r, includeEnemies, stopOnNonEmpty, color);
    }
    private protected void SpawnHighlightsPerp(int offset, float r, bool includeEnemies, bool stopOnNonEmpty)
    {
        SpawnHighlightsPerp(offset, new bool[] { true, true, true, true }, r, includeEnemies, stopOnNonEmpty, defaultHighlightColor);
    }
    private protected void SpawnHighlightsPerp(float r, bool includeEnemies, bool stopOnNonEmpty)
    {
        SpawnHighlightsPerp(1, new bool[] { true, true, true, true }, r, includeEnemies, stopOnNonEmpty, defaultHighlightColor);
    }
    private protected void SpawnHighlightsLine(Vector3Int start, int offset, float r, int dir, bool includeEnemies, bool stopOnNonEmpty, Color color) //Spawns highlights in a line
    {
        for (int dist = offset; dist <= r; dist++)
        {
            Vector3Int coord = start + (Vector3Int)(Globals.IntDirectionToVector2(dir) * dist);
            if (!CheckCell(coord, includeEnemies, stopOnNonEmpty, color))
            {
                break;
            }
        }
    }
    private protected void SpawnHighlightsLine(Vector3Int start, Vector3Int end, bool includeEnemies, bool stopOnNonEmpty)
    {
        SpawnHighlightsLine(start, end, includeEnemies, stopOnNonEmpty, defaultHighlightColor);
    }
    private protected void SpawnHighlightsLine(Vector3Int start, Vector3Int end, bool includeEnemies, bool stopOnNonEmpty, Color color)
    {
        List<Vector3Int> cells = Globals.GetCellsInLine(start, end, false, true);
        foreach (Vector3Int cell in cells)
        {
            if (!CheckCell(cell, includeEnemies, stopOnNonEmpty, color))
            {
                break;
            }
        }
    }
    private protected void SpawnHighlightsLineCentered(Vector3Int center, int offset, float r, int dir, bool includeEnemies, bool stopOnNonEmpty, Color color)
    {
        for (int dist = offset; dist <= r; dist++)
        {
            Vector3Int cellA = center + (Vector3Int)(Globals.IntDirectionToVector2(dir) * dist);
            if (!CheckCell(cellA, includeEnemies, stopOnNonEmpty, color))
            {
                break;
            }
            Vector3Int cellB = center - (Vector3Int)(Globals.IntDirectionToVector2(dir) * dist);
            if (!CheckCell(cellB, includeEnemies, stopOnNonEmpty, color))
            {
                break;
            }
        }
    }
    private protected void SpawnHighlightsLineCentered(Vector3Int center, int offset, float r, int dir, bool includeEnemies, bool stopOnNonEmpty)
    {
        SpawnHighlightsLineCentered(center, offset, r, dir, includeEnemies, stopOnNonEmpty, defaultHighlightColor);
    }
    private protected void SpawnHighlight(Vector3Int coord)
    {
        SpawnHighlight(coord, defaultHighlightColor);
    }
    private protected void SpawnHighlight(Vector3Int coord, Color color) //Spawns a highlight at the specified cell
    {
        Vector3 pos = playerCombat.CurrentRoom.GetFloor().GetCellCenterWorld(coord);
        HighlightScript temp = Globals.PrefabManager.SpawnObjectFromPool<HighlightScript>(PrefabManager.ObjectPool.Highlight, Globals.World.transform, pos);
        temp.SetColor(color);
        temp.SetCellCoord(coord);
        temp.Enable();
        highlightList.Add(temp);
    }
    private protected bool CheckCell(Vector3Int cell, bool includeEnemies, bool stopOnNonEmpty, Color color)    //Checks if there should be a highlight spawned at this cell and spawns one
    {                                                                                                           //(if there is a tile there, if the highlights should include enemies,
        // only show highlights for possible target cells                                                       //if the highlights should stop after a nonempty cell in a line)
        /* bool result = Check(cell);
        if (result)
        {
            SpawnCardHighlight(cell, color);
        }
        return result;*/
        if (playerCombat.CurrentRoom.GetFloor().HasTile(cell))
        {
            if (playerCombat.CurrentRoom.HasEmptyTile(cell) || (includeEnemies && playerCombat.CurrentRoom.TryGetEnemyAtCell(cell, out EnemyScript enemy)))
            {
                SpawnHighlight(cell, color);
                return playerCombat.CurrentRoom.HasEmptyTile(cell) || !stopOnNonEmpty;
            }
        }
        return false;
    }
    private protected bool IsPathClear(Vector3Int targetCell, bool inclusive)   //Returns true if there is a clear path from the player to the target cell
    {
        if (!Globals.CheckPerpendicular(playerCombat.CellCoord, targetCell))
        {
            return false;
        }
        List<Vector3Int> cells = Globals.GetCellsInLine(playerCombat.CellCoord, targetCell, false, inclusive);
        foreach (Vector3Int cell in cells)
        {
            if (!playerCombat.CurrentRoom.HasEmptyTile(cell))
            {
                return false;
            }
        }
        return true;
    }
    private protected bool CheckPerpRange(Vector3Int cell, float range)
    {
        return CheckPerpRange(cell, 1, (int)range);
    }
    private protected bool CheckPerpRange(Vector3Int cell, int rangeMin, int rangeMax)
    {
        return CheckPerpRange(cell, rangeMin, rangeMax, new bool[] { true, true, true, true} );
    }
    private protected bool CheckPerpRange(Vector3Int cell, int rangeMin, int rangeMax, bool[] dir)  //returns true if the player and parameter cell are perpendicular, if they are 
    {                                                                                               //within range, and if there are no directional issues
        return Globals.CheckPerpRange(playerCombat.CellCoord, cell, rangeMin, rangeMax) && dir[Globals.Vector3ToDir(playerCombat.CellCoord, cell)];
    }
    private void OnDestroy()        //Removes all highlights and destroys all previews when the card is destroyed
    {
        RemoveHighlights();
        DestroyPreviews(true);
    }
}

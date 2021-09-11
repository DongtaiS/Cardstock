using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.EventSystems;
public class RaycastManager : MonoBehaviour
{
    [SerializeField] public HighlightScript Highlight;
    [SerializeField] private Color defaultHighlightColor;
    [SerializeField] private Tilemap tilemap;
    private Vector3Int lastHoveredCell;
    private Vector3Int lastCardHoveredCell;
    private CombatScript lastSelectedCombatable;
    private Vector3 mouseDownPos;
    private bool isMouseDown;
    private bool dragging;
    private bool canUpdate;
    private bool selectedCard = false;
    private PlayerCombatScript playerCombat;
    private CameraManagerScript cameraManager;
    void Start()
    {
        Highlight = Instantiate(Highlight, Globals.World.transform);
        Highlight.SetColor(defaultHighlightColor);
        Highlight.gameObject.SetActive(false);
        Highlight.GetComponentInChildren<BoxCollider2D>().enabled = false;
        playerCombat = Globals.PlayerCombat;
        cameraManager = Globals.CameraManager;
    }
    private void Update()
    {
        Vector3 mousePos = Globals.ScreenToWorld();
        if (!Globals.IsGamePaused)
        {
            Debug.DrawLine(Vector3.zero, mousePos);
            Debug.DrawLine(mousePos + Vector3.forward * 100, mousePos - Vector3.forward * 100, Color.blue);
            Debug.DrawLine(mousePos + Vector3.right * 100, mousePos - Vector3.right * 100, Color.green);
            SelectedCardRaycast(mousePos);
            HighlightRaycast(mousePos);
            RoomRaycast(mousePos);
            CheckClick(mousePos);
        }
        if (Input.GetMouseButtonDown(0))
        {
            selectedCard = false;
            mouseDownPos = mousePos;
            isMouseDown = true;
            canUpdate = cameraManager.CanUpdate();
        }
    }
    private void CheckClick(Vector3 mousePos)
    {
        if (Input.GetMouseButtonUp(0))
        {
            if (!dragging && !selectedCard)
            {
                Collider2D collider = Globals.Vector3OverlapPoint(mouseDownPos, Globals.CombatableMask);
                BaseUnit unit = collider != null ? collider.GetComponentInParent<BaseUnit>() : null;
                if (unit is CombatScript)
                {
                    CombatScript combatable = (CombatScript)unit;
                    if (combatable != lastSelectedCombatable)
                    {
                        if (lastSelectedCombatable != null)
                        {
                            lastSelectedCombatable.Deselect();
                            lastSelectedCombatable = null;
                        }
                        if (combatable != null)
                        {
                            cameraManager.ActivateUnitCam(combatable.transform);
                            combatable.OnSelect();
                            lastSelectedCombatable = combatable;
                        }
                    }
                }
                else if (lastSelectedCombatable != null)
                {
                    lastSelectedCombatable.Deselect();
                    lastSelectedCombatable = null;
                }
                if (unit is EventUnit)
                {
                    Debug.Log(mouseDownPos);
                    EventUnit eventUnit = (EventUnit)unit;
                    eventUnit.OnMouseDown();
                }
            }
            selectedCard = false;
            canUpdate = false;
            dragging = false;
            isMouseDown = false;
        }
        else if (!dragging && isMouseDown && Vector3.Distance(mouseDownPos, mousePos) > Screen.height / 250)
        {
            dragging = true;
            cameraManager.ActivatePanCam();
        }
        if (dragging && canUpdate)
        {
            cameraManager.UpdatePanCamera();
        }
        if (playerCombat.SelectedCard != null)
        {
            selectedCard = true;
        }
    }
    private void SelectedCardRaycast(Vector3 mousePos)
    {
        Vector3Int hoveredCell = playerCombat.CurrentRoom.GetFloor().WorldToCell(mousePos);
        if (!playerCombat.CanSelectCard && playerCombat.SelectedCard != null && hoveredCell != lastCardHoveredCell)
        {
            playerCombat.SelectedCard.HoverCheck(hoveredCell);
            lastCardHoveredCell = hoveredCell;
        }
    }
    private void RoomRaycast(Vector3 mousePos)
    {
        Tilemap floor = playerCombat.CurrentRoom.GetFloor();
        Vector3Int floorCellPos = floor.WorldToCell(mousePos);
        if (floor.HasTile(floorCellPos))
        {
            if (!Highlight.isActiveAndEnabled)
            {
                Highlight.Enable();
            }
            if (floorCellPos != lastHoveredCell)
            {
                Highlight.transform.position = floor.GetCellCenterWorld(floorCellPos);
            }
            lastHoveredCell = floorCellPos;
        }
        else if (!Highlight.IsFading && Highlight.isActiveAndEnabled)
        {
            StartCoroutine(Highlight.FadeOut(0.1f));
        }
    }
    private void HighlightRaycast(Vector3 mousePos)
    {
        Collider2D highlightCollider = Globals.Vector3OverlapPoint(mousePos, Globals.HighlightMask);
        HighlightScript highlightAtMouse = highlightCollider != null ? highlightCollider.GetComponentInParent<HighlightScript>() : null;
        if (highlightAtMouse != null)
        {
            if (highlightAtMouse.CurrentColor != Highlight.CurrentColor)
            {
                Highlight.SetColor(Globals.ChangeColorAlpha(highlightAtMouse.CurrentColor, 1));
            }
        }
        else if (Highlight.isActiveAndEnabled && !Highlight.IsFading)
        {
            Highlight.SetColor(defaultHighlightColor);
        }
    }
}

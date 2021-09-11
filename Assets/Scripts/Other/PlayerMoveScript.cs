using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
public class PlayerMoveScript : MonoBehaviour
{
    public bool isAnimationPlaying;
    [System.NonSerialized] public string[] walkStates = { "WalkE", "WalkE", "WalkE", "WalkE" };
    [System.NonSerialized] public string[] idleStates = { "IdleE", "IdleE", "IdleE", "IdleE" };
    [System.NonSerialized] public string[] eyeStates = { "EyesH", "EyesH" };

    private Animator animator;
    private PlayerCombatScript playerCombat;
    private int walkDir = 0;
    private int lastDir = 0;
    private float baseDuration = 0.75f;
    private Coroutine currentAnim;
    public void Setup(PlayerCombatScript pCombat)
    {
        playerCombat = pCombat;
        animator = playerCombat.AnimObject.animator;
    }
    public void Move()
    {
        float duration;
        Vector3Int targetCell = playerCombat.CellCoord + (Vector3Int)Globals.IntDirectionToVector2(walkDir);
        if (playerCombat.CurrentRoom.TryMoveTo(targetCell))
        {
            if (isAnimationPlaying)
            {
                StopCoroutine(currentAnim);
            }
            float temp = Mathf.Abs(Vector3.Distance(playerCombat.CurrentRoom.GetFloor().GetCellCenterLocal(targetCell), transform.localPosition));
            duration = baseDuration*temp/playerCombat.CurrentRoom.GetFloor().cellSize.y;
/*            playerCombat.SetDirection(walkDir);*/
            animator.Play(walkStates[walkDir], 0, 1 - (duration / baseDuration));
            playerCombat.CurrentRoom.RemoveAtCell(playerCombat.CellCoord);
            playerCombat.SetCellCoord(targetCell);
            isAnimationPlaying = true;
            StartCoroutine(playerCombat.CheckRotate(walkDir));
            currentAnim = StartCoroutine(AnimatePosition(targetCell, duration));
        }
        else
        {
            if (playerCombat.FacingDirection != walkDir && walkDir != lastDir)
            {
                StartCoroutine(playerCombat.CheckRotate(walkDir));
            }
        }
        lastDir = walkDir;
    }
    public void SetDirection(int dir)
    {
        animator.Play(eyeStates[dir % 2], 1, animator.GetCurrentAnimatorStateInfo(1).normalizedTime);
    }
    void Update()
    {
        if (!Globals.IsGamePaused)
        {
            if (Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0)
            {
                if (Input.GetAxisRaw("Horizontal") > 0.01)
                {
                    walkDir = 1;
                }
                else if (Input.GetAxisRaw("Horizontal") < -0.01)
                {
                    walkDir = 3;
                }
                else if (Input.GetAxisRaw("Vertical") > 0.01)
                {
                    walkDir = 0;
                }
                else if (Input.GetAxisRaw("Vertical") < -0.01)
                {
                    walkDir = 2;
                }
                if (!isAnimationPlaying || (lastDir != walkDir && (walkDir + lastDir) % 2 == 0))
                {
                    Move();
                }
            }
            else
            {
            }
        }
    }

    private IEnumerator AnimatePosition(Vector3Int targetCell, float duration)
    {
        Vector3 target = playerCombat.CurrentRoom.GetFloor().GetCellCenterLocal(targetCell);
        yield return Globals.InterpVector3(transform.localPosition, target, duration, newPos => transform.localPosition = newPos);
        UpdatePlayerPosition();
        isAnimationPlaying = false;
        animator.Play(idleStates[walkDir]);
    }
    public void UpdatePlayerPosition()
    {
        playerCombat.CurrentRoom.SetAtCell(transform.position, gameObject);
        playerCombat.UpdateCellCoord();
    }
    public void OnDisable()
    {
        StopAllCoroutines();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
public class PlayerCombatScript : CombatScript
{
    public int MaxEnergy;
    public int Energy;
    [System.NonSerialized] public bool CanSelectCard = false;
    [System.NonSerialized] public CardScript SelectedCard;
    [System.NonSerialized] public Coroutine CurrentCard;
    public int DrawAmount = 1;
    public int Gold;
    public const float WalkDuration = 0.75f;
    public const float DashDuration = 0.5f;
    [System.NonSerialized] public List<Vector3Int> CellsMovedToSinceTurnStart = new List<Vector3Int>();
    public Vector3Int CellAtTurnStart { get; private set; }
    public Animator MoveAnim { get; private set; }
    public PlayerMoveScript playerMove { get; private set; }

    private DeckScript deck;
    private TraitManagerScript traitManager;

    public bool TurnOver { get; private set; }
    public override void Setup(RoomScript currentRoom, AssetReference assetRef)
    {
        base.Setup(currentRoom, assetRef);
        playerMove = GetComponent<PlayerMoveScript>();
        playerMove.Setup(this);
        SetDirection(1);
        MoveAnim = AnimObject.animator;
        deck = Globals.Deck;
        traitManager = Globals.TraitManager;
        UIManagerScript.UpdateHealth(Hp);
        CurrentRoom = currentRoom;
        CurrentRoom.SetInMap(CellCoord, gameObject);
        Globals.CameraManager.SetupPlayerCam(transform);
        UIManagerScript.EndTurnButton.GetComponent<Button>().onClick.AddListener(EndTurn);
        UpdateCellCoord();
    }
    private protected override IEnumerator StartOfTurn()
    {
        yield return base.StartOfTurn();
        CellAtTurnStart = CellCoord;
        CellsMovedToSinceTurnStart.Clear();
        CellsMovedToSinceTurnStart.Add(CellCoord);
        SetEnergy(MaxEnergy);
        if (continueTurn)
        {
            StartCoroutine(UIManagerScript.OnPlayerTurnStart());
            yield return UIManagerScript.Hand.OpenHand();
            yield return deck.DrawCard(DrawAmount);
        }
        traitManager.OnTurnStart();
        TurnOver = false;
    }
    private protected override IEnumerator Act()
    {
        yield return new WaitUntil(() => TurnOver);
    }
    private protected override IEnumerator EndOfTurn()
    {
        Coroutine hideUI = StartCoroutine(UIManagerScript.HideCombatUI());
        yield return CurrentCard;
        yield return hideUI;
        deck.DiscardHand();
        // itemManager.OnTurnEnd();
    }
    public override void EndTurn()
    {
        TurnOver = true;
    }
    public void OnCombatEnd()
    {
        CanSelectCard = false;
        Buffs.DisableAll();
    }
    public override void OnSelect()
    {
        base.OnSelect();
    }
    public override IEnumerator LoseHp(int hpLost, DamageSFXType dmgType, int dir)
    {
        yield return base.LoseHp(hpLost, dmgType, dir);
        UIManagerScript.UpdateHealth(Hp);
    }
    public void LoseEnergy(int energyLost)
    {
        SetEnergy(Energy - energyLost);
    }
    public void AddGold(int addAmount)
    {
        Gold += addAmount;
        UIManagerScript.UpdateGold(Gold);
    }
    public override void SetDirection(int direction)
    {
        base.SetDirection(direction);
        /*        MoveAnim.Play(playerMove.idleStates[FacingDirection]);*/
        playerMove.SetDirection(direction);
    }
    public IEnumerator Move(Vector3Int targetCell, AnimationCurve curve, float cellDuration = WalkDuration, bool changeDir = true)
    {
        Premove();
        if (changeDir)
        {
            int dir = Globals.Vector3ToDir(CellCoord, targetCell);
            SetDirection(dir);
        }
        MoveAnim.Play(playerMove.walkStates[FacingDirection]);
        Vector3 targetPos = CurrentRoom.GetFloor().GetCellCenterLocal(targetCell);
        float duration = cellDuration * Vector3Int.Distance(CellCoord, targetCell);
        yield return Globals.InterpVector3(transform.localPosition, targetPos, duration, curve, newPos => transform.localPosition = newPos);
        Postmove();
    }
    public IEnumerator Dash(Vector3Int targetCell)
    {
        yield return Move(targetCell, Globals.AnimationCurves.IncEaseOut, DashDuration);
    }
    public IEnumerator Walk(Vector3Int targetCell)
    {
        yield return Move(targetCell, Globals.AnimationCurves.IncLinear, WalkDuration);
    }
    public IEnumerator DashWithoutTurn(Vector3Int targetCell)
    {
        yield return Move(targetCell, Globals.AnimationCurves.IncEaseOut, DashDuration, false);
    }
    private void SetEnergy(int newEnergy)
    {
        Energy = newEnergy;
        UIManagerScript.UpdateEnergy(Energy);
    }
    private protected override void Postmove()
    {
        base.Postmove();
        MoveAnim.Play(playerMove.idleStates[FacingDirection]);
        CellsMovedToSinceTurnStart.Add(CellCoord);
    }
    private protected override IEnumerator DieAnim()
    {
        yield return base.DieAnim();
    }
}

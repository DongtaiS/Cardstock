using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lunge : AttackCard
{
    [SerializeField] private int baseAttackRange;
    public AnimationCurve curve;
    private int attackRange;
    private Vector3Int savedCell;
    private Vector3Int attackCell;
    private AnimationScript sword;
    public override CardType Type { get { return CardType.Movement; } }
    public override void Setup()
    {
        base.Setup();
        attackType = AttackType.Melee;
        defaultHighlightColor = staticCardInfo.MovementHighlight;
    }
    public override bool Check(Vector3Int targetCell)
    { 
        if (CheckDirection(targetCell) && CheckPerpRange(targetCell, cardData.Range) && IsPathClear(targetCell, true) && targetCell != playerCombat.CellCoord)
        {
            for (int i = 1; i <= attackRange; i++)
            {
                Vector3Int tempCell = targetCell + i * (Vector3Int)Globals.IntDirectionToVector2(Globals.Vector3ToDir(playerCombat.CellCoord, targetCell));
                if (base.Check(tempCell) && CheckHits(new List<Vector3Int> { tempCell }))
                {
                    attackCell = tempCell;
                    break;
                }
            }
            savedCell = targetCell;
            cardData.MoveDist = (int)Vector3Int.Distance(playerCombat.CellCoord, targetCell);
            return true;

        }
        return false;
    }
    public override void UpdateValues()
    {
        base.UpdateValues();
        cardData.Range = baseRange + playerBuffs.Quick.GetValue();
        damage = baseDamage + playerBuffs.Strength.GetValue();
        attackRange = baseAttackRange + playerBuffs.Reach.Value;
    }
    public override void UpdateDescription(bool successfulCheck)
    {
        base.UpdateDescription(successfulCheck);
        if (successfulCheck && cardData.HitEnemies.Count > 1)
        {
            CardGameObjects.description.text = Keywords.Move + " " + cardData.Range + " units, deal " + Keywords.DamageNum(cardData.HitEnemies[0].damage, baseDamage, attackType) + " " + Keywords.damage +".";
        }
        else
        {
            CardGameObjects.description.text = Keywords.Move + " " + cardData.Range + " units, deal " + Keywords.DamageNum(damage, baseDamage, attackType) + " " + Keywords.damage + ".";
        }
    }
    private protected override void CreatePreviews(Vector3Int targetCell)
    {
        base.CreatePreviews(targetCell);
        DestroyPreviews(false);
        AnimationScript playerPreview = SpawnPreview(playerCombat, playerCombat.transform.position, cardData.Direction, 0.5f);
        float duration = 0.25f * Vector3.Distance(playerCombat.CellCoord, savedCell);
        Coroutine dash = playerPreview.StartCoroutine(Globals.InterpVector3(playerPreview.transform.localPosition, playerCombat.CurrentRoom.GetFloor().GetCellCenterLocal(savedCell), duration, Globals.AnimationCurves.IncEaseOut, res => playerPreview.transform.localPosition = res));
        sword = Globals.PrefabManager.SpawnAnimationObject(AnimationObjectType.Sword, playerPreview.transform.position, 1, playerPreview.transform);
        sword.gameObject.SetActive(false);
        StartCoroutine(DelayedSwordAnim(dash));
        previews.Add(sword);
    }
    private IEnumerator DelayedSwordAnim(Coroutine waitFor)
    {
        yield return waitFor;
        if (sword != null)
        {
            sword.gameObject.SetActive(true);
            sword.animator.Play(WeaponAnimations.Sword.SwordSwingSlow.ToString());
        }
    }
    private protected override IEnumerator PlayEffect()
    {
        camScript.ActivatePlayerCam();
        previews.Remove(sword);
        DestroyPreviews(true);
        sword.transform.SetParent(playerCombat.CurrentRoom.transform);
        sword.gameObject.SetActive(false);
        yield return playerCombat.CheckRotate(cardData.Direction, 0);
        yield return playerCombat.Dash(savedCell);
        sword.gameObject.SetActive(true);
        Coroutine swing = sword.StartCoroutine(sword.PlayAndWaitForAnim(WeaponAnimations.Sword.SwordSwing.ToString()));
        yield return DamageEnemies(DamageSFXType.Blade);
        yield return swing;
        yield return FadeThenDestroyAnimObject(sword);
        yield return base.PlayEffect();
    }
    public override void OnMouseOverHighlight(Vector3Int coord)
    {
        base.OnMouseOverHighlight(coord);
        if (hitEnemies.Count > 0)
        {
            SpawnHighlight(attackCell, staticCardInfo.Attackhighlight);
        }
        SpawnHighlightsLine(playerCombat.CellCoord, coord, false, true);
    }
    private protected override void CreateHighlights()
    {
        SpawnHighlightsPerp(cardData.Range, false, true);
    }
}

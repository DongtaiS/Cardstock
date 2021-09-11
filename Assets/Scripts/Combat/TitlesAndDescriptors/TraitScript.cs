using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System;

public enum TraitTypes { moveRange, duelist, underdog, traitorous, combo, quickdraw }
[Serializable]
public abstract class TraitScript
{
    public abstract TraitTypes TraitType { get; } 
    public abstract string Name { get; }
    private protected abstract string tooltipDesc { get; }
    private protected TextScript text;
    private protected TooltipScript tooltip;
    private protected PlayerCombatScript playerCombat;
    public virtual void OnPickup(TextScript tx, TooltipScript tltip)
    {
        playerCombat = Globals.PlayerCombat;
        text = tx;
        text.SetText(Name);
        text.StartCoroutine(PickupAnim());
        tooltip = tltip.CreateTooltip(Globals.Vector3ChangeY(tltip.transform.localPosition, text.transform.localPosition.y), text.transform.parent);
        tooltip.SetTitleText(Name);
        tooltip.SetDescText(tooltipDesc);
        text.tooltip = tooltip;
    }
    public IEnumerator PickupAnim()
    {
        text.SetAlpha(0);
        yield return text.AnimateColor(Globals.ChangeColorAlpha(text.GetColor(), 1), 0.25f);
    }
}
public class AtkBuffOneEnemy : TraitScript, IOnPlay, IOnTurnStart
{
    public override TraitTypes TraitType { get { return TraitTypes.duelist; } }
    public override string Name { get { return "Duelist"; } }
    private protected override string tooltipDesc { get { return "When there is only 1 enemy within a radius of 1 tile around you, gain 1 strength."; } }
    private bool active;
    public void OnPlay(CardData data)
    {
        CheckEffect();
    }
    public void OnTurnStart()
    {
        CheckEffect();
    }
    private void CheckEffect()
    {
        int counter = 0;
        Vector3Int playerCoord = Globals.PlayerCombat.CellCoord;
        foreach (CombatScript enemy in Globals.TurnManager.turnList)
        {
            if (enemy is EnemyScript && Globals.InCellRadius(playerCoord, enemy.CellCoord, 1f))
            {
                counter++;
            }
        }
        if (counter == 1 && !active)
        {
            active = true;
            Globals.PlayerCombat.Buffs.Strength.IncrementValue(1);
            Globals.PlayerCombat.Buffs.Powerful.IncrementValue(1);
        }
        else if (counter != 1 && active)
        {
            active = false;
            Globals.PlayerCombat.Buffs.Strength.IncrementValue(-1);
            Globals.PlayerCombat.Buffs.Powerful.IncrementValue(-1);
            // remove it 
        }
    }
}
public class ThreeEnemyAtkTrait : TraitScript, IOnPlay, IOnTurnStart
{
    public override string Name { get { return "Underdog"; } }
    public override TraitTypes TraitType { get { return TraitTypes.underdog; } }
    private protected override string tooltipDesc { get { return "When there are more than 2 enemies in a radius of 1.5 tiles around you, gain 2 strength."; } }
    private bool active;
    public void OnPlay(CardData data)
    {
        CheckEffect();
    }
    public void OnTurnStart()
    {
        CheckEffect();
    }
    private void CheckEffect()
    {
        int counter = 0;
        Vector3Int playerCoord = Globals.PlayerCombat.CellCoord;
        foreach (CombatScript enemy in Globals.TurnManager.turnList)
        {
            if (enemy is EnemyScript && Globals.InCellRadius(playerCoord, enemy.CellCoord, 1.5f))
            {
                counter++;
            }
        }
        if (counter >= 3 && !active)
        {
            active = true;
            Globals.PlayerCombat.Buffs.Strength.IncrementValue(2);
        }
        else if (counter < 3 && active)
        {
            active = false;
            Globals.PlayerCombat.Buffs.Strength.IncrementValue(-2);
        }
    }
}
public class MoveRangeTrait : TraitScript, IOnCombatStart
{
    public override TraitTypes TraitType { get { return TraitTypes.moveRange; } }
    public override string Name { get { return "Speedy"; } }
    private protected override string tooltipDesc { get { return "Gain 1 quick at the start of a battle."; } }
    public void OnCombatStart()
    {
        Globals.PlayerCombat.Buffs.Quick.Activate(Globals.PlayerCombat.gameObject, true, 1);
    }
}
public class BackstabTrait : TraitScript, IDmgAdd
{
    public override TraitTypes TraitType { get { return TraitTypes.traitorous; } }
    public override string Name { get { return "Traitorous"; } }
    private protected override string tooltipDesc { get { return "Deal 5 extra damage when attacking an enemy from behind"; } }
    public void DmgAdd(CardData data)
    {
        foreach(CardAttackData atk in data.HitEnemies)
        {
            if (atk.AtkType == AttackType.Melee && Globals.CheckPerpendicular(playerCombat.CellCoord, atk.combatable.CellCoord))
            {
                int dir = Globals.Vector3ToDir(playerCombat.CellCoord, atk.combatable.CellCoord);
                if (dir == playerCombat.FacingDirection && dir == atk.combatable.FacingDirection)
                {
                    atk.damage += 5;
                }
            }
        }
    }
}
public class MoveAttackCombo : TraitScript, IOnTurnStart, IOnPlay, IDmgAdd
{
    public override TraitTypes TraitType { get { return TraitTypes.combo; } }
    public override string Name { get { return "Combo Maker"; } }
    private protected override string tooltipDesc { get { return "Deal 3 extra damage when playing an attack card after a movement card."; } }
    bool lastCardIsMovement = false;
    public void OnTurnStart()
    {
        lastCardIsMovement = false;
    }
    public void OnPlay(CardData data)
    {
        lastCardIsMovement = data.Type == CardType.Movement;
        if (lastCardIsMovement)
        {
            text.StartCoroutine(text.AnimateColor(Color.yellow, 0.25f));
        }
        else
        {
            text.StartCoroutine(text.AnimateColor(Color.white, 0.25f));
        }
    }
    public void DmgAdd(CardData data)
    {
        if (lastCardIsMovement)
        {
            foreach (CardAttackData atk in data.HitEnemies)
            {
                atk.damage += 3;
            }
        }
    }
}
public class DrawCombatStart : TraitScript, IOnCombatStart, IOnTurnStart
{
    public override TraitTypes TraitType { get { return TraitTypes.moveRange; } }
    public override string Name { get { return "Quickdraw"; } }
    private protected override string tooltipDesc { get { return "Draw 2 extra cards on your first turn of a battle."; } }
    bool firstTurn = true;
    public void OnCombatStart()
    {
        firstTurn = true;
    }
    public void OnTurnStart()
    {
        if (firstTurn)
        {
            playerCombat.StartCoroutine(DelayedDraw());
        }
    }
    private IEnumerator DelayedDraw()
    {
        text.StartCoroutine(text.FlashColor(Color.black, 0.5f));
        yield return Globals.WaitForSeconds(0.25f);
        yield return playerCombat.StartCoroutine(Globals.Deck.DrawCard(2));
        firstTurn = false;
    }
}
public interface IOnPlay
{
    void OnPlay(CardData data);
}
public interface IPassive
{
}
public interface IDmgAdd
{
    void DmgAdd(CardData data);
}
public interface IDmgMultiply
{
    void DmgMultiply(CardData data);
}
public interface IOnCombatStart
{
    void OnCombatStart();
}
public interface IOnCombatEnd
{
    void OnCombatEnd();
}
public interface IOnBuff
{
    void OnBuff();
}
public interface IOnTurnStart
{
    void OnTurnStart();
}
public interface IOnDeckShuffle
{
    void OnDeckShuffle();
}
public interface IOnDiscard
{
    void OnDiscard(CardData data);
}
public interface IOnTileHover
{
    void OnTileHover(CardData data, Vector3Int cellCoord);
}
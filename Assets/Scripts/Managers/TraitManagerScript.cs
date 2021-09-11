using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class TraitManagerScript : MonoBehaviour
{
    [SerializeField] private Transform traitParent;
    [SerializeField] private TextScript traitText;
    [SerializeField] private TooltipScript tooltip;
    private Dictionary<Type, List<TraitScript>> traitTypes = new Dictionary<Type, List<TraitScript>>();
    private List<TraitScript> activeTraits = new List<TraitScript>();
    public Dictionary<TraitTypes, TraitScript> Traits = new Dictionary<TraitTypes, TraitScript>();
    private void Start()
    {
        SetupDictionary();
        CreateTrait(Traits[TraitTypes.quickdraw]);
        CreateTrait(Traits[TraitTypes.combo]);
    }
    public TraitScript CreateTrait(TraitScript trait)
    {
        TextScript text = traitText.CreateTextScript(traitParent, new Vector3(0, activeTraits.Count * -50));
        trait.OnPickup(text, tooltip);
        AddToLists(trait);
        activeTraits.Add(trait);
        //Animation for getting new trait or something
        return trait;
    }
    public void OnPlay(CardData data)
    {
        foreach (IOnPlay item in traitTypes[typeof(IOnPlay)])
        {
            item.OnPlay(data);
        }
    }
    public void CalculateDamage(CardData data)
    {
        foreach (IDmgAdd item in traitTypes[typeof(IDmgAdd)])
        {
            item.DmgAdd(data);
        }
        foreach (IDmgMultiply item in traitTypes[typeof(IDmgMultiply)])
        {
            item.DmgMultiply(data);
        }
    }
    public void OnBuff(CombatScript caster, CombatScript target)
    {
        foreach (IOnBuff item in traitTypes[typeof(IOnBuff)])
        {
            item.OnBuff();
        }
    }
    public void OnCombatStart()
    {
        foreach (IOnCombatStart item in traitTypes[typeof(IOnCombatStart)])
        {
            item.OnCombatStart();
        }
    }
    public void OnCombatEnd()
    {
        foreach (IOnCombatEnd item in traitTypes[typeof(IOnCombatEnd)])
        {
            item.OnCombatEnd();
        }
    }
    public void OnTurnStart()
    {
        foreach (IOnTurnStart item in traitTypes[typeof(IOnTurnStart)])
        {
            item.OnTurnStart();
        }
    }
    public void OnDeckShuffle()
    {
        foreach (IOnDeckShuffle item in traitTypes[typeof(IOnDeckShuffle)])
        {
            item.OnDeckShuffle();
        }
    }
    public void OnDiscard(CardData data)
    {
        foreach (IOnDiscard item in traitTypes[typeof(IOnDiscard)])
        {
            item.OnDiscard(data);
        }
    }
    public void AddToLists(TraitScript item)
    {
        foreach (Type type in item.GetType().GetInterfaces())
        {
            traitTypes[type].Add(item);
        }
    }
    private void SetupDictionary()
    {
        traitTypes.Add(typeof(IOnPlay), new List<TraitScript>());
        traitTypes.Add(typeof(IPassive), new List<TraitScript>());
        traitTypes.Add(typeof(IDmgAdd), new List<TraitScript>());
        traitTypes.Add(typeof(IDmgMultiply), new List<TraitScript>());
        traitTypes.Add(typeof(IOnCombatStart), new List<TraitScript>());
        traitTypes.Add(typeof(IOnCombatEnd), new List<TraitScript>());
        traitTypes.Add(typeof(IOnBuff), new List<TraitScript>());
        traitTypes.Add(typeof(IOnTurnStart), new List<TraitScript>());
        traitTypes.Add(typeof(IOnDeckShuffle), new List<TraitScript>());
        traitTypes.Add(typeof(IOnDiscard), new List<TraitScript>());
        traitTypes.Add(typeof(IOnTileHover), new List<TraitScript>());
        Traits.Add(TraitTypes.duelist, new AtkBuffOneEnemy());
        Traits.Add(TraitTypes.underdog, new ThreeEnemyAtkTrait());
        Traits.Add(TraitTypes.moveRange, new MoveRangeTrait());
        Traits.Add(TraitTypes.traitorous, new BackstabTrait());
        Traits.Add(TraitTypes.combo, new MoveAttackCombo());
        Traits.Add(TraitTypes.quickdraw, new DrawCombatStart());
    }
}
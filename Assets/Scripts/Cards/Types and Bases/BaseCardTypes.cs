using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public abstract class MoveCard : HighlightCard
{
    public override CardType Type { get { return CardType.Movement; } }
    public override void Setup()
    {
        base.Setup();
        defaultHighlightColor = staticCardInfo.MovementHighlight;
    }
    public override void UpdateValues()
    {
        cardData.Range = baseRange + playerBuffs.Quick.GetValue();
    }
}
public abstract class AbilityCard : HighlightCard
{
    public override CardType Type { get { return CardType.Ability; } }
    public override void Setup()
    {
        base.Setup();
        defaultHighlightColor = staticCardInfo.AbilityHighlight;
    }
}
public abstract class AttackCard : HighlightCard
{
    [SerializeField] private protected int baseDamage;
    private protected int damage;
    private protected List<CardAttackData> hitEnemies = new List<CardAttackData>();
    private protected AttackType attackType;
    private protected List<CardAttackData> lastHitEnemies = new List<CardAttackData>();
    private protected override void Deselect()
    {
        base.Deselect();
        ResetPreview(cardData.HitEnemies);
        ClearLists();
        lastHitEnemies.Clear();
    }
    public override void UpdateValues()
    {
        damage = baseDamage + playerBuffs.Powerful.Value;
    }
    private protected override IEnumerator PlayEffect()
    {
        yield return base.PlayEffect();
        ClearLists();
        lastHitEnemies.Clear();
    }
    private protected virtual void ClearLists()
    {
        hitEnemies.Clear();
    }
    public override bool Check(Vector3Int targetCell)
    {
        ClearLists();
        if (playerCombat.CurrentRoom.TryGetEnemyAtCell(targetCell, out EnemyScript enemy))
        {
            return CheckTaunt(enemy);
        }
        else
        {
            return !playerBuffs.Taunted.IsActive;
        }
    }
    public override bool HoverCheck(Vector3Int targetCell)
    {
        bool check = base.HoverCheck(targetCell);
        if (cardData.HitEnemies != null)
        {
            if (!Globals.CompareLists(hitEnemies, lastHitEnemies))
            {
                ResetPreview(lastHitEnemies);
                cardData.HitEnemies.ForEach(atk => atk.combatable.StartCoroutine(atk.combatable.PreviewLoseHp(atk.damage)));
                lastHitEnemies.Clear();
                lastHitEnemies.AddRange(hitEnemies);
            }
        }
        return check;
    }
    private protected bool CheckHits(List<Vector3Int> coordsToCheck)
    {
        return CheckHits(coordsToCheck, Enumerable.Repeat(damage, coordsToCheck.Count).ToList());
    }
    private protected bool CheckHits(List<Vector3Int> coordsToCheck, List<int> damageValues)
    {
        bool hitEnemy = false;
        for (int i = 0; i < coordsToCheck.Count; i++)
        {
            Vector3Int coord = coordsToCheck[i];
            if (playerCombat.CurrentRoom.TryGetEnemyAtCell(coord, out EnemyScript enemy))
            {
                hitEnemies.Add(new CardAttackData(enemy, damageValues[i], attackType));
                hitEnemy = true;
            }
        }
        if (hitEnemy)
        {
            cardData.SetHitEnemies(hitEnemies);
            traitManager.CalculateDamage(cardData);
            for (int i = 0; i < cardData.HitEnemies.Count; i++)
            {
                if (cardData.HitEnemies[i].combatable.Buffs.Targeted.IsActive)
                {
                    cardData.HitEnemies[i].damage = Globals.RoundToNearestInt(cardData.HitEnemies[i].damage * 1.25f);
                }
            }
            return true;
        }
        return false;
    }
    private protected bool CheckTaunt(CombatScript target)
    {
        if (playerBuffs.Taunted.IsActive)
        {
            if (target != playerBuffs.Taunted.TauntTarget)
            {
                return false;
            }
        }
        return true;
    }
    private protected IEnumerator DamageEnemies(DamageSFXType dmgType = DamageSFXType.Blade)
    {
        List<Coroutine> tempList = new List<Coroutine>();
        for (int i = 0; i < cardData.HitEnemies.Count; i++)
        {
            CardAttackData atk = cardData.HitEnemies[i];
            tempList.Add(atk.combatable.StartCoroutine(atk.combatable.LoseHp(atk.damage, dmgType)));
        }
        for (int i = 0; i < tempList.Count; i++)
        {
            yield return tempList[i];
        }
    }
    public void ResetPreview(List<CardAttackData> list)
    {
        if (list != null)
        {
            foreach (CardAttackData atk in list)
            {
                atk.combatable.StartCoroutine(atk.combatable.ResetPreviewHp());
            }
        }
    }
}
public abstract class RangedAttackCard : AttackCard
{
    public override CardType Type { get { return CardType.RangedAttack; } }
    public override void Setup()
    {
        base.Setup();
        attackType = AttackType.Ranged;
        defaultHighlightColor = staticCardInfo.Attackhighlight;
    }
    public override bool Check(Vector3Int targetCell)
    {
        if (base.Check(targetCell))
        {
            return !playerBuffs.Intimidated.IsActive;
        }
        return false;
    }
    public override void UpdateValues()
    {
        base.UpdateValues();
        damage += playerBuffs.SharpSight.GetValue();
        cardData.Range = baseRange + playerBuffs.Range.Value;
    }
}
public abstract class MeleeAttackCard : AttackCard
{
    public override CardType Type { get { return CardType.MeleeAttack; } }
    public override void Setup()
    {
        base.Setup();
        attackType = AttackType.Melee;
        defaultHighlightColor = staticCardInfo.Attackhighlight;
    }
    public override bool Check(Vector3Int targetCell)
    {
        if (base.Check(targetCell))
        {
            return !playerBuffs.Disarmed.IsActive;
        }
        return false;
    }
    public override void UpdateValues()
    {
        base.UpdateValues();
        damage += playerBuffs.Strength.GetValue();
        cardData.Range = baseRange + playerBuffs.Reach.Value;
    }
}

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public enum BuffType
{
    Aversion_E, Aversion_N, Aversion_S, Aversion_W, 
    Cower, Disarmed, Entranced, Immobile, Imprecise, 
    Intimidated, Learned, Mend, Petrified, Powerful, 
    Purge, Quick, Range, Reach, SharpSight, Slow, 
    Strength, Stunned, Targeted, Taunted, Weak
}

public class BuffData
{
    private BuffManager buffManager;
    private CombatScript combatable;
    public StatusEffect Immobile = new StatusEffect(BuffType.Immobile);
    public StatusEffect Stunned = new StatusEffect(BuffType.Stunned);
    public StatusEffect Disarmed = new StatusEffect(BuffType.Disarmed);
    public StatusEffect Intimidated = new StatusEffect(BuffType.Intimidated);
    public StatusEffect Targeted = new StatusEffect(BuffType.Targeted);
    public StatusEffect[] Aversion = { new StatusEffect(BuffType.Aversion_N), new StatusEffect(BuffType.Aversion_E), new StatusEffect(BuffType.Aversion_S), new StatusEffect(BuffType.Aversion_W) };
    public DirectionDebuff Petrified = new DirectionDebuff(BuffType.Petrified);
    public DirectionDebuff Cower = new DirectionDebuff(BuffType.Cower);
    public DirectionDebuff Entranced = new DirectionDebuff(BuffType.Entranced);
    public Taunted Taunted = new Taunted(BuffType.Taunted);
    public DecrementBuff Mend = new DecrementBuff(BuffType.Mend);
    public DecrementPosNeg SharpSight = new DecrementPosNeg(BuffType.SharpSight, BuffType.Imprecise);
    public DecrementPosNeg Strength = new DecrementPosNeg(BuffType.Strength, BuffType.Weak);
    public StackableBuff Quick = new StackableBuff(BuffType.Quick, BuffType.Slow);
    public PersistentBuff Powerful = new PersistentBuff(BuffType.Powerful);
    public PersistentBuff Reach = new PersistentBuff(BuffType.Reach);
    public PersistentBuff Range = new PersistentBuff(BuffType.Range);
    public List<Buff> AllBuffs = new List<Buff>();
    public Dictionary<BuffType, PersistentBuff> PersistentBuffs = new Dictionary<BuffType, PersistentBuff>();
    public BuffData(BuffManager manager, CombatScript c)
    {
        buffManager = manager;
        combatable = c;
        AllBuffs.Add(Immobile);
        AllBuffs.Add(Stunned);
        AllBuffs.Add(Disarmed);
        AllBuffs.Add(Intimidated);
        AllBuffs.Add(Targeted);
        AllBuffs.Add(Petrified);
        AllBuffs.Add(Cower);
        AllBuffs.Add(Entranced);
        AllBuffs.Add(Taunted);
        AllBuffs.Add(Mend);
        AllBuffs.Add(Powerful);
        AllBuffs.Add(SharpSight);
        AllBuffs.Add(Strength);
        AllBuffs.Add(Quick);
        AllBuffs.Add(Range);
        AllBuffs.Add(Reach);
        /*        PersistentBuffs.Add(BuffType.Sharp_Sight, SharpSight);
                PersistentBuffs.Add(BuffType.Powerful, Powerful);
                PersistentBuffs.Add(BuffType.Strength, Strength);
                PersistentBuffs.Add(BuffType.Quick, Quick);
                PersistentBuffs.Add(BuffType.Range, Range);
                PersistentBuffs.Add(BuffType.Reach, Reach);*/
        foreach (StatusEffect aversion in Aversion)
        {
            AllBuffs.Add(aversion);
        }
        foreach (Buff buff in AllBuffs)
        {
            buff.SetBuffManager(buffManager);
        }
    }
    public void OnTurnEnd() // make sure to read value first, then decrement
    {
        Immobile.Decrement();
        Stunned.Decrement();
        Disarmed.Decrement();
        Intimidated.Decrement();
        Targeted.Decrement();
        Petrified.Decrement();
        Cower.Decrement();
        Entranced.Decrement();
        Taunted.Decrement();
        Mend.Decrement();
        Quick.Decrement();
        foreach (StatusEffect aversion in Aversion)
        {
            aversion.Decrement();
        }
    }
    public bool CheckDir(int dir)
    {
        return !Aversion[dir].IsActive && Cower.CheckDir(dir) && Petrified.CheckDir(dir) && Entranced.CheckDir(dir);
    }
    public bool CanMoveTo(Vector3Int targetCell, bool checkDir = true)
    {
        return !Immobile.IsActive && !Stunned.IsActive && (!checkDir || CheckDir(Globals.Vector3ToDir(combatable.CellCoord, targetCell)));
    }
    public void DisableAll()
    {
        foreach (Buff buff in AllBuffs)
        {
            if (buff.IsActive)
            {
                buff.Disable();
            }
        }
    }
}
public class Buff
{
    public BuffType Type { get; private protected set; }
    public bool IsActive { get; private protected set; } = false;
    private protected BuffIconScript buffIcon;
    public bool Immune = false;
    private protected BuffManager buffManager;
    public void SetBuffManager(BuffManager m)
    {
        buffManager = m;
    }
    public Buff(BuffType type)
    {
        Type = type;
    }
    public void SetBuffIcon(BuffIconScript icon)
    {
        buffIcon = icon;
    }
    public virtual void Disable()
    {
        buffManager.DisableBuff(buffIcon);
        IsActive = false;
    }
    private protected virtual void OnValueChange()
    {
        Globals.Deck.UpdateAllCards();
    }
}
public abstract class DurationBuff : Buff
{
    public int Duration { get; private protected set; } = 0;
    public virtual void Decrement()
    {
        if (IsActive)
        {
            Duration--;
            buffIcon.SetText(Duration.ToString());
            if (Duration <= 0)
            {
                Disable();
            }
            OnValueChange();
        }
    }
    public override void Disable()
    {
        base.Disable();
        Duration = 0;
    }
    public DurationBuff(BuffType type) : base(type)
    {
    }
}
public class StatusEffect : DurationBuff
{
    public virtual void Activate(int duration)
    {
        Duration += duration;
        buffIcon.SetText(duration.ToString());
        if (!IsActive)
        {
            IsActive = true;
            buffManager.ActivateBuff(buffIcon);
        }
        OnValueChange();
    }
    public StatusEffect(BuffType type) : base(type)
    {
    }
}
public class Taunted : DurationBuff
{
    public CombatScript TauntTarget;
    public void Activate(int duration, CombatScript target)
    {
        if (TauntTarget != target)
        {
            Duration = 0;
        }
        TauntTarget = target;
        Duration += duration;
        buffIcon.SetText(duration.ToString());
        if (!IsActive)
        {
            IsActive = true;
            buffManager.ActivateBuff(buffIcon);
        }
        OnValueChange();
    }
    public override void Disable()
    {
        base.Disable();
        Duration = 0;
        TauntTarget = null;
    }
    public Taunted(BuffType type) : base(type)
    {
    }
}
public class DirectionDebuff : DurationBuff
{
    public int Direction;
    public void Activate(int duration, int direction)
    {
        Duration += duration;
        buffIcon.SetText(duration.ToString());
        if (!IsActive)
        {
            IsActive = true;
            buffManager.ActivateBuff(buffIcon);
        }
        Direction = direction;
        OnValueChange();
        // make the unit turn to face the direction
    }
    public override void Disable()
    {
        base.Disable();
        Direction = 0;
    }
    public bool CheckDir(int dir)
    {
        return !IsActive || dir == Direction;
    }
    public DirectionDebuff(BuffType type) : base(type)
    {
    }
}
public class DecrementBuff : DurationBuff
{
    public DecrementBuff(BuffType type) : base(type)
    {
    }
    public int GetValue()
    {
        return Duration;
    }
    public virtual void IncrementValue(int change)
    {
        Duration += change;
        buffIcon.SetText(Duration.ToString());
        if (Duration == 0)
        {
            Disable();
        }
        else if (!IsActive)
        {
            IsActive = true;
            buffManager.ActivateBuff(buffIcon);
        }
        OnValueChange();
    }
}
public class DecrementPosNeg : DecrementBuff
{
    private BuffType positive;
    private BuffType negative;
    public DecrementPosNeg(BuffType p, BuffType n) : base(p)
    {
        positive = p;
        negative = n;
    }
    public override void IncrementValue(int change)
    {
        Duration += change;
        buffIcon.SetText(Duration.ToString());
        if (Duration > 0)
        {
            Type = positive;
            buffManager.SetBuffIconSprite(buffIcon, Type);
        }
        else if (Duration < 0)
        {
            Type = negative;
            buffManager.SetBuffIconSprite(buffIcon, Type);
        }
        if (Duration == 0)
        {
            Disable();
        }
        else if (!IsActive)
        {
            IsActive = true;
            buffManager.ActivateBuff(buffIcon);
        }
        OnValueChange();
    }
    public override void Decrement()
    {
        if (IsActive)
        {
            if (Duration > 0)
            {
                Duration--;
            }
            else
            {
                Duration++;
            }
            buffIcon.SetText(Duration.ToString());
            if (Duration == 0)
            {
                Disable();
            }
            OnValueChange();
        }
    }
}
public class StackableBuff : Buff
{
    private BuffType positive;
    private BuffType negative;
    private Dictionary<GameObject, int> posInstances = new Dictionary<GameObject, int>();
    private Dictionary<GameObject, int> negInstances = new Dictionary<GameObject, int>();
    public StackableBuff(BuffType p, BuffType n) : base(p)
    {
        positive = p;
        negative = n;
    }
    public int GetValue()
    {
        return posInstances.Count - negInstances.Count;
    }
    public void Decrement()
    {
        for (int i = 0; i < posInstances.Count; i++)
        {
            GameObject GO = posInstances.Keys.ToList()[i];
            posInstances[posInstances.Keys.ToList()[i]]--;
            if (posInstances[GO] <= 0)
            {
                posInstances.Remove(GO);
                i--;
            }
        }
        for (int i = 0; i < negInstances.Count; i++)
        {
            GameObject GO = negInstances.Keys.ToList()[i];
            negInstances[negInstances.Keys.ToList()[i]]--;
            if (negInstances[GO] <= 0)
            {
                negInstances.Remove(GO);
                i--;
            }
        }
        if (posInstances.Count == 0 && negInstances.Count == 0 && IsActive)
        {
            Debug.Log("disable");
            Disable();
        }
        OnValueChange();
    }
    public void Activate(GameObject GO, bool pos, int dur)
    {
        Dictionary<GameObject, int> instances;
        if (pos)
        {
            instances = posInstances;
        }
        else
        {
            instances = negInstances;
        }
        if (instances.ContainsKey(GO))
        {
            instances[GO] += dur;
        }
        else
        {
            instances.Add(GO, dur);
        }
        OnValueChange();
    }
    public void Deactivate(bool pos, GameObject GO)
    {
        Dictionary<GameObject, int> instances;
        if (pos)
        {
            instances = posInstances;
        }
        else
        {
            instances = negInstances;
        }
        instances.Remove(GO);
        if (posInstances.Count == 0 && negInstances.Count == 0)
        {
            Disable();
        }
    }
    private protected override void OnValueChange()
    {
        base.OnValueChange();
        if (GetValue() > 0)
        {
            Type = positive;
        }
        else if (GetValue() < 0)
        {
            Type = negative;
        }
        if (!IsActive && GetValue() != 0)
        {
            Debug.Log(GetValue());
            buffManager.SetBuffIconSprite(buffIcon, Type);
            buffManager.ActivateBuff(buffIcon);
            IsActive = true;
        }
    }
}
public class PersistentBuff : Buff
{
    public int Value { get; private protected set; } = 0;
    public PersistentBuff(BuffType type) : base(type)
    {
    }
    public virtual void IncrementValue(int change)
    {
        Value += change;
        buffIcon.SetText(Value.ToString());
        if (Value == 0)
        {
            Disable();
        }
        else if (!IsActive)
        {
            IsActive = true;
            buffManager.ActivateBuff(buffIcon);
        }
        OnValueChange();
    }
    public override void Disable()
    {
        base.Disable();
        Value = 0;
    }
}
public class PersistentBuffDebuff : PersistentBuff
{
    public BuffType Positive;
    public BuffType Negative;
    public PersistentBuffDebuff(BuffType p, BuffType n) : base(p)
    {
        Positive = p;
        Negative = n;
    }
    public override void IncrementValue(int change)
    {
        Value += change;
        buffIcon.SetText(Value.ToString());
        if (Value > 0)
        {
            Type = Positive;
            buffManager.SetBuffIconSprite(buffIcon, Type);
        }
        else if (Value < 0)
        {
            Type = Negative;
            buffManager.SetBuffIconSprite(buffIcon, Type);
        }
        if (!IsActive)
        {
            IsActive = true;
            buffManager.ActivateBuff(buffIcon);
        }
        if (Value == 0)
        {
            Disable();
        }
        OnValueChange();
    }
}

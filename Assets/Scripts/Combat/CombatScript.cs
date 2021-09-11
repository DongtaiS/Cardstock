using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
public enum DamageSFXType { Blade, Stab, Fist, Blunt }
public abstract class CombatScript : BaseUnit
{
    [SerializeField] public SerializedCombatableInfo Info;
    [SerializeField] public int MaxHp;
    [SerializeField] public int Hp;
    [SerializeField] public float DmgMod;
    [System.NonSerialized] public BuffData Buffs;
    [System.NonSerialized] public BuffManager buffManager;
    [System.NonSerialized] public Transform BuffTransform;
    [System.NonSerialized] public HealthBarScript healthbar;

    private protected Material dither;
    private protected Material defaultMaterial;

    private protected AudioManager audioManager;

    private protected List<CombatScript> hiddenUnits = new List<CombatScript>();
    private protected HighlightScript highlight;
    private protected IEnumerator currentSelectAnim;
    private protected IEnumerator currentTurn;
    private protected bool continueTurn;
    private bool isMoving;
    private protected float ditherDuration = 0.5f;

    public override void Setup(RoomScript currentRoom, AssetReference assetRef)
    {
        Info.Setup();
        MainSprite = Info.MainSprite;
        audioManager = Globals.AudioManager;
        BuffTransform = Info.BuffTransform;
        dither = Info.Dither;
        defaultMaterial = Info.DefaultMaterial;
        buffManager = Info.BuffManager;
        base.Setup(currentRoom, assetRef);
        AnimObject.GetComponentInChildren<AudioScript>().SetAudioSource(Info.Audio);
        SpriteCollider = MainSprite.GetComponent<BoxCollider2D>();
        SetupHealthbar();
        Buffs = new BuffData(buffManager, this);
        buffManager.Setup(Buffs, this);
    }
    public IEnumerator CheckRotate(int direction, float waitTime = 0.1f, float turnSpeed = 0.25f)
    {
        currentTurn = Globals.CheckAnim(currentTurn, Rotate(direction, waitTime, turnSpeed), this);
        yield return StartCoroutine(currentTurn);
    }
    private IEnumerator Rotate(int direction, float waitTime = 0.1f, float turnSpeed = 0.25f)
    {
        Vector3 original = MainSprite.transform.localEulerAngles;
        int newY = (direction - 1) * 90;
        float duration = turnSpeed * Mathf.Min(Mathf.Abs(360 - original.y + newY), Mathf.Abs(original.y - newY)) / 90;
        if (FacingDirection != direction && (FacingDirection % 2 == direction % 2 || FacingDirection / 2 == direction / 2))
        {
            float tempY = -45;
            float targetA = 135;
            float targetB = -45;
            if (FacingDirection % 2 == direction % 2)
            {
                targetA = 180 - (FacingDirection % 2) * 90;
                targetB = targetA - 180;
            }
            if (Mathf.Abs(Mathf.DeltaAngle(original.y, targetA)) <= Mathf.Abs(Mathf.DeltaAngle(original.y, targetB)))
            {
                tempY = 135;
            }
            duration = turnSpeed * Mathf.Abs(Mathf.DeltaAngle(original.y, tempY)) / 90;
            yield return Globals.InterpAngle(original.y, tempY, duration, false, ang => MainSprite.transform.localEulerAngles = Globals.Vector3ChangeY(original, ang));
            FacingDirection = direction;
            AnimObject.FlipSprites();
            duration = turnSpeed * Mathf.Abs(Mathf.DeltaAngle(tempY, newY)) / 90;
            yield return Globals.InterpAngle(tempY, newY, duration, false, ang => MainSprite.transform.localEulerAngles = Globals.Vector3ChangeY(original, ang));
        }
        else
        {
            FacingDirection = direction;
            yield return Globals.InterpAngle(original.y, newY, duration, false, ang => MainSprite.transform.localEulerAngles = Globals.Vector3ChangeY(original, ang));
        }
        yield return Globals.WaitForSeconds(waitTime);
    }
    public IEnumerator UseTurn()
    {
        yield return StartOfTurn();
        if (continueTurn)
        {
            yield return Act();
        }
        yield return EndOfTurn();
    }
    private protected virtual IEnumerator StartOfTurn()
    {
        yield return Globals.WaitForSeconds(0.25f);
        continueTurn = true;
        if (Buffs.Stunned.IsActive)
        {
            continueTurn = false;
        }
        if (Buffs.Mend.IsActive)
        {
            yield return healthbar.GainHp(Buffs.Mend.GetValue());
        }
    }
    private protected abstract IEnumerator Act();
    private protected virtual IEnumerator EndOfTurn()
    {
        Buffs.OnTurnEnd();
        yield return null;
    }
    public abstract void EndTurn();
    public void SetMaxHp(int newMaxHp)
    {
        MaxHp = newMaxHp;
        healthbar.SetMaxHp(MaxHp);
    }
    public virtual IEnumerator LoseHp(int hpLost, DamageSFXType dmgSource, int dir)
    {
        Coroutine hitAnim = StartCoroutine(OnHitAnim(dir));
        hpLost = Mathf.Min(Hp, hpLost);
        Hp -= hpLost;
        OnHitSound(dmgSource);
        yield return healthbar.CheckAnim(healthbar.LoseHp(hpLost));
        yield return hitAnim;
        if (Hp <= 0)
        {
            StartCoroutine(DieAnim());
        }
    }
    public IEnumerator LoseHp(int hpLost, DamageSFXType dmgSource)
    {
        yield return LoseHp(hpLost, dmgSource, Globals.OppositeDirection(FacingDirection));
    }
    private protected virtual void OnHitSound(DamageSFXType dmgSource)
    {
        switch (dmgSource)
        {
            case DamageSFXType.Blade:
                PlaySound(audioManager.GetClip(AudioManager.CombatSFXEnum.FleshHit));
                break;
            case DamageSFXType.Blunt:
                PlaySound(audioManager.GetClip(AudioManager.CombatSFXEnum.FleshSmash));
                break;
            case DamageSFXType.Fist:
                PlaySound(audioManager.GetClip(AudioManager.CombatSFXEnum.FleshSmash));
                break;
            case DamageSFXType.Stab:
                PlaySound(audioManager.GetClip(AudioManager.CombatSFXEnum.FleshPierce));
                break;
        }
    }
    private void PlaySound(AudioClip clip)
    {
        audioManager.PlaySoundEffect(Info.Audio, clip);
    }
    private protected virtual IEnumerator OnHitAnim(int dir)
    {
        MainSprite.color = new Color(1f, 0.2f, 0.2f);
        if (!isMoving)
        {
            yield return Globals.InterpVector3(transform.localPosition, transform.localPosition + 2.5f * (Vector3)Globals.DirectionToWorldVector3(dir), 0.1f, pos => transform.localPosition = pos);
            yield return Globals.InterpVector3(transform.localPosition, CurrentRoom.GetFloor().GetCellCenterLocal(CellCoord), 0.3f, pos => transform.localPosition = pos);
        }
        else
        {
            yield return Globals.WaitForSeconds(0.4f);
        }
        MainSprite.color = Color.white;
    }
    public virtual IEnumerator HealHp(int hpGained)
    {
        hpGained = Mathf.Min(MaxHp - Hp, hpGained);
        Debug.Log(hpGained);
        Hp += hpGained;
        yield return healthbar.CheckAnim(healthbar.GainHp(hpGained));
    }
    public virtual IEnumerator Knockback(int dir, int dist)
    {
        isMoving = true;
        Premove();
        Vector3Int newCoord = CellCoord + (Vector3Int)Globals.IntDirectionToVector2(dir) * dist;
        yield return Globals.InterpVector3(transform.localPosition, CurrentRoom.GetFloor().GetCellCenterLocal(newCoord), dist * 0.25f, pos => transform.localPosition = pos);
        Postmove();
        isMoving = false;
    }
    public virtual IEnumerator PreviewLoseHp(int hpLost)
    {
        yield return healthbar.CheckAnim(healthbar.PreviewHp(Mathf.Min(Hp, hpLost)));
    }
    public virtual IEnumerator ResetPreviewHp()
    {
        yield return healthbar.CheckAnim(healthbar.ResetPreviewHp());
    }
    private protected virtual void Premove() //TODO: Implement this 
    {
        CurrentRoom.RemoveAtCell(CellCoord);
    }
    private protected virtual void Postmove()
    {
        UpdateCellCoord();
        CurrentRoom.SetAtCell(CellCoord, gameObject);
    }
    private protected virtual IEnumerator DieAnim()
    {
        Globals.RTCreator.RemoveRenderObject(renderData);
        Buffs.DisableAll();
        Deselect();
        yield return null;
    }
    private protected void SetupHealthbar()
    {
        healthbar = GetComponentInChildren<HealthBarScript>();
        healthbar.Setup(MaxHp, Hp);
    }
    public virtual void OnSelect()
    {
        ShowUnitsInFront();
        GetCombatablesFromColliders();
        foreach(CombatScript combatable in hiddenUnits)
        {
            combatable.CheckSelectAnim(combatable.Hide(CellCoord.y));
        }
        highlight = Globals.PrefabManager.SpawnObjectFromPool<HighlightScript>(PrefabManager.ObjectPool.Highlight, transform, transform.position);
        highlight.SetColor(Color.green);
    }
    public void HideUnitsInFront()
    {
        GetCombatablesFromColliders();
        foreach (CombatScript combatable in hiddenUnits)
        {
            if (combatable.CellCoord.y < CellCoord.y)
            {
                combatable.CheckSelectAnim(combatable.Hide(CellCoord.y));
            }
        }
    }
    public void ShowUnitsInFront()
    {
        foreach (CombatScript combatable in hiddenUnits)
        {
            if (combatable != null && combatable.CellCoord.y < CellCoord.y)
            {
                combatable.CheckSelectAnim(combatable.Show());
            }
        }
    }
    public virtual void Deselect()
    {
        foreach (CombatScript combatable in hiddenUnits)
        {
            if (combatable != null)
            {    
                combatable.CheckSelectAnim(combatable.Show());
            }
        }
        if (highlight != null)
        {
            Globals.PrefabManager.StartCoroutine(highlight.FadeOut(0.25f));
        }
    }
    public void GetCombatablesFromColliders()
    {
        List<Collider2D> tempList = new List<Collider2D>();
        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(Globals.UnitCollisionMask);
        SpriteCollider.OverlapCollider(filter, tempList);
        hiddenUnits.Clear();
        foreach (Collider2D collider in tempList)
        {
            hiddenUnits.Add(collider.GetComponentInParent<CombatScript>());
        }
    }
    public virtual IEnumerator Hide(int yPos)
    {
        if (CellCoord.y > yPos)
        {
            dither.SetFloat("MinValue", 0.5f);
            dither.SetFloat("MinValue", 0.5f);
        }
        else
        {
            dither.SetFloat("MinValue", 0.25f);
            dither.SetFloat("MinValue", 0.25f);
        }
        buffManager.SetIconMaterial(dither);
        yield return DitherAnim(0, 1, ditherDuration);
    }
    public virtual IEnumerator Show()
    {
        yield return DitherAnim(1, 0, ditherDuration);
    }
    public virtual IEnumerator DitherAnim(float a, float b, float dur)
    {
        MainSprite.material = dither;
        float value = b;
        float curr = dither.GetFloat("Progress");
        float duration = dur * Mathf.InverseLerp(b, a, curr);
        StartCoroutine(Globals.InterpFloat(curr, b, duration, v => value = v));
        while (value != b)
        {
            healthbar.SetHealthBarAlpha(Mathf.Lerp(1, 0.25f, value));
            dither.SetFloat("Progress", value);
            yield return Globals.FixedUpdate;
        }
        healthbar.SetHealthBarAlpha(Mathf.Lerp(1, 0.25f, value));
        dither.SetFloat("Progress", b);
    }
    public void CheckSelectAnim(IEnumerator inAnim)
    {
        if (currentSelectAnim != null)
        {
            StopCoroutine(currentSelectAnim);
        }
        currentSelectAnim = inAnim;
        StartCoroutine(currentSelectAnim);
    }
}

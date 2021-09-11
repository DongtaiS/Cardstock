using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public enum CardType { MeleeAttack, RangedAttack, Movement, Ability, Passive } //Types of cards
public enum AttackType { Melee, Ranged } //Type of attack
public struct Keywords
{
    public static string damage = "<b>damage</b>";
    public static string Move = "<#00E3FFFF><b>Move</b>" + GetSprite("Move") + "</color>";
    public static string move = "<#00E3FFFF><b>move</b>" + GetSprite("Move") + "</color>";
    public static string Draw = "<b>Draw</b>";
    public static string draw = "<b>draw</b>";
    public static string Piercing = "<b>Piercing</b>";
    public static string piercing = "<b>piercing</b>";
    public static string stun = "<b>stun</b>";
    public static string Bold(string input)
    {
        return "<b>" + input + "</b>";
    }
    public static string DamageNum(int dmg, int baseDmg, AttackType type)
    {
        string sprite;
        if (type == AttackType.Melee)
        {
            sprite = GetSprite("MeleeAttack");
        }
        else
        {
            sprite = GetSprite("RangedAttack");
        }
        if (dmg > baseDmg)
        {
            return "<#26be1cff>" + dmg.ToString() + sprite + "</color>"; //Green
        }
        else if (dmg < baseDmg)
        {
            return "<#ad0f0fff>" + dmg.ToString() + sprite + "</color>"; //Red
        }
        return "<#B7B7B7FF>" + dmg.ToString() + sprite + "</color>"; //none (default color)
    }
    public static string GetSprite(string name)
    {
        return "<sprite tint=1 name=" + name + ">";
    }
}
public class CardAttackData : System.IEquatable<CardAttackData> //Data class used when an enemy is attacked, accessed by traits for various effects
{
    public CombatScript combatable; //Reference to the enemy that was hit
    public int damage;
    public AttackType AtkType;
    public CardAttackData(CombatScript c, int dmg, AttackType type)
    {
        combatable = c;
        damage = dmg;
        AtkType = type;
    }
    public bool Equals(CardAttackData cad)
    {
        if (cad != null)
        {
            return cad.combatable.Equals(combatable) && cad.damage == damage && cad.AtkType == AtkType;
        }
        return false;
    }
    public override bool Equals(object obj)
    {
        return Equals(obj as CardAttackData);
    }
    public override int GetHashCode() => (combatable, damage, AtkType).GetHashCode();
}
public class CardBuffData //Data class used when an enemy is debuffed
{
    public EnemyScript enemy;
    public BuffType buff;
    public CardBuffData(EnemyScript e, BuffType b)
    {
        enemy = e;
        buff = b;
    }
}
public class CardData //Data class containing information about the card, updated when played and passed to traits for effects
{
    public List<CardAttackData> HitEnemies = new List<CardAttackData>();    //List of CardAttackData of enemies hit
    public List<CardBuffData> DebuffedEnemies = new List<CardBuffData>();   //List of CardBuffData of enemies debuffed
    public int EnergyCost;
    public float Range = -1;                                                //Range (if no range, defaults to -1)
    public int Direction = -1;
    public CardType Type;
    public int HealVal = -1;
    public bool AoE = false;
    public int MoveDist = -1;
    public CardData(CardType type, int cost)
    {
        Type = type;
        EnergyCost = cost;
    }
    public CardData(CardType type, int cost, int range, bool aoe) : this(type, cost)
    {
        Type = type;
        EnergyCost = cost;
        Range = range;
        AoE = aoe;
    }
    public void SetHitEnemies(List<CardAttackData> list)
    {
        HitEnemies = list;
    }
}
public abstract class CardScript : MonoBehaviour            //Base card class that contains various coroutine animations and mouse events
{
    public CardEnum CardName;                                      //each card has a unique id #
    public int rarity;                                  //not implemented yet
    public int defaultCost;                             //default energy cost of card
    [System.NonSerialized] public int cost;             //current energy cost of card
    public SerializedCardInfo CardGameObjects;                 //Data class of serialized references to card sprites    
    private protected CameraManagerScript camScript;
    private protected PlayerCombatScript playerCombat;
    private protected BuffData playerBuffs;
    private protected DeckScript deck;
    private protected AudioManager audioManager;
    private protected TraitManagerScript traitManager;
    private protected CardMenuItem cardMenu;            //Reference to a card select menu the card is in if applicable
    private protected ShopScript shop;                  //Reference to a shop the card is in if applicable

    private protected AudioSource audioSource;

    public abstract CardType Type { get; }              //Abstract card type, must be overidden in child classes
    public CardData cardData;
    [System.NonSerialized] public bool isSelected;      //Bool representing if the card is currently selected or not

    private protected StaticCardInfo staticCardInfo;    //Data class containing default colors and icons for card types
    private protected List<IEnumerator> currentMovementAnims = new List<IEnumerator>();    //List of movement coroutines currently running
    private protected IEnumerator currentFade;      //Current fade coroutine
    private protected bool playable = true;         //boolean that determines if the card is playable (not playing in shops or during card selection)
    private protected bool returning;               //true if the card is returning to its normal location in the hand
    private protected bool moving;                  //true if the card is moving (in coroutine)
    private protected bool mouseOver;               //true if the mouse is currently over the card
    private protected bool drag;                    //true if the card is selected and the mouse is dragging
    private protected Vector3 anchorPos;            //The normal position of the card in the hand
    private protected float anchorAngle;            //The normal angle of the card in the hand
    private protected Vector3 liftPos;              //The position the card is raised to when hovered
    private protected int handIndex;                //Index of the card in the hand (relative to other cards, this determines sorting order)
    private protected bool OverCombatable;          //remove ?
    public virtual void Setup()                             //Setup function called when the card is instantiated
    {
        CardGameObjects = GetComponent<SerializedCardInfo>();
        CardGameObjects.SetupFadeMaterial();
        camScript = Globals.CameraManager;
        playerCombat = Globals.PlayerCombat;
        playerBuffs = playerCombat.Buffs;
        deck = Globals.Deck;
        audioManager = Globals.AudioManager;
        traitManager = Globals.TraitManager;
        cost = defaultCost;
        cardData = new CardData(Type, cost);
        staticCardInfo = CardGameObjects.colors;
        audioSource = GetComponentInChildren<AudioSource>();
        CardGameObjects.cardTypeImage.sprite = staticCardInfo.CardTypeIcons[Type];
        UpdateValues();
        UpdateDescription(false);
    }
    public virtual void Activate()      
    {
        gameObject.SetActive(true);
        CardGameObjects.SetFadeMaterialProgress(0);
    }
    public virtual void UpdateValues()          // Updates the values of data to match the card's current values. Is overridden in child classes to include range and more
    {
        cardData.EnergyCost = cost;
    }
    public virtual void UpdateDescription(bool sucessfulCheck)      // Updates the text of the card itself, is also overridden in child classes
    {
        CardGameObjects.costText.text = cardData.EnergyCost.ToString();
        CardGameObjects.rangeText.text = cardData.Range.ToString();
    }
    public void SetCardMenuItem(CardMenuItem menuItem)      // Sets a reference to a card select menu item and makes the card not playable
    {
        cardMenu = menuItem;
        SetPlayable(false);
    }
    public void SetShop(ShopScript inShop)      // Sets a reference to a shop and makes the card not playable
    {
        shop = inShop;
        SetPlayable(false);
    }
    public abstract bool Check(Vector3Int targetCell);      //Abstract method that is called to check whether a card can be played
    public virtual bool HoverCheck(Vector3Int targetCell)   //Overridable method that is called when the card is selected and hovering over a cell
    {                                                       //In child classes, this method shows the hp enemies will lose if played for example
        return Check(targetCell);
    }
    private protected virtual IEnumerator PlayEffect()      //overridable method that is the actual effect of the card when played
    {
        if (!playerCombat.TurnOver)
        {
            UIManagerScript.OpenHand();
        }
        traitManager.OnPlay(cardData);
        playerCombat.CurrentCard = null;

        yield return null;
    }
    public void ResetAll()      //Resets card alpha and scale (size)
    {
        CardGameObjects.SetAlphaAll(1);
        transform.localScale = new Vector3(1, 1, 1);
    }
    public virtual void OnPointerExit()     //Event method called when the mouse leaves the card
    {
        mouseOver = false;
        if (playerCombat.CanSelectCard && !isSelected && !returning && playable)
        {
            StartCoroutine(CheckMoveAnim(MoveFromCurrent(anchorPos, anchorAngle, 0.1f)));
        }
        else if (isSelected && !OverCombatable)
        {
            StartCoroutine(CheckFade(CardGameObjects.cardBG.color.a, 1, 0.25f, false));
        }
        else if (!playable)
        {
            if (cardMenu != null)
            {
                StartCoroutine(Globals.UnscaledInterpVector3(transform.localScale, new Vector3(1, 1, 1), 0.1f, scale => transform.localScale = scale));

            }
            if (shop != null)
            {
                StartCoroutine(Globals.UnscaledInterpVector3(transform.localScale, new Vector3(1, 1, 1), 0.1f, scale => transform.localScale = scale));
            }
        }
    }
    public virtual void OnPointerEnter()        //Event method called when the mouse enters the card
    {
        mouseOver = true;
        if (!playable)
        {
            if (cardMenu != null)
            {
                StartCoroutine(Globals.UnscaledInterpVector3(transform.localScale, new Vector3(1.1f, 1.1f, 1), 0.1f, scale => transform.localScale = scale));
            }
            if (shop != null)
            {
                StartCoroutine(Globals.UnscaledInterpVector3(transform.localScale, new Vector3(1.1f, 1.1f, 1), 0.1f, scale => transform.localScale = scale));
            }
        }
    }
    public void OnDrag()        //Event method called when the mouse is dragging the card
    {
        if (playerCombat.CanSelectCard || isSelected)
        {
            if (!returning)
            {
                drag = true;
            }
        }
    }
    public void OnPointerDown()     //Event method called when the mouse is clicked on the card
    {
        if (playerCombat.CanSelectCard && !returning)
        {
            if (!mouseOver)
            {
                OnPointerEnter();
            }
            drag = false;
            StartCoroutine(Select());
        }
        else if (!playable)
        {
            if (cardMenu != null)
            {
                cardMenu.StartCoroutine(cardMenu.SelectCard(this));
            }
            if (shop != null)
            {
                shop.StartCoroutine(shop.TryPurchaseCard(this));
            }
        }
    }
    public virtual void OnPointerUp()       //Event method called when the mouse is released (mouse does not have to be on the card)
    {
        if (isSelected && drag)
        {
            drag = false;
            StartCoroutine(PlayCard());
        }
    }
    public void SetPlayable(bool inPlayable)    
    {
        playable = inPlayable;
    }
    public void SetAnchor(Vector3 inPos, float inAngle)     //Sets the default position of the card in the hand, and sets the liftPos based off of it
    {
        anchorPos = inPos;
        anchorAngle = inAngle;
        liftPos = new Vector3(anchorPos.x, UIManagerScript.Hand.RectTransform.rect.position.y + UIManagerScript.Hand.RectTransform.rect.height*(2f/3));
    }
    public void SetHandIndex(int inHandIndex)
    {
        handIndex = inHandIndex;
    }
    public IEnumerator CheckMoveAnim(IEnumerator inAnim)    //Coroutine that stops all previously running movement coroutines and then executes the new one
    {
        if (currentMovementAnims.Count > 0)
        {
            foreach (IEnumerator anim in currentMovementAnims)
            {
                StopCoroutine(anim);
            }
        }
        currentMovementAnims.Clear();
        currentMovementAnims.Add(inAnim);
        yield return inAnim;
    }
    public IEnumerator AnimateReturn(float duration)    //Coroutine that returns the card to its default position
    {
        returning = true;
        moving = false;
        transform.SetParent(UIManagerScript.Hand.transform);
        duration *= Mathf.Abs(Mathf.InverseLerp(0, 500, Vector3.Distance(transform.localPosition, anchorPos)));
        IEnumerator rotate = Globals.InterpAngle(transform.eulerAngles.z, anchorAngle, duration, false, ang => transform.eulerAngles = new Vector3(0, 0, ang));
        StartCoroutine(rotate);
        currentMovementAnims.Add(rotate);
        yield return Globals.InterpVector3(transform.localPosition, anchorPos, duration, Globals.AnimationCurves.IncEaseIn, newPos => transform.localPosition = newPos);
        returning = false;
    }
    private protected IEnumerator MoveFromCurrent(Vector3 posB, float angle, float duration)    // Coroutine that moves the card from its current position to a new one
    {
        moving = true;
        returning = false;
        IEnumerator rotate = Globals.InterpAngle(transform.eulerAngles.z, angle, duration, false, ang => transform.eulerAngles = new Vector3(0, 0, ang));
        StartCoroutine(rotate);
        currentMovementAnims.Add(rotate);
        yield return Globals.InterpVector3(transform.localPosition, posB, duration, Globals.AnimationCurves.IncEaseIn, newPos => transform.localPosition = newPos);
        moving = false;
    }
    private protected IEnumerator Select()      //Coroutine called when the card is selected (only called in OnPointerDown)
    {
        Vector3 newPos = UIManagerScript.Hand.transform.TransformPoint(new Vector3(0, liftPos.y));
        playerCombat.CanSelectCard = false;
        playerCombat.SelectedCard = this;
        UIManagerScript.CloseHand();
        camScript.ActivateSelectedCam();
        transform.SetParent(UIManagerScript.Canvas.transform);
        StartCoroutine(CheckMoveAnim(MoveFromCurrent(UIManagerScript.Canvas.transform.InverseTransformPoint(newPos), 0, 0.25f)));
        yield return Globals.FixedUpdate;
        isSelected = true;
        while (isSelected)
        {
            if (Input.GetMouseButton(1))
            {
                Deselect();
                break;
            }
            yield return Globals.FixedUpdate;
        }
    }
    private protected virtual void Deselect()       //Method called when the card is deselected
    {
        StartCoroutine(CheckFade(CardGameObjects.cardBG.color.a, 1, 0.25f, false));
        UIManagerScript.OpenHand();
        UpdateDescription(false);
        StartCoroutine(CheckMoveAnim(AnimateReturn(0.25f)));
        camScript.CardDeselected();
        isSelected = false;
        playerCombat.CanSelectCard = true;
        playerCombat.SelectedCard = null;
        audioSource.pitch = 1;
        audioSource.PlayOneShot(audioManager.GetClip(AudioManager.UISFXEnum.Woosh));
    }
    public IEnumerator CheckFade(float valA, float valB, float duration, bool unscaled) //Stops the current fade animation and plays a new one
    {
        if (currentFade != null)
        {
            StopCoroutine(currentFade);
        }
        currentFade = Fade(valA, valB, duration, unscaled);
        yield return StartCoroutine(currentFade);
        currentFade = null;
    }
    private IEnumerator Fade(float valA, float valB, float duration, bool unscaled) //Fades the transparency of the card from A to B
    {
        if (unscaled)
        {
            yield return Globals.UnscaledInterpFloat(valA, valB, duration, false, a => CardGameObjects.SetAlphaAll(a));
        }
        else
        {
            yield return Globals.InterpFloat(valA, valB, duration, a => CardGameObjects.SetAlphaAll(a));
        }
    }
    private IEnumerator FadeOut(float duration, bool unscaled) //Uses the fadeMaterial to fade out the card with a burn effect
    {
        if (unscaled)
        {
            yield return Globals.UnscaledInterpFloat(0, 1, duration, false, a => CardGameObjects.SetFadeMaterialProgress(a));
        }
        else
        {
            yield return Globals.InterpFloat(0, 1, duration, a => CardGameObjects.SetFadeMaterialProgress(a));
        }
    }
    private protected virtual IEnumerator PlayCard()    //Coroutine used when the card is played
    {
        Vector3Int targetCell = playerCombat.CurrentRoom.GetFloor().WorldToCell(Globals.ScreenToWorld());
        if (cost <= playerCombat.Energy && Check(targetCell))
        {
            isSelected = false;
            playerCombat.SelectedCard = null;
            playerCombat.LoseEnergy(cost);
            camScript.CardDeselected();
            StartCoroutine(CheckFade(CardGameObjects.cardBG.color.a, 0, 0.5f, false));
            yield return FadeOut(0.5f, false);
            playerCombat.CurrentCard = playerCombat.StartCoroutine(PlayEffect());
            OnPlayComplete();
        }
        else
        {
            Deselect();
        }
        playerCombat.CanSelectCard = true;
    }
    private protected virtual void OnPlayComplete()     //Method called after the card is played
    {
        deck.Discard(this);
        deck.StartCoroutine(deck.CheckAnim(deck.FanHand(0)));
        ResetAll();
        currentMovementAnims.Clear();
        mouseOver = false;
        deck.UpdateAllCards();
        StopAllCoroutines();
    }
    private void Update() //Various animations
    {
        if (playable && !returning)
        {
            if (isSelected && currentFade == null)                                          //All when the card is selected
            {
                Vector3[] temparr = new Vector3[4];
                GetComponent<RectTransform>().GetWorldCorners(temparr);
                if (mouseOver && !moving)
                {
                    StartCoroutine(CheckFade(CardGameObjects.cardBG.color.a, 0.25f, 0.25f, false)); //If the mouse is over the card, make the card semi-transparent to see better
                }
                else if (Physics2D.OverlapBox(Camera.main.ScreenToWorldPoint(transform.position), Camera.main.ScreenToWorldPoint(temparr[2]) - Camera.main.ScreenToWorldPoint(temparr[0]), 0, Globals.CombatableMask) != null)
                {       
                    OverCombatable = true;
                    StartCoroutine(CheckFade(CardGameObjects.cardBG.color.a, 0.25f, 0.25f, false)); //If the card is over a unit, make the card semi-transparent
                }
                else
                {
                    OverCombatable = false;
                    if (!mouseOver)
                    {
                        StartCoroutine(CheckFade(CardGameObjects.cardBG.color.a, 1, 0.25f, false)); //If card isn't touching anything, make the card fully opaque again
                    }
                }
            }
            else if (mouseOver && !moving && !isSelected && playerCombat.CanSelectCard && transform.localPosition != liftPos) //If the mouse is hovered over the card (but the card is not selected), lift the card
            {
                audioSource.pitch = Globals.random.Next(90, 110) / 100f;
                audioSource.PlayOneShot(audioManager.GetClip(AudioManager.UISFXEnum.ShortSlide));
/*                audioManager.PlaySoundEffectGlobal(audioManager.GetClip(AudioManager.UISoundEffectEnum.Slide));*/
                StartCoroutine(CheckMoveAnim(MoveFromCurrent(liftPos, 0, 0.2f)));
                deck.ReorderHandSiblings(handIndex);
            }
        }
        if (isSelected && !drag && Input.GetMouseButtonDown(0))     //If the card is selected with a click and the player clicks again, play the card
        {
            StartCoroutine(PlayCard());
        }
    }
}
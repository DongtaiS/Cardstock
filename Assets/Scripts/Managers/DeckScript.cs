using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
public enum CardEnum {  Dash, Attack, GreatCleave, BladeCyclone, PikeThrust, Pierce, Lunge, 
                        DaggerToss, ForcefulStrike, Quake, Impale, Backtrack, AuraofMight, 
                        TacticalRetreat, OverheadSlam }
public class DeckScript : MonoBehaviour
{
    public List<AssetReference> AllCardsList;
    [SerializeField] private int handSizeLimit;
    public Dictionary<CardEnum, CardScript> AllCardsDict = new Dictionary<CardEnum, CardScript>();
    private GameObject deckObj;
    private GameObject combatDeckGO;

    private List<CardScript> deckList = new List<CardScript>();
    private List<CardScript> combatDeck = new List<CardScript>();
    private List<CardScript> hand = new List<CardScript>();
    private List<CardScript> discard = new List<CardScript>();

    private IEnumerator currentAnimation;
    private TraitManagerScript itemManager;
    private void Start()
    {
        combatDeckGO = new GameObject("CombatDeck");
        itemManager = Globals.TraitManager;
        deckObj = new GameObject("Deck");
        foreach (AssetReference card in AllCardsList)
        {
            GameObject go = (GameObject)card.editorAsset;
            CardScript c = go.GetComponent<CardScript>();
            AllCardsDict.Add(c.CardName, c);
        }
        InstantiateCard(CardEnum.Dash, 4);
        InstantiateCard(CardEnum.Attack, 3);
        InstantiateCard(CardEnum.BladeCyclone, 1);
        InstantiateCard(CardEnum.DaggerToss, 1);
        InstantiateCard(CardEnum.GreatCleave, 1);
        InstantiateCard(CardEnum.Impale, 1);
        InstantiateCard(CardEnum.Lunge, 1);
        InstantiateCard(CardEnum.PikeThrust, 1);
        InstantiateCard(CardEnum.Pierce, 1);
        InstantiateCard(CardEnum.Quake, 1);
/*        InstantiateCard(CardEnum.ForcefulStrike, 1);*/
/*        InstantiateCard(CardEnum.AuraofMight, 1);
        InstantiateCard(CardEnum.Backtrack, 1);*/
/*        InstantiateCard(CardEnum.TacticalRetreat, 1);
        InstantiateCard(CardEnum.OverheadSlam, 3);*/
    }
    public void OnCombatStart()
    {
        hand.Clear();
        discard.Clear();
        combatDeck.Clear();
        combatDeck.AddRange(deckList.ConvertAll(card => Instantiate(card).GetComponent<CardScript>()));
        combatDeck.ForEach(card =>
        {
            card.transform.SetParent(combatDeckGO.transform);
            card.transform.localPosition = new Vector3(-1080, 0);
            card.Activate();
            card.Setup();
        });
        ShuffleDeck();
        itemManager.OnCombatStart();
        UpdateAllCards();
    }
    public IEnumerator DrawCard(int amount)
    {
        if (amount > combatDeck.Count)
        {
            ShuffleDiscard();
            combatDeck.AddRange(discard);
            discard.Clear();
        }
        amount = Mathf.Min(amount, Mathf.Min(combatDeck.Count, handSizeLimit - hand.Count));
        for (int i = 0; i < amount; i++)
        {
            CardScript drawnCard = combatDeck[0];
            drawnCard.Activate();
            drawnCard.transform.SetParent(UIManagerScript.Hand.transform, false);
            drawnCard.transform.localPosition = new Vector3(-1080 * 1.5f, UIManagerScript.Hand.RectTransform.rect.height / 3);
            drawnCard.transform.SetAsFirstSibling();
            drawnCard.SetHandIndex(hand.Count - 1);
            hand.Add(drawnCard);
            combatDeck.RemoveAt(0);
        }
        UpdateAllCards();
        if (amount > 0)
        {
            yield return CheckAnim(FanHand(amount));
        }
    }
    public IEnumerator FanHand(int amountDrawn)
    {
        int handCount = hand.Count;
        float handSize = (UIManagerScript.Hand.RectTransform.rect.width) * 0.5f;
        float eachPos = Mathf.Min(handSize / handCount, handSize / 4);
        float start = eachPos * ((handCount - 1) / 2f);
        float duration = 1f;
        if (amountDrawn == 0)
        {
            duration = 0.5f;
        }
        float angleRange = (handCount - 1) * 3f;
        Coroutine lastAnim = null;
        for (int i = 0; i < handCount; i++)
        {
            int index = handCount - i - 1;
            CardScript card = hand[i];
            Vector3 target = UIManagerScript.Hand.transform.localPosition;
            lastAnim = StartCoroutine(Move(card, RotatePointAroundPivot(new Vector3(start - (eachPos * i), UIManagerScript.Hand.RectTransform.rect.height / 3), target, new Vector3(0, 0, -angleRange/2 + 3f * i)), -angleRange / 2 + 3f * i, duration));
            if (index < amountDrawn)
            {
                yield return Globals.WaitForSeconds(0.15f);
            }
            hand[i].SetHandIndex(i);
        }
        if (lastAnim != null)
        {
            yield return lastAnim;
        }
    }
    public Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles)
    {
        return Quaternion.Euler(angles) * (point - pivot) + pivot;
    }
    public void ReorderHandSiblings(int index)
    {
        for (int i = hand.Count - 1; i >= 0; i--)
        {
            if (i > index)
            {
                hand[i].transform.SetSiblingIndex(hand.Count - 1 - i);
            }
            else if (i == index)
            {
                hand[i].transform.SetSiblingIndex(hand.Count - 1);
            }
            else
            {
                hand[i].transform.SetSiblingIndex(i);
            }
        }
    }
    public IEnumerator Move(CardScript inCard, Vector3 pos, float angle, float duration)
    {
        inCard.SetAnchor(pos, angle);
        yield return inCard.CheckMoveAnim(inCard.AnimateReturn(duration));
    }
    public void AddToHand(CardScript inCard)
    {
        hand.Add(inCard);
    }
    public void AddToCombatDeck(CardScript inCard)
    {
        combatDeck.Add(inCard);
    }
    public void Discard(CardScript inCard)
    {
        hand.Remove(inCard);
        inCard.transform.SetParent(combatDeckGO.transform, false);
        inCard.gameObject.SetActive(false);
        discard.Add(inCard);
        itemManager.OnDiscard(inCard.cardData);
    }
    public void DiscardHand()
    {
        while(hand.Count > 0)
        {
            Discard(hand[0]);
        }
    }
    public void ShuffleDiscard()
    {
        Globals.Shuffle(discard);
        itemManager.OnDeckShuffle();
    }
    public void ShuffleDeck()
    {
        Globals.Shuffle(combatDeck);
    }
    public void UpdateAllCards()
    {
        foreach (CardScript card in hand)
        {
            card.UpdateValues();
            card.UpdateDescription(false);
        }
        foreach (CardScript card in combatDeck)
        {
            card.UpdateValues();
        }
        foreach (CardScript card in discard)
        {
            card.UpdateValues();
        }
    }
    public IEnumerator CheckAnim(IEnumerator inAnimation)
    {
        if (currentAnimation != null)
        {
            yield return currentAnimation;
        }
        currentAnimation = inAnimation;
        yield return currentAnimation;
    }
    public List<CardScript> GetHand()
    {
        return hand;
    }
    public List<CardScript> GetDiscard()
    {
        return discard;
    }
    public List<CardScript> GetCombatDeck()
    {
        return combatDeck;
    }
    public List<CardScript> GetDeckList()
    {
        return deckList;
    }
    public CardScript GetCard(int index)
    {
        if (index < deckList.Count)
        {
            return deckList[index];
        }
        else
        {
            return null;
        }
    }
    public void AddToDeck(CardScript inCard)
    {
        deckList.Add(inCard);
        inCard.transform.SetParent(deckObj.transform, false);
        inCard.SetPlayable(true);
        inCard.gameObject.SetActive(false);
    }
    public void RemoveCard(CardScript inCard)
    {
        deckList.Remove(inCard);
    }
    private void InstantiateCard(CardEnum card, int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            AddToDeck(Instantiate(AllCardsDict[card], deckObj.transform));
        }
    }
}

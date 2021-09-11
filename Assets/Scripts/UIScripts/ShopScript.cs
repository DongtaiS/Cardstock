using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class ShopScript : MenuScript
{
    private struct ShopItem
    {
        public CardScript card;
        public int price;
        public GameObject priceBanner;
        public ShopItem(CardScript c, int p, GameObject banner)
        {
            card = c;
            price = p;
            priceBanner = banner;
        }
    }
    [SerializeField] private Image mainBG;
    [SerializeField] private List<Transform> commonCardPos;
    [SerializeField] private List<Transform> rareCardPos;
    [SerializeField] private List<Transform> cursedCardPos;
    [SerializeField] private GameObject priceBanner;
    private DeckScript deck;
    private Dictionary<CardScript, ShopItem> cards = new Dictionary<CardScript, ShopItem>();
    private PlayerCombatScript playerCombat;

    private protected override Vector3 closePos { get { return new Vector3(-1920, 0); } }

    private void Start()
    {
        deck = Globals.Deck;
        playerCombat = Globals.PlayerCombat;
        GenerateCards();
    }
    public override IEnumerator OpenAnim()
    {
        Globals.Pause(1);
        yield return base.OpenAnim();
    }
    public override IEnumerator CloseAnim()
    {
        Globals.Unpause(1);
        yield return base.CloseAnim();
    }
    public IEnumerator TryPurchaseCard(CardScript card)
    {
        if (playerCombat.Gold > cards[card].price)
        {
            playerCombat.AddGold(-cards[card].price);
            yield return card.CheckFade(card.CardGameObjects.cardBG.color.a, 0, 0.5f, true);
            Destroy(cards[card].priceBanner);
            card.ResetAll();
            deck.AddToDeck(card);
            cards.Remove(card);
        }
        else
        {
            //play some animation
        }
    }
    public void GenerateCards()
    {
        for (int i = 0; i < commonCardPos.Count; i++)
        {
            CardScript card = deck.AllCardsList[Globals.random.Next(0, deck.AllCardsList.Count)].InstantiateAsync(commonCardPos[i].transform).WaitForCompletion().GetComponent<CardScript>();
            card.transform.position = commonCardPos[i].position;
            card.Setup();
            card.SetShop(this);
            int price = GeneratePrice();
            GameObject banner = Instantiate(priceBanner, card.transform);
            banner.GetComponentInChildren<TextMeshProUGUI>().text = price.ToString();
            cards.Add(card ,new ShopItem(card, price, banner));
        }
        // do the same for rare/cursed cards
    }
    private protected virtual int GeneratePrice()
    {
        return Globals.RoundToNearestInt(Globals.random.Next(50, 100));
    }
    private void OnDestroy()
    {
/*        foreach(ShopItem card in cards.Values)
        {
            Destroy(card.priceBanner);
            Destroy(card.card);
        }*/
    }

    public override void OnEscape()
    {
        Close();
    }
}

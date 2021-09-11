using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
public class UIManagerScript : MonoBehaviour
{
    [SerializeField] private Image serializedDarkBG;
    [SerializeField] private Image serializedEndTurnButton;
    [SerializeField] private GameObject serializedCombatUIParent;
    [SerializeField] private MenuScript pauseMenu;
    private static List<MenuScript> activeMenus = new List<MenuScript>();
    private static UIManagerScript instance;
    public static Canvas Canvas { get; private set; }
    public static BattleEndMenu battleEndMenu { get; private set; }
    public static HandObjectScript Hand { get; private set; }
    public static Image EndTurnButton;
    private static HealthScript healthText;
    private static EnergyScript energyText;
    private static GoldScript goldText;
    private static DeckScript deck;
    private static CardViewMenu deckView;
    private static List<Image> CombatUIImages = new List<Image>();
    private static List<TextMeshProUGUI> CombatUIText = new List<TextMeshProUGUI>();
    private static Image darkBG;
    private static GameObject combatUIParent;

    private static int savedDarkBGOrder = 0;
    private void Awake()
    {
        if (instance == null || instance == this)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        darkBG = serializedDarkBG;
        EndTurnButton = serializedEndTurnButton;
        Canvas = GameObject.FindGameObjectWithTag("Canvas").GetComponent<Canvas>();
        deck = Globals.Deck;
        Hand = Canvas.GetComponentInChildren<HandObjectScript>();
        healthText = Canvas.GetComponentInChildren<HealthScript>();
        energyText = Canvas.GetComponentInChildren<EnergyScript>();
        goldText = Canvas.GetComponentInChildren<GoldScript>();
        battleEndMenu = Canvas.GetComponentInChildren<BattleEndMenu>();
        deckView = Canvas.GetComponentInChildren<CardViewMenu>();
        serializedCombatUIParent.GetComponentsInChildren(CombatUIImages);
        serializedCombatUIParent.GetComponentsInChildren(CombatUIText);
        StartCoroutine(HideCombatUI(0));
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (activeMenus.Count > 0)
            {
                activeMenus[activeMenus.Count - 1].OnEscape();
            }
            else
            {
                pauseMenu.OnEscape();
            }
        }
    }
    public static void ActivateMenu(MenuScript menu)
    {
        activeMenus.Add(menu);
    }
    public static void DeactivateMenu(MenuScript menu)
    {
        activeMenus.Remove(menu);
    }
    public static Image GetDarkBG()
    {
        return darkBG;
    }
    public static void SetDarkBGOrder(int newindex)
    {
        savedDarkBGOrder = darkBG.transform.GetSiblingIndex();
        darkBG.transform.SetSiblingIndex(newindex);
    }
    public static void ReturnDarkBGOrder()
    {
        darkBG.transform.SetSiblingIndex(0);
    }
    public static void ViewDiscard()
    {
        deckView.CreateCards(deck.GetDiscard());
        deckView.Open();
    }
    public static void ViewCombatDeck()
    {
        deckView.CreateCards(deck.GetCombatDeck());
        deckView.Open();
    }
    public static void ViewDeck()
    {
        deckView.CreateCards(deck.GetDeckList());
        deckView.Open();
    }
    public static IEnumerator OnPlayerTurnStart()
    {
        OpenHand();
        float alpha = 0;
        instance.StartCoroutine(Globals.InterpFloat(0, 1, 0.25f, a => alpha = a));
        while (alpha <= 1)
        {
            foreach(Image img in CombatUIImages)
            {
                img.color = Globals.ChangeColorAlpha(img.color, alpha);
            }
            foreach(TextMeshProUGUI tmp in CombatUIText)
            {
                tmp.color = Globals.ChangeColorAlpha(tmp.color, alpha);
            }
            if (alpha == 1)
            {
                break;
            }
            yield return Globals.FixedUpdate;
        }
    }
    public static IEnumerator HideCombatUI(float duration = 0.25f)
    {
        CloseHand();
        float alpha = 0;
        instance.StartCoroutine(Globals.InterpFloat(1, 0, duration, a => alpha = a));
        while (alpha >= 0)
        {
            foreach (Image img in CombatUIImages)
            {
                img.color = Globals.ChangeColorAlpha(img.color, alpha);
            }
            foreach (TextMeshProUGUI tmp in CombatUIText)
            {
                tmp.color = Globals.ChangeColorAlpha(tmp.color, alpha);
            }
            if (alpha == 0)
            {
                break;
            }
            yield return Globals.FixedUpdate;
        }
    }
    public static void OpenHand()
    {
        instance.StartCoroutine(Hand.OpenHand());
    }
    public static void CloseHand()
    {
        instance.StartCoroutine(Hand.CloseHand());
    }
    public static void UpdateHealth(int inHp)
    {
        healthText.SetHp(inHp);
    }
    public static void UpdateEnergy(int inEnergy)
    {
        energyText.SetEnergy(inEnergy);
    }
    public static void UpdateGold(int inGold)
    {
        goldText.SetGold(inGold);
    }
}

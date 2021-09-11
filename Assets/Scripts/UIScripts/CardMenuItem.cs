using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using TMPro;

public class CardMenuItem : BattleEndMenuItem
{
    [SerializeField] private int cardAmount;
    [SerializeField] private GameObject returnButton;
    private List<CardScript> cardList = new List<CardScript>();
    private List<AssetReference> allCards;
    private bool cardsCreated;
    private List<IEnumerator> currentAnim = new List<IEnumerator>();
    private GameObject cardParent;
    private void Start()
    {
        Setup();
    }
    public void OnReturnButtonClick()
    {
        StartCoroutine(FadeOutAll(0.25f));
    }
    public override void OnClick()
    {
        if (!cardsCreated)
        {
            CreateCards();
            cardsCreated = true;
        }
        else
        {
            StartCoroutine(FadeInAll(0.5f));
        }
    }
    public IEnumerator SelectCard(CardScript card)
    {
        ClearAnim();
        yield return FadeOutAll(0.5f);
        for (int i = 0; i < cardList.Count; i++)
        {
            if (cardList[i] != card)
            {
                Destroy(cardList[i].gameObject);
            }
        }
        Globals.Deck.AddToDeck(card);
        card.ResetAll();
        cardList.Clear();
        StartCoroutine(FadeOut(1f));
    }
    private IEnumerator FadeOutAll(float duration)
    {
        battleEndMenu.innerDarkBG.raycastTarget = false;
        battleEndMenu.innerDarkBG.transform.SetAsFirstSibling();
        returnButton.SetActive(false);
        ClearAnim();
        currentAnim.Add(FadeOutCards(duration));
        currentAnim.Add(FadeButton(1, 0, duration));
        yield return StartAnim();
        cardParent.transform.SetAsFirstSibling();
    }
    private IEnumerator FadeInAll(float duration)
    {
        battleEndMenu.innerDarkBG.raycastTarget = true;
        battleEndMenu.innerDarkBG.transform.SetAsLastSibling();
        returnButton.SetActive(true);
        returnButton.transform.SetAsLastSibling();
        cardParent.transform.SetAsLastSibling();
        ClearAnim();
        currentAnim.Add(FadeInCards(duration));
        currentAnim.Add(FadeButton(0, 1, duration));
        yield return StartAnim();
    }
    private IEnumerator FadeOutCards(float duration)
    {
        for (int i = 1; i < cardList.Count; i++)
        {
            StartCoroutine(cardList[i].CheckFade(1, 0, duration, true));
        }
        yield return cardList[0].CheckFade(1, 0, duration, true);
    }
    private IEnumerator FadeInCards(float duration)
    {
        for (int i = 1; i < cardList.Count; i++)
        {
            StartCoroutine(cardList[i].CheckFade(0, 1, duration, true));
            cardList[i].transform.SetAsLastSibling();
        }
        cardList[0].transform.SetAsLastSibling();
        yield return cardList[0].CheckFade(0, 1, duration, true);
    }
    private void CreateCards()
    {
        float width = UIManagerScript.Canvas.GetComponent<RectTransform>().rect.width * 0.75f;
        float itemWidth = width / cardAmount;
        float start = itemWidth * ((cardAmount - 1f) / 2f);
        cardParent = new GameObject("Card parent");
        cardParent.transform.SetParent(UIManagerScript.battleEndMenu.transform, false);
        cardParent.transform.localPosition = new Vector3(0, 0);
        cardParent.transform.SetAsLastSibling();
        for (int i = 0; i < cardAmount; i++)
        {
            CardScript card = allCards[Globals.random.Next(0, allCards.Count)].InstantiateAsync(cardParent.transform).WaitForCompletion().GetComponent<CardScript>();
            card.transform.localPosition = new Vector3(start - itemWidth * i, 0);
            card.Setup();
            card.SetCardMenuItem(this);
            cardList.Add(card);
        }
        StartCoroutine(FadeInAll(0.5f));
    }
    private IEnumerator FadeButton(float alphaA, float alphaB, float duration)
    {
        Image buttonImage = returnButton.GetComponent<Image>();
        float alpha = 0;
        StartCoroutine(Globals.UnscaledInterpFloat(alphaA, alphaB, duration, false, a => alpha = a));
        while (alpha != alphaB)
        {
            buttonImage.color = Globals.ChangeColorAlpha(buttonImage.color, alpha);
            yield return Globals.EndOfFrame;
        }
        buttonImage.color = Globals.ChangeColorAlpha(buttonImage.color, alpha);
    }
    private void OnDestroy()
    {
        Destroy(returnButton.gameObject);
        Destroy(cardParent);
    }
    private void ClearAnim()
    {
        foreach (IEnumerator anim in currentAnim)
        {
            StopCoroutine(anim);
        }
        currentAnim.Clear();
    }
    private IEnumerator StartAnim()
    {
        for (int i = 0; i < currentAnim.Count-1; i++)
        {
            StartCoroutine(currentAnim[i]);
        }
        yield return currentAnim[currentAnim.Count - 1];
    }
    public override void Setup()
    {
        base.Setup();
        allCards = Globals.Deck.AllCardsList;
        returnButton = Instantiate(returnButton, battleEndMenu.transform);
        returnButton.GetComponent<RectTransform>().localScale = new Vector3(0.75f, 0.75f, 0.75f);
        returnButton.transform.localPosition = new Vector3(0, -UIManagerScript.Canvas.GetComponent<RectTransform>().rect.height * (1 / 4f));
        returnButton.GetComponent<Button>().onClick.AddListener(() => OnReturnButtonClick());
        returnButton.GetComponentInChildren<TextMeshProUGUI>().text = "Close";
        returnButton.SetActive(false);
    }
}

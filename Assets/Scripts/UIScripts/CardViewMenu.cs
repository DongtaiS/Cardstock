using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CardViewMenu : MenuScript
{
    [SerializeField] private RectTransform parent;
    private RectTransform parentRect;
    private List<CardScript> cards = new List<CardScript>();
    private protected override Vector3 closePos { get { return new Vector3(1920, 0); } }

    private void Start()
    {
        parentRect = parent.GetComponent<RectTransform>();
    }
    private void Update() //TODO Clean this up
    {
/*        if (parent.rect.Contains(Input.mousePosition + transform.localPosition - new Vector3(Screen.width / 2, Screen.height / 2)))
        {
            float scroll = Input.mouseScrollDelta.y;
            if (scroll > 0 && parent.transform.localPosition.x < 0)
            {
                target.x += Mathf.Min(scroll * 50, -target.x);
            }
            else if (scroll < 0 && parent.transform.localPosition.x > Screen.width - (cards.Count / 2) * 260 - 60)
            {
                target.x += Mathf.Max(scroll * 50, Screen.width - (cards.Count / 2) * 260 - 60 - target.x);
            }
            if (parent.transform.localPosition != target)
            {
                parent.transform.localPosition = Vector3.MoveTowards(parent.transform.localPosition, target, Time.unscaledDeltaTime * Mathf.Abs(parent.transform.localPosition.x - target.x) * 20);
            }
        }*/
    }
    public void CreateCards(List<CardScript> c)
    {
        DeleteCards();
        parentRect.sizeDelta = new Vector2(Mathf.Max(1920, (c.Count / 2) * 260 + 360), parentRect.sizeDelta.y);
        for (int i = 0; i < c.Count; i++)
        {
            CardScript newCard = Instantiate(c[i], parent.transform, false);
            newCard.transform.localPosition = new Vector3((i / 2) * 260 + 180, 190);
            if (i % 2 == 1)
            {
                newCard.transform.localPosition = new Vector3(newCard.transform.localPosition.x, -190);
            }
            newCard.Setup();
            newCard.Activate();
            newCard.SetPlayable(false);
            cards.Add(newCard);
        }
    }
    public void DeleteCards()
    {
        while (cards.Count > 0)
        {
            Destroy(cards[0].gameObject);
            cards.RemoveAt(0);
        }
        parentRect.sizeDelta = new Vector2(1920, parentRect.sizeDelta.y);
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
    public override void OnEscape()
    {
        Close();
    }
}

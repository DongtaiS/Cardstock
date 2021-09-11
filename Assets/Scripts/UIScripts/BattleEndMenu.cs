using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class BattleEndMenu : MenuScript
{
    public enum MenuItems { Gold, Cards }
    public Image innerDarkBG;
    [SerializeField] private Image main;
    [SerializeField] private GenericListToDictionary<MenuItems, BattleEndMenuItem> MenuItemList = new GenericListToDictionary<MenuItems, BattleEndMenuItem>();
    public Dictionary<MenuItems, BattleEndMenuItem> AllMenuItems;
    private List<BattleEndMenuItem> itemList = new List<BattleEndMenuItem>();
    private protected override Vector3 closePos { get { return new Vector3(0, 1080); } }
    private void Awake()
    {
        AllMenuItems = MenuItemList.ToDictionary();
    }
    public override void OnEscape()
    {
        Close();
    }
    public void Fan()
    {
        float bannerSize =  main.GetComponent<RectTransform>().rect.width * 0.8f;
        float itemSize = bannerSize / itemList.Count;
        float start = itemSize * ((itemList.Count - 1) / 2f);
        for (int i = 0; i < itemList.Count; i++)
        {
            BattleEndMenuItem tempItem = itemList[i];
            StartCoroutine(Globals.UnscaledInterpVector3(tempItem.transform.localPosition, new Vector3(start - itemSize * i, 0), openCloseDuration / 2f, Globals.AnimationCurves.IncEaseInOut, newPos => tempItem.transform.localPosition = newPos));
        }
    }
    public Image GetMain()
    {
        return main;
    }
    public void CreateMenuItem(MenuItems item)
    {
        BattleEndMenuItem menuItem = Instantiate(AllMenuItems[item], main.transform).GetComponent<BattleEndMenuItem>();
        menuItem.transform.SetSiblingIndex(2);
        itemList.Add(menuItem);
    }
    public void RemoveMenuItem(BattleEndMenuItem menuItem)
    {
        itemList.Remove(menuItem);
    }
    public override IEnumerator OpenAnim()
    {
        Globals.Pause(2);
        StartCoroutine(Globals.UnscaledInterpFloat(innerDarkBG.color.a, 0.75f, openCloseDuration, false, a => innerDarkBG.color = Globals.ChangeColorAlpha(innerDarkBG.color, a)));
        yield return base.OpenAnim();
        Fan();
    }
    public override IEnumerator CloseAnim()
    {
        Globals.Unpause(2);
        StartCoroutine(Globals.GameState.ChangeState(GameStates.EXPLORE));
        StartCoroutine(Globals.UnscaledInterpFloat(innerDarkBG.color.a, 0, openCloseDuration, false, a => innerDarkBG.color = Globals.ChangeColorAlpha(innerDarkBG.color, a)));
        yield return base.CloseAnim();
        StopAllCoroutines();
        while (itemList.Count > 0)
        {
            Destroy(itemList[0].gameObject);
            itemList.RemoveAt(0);
        }
    }
}

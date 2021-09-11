using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
public class TurnManagerScript : MonoBehaviour
{
    public List<CombatScript> turnList;

    private DeckScript deck;
    private PlayerCombatScript playerCombat;
    private TraitManagerScript itemManager;
    private CameraManagerScript cameraManager;

    private IEnumerator currentAction;
    private void Start()
    {
        deck = Globals.Deck;
        playerCombat = Globals.PlayerCombat;
        itemManager = Globals.TraitManager;
        cameraManager = Globals.CameraManager;
    }
    public IEnumerator StartCombat()
    {
        GetRoomCombatables();
        turnList.Remove(Globals.PlayerCombat);
        turnList.Insert(0, Globals.PlayerCombat);
        int counter = 0;
        deck.OnCombatStart();
        while (turnList.Count > 1)
        {
            cameraManager.ActivateUnitCam(turnList[counter].transform);
            currentAction = turnList[counter].UseTurn();
            yield return StartCoroutine(currentAction);
            counter = counter + 1 >= turnList.Count ? 0 : counter + 1;
        }
    }
    public void RemoveFromCombat(CombatScript combatScript)
    {
        turnList.Remove(combatScript);
        if (turnList.Count <= 1)
        {
            StopAllCoroutines();
            StartCoroutine(EndCombat());
        }
    }
    private IEnumerator EndCombat()
    {
        StartCoroutine(UIManagerScript.HideCombatUI());
        yield return Globals.WaitForSeconds(1f);
        deck.StopAllCoroutines();
        deck.GetHand().ForEach(card => Destroy(card.gameObject));
        playerCombat.OnCombatEnd();
        itemManager.OnCombatEnd();
        UIManagerScript.battleEndMenu.CreateMenuItem(BattleEndMenu.MenuItems.Cards);
        UIManagerScript.battleEndMenu.CreateMenuItem(BattleEndMenu.MenuItems.Gold);
        yield return UIManagerScript.battleEndMenu.OpenAnim();
    }
    private void GetRoomCombatables()
    {
        turnList = playerCombat.CurrentRoom.GetAllCombatables();
    }
}

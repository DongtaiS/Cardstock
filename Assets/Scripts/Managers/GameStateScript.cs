using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum GameStates { COMBAT, EXPLORE, SHOP, MAP };
public class GameStateScript : MonoBehaviour
{
    public GameStates CurrentGameState { get; private set; }
    private TurnManagerScript turnManager;
    private void Start()
    {
        CurrentGameState = GameStates.EXPLORE;
        turnManager = Globals.TurnManager;
    }
    public IEnumerator ChangeState(GameStates inState)
    {
        CurrentGameState = inState;
        switch (inState)
        {
            case GameStates.COMBAT:
                Globals.PlayerCombat.CanSelectCard = true;
                Globals.PlayerCombat.playerMove.enabled = false;
                yield return Globals.WaitForSeconds(2f);
                StartCoroutine(turnManager.StartCombat());
                break;
            case GameStates.EXPLORE:
                StartCoroutine(Globals.AudioManager.FadeMusic(AudioManager.MusicEnum.Explore_DividedWeEcho, 2f));
                Globals.PlayerCombat.playerMove.enabled = true;
                break;
            case GameStates.SHOP:
                break;
            case GameStates.MAP:
                break;
        }
    }
}

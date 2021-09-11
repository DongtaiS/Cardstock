using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StaticCardInfo", menuName = "ScriptableObjects/StaticCardInfo", order = 1)]
public class StaticCardInfo : ScriptableObject
{
    [SerializeField] private GenericListToDictionary<CardType, Sprite> cardTypeIconList = new GenericListToDictionary<CardType, Sprite>();
    public Dictionary<CardType, Sprite> CardTypeIcons = new Dictionary<CardType, Sprite>();
    public Color MovementHighlight;
    public Color Attackhighlight;
    public Color AbilityHighlight;
    private void OnEnable()
    {
        CardTypeIcons = cardTypeIconList.ToDictionary();
    }
}

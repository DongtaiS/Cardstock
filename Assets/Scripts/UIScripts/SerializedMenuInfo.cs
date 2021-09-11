using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SerializedMenuInfo : MonoBehaviour
{
    public Image mainImage;
    public Image highlightImage;
    [ColorUsage(true, true)] public Color HighlightBase;
    [ColorUsage(true, true)] public Color HighlightHover;
    [ColorUsage(true, true)] public Color FadeColor1;
    [ColorUsage(true, true)] public Color FadeColor2;
    public Material mainImageMaterial;
    [SerializeField] private BattleEndMenuItem menuItem;
    public void OnClick()
    {
        menuItem.OnClick();
    }
    public void OnEnter()
    {
        menuItem.OnEnter();
    }
    public void OnExit()
    {
        menuItem.OnExit();
    }
}

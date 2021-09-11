using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class SerializedCardInfo : MonoBehaviour
{
    public GameObject cardSprite;
    public Image splashArt;
    public Image cardBG;
    public Image costBG;
    public Image rangeBackground;
    public Image rangeIcon;
    public Image banner;
    public Image cardTypeImage;
    public TextMeshProUGUI bannerText;
    public TextMeshProUGUI costText;
    public TextMeshProUGUI description;
    public TextMeshProUGUI rangeText;
    public StaticCardInfo colors;
    public Material fadeMaterial;
    [SerializeField] private CardScript parentCard;
    public void SetAlphaAll(float alpha)
    {
        cardBG.color = Globals.ChangeColorAlpha(cardBG.color, alpha);
        costBG.color = Globals.ChangeColorAlpha(costBG.color, alpha);
        banner.color = Globals.ChangeColorAlpha(banner.color, alpha);
        bannerText.color = Globals.ChangeColorAlpha(bannerText.color, alpha);
        costText.color = Globals.ChangeColorAlpha(costText.color, alpha);
        description.color = Globals.ChangeColorAlpha(description.color, alpha);
        splashArt.color = Globals.ChangeColorAlpha(splashArt.color, alpha);
        rangeBackground.color = Globals.ChangeColorAlpha(rangeBackground.color, alpha);
        rangeIcon.color = Globals.ChangeColorAlpha(rangeIcon.color, alpha);
        rangeText.color = Globals.ChangeColorAlpha(rangeText.color, alpha);
        cardTypeImage.color = Globals.ChangeColorAlpha(cardTypeImage.color, alpha);
    }
    public void SetupFadeMaterial()
    {
        fadeMaterial = new Material(fadeMaterial);
        cardBG.material = fadeMaterial;
/*        costBG.material = fadeMaterial;
        banner.material = fadeMaterial;
        bannerText.material = fadeMaterial;
        costText.material = fadeMaterial;
        description.material = fadeMaterial;*/
    }
    public void SetFadeMaterialProgress(float val)
    {
        fadeMaterial.SetFloat("Progress", val);
    }
    public void OnPointerDown()
    {
        parentCard.OnPointerDown();
    }
    public void OnPointerUp()
    {
        parentCard.OnPointerUp();
    }
    public void OnPointerEnter()
    {
        parentCard.OnPointerEnter();
    }
    public void OnPointerExit()
    {
        parentCard.OnPointerExit();
    }
    public void OnDrag()
    {
        parentCard.OnDrag();
    }
}
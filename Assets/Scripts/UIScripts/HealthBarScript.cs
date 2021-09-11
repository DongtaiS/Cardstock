using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class HealthBarScript : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private Slider backSlider;
    [SerializeField] private Image backImage;
    [SerializeField] private Slider frontSlider;
    [SerializeField] private Image frontImage;
    private int maxHp;
    private int hp;
    private IEnumerator currentAnim;
    private IEnumerator FadeInOut;
    public IEnumerator LoseHp(int lostHp)
    {
        hp -= lostHp;
        if (FadeInOut != null)
        {
            StopCoroutine(FadeInOut);
        }
        UpdateHpText(hp);
        if (frontSlider.value != hp)
        {
            StartCoroutine(Globals.InterpFloat(frontSlider.value, hp, CalculateDuration(lostHp), Globals.AnimationCurves.IncEaseInOut, val => frontSlider.value = val));
        }
        backImage.color = Globals.ChangeColorAlpha(backImage.color, 0.5f);
        yield return Globals.InterpFloat(hp + lostHp, hp, CalculateDuration(lostHp), Globals.AnimationCurves.IncEaseInOut, val => backSlider.value = val);
    }
    public IEnumerator GainHp(int gainedHp)
    {
        hp += gainedHp;
        Debug.Log(hp);
        if (FadeInOut != null)
        {
            StopCoroutine(FadeInOut);
        }
        UpdateHpText(hp);
        if (frontSlider.value != hp)
        {
            StartCoroutine(Globals.InterpFloat(frontSlider.value, hp, CalculateDuration(gainedHp), Globals.AnimationCurves.IncEaseInOut, val => frontSlider.value = val));
        }
        backImage.color = Globals.ChangeColorAlpha(backImage.color, 0.5f);
        yield return Globals.InterpFloat(hp - gainedHp, hp, CalculateDuration(gainedHp), Globals.AnimationCurves.IncEaseInOut, val => backSlider.value = val);
        Debug.Log(frontSlider.value);
    }
    public IEnumerator PreviewHp(int lostHp)
    {
        UpdateHpText(hp - lostHp);
        FadeInOut = FadeInOutBack();
        StartCoroutine(FadeInOut);
        yield return Globals.InterpFloat(hp, hp-lostHp, CalculateDuration(lostHp), Globals.AnimationCurves.IncEaseInOut, val => frontSlider.value = val);
    }
    public IEnumerator ResetPreviewHp()
    {
        if (FadeInOut != null)
        {
            StopCoroutine(FadeInOut);
        }
        UpdateHpText(hp);
        yield return Globals.InterpFloat(frontSlider.value, hp, CalculateDuration(hp - frontSlider.value)/2, Globals.AnimationCurves.IncEaseInOut, val => frontSlider.value = val);
    }
    public IEnumerator CheckAnim(IEnumerator inAnim)
    {
        if (currentAnim != null)
        {
            StopCoroutine(currentAnim);
        }
        currentAnim = inAnim;
        yield return StartCoroutine(currentAnim);
    }
    public void SetMaxHp(int newMaxHp)
    {
        maxHp = newMaxHp;
        backSlider.maxValue = maxHp;
        frontSlider.maxValue = maxHp;
        healthText.SetText(hp + "/" + maxHp);
    }
    public void SetAlphaAll(float alpha)
    {
        SetHealthBarAlpha(alpha);
        healthText.color = Globals.ChangeColorAlpha(healthText.color, alpha);
    }
    public void SetHealthBarAlpha(float alpha)
    {
        backImage.color = Globals.ChangeColorAlpha(backImage.color, alpha/2);
        frontImage.color = Globals.ChangeColorAlpha(frontImage.color, alpha);
    }
    public void Setup(int newMaxhp, int newHp)
    {
        maxHp = newMaxhp;
        hp = newHp;
        backSlider.maxValue = maxHp;
        frontSlider.maxValue = maxHp;
        backSlider.value = hp;
        frontSlider.value = hp;
        healthText.SetText(hp + "/" + maxHp);
    }
    private void UpdateHpText(int newHp)
    {
        healthText.SetText(newHp + "/" + maxHp);
    }
    private float CalculateDuration(float hpLost)
    {
        return hpLost / (2*maxHp) + 0.5f;
    }
    private IEnumerator FadeInOutBack()
    {
        float current = frontImage.color.a/2;
        while (1 == 1)
        {
            yield return Globals.InterpFloat(current, 0f, 0.5f, Globals.AnimationCurves.IncEaseInOut, a => backImage.color = Globals.ChangeColorAlpha(backImage.color, a));
            yield return Globals.InterpFloat(0f, current, 0.5f, Globals.AnimationCurves.IncEaseInOut, a => backImage.color = Globals.ChangeColorAlpha(backImage.color, a));
        }
    }
}

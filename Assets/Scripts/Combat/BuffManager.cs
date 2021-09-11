using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D.Animation;

[System.Serializable]
public class BuffManager : MonoBehaviour
{
    [SerializeField] public int width; //TODO: make new row when there are too many buffs
    private CombatScript parent;
    private List<BuffIconScript> activeBuffIcons = new List<BuffIconScript>();
    private List<BuffIconScript> buffsToActivate = new List<BuffIconScript>();
    private IEnumerator currentTranslate;
    private int iconWidth = 4;
    public void Setup(BuffData data, CombatScript p)
    {
        parent = p;
        foreach (Buff buff in data.AllBuffs)
        {
            buff.SetBuffIcon(CreateBuffIcon(buff.Type));
        }
    }
    public BuffIconScript CreateBuffIcon(BuffType type)
    {
        BuffIconScript icon;
        icon = Instantiate(Globals.PrefabManager.BuffIconPrefab, parent.BuffTransform).GetComponent<BuffIconScript>();
        icon.gameObject.SetActive(false);
        SetBuffIconSprite(icon, type);
        return icon;
    }
    public void SetBuffIconSprite(BuffIconScript icon, BuffType type)
    {
        icon.SetSprite(Globals.PrefabManager.BuffIcons[type]);
    }
    public void SetIconMaterial(Material material)
    {
        activeBuffIcons.ForEach(buff => buff.sprite.material = material);
    }
    public void DisableBuff(BuffIconScript inIcon)
    {
        inIcon.Disable();
        int index = activeBuffIcons.IndexOf(inIcon);
        activeBuffIcons.Remove(inIcon);
        StartCoroutine(CheckTranslateAnim(RemoveBuff(inIcon, index)));
    }
    public void ActivateBuff(BuffIconScript inIcon)
    {
        buffsToActivate.Add(inIcon);
        inIcon.Enable();
        StartCoroutine(CheckTranslateAnim(AddBuff()));
    }
    private IEnumerator AddBuff()
    {
        yield return Globals.FixedUpdate;
        for (int i = 0; i < activeBuffIcons.Count; i++)
        {
            if (activeBuffIcons[i].TargetPos.x + buffsToActivate.Count * -iconWidth < -width)
            {
/*                StartCoroutine(activeBuffIcons[i].Fade(1, 0, 0.25f));*/
                float xDiff = activeBuffIcons[i].TargetPos.x + (buffsToActivate.Count+1) * -iconWidth + width;
                activeBuffIcons[i].row++;
                activeBuffIcons[i].transform.localPosition = new Vector3((i + 2) * iconWidth, activeBuffIcons[i].row * iconWidth);
                activeBuffIcons[i].ResetTargetPos();
                StartCoroutine(activeBuffIcons[i].CheckTranslate(new Vector3(xDiff, 0), 0.25f));
            }
            else
            {
                StartCoroutine(activeBuffIcons[i].CheckTranslate(new Vector3(buffsToActivate.Count * -iconWidth, 0), 0.25f));
            }
        }
        for (int i = 0; i < buffsToActivate.Count; i++)
        {
            buffsToActivate[i].transform.localPosition = new Vector3((i + 1) * iconWidth, 0);
            buffsToActivate[i].ResetTargetPos();
            StartCoroutine(buffsToActivate[i].CheckTranslate(new Vector3(buffsToActivate.Count * -iconWidth, 0), 0.25f));
            activeBuffIcons.Add(buffsToActivate[i]);
        }
        buffsToActivate.Clear();
        yield return null;
    }
    private IEnumerator RemoveBuff(BuffIconScript buff, int index)
    {
        for (int i = 0; i < index; i++)
        {
            if (activeBuffIcons[i].TargetPos.x + iconWidth > 0)
            {
                activeBuffIcons[i].row--;
                activeBuffIcons[i].transform.localPosition = new Vector3(-width - iconWidth, activeBuffIcons[i].row * iconWidth);
                activeBuffIcons[i].ResetTargetPos();
            }
            StartCoroutine(activeBuffIcons[i].CheckTranslate(new Vector3(iconWidth, 0), 0.25f));
        }
        yield return null;
    }
    private IEnumerator CheckTranslateAnim(IEnumerator inAnim)
    {
        if (currentTranslate != null)
        {
            StopCoroutine(currentTranslate);
        }
        currentTranslate = inAnim;
        yield return currentTranslate;
    }
}
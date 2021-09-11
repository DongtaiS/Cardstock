using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MenuScript : MonoBehaviour
{
    [SerializeField] private protected float openCloseDuration;
    [System.NonSerialized] public bool IsOpen;
    private protected virtual Vector3 openPos { get { return new Vector3(0, 0); } }
    private protected abstract Vector3 closePos { get; }
    private protected IEnumerator currentAnim;
    public abstract void OnEscape();
    public virtual void Open()
    {
        StartCoroutine(OpenAnim());
    }
    public virtual void Close()
    {
        StartCoroutine(CloseAnim());
    }
    public virtual IEnumerator OpenAnim()
    {
        IsOpen = true;
        UIManagerScript.ActivateMenu(this);
        UIManagerScript.GetDarkBG().raycastTarget = true;
        UIManagerScript.SetDarkBGOrder(transform.GetSiblingIndex() - 1);
        currentAnim = Globals.CheckAnim(currentAnim, MoveMenu(closePos, openPos, openCloseDuration), this);
        yield return currentAnim;
    }
    public virtual IEnumerator CloseAnim()
    {
        UIManagerScript.DeactivateMenu(this);
        IsOpen = false;
        UIManagerScript.ReturnDarkBGOrder();
        currentAnim = Globals.CheckAnim(currentAnim, MoveMenu(openPos, closePos, openCloseDuration), this);
        UIManagerScript.GetDarkBG().raycastTarget = false;
        yield return currentAnim;
    }
    private IEnumerator MoveMenu(Vector3 posA, Vector3 posB, float duration)
    {
        duration *= Vector3.Distance(posB, transform.localPosition) / Vector3.Distance(posB, posA);
        yield return Globals.UnscaledInterpVector3(transform.localPosition, posB, duration, Globals.AnimationCurves.IncEaseInOut, pos => transform.localPosition = pos);
    }
}

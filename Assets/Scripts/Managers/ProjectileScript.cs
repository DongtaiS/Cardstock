using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ProjectileTypes { Dagger }

public class ProjectileScript : BasicSpriteObject
{   
    public IEnumerator MoveTo(Vector3 start, Vector3 target, float duration, int direction)
    {
        Vector3 temp = spriteContainer.localEulerAngles;
        temp.z = Globals.DirectionToRotation(direction);
        spriteContainer.localEulerAngles = temp;
        yield return Globals.InterpVector3(start, target, duration, pos => transform.localPosition = pos);
    }
    public T Cast<T>()
    {
        if (this is T)
        {
            return (T)(object)this;
        }
        else
        {
            return default;
        }
    }
}

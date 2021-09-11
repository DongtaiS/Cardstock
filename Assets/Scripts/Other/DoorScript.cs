using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorScript : MonoBehaviour, IInteractable
{
    public Transform LeftCorner;
    [System.NonSerialized] public SpriteRenderer sprite;
    private int direction;
    public void Setup(int dir)
    {
        direction = dir;
        sprite = GetComponent<SpriteRenderer>();
        sprite.material = new Material(sprite.material);
        sprite.material.SetFloat("MinValue", 0.4f);
        if (direction < 2)
        {
            sprite.material.SetFloat("Progress", 0);
            SetColor(Globals.ChangeColorAlpha(sprite.color, 1f));
        }
        else
        {
            sprite.material.SetFloat("Progress", 1f);
            SetColor(Globals.ChangeColorAlpha(sprite.color, 0.5f));
        }
    }
    public void SetColor(Color newColor)
    {
        sprite.color = newColor;
    }
    public IEnumerator Open()
    {
        Vector3 startPos = transform.position;
        Vector3 targetPos = new Vector3(transform.position.x, transform.position.y + 50);
        float startTime = Time.time;
        float t = 0;
        float alpha = sprite.color.a;
        while (t < 1)
        {
            t = (Time.time - startTime) / 0.5f;
            transform.position = Vector3.Lerp(startPos, targetPos, t);
            sprite.color = Globals.ChangeColorAlpha(sprite.color, Mathf.Lerp(alpha, 0, t));
            if (t >= 1)
            {
                break;
            }
            yield return Globals.FixedUpdate;
        }
        Destroy(gameObject);
    }
    public void Interact(RoomScript room)
    {
        room.OpenDoor(this);
    }
}

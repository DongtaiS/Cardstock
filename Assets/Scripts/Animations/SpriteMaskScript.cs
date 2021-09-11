using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteMaskScript : MonoBehaviour
{
    private List<SpriteMask> masks = new List<SpriteMask>();
    private List<SpriteRenderer> sprites = new List<SpriteRenderer>();
    void Start()
    {
        GetComponentsInChildren(masks);
        foreach(SpriteMask mask in masks)
        {
            sprites.Add(mask.GetComponent<SpriteRenderer>());
        }
    }
    public void LateUpdate()
    {
        for (int i = 0; i < masks.Count; i++)
        {
            if (masks[i].sprite != sprites[i].sprite)
            {
                masks[i].sprite = sprites[i].sprite;
            }
        }
    }
}

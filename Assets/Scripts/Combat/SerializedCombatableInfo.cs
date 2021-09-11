using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
public class SerializedCombatableInfo : MonoBehaviour
{
    [SerializeField] public Transform BuffTransform;
    [SerializeField] public Material Dither;
    [SerializeField] public Material DefaultMaterial;
    [SerializeField] public BuffManager BuffManager;
    [SerializeField] public SpriteRenderer MainSprite;
    [SerializeField] public AudioSource Audio;
    [System.NonSerialized] public List<SpriteRenderer> Sprites = new List<SpriteRenderer>();
    public void Setup()
    {
        BuffManager = GetComponent<BuffManager>();
        Dither = new Material(Dither);
        Dither.SetFloat("Progress", 0);
        DefaultMaterial = new Material(DefaultMaterial);
    }
}

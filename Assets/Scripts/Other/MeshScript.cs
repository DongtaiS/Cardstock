using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Experimental.Rendering.Universal;
public class MeshScript : MonoBehaviour
{
    [SerializeField] private Sprite sprite;
    private Mesh mesh;
    private void Start()
    {
        mesh = new Mesh();
        mesh.vertices = Array.ConvertAll(sprite.vertices, i => (Vector3)i);
        mesh.uv = sprite.uv;
        mesh.triangles = Array.ConvertAll(sprite.triangles, i => (int)i);
        GetComponent<MeshFilter>().mesh = mesh;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseTarget : MonoBehaviour
{
    void Update()
    {
        Debug.DrawLine(Vector3.zero, transform.position, Color.red);
    }
}

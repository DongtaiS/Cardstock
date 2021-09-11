using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AnimationCurves", menuName = "ScriptableObjects/AnimationCurves", order = 1)]
public class AnimationCurvesSO : ScriptableObject
{
    public AnimationCurve IncLinear;
    public AnimationCurve IncEaseIn;
    public AnimationCurve IncEaseOut;
    public AnimationCurve IncEaseInOut;
    public AnimationCurve DecLinear;
    public AnimationCurve DecEaseIn;
    public AnimationCurve DecEaseOut;
    public AnimationCurve DecEaseInOut;
}


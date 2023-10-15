using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DefaultProfile", menuName = "Blobby/Player/Profile")]
public class PlayerProfile : ScriptableObject {

    [Tooltip("Thresholds have to be in ascending order.")]
    public List<ReleaseLevel> ReleaseLevels;
    public float ChargeForce;
    public float InflationLevel;
    public bool CanBreak;
    public bool CanFloat;

    [Space(5)]
    public PointMassesController.SoftBodyConfig SoftBodyConfig;
}

[Serializable]
public struct ReleaseLevel {
    [Range(0f, 1f)] public float Threshold;
    public float Strength;
}
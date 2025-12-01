using System;
using UnityEngine;

[Serializable]
public struct UpgradeTier
{
    [Tooltip("The money cost to unlock this specific tier.")]
    public float Cost;

    [Tooltip("The amount by which the stat is increased at this tier.")]
    public float ValueIncrease;
}
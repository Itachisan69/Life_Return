using UnityEngine;

[CreateAssetMenu(fileName = "Vacuum Stats", menuName = "EcoVac/Vacuum Stats")]
public class VacuumStats : ScriptableObject
{
    [Header("Suction Power")]
    [Tooltip("Base suction strength (upgradable)")]
    public float suctionPower = 10f;

    [Header("Detection")]
    public float detectionRange = 15f;
    public LayerMask trashLayer;

    [Header("Suction Cone")]
    public float coneRadius = 3f;
    public float coneAngle = 30f;
    public float ambientSuctionForce = 2f; // Force applied to non-targeted items in cone

    [Header("Animation Thresholds")]
    [Tooltip("Distance at which item starts rotating toward nozzle")]
    public float rotateDistance = 1.5f;

    [Tooltip("Distance at which item starts shrinking")]
    public float shrinkDistance = 0.5f;

    [Tooltip("Distance at which item gets collected")]
    public float collectDistance = 0.2f;

    [Header("Polish")]
    public AnimationCurve suctionAccelerationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public float magnetSnapSpeed = 20f;
    public float fovIncreaseAmount = 5f;
    public float fovChangeSpeed = 3f;
}
using UnityEngine;
using System.Collections.Generic;

public class TrashSpawnSystem : MonoBehaviour
{
    [System.Serializable]
    public enum TrashRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic
    }

    [System.Serializable]
    public class TrashType
    {
        public TrashRarity rarity;
        public GameObject prefab;
        [Range(0f, 1f)]
        public float baseSpawnChance = 0.25f;
    }

    [System.Serializable]
    public class RarityDistanceCurve
    {
        public TrashRarity rarity;
        [Tooltip("Probability of this rarity at different distances from hub. X = distance, Y = probability multiplier")]
        public AnimationCurve probabilityCurve = AnimationCurve.Linear(0, 1, 100, 0);
    }

    [Header("Hub Settings")]
    [Tooltip("The central hub position")]
    public Transform hubTransform;

    [Tooltip("Minimum distance from hub where trash can spawn")]
    public float safeRadius = 20f;

    [Header("Terrain Settings")]
    public Terrain terrain;

    [Tooltip("Maximum distance from hub to consider for spawning")]
    public float maxSpawnDistance = 200f;

    [Header("Trash Types")]
    public List<TrashType> trashTypes = new List<TrashType>();

    [Header("Rarity Distribution")]
    [Tooltip("How each rarity's spawn chance changes with distance from hub")]
    public List<RarityDistanceCurve> rarityDistribution = new List<RarityDistanceCurve>();

    [Header("No-Spawn Zones")]
    [Tooltip("Areas where trash should never spawn")]
    public List<NoSpawnZone> noSpawnZones = new List<NoSpawnZone>();

    [Header("Spawn Settings")]
    [Tooltip("Total number of trash items to spawn")]
    public int totalTrashCount = 100;

    [Tooltip("Maximum attempts to find valid spawn position per item")]
    public int maxSpawnAttempts = 30;

    [Tooltip("Minimum height above terrain for spawning")]
    public float heightOffset = 0.1f;

    [Header("Debug")]
    public bool showDebugGizmos = true;
    public Color safeRadiusColor = new Color(1f, 0f, 0f, 0.3f);
    public Color maxDistanceColor = new Color(0f, 1f, 0f, 0.3f);

    private List<GameObject> spawnedTrash = new List<GameObject>();

    void Start()
    {
        ValidateSetup();
        SpawnAllTrash();
    }

    void ValidateSetup()
    {
        if (hubTransform == null)
        {
            Debug.LogError("Hub Transform not assigned! Using system position as hub.");
            hubTransform = transform;
        }

        if (terrain == null)
        {
            terrain = Terrain.activeTerrain;
            if (terrain == null)
            {
                Debug.LogError("No terrain found!");
            }
        }

        if (trashTypes.Count == 0)
        {
            Debug.LogWarning("No trash types defined!");
        }

        // Initialize default rarity curves if none exist
        if (rarityDistribution.Count == 0)
        {
            InitializeDefaultRarityCurves();
        }
    }

    void InitializeDefaultRarityCurves()
    {
        // Common: High probability near hub, decreases with distance
        rarityDistribution.Add(new RarityDistanceCurve
        {
            rarity = TrashRarity.Common,
            probabilityCurve = AnimationCurve.EaseInOut(0, 1f, maxSpawnDistance, 0.2f)
        });

        // Uncommon: Low near hub, moderate at medium distance
        rarityDistribution.Add(new RarityDistanceCurve
        {
            rarity = TrashRarity.Uncommon,
            probabilityCurve = AnimationCurve.EaseInOut(0, 0.3f, maxSpawnDistance, 0.8f)
        });

        // Rare: Very low near hub, high far away
        rarityDistribution.Add(new RarityDistanceCurve
        {
            rarity = TrashRarity.Rare,
            probabilityCurve = AnimationCurve.EaseInOut(0, 0.1f, maxSpawnDistance, 1.0f)
        });

        // Epic: Almost none near hub, highest far away
        rarityDistribution.Add(new RarityDistanceCurve
        {
            rarity = TrashRarity.Epic,
            probabilityCurve = AnimationCurve.EaseInOut(0, 0.05f, maxSpawnDistance, 1.2f)
        });
    }

    public void SpawnAllTrash()
    {
        ClearSpawnedTrash();

        int successfulSpawns = 0;
        int totalAttempts = 0;

        for (int i = 0; i < totalTrashCount; i++)
        {
            bool spawned = false;
            int attempts = 0;

            while (!spawned && attempts < maxSpawnAttempts)
            {
                attempts++;
                totalAttempts++;

                Vector3 spawnPosition = GetRandomSpawnPosition();

                if (IsValidSpawnPosition(spawnPosition))
                {
                    TrashRarity selectedRarity = SelectRarityForPosition(spawnPosition);
                    GameObject trashPrefab = GetTrashPrefabForRarity(selectedRarity);

                    if (trashPrefab != null)
                    {
                        GameObject trash = Instantiate(trashPrefab, spawnPosition, Quaternion.Euler(0, Random.Range(0f, 360f), 0));
                        trash.transform.SetParent(transform);
                        spawnedTrash.Add(trash);
                        spawned = true;
                        successfulSpawns++;
                    }
                }
            }
        }

        Debug.Log($"Spawned {successfulSpawns}/{totalTrashCount} trash items in {totalAttempts} total attempts");
    }

    Vector3 GetRandomSpawnPosition()
    {
        if (terrain == null) return Vector3.zero;

        Vector3 hubPos = hubTransform.position;
        Vector3 terrainSize = terrain.terrainData.size;
        Vector3 terrainPos = terrain.transform.position;

        // Random point on terrain
        float randomX = Random.Range(terrainPos.x, terrainPos.x + terrainSize.x);
        float randomZ = Random.Range(terrainPos.z, terrainPos.z + terrainSize.z);

        float terrainHeight = terrain.SampleHeight(new Vector3(randomX, 0, randomZ));
        Vector3 spawnPos = new Vector3(randomX, terrainPos.y + terrainHeight + heightOffset, randomZ);

        return spawnPos;
    }

    bool IsValidSpawnPosition(Vector3 position)
    {
        Vector3 hubPos = hubTransform.position;
        float distanceFromHub = Vector2.Distance(
            new Vector2(position.x, position.z),
            new Vector2(hubPos.x, hubPos.z)
        );

        // Check if outside safe radius
        if (distanceFromHub < safeRadius)
            return false;

        // Check if within max spawn distance
        if (distanceFromHub > maxSpawnDistance)
            return false;

        // Check no-spawn zones
        foreach (var zone in noSpawnZones)
        {
            if (zone != null && zone.IsPointInside(position))
                return false;
        }

        return true;
    }

    TrashRarity SelectRarityForPosition(Vector3 position)
    {
        Vector3 hubPos = hubTransform.position;
        float distanceFromHub = Vector2.Distance(
            new Vector2(position.x, position.z),
            new Vector2(hubPos.x, hubPos.z)
        );

        // Calculate weighted probabilities for each rarity
        Dictionary<TrashRarity, float> rarityWeights = new Dictionary<TrashRarity, float>();
        float totalWeight = 0f;

        foreach (var trashType in trashTypes)
        {
            float baseChance = trashType.baseSpawnChance;
            float distanceMultiplier = GetDistanceMultiplierForRarity(trashType.rarity, distanceFromHub);
            float weight = baseChance * distanceMultiplier;

            rarityWeights[trashType.rarity] = weight;
            totalWeight += weight;
        }

        // Select rarity based on weighted random
        if (totalWeight <= 0f)
            return TrashRarity.Common;

        float randomValue = Random.Range(0f, totalWeight);
        float cumulativeWeight = 0f;

        foreach (var kvp in rarityWeights)
        {
            cumulativeWeight += kvp.Value;
            if (randomValue <= cumulativeWeight)
                return kvp.Key;
        }

        return TrashRarity.Common;
    }

    float GetDistanceMultiplierForRarity(TrashRarity rarity, float distance)
    {
        foreach (var curve in rarityDistribution)
        {
            if (curve.rarity == rarity)
            {
                return curve.probabilityCurve.Evaluate(distance);
            }
        }
        return 1f; // Default multiplier if no curve found
    }

    GameObject GetTrashPrefabForRarity(TrashRarity rarity)
    {
        List<GameObject> validPrefabs = new List<GameObject>();

        foreach (var trashType in trashTypes)
        {
            if (trashType.rarity == rarity && trashType.prefab != null)
            {
                validPrefabs.Add(trashType.prefab);
            }
        }

        if (validPrefabs.Count > 0)
            return validPrefabs[Random.Range(0, validPrefabs.Count)];

        return null;
    }

    public void ClearSpawnedTrash()
    {
        foreach (var trash in spawnedTrash)
        {
            if (trash != null)
                DestroyImmediate(trash);
        }
        spawnedTrash.Clear();
    }

    void OnDrawGizmos()
    {
        if (!showDebugGizmos || hubTransform == null)
            return;

        Vector3 hubPos = hubTransform.position;

        // Draw safe radius
        Gizmos.color = safeRadiusColor;
        DrawCircle(hubPos, safeRadius, 64);

        // Draw max spawn distance
        Gizmos.color = maxDistanceColor;
        DrawCircle(hubPos, maxSpawnDistance, 64);

        // Draw no-spawn zones
        foreach (var zone in noSpawnZones)
        {
            if (zone != null)
                zone.DrawGizmos();
        }
    }

    void DrawCircle(Vector3 center, float radius, int segments)
    {
        float angleStep = 360f / segments;
        Vector3 prevPoint = center + new Vector3(radius, 0, 0);

        for (int i = 1; i <= segments; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector3 newPoint = center + new Vector3(
                Mathf.Cos(angle) * radius,
                0,
                Mathf.Sin(angle) * radius
            );
            Gizmos.DrawLine(prevPoint, newPoint);
            prevPoint = newPoint;
        }
    }

    // Editor helper methods
    public void RegenerateTrash()
    {
        SpawnAllTrash();
    }
}
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Analyzes and displays statistics about spawned trash
/// Attach to the same GameObject as TrashSpawnSystem
/// </summary>
[RequireComponent(typeof(TrashSpawnSystem))]
public class TrashSpawnAnalytics : MonoBehaviour
{
    [Header("Analysis Settings")]
    public bool analyzeOnSpawn = true;
    public bool showDistanceHistogram = true;

    [Header("Distance Bands")]
    [Tooltip("Divide the spawn area into bands for analysis")]
    public int numberOfBands = 5;

    [System.Serializable]
    public class SpawnStatistics
    {
        public int totalSpawned;
        public Dictionary<TrashSpawnSystem.TrashRarity, int> rarityCount;
        public Dictionary<TrashSpawnSystem.TrashRarity, float> rarityPercentage;
        public Dictionary<int, Dictionary<TrashSpawnSystem.TrashRarity, int>> bandDistribution;
        public float averageDistance;
        public float minDistance;
        public float maxDistance;
    }

    private TrashSpawnSystem spawnSystem;
    private SpawnStatistics currentStats;

    void Start()
    {
        spawnSystem = GetComponent<TrashSpawnSystem>();
    }

    public void AnalyzeSpawnedTrash()
    {
        if (spawnSystem == null)
        {
            Debug.LogError("TrashSpawnSystem not found!");
            return;
        }

        currentStats = new SpawnStatistics
        {
            rarityCount = new Dictionary<TrashSpawnSystem.TrashRarity, int>(),
            rarityPercentage = new Dictionary<TrashSpawnSystem.TrashRarity, float>(),
            bandDistribution = new Dictionary<int, Dictionary<TrashSpawnSystem.TrashRarity, int>>()
        };

        // Initialize rarity counts
        foreach (TrashSpawnSystem.TrashRarity rarity in System.Enum.GetValues(typeof(TrashSpawnSystem.TrashRarity)))
        {
            currentStats.rarityCount[rarity] = 0;
        }

        // Initialize band distributions
        for (int i = 0; i < numberOfBands; i++)
        {
            currentStats.bandDistribution[i] = new Dictionary<TrashSpawnSystem.TrashRarity, int>();
            foreach (TrashSpawnSystem.TrashRarity rarity in System.Enum.GetValues(typeof(TrashSpawnSystem.TrashRarity)))
            {
                currentStats.bandDistribution[i][rarity] = 0;
            }
        }

        Vector3 hubPos = spawnSystem.hubTransform.position;
        float totalDistance = 0f;
        currentStats.minDistance = float.MaxValue;
        currentStats.maxDistance = float.MinValue;

        // Analyze each spawned item
        int childCount = transform.childCount;
        currentStats.totalSpawned = childCount;

        for (int i = 0; i < childCount; i++)
        {
            Transform child = transform.GetChild(i);

            // Calculate distance from hub
            float distance = Vector2.Distance(
                new Vector2(child.position.x, child.position.z),
                new Vector2(hubPos.x, hubPos.z)
            );

            totalDistance += distance;
            currentStats.minDistance = Mathf.Min(currentStats.minDistance, distance);
            currentStats.maxDistance = Mathf.Max(currentStats.maxDistance, distance);

            // Determine rarity from prefab name
            TrashSpawnSystem.TrashRarity rarity = DetermineRarityFromName(child.name);
            currentStats.rarityCount[rarity]++;

            // Determine distance band
            int band = GetDistanceBand(distance);
            if (band >= 0 && band < numberOfBands)
            {
                currentStats.bandDistribution[band][rarity]++;
            }
        }

        // Calculate percentages
        if (currentStats.totalSpawned > 0)
        {
            currentStats.averageDistance = totalDistance / currentStats.totalSpawned;

            foreach (var rarity in currentStats.rarityCount.Keys.ToList())
            {
                currentStats.rarityPercentage[rarity] =
                    (float)currentStats.rarityCount[rarity] / currentStats.totalSpawned * 100f;
            }
        }

        PrintStatistics();
    }

    TrashSpawnSystem.TrashRarity DetermineRarityFromName(string name)
    {
        string lowerName = name.ToLower();

        if (lowerName.Contains("epic"))
            return TrashSpawnSystem.TrashRarity.Epic;
        if (lowerName.Contains("rare"))
            return TrashSpawnSystem.TrashRarity.Rare;
        if (lowerName.Contains("uncommon"))
            return TrashSpawnSystem.TrashRarity.Uncommon;

        return TrashSpawnSystem.TrashRarity.Common;
    }

    int GetDistanceBand(float distance)
    {
        float bandSize = spawnSystem.maxSpawnDistance / numberOfBands;
        int band = Mathf.FloorToInt(distance / bandSize);
        return Mathf.Clamp(band, 0, numberOfBands - 1);
    }

    void PrintStatistics()
    {
        if (currentStats == null)
            return;

        Debug.Log("=== TRASH SPAWN STATISTICS ===");
        Debug.Log($"Total Spawned: {currentStats.totalSpawned}");
        Debug.Log($"Average Distance from Hub: {currentStats.averageDistance:F2}m");
        Debug.Log($"Distance Range: {currentStats.minDistance:F2}m - {currentStats.maxDistance:F2}m");

        Debug.Log("\n--- Rarity Distribution ---");
        foreach (var kvp in currentStats.rarityCount)
        {
            float percentage = currentStats.rarityPercentage[kvp.Key];
            Debug.Log($"{kvp.Key}: {kvp.Value} ({percentage:F1}%)");
        }

        if (showDistanceHistogram)
        {
            Debug.Log("\n--- Distance Band Distribution ---");
            float bandSize = spawnSystem.maxSpawnDistance / numberOfBands;

            for (int i = 0; i < numberOfBands; i++)
            {
                float bandStart = i * bandSize;
                float bandEnd = (i + 1) * bandSize;

                Debug.Log($"\nBand {i + 1}: {bandStart:F0}m - {bandEnd:F0}m");

                int bandTotal = 0;
                foreach (var rarity in currentStats.bandDistribution[i].Keys)
                {
                    bandTotal += currentStats.bandDistribution[i][rarity];
                }

                foreach (var kvp in currentStats.bandDistribution[i])
                {
                    if (kvp.Value > 0)
                    {
                        float bandPercentage = bandTotal > 0 ? (float)kvp.Value / bandTotal * 100f : 0f;
                        Debug.Log($"  {kvp.Key}: {kvp.Value} ({bandPercentage:F1}% of band)");
                    }
                }
            }
        }

        Debug.Log("\n=== END STATISTICS ===");
    }

    public SpawnStatistics GetCurrentStatistics()
    {
        return currentStats;
    }

    // Visualize statistics in Scene view
    void OnDrawGizmos()
    {
        if (!Application.isPlaying || currentStats == null || spawnSystem == null)
            return;

        Vector3 hubPos = spawnSystem.hubTransform.position;
        float bandSize = spawnSystem.maxSpawnDistance / numberOfBands;

        // Draw distance bands
        for (int i = 0; i < numberOfBands; i++)
        {
            float radius = (i + 1) * bandSize;
            Color bandColor = Color.Lerp(Color.green, Color.red, (float)i / numberOfBands);
            bandColor.a = 0.1f;

            Gizmos.color = bandColor;
            DrawCircle(hubPos, radius, 32);
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
}
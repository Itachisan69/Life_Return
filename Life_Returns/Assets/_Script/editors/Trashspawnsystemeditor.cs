using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TrashSpawnSystem))]
public class TrashSpawnSystemEditor : Editor
{
    private TrashSpawnSystem spawnSystem;

    void OnEnable()
    {
        spawnSystem = (TrashSpawnSystem)target;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Spawn Controls", EditorStyles.boldLabel);

        if (GUILayout.Button("Generate Trash", GUILayout.Height(30)))
        {
            spawnSystem.SpawnAllTrash();
        }

        if (GUILayout.Button("Clear All Trash", GUILayout.Height(30)))
        {
            spawnSystem.ClearSpawnedTrash();
        }

        EditorGUILayout.Space(5);

        // Analytics button
        TrashSpawnAnalytics analytics = spawnSystem.GetComponent<TrashSpawnAnalytics>();
        if (analytics != null)
        {
            if (GUILayout.Button("Analyze Spawn Distribution", GUILayout.Height(25)))
            {
                analytics.AnalyzeSpawnedTrash();
            }
        }
        else
        {
            EditorGUILayout.HelpBox("Add TrashSpawnAnalytics component for spawn analysis", MessageType.Info);
            if (GUILayout.Button("Add Analytics Component"))
            {
                spawnSystem.gameObject.AddComponent<TrashSpawnAnalytics>();
            }
        }

        EditorGUILayout.Space(10);
        EditorGUILayout.HelpBox(
            "1. Assign hub transform and terrain\n" +
            "2. Add trash prefabs with rarities\n" +
            "3. Adjust rarity curves (X = distance, Y = multiplier)\n" +
            "4. Add NoSpawnZone components for exclusion areas\n" +
            "5. Click 'Generate Trash' to spawn items",
            MessageType.Info
        );

        EditorGUILayout.Space(5);

        if (spawnSystem.rarityDistribution.Count > 0)
        {
            EditorGUILayout.LabelField("Rarity Curve Preview", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "These curves determine how spawn probability changes with distance.\n" +
                "X-axis = Distance from hub\n" +
                "Y-axis = Probability multiplier",
                MessageType.None
            );
        }
    }
}
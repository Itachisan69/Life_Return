// SuckableObject.cs
using UnityEngine;

public class SuckableObject : MonoBehaviour
{
    public SuckableObjectData data;

    void Start()
    {
        // Basic check to ensure a data asset is assigned in the Inspector
        if (data == null)
        {
            Debug.LogError($"SuckableObject on {gameObject.name} is missing its SuckableObjectData.", this);
            enabled = false;
        }
    }
}
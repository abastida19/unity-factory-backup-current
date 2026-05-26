using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "LayoutPrefabLibrary",
    menuName = "Layout/Prefab Library"
)]
public class LayoutPrefabLibrary : ScriptableObject
{
    public List<PrefabMapping> mappings = new List<PrefabMapping>();

    private void OnValidate()
    {
        foreach (var mapping in mappings)
        {
            if (mapping == null) continue;

            mapping.extraScale = new Vector3(
                mapping.extraScale.x == 0f ? 1f : mapping.extraScale.x,
                mapping.extraScale.y == 0f ? 1f : mapping.extraScale.y,
                mapping.extraScale.z == 0f ? 1f : mapping.extraScale.z
            );
        }
    }
}

[Serializable]
public class PrefabMapping
{
    public string nodeId;
    public string machineName;

    public GameObject prefab;

    public Vector3 extraScale = Vector3.one;
    public Vector3 rotation = Vector3.zero;
    public Vector3 offset = Vector3.zero;
}
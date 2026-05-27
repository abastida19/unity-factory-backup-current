// Assets/Editor/LayoutJsonImporterWindow_PrefabTest.cs
using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LayoutJsonImporterWindow_PrefabTest : EditorWindow
{
    [Serializable] private class Root { public Meta meta; public Solution solution; }
    [Serializable] private class Meta { public string factory_type; public bool success; public string error; }
    [Serializable] private class Solution { public ProductionGraph production_graph; }
    [Serializable] private class ProductionGraph { public Node[] nodes; public Edge[] edges; }

    [Serializable]
    private class Node
    {
        public string id;
        public string machine_name;
        public string process_step;
        public string machine_description;
        public string type;
        public bool is_modular;
        public float approx_width;
        public float approx_depth;
        public float x;
        public float y;
    }

    [Serializable]
    private class Edge
    {
        public string from;
        public string to;
        public string relation;
        public string note;
        public float weight;
    }

    private string jsonPath = "";
    private string outScenePath = "Assets/Scenes/Generated/LayoutGenerated_PrefabTest.unity";

    private LayoutPrefabLibrary prefabLibrary;
    private GameObject uiCanvasPrefab;

    private Vector2 scrollPos;

    private float scale = 1.0f;
    private float boxHeight = 1.0f;
    private float labelHeight = 0.8f;
    private float labelSize = 0.01f;

    private float connectorHeight = 3.0f;
    private float connectorThickness = 0.12f;

    private int minRelationWeight = 7;
    private bool showLegend = true;

    [MenuItem("Tools/Layout/Import JSON (Prefab Library)")]
    public static void ShowWindow()
    {
        var w = GetWindow<LayoutJsonImporterWindow_PrefabTest>("Layout JSON Prefab Library");
        w.minSize = new Vector2(600, 500);
        w.Show();
    }

    private void OnGUI()
    {
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        EditorGUILayout.LabelField("Layout JSON → Unity Scene (Prefab Library)", EditorStyles.boldLabel);
        EditorGUILayout.Space(6);

        using (new EditorGUILayout.HorizontalScope())
        {
            EditorGUILayout.LabelField("JSON", GUILayout.Width(40));
            EditorGUILayout.SelectableLabel(string.IsNullOrEmpty(jsonPath) ? "(none)" : jsonPath, GUILayout.Height(18));

            if (GUILayout.Button("Choose…", GUILayout.Width(90)))
            {
                string picked = EditorUtility.OpenFilePanel("Select layout.json", "", "json");
                if (!string.IsNullOrEmpty(picked))
                    jsonPath = picked;
            }
        }

        EditorGUILayout.Space(8);
        outScenePath = EditorGUILayout.TextField("Output Scene", outScenePath);

        EditorGUILayout.Space(8);
        prefabLibrary = (LayoutPrefabLibrary)EditorGUILayout.ObjectField(
            "Prefab Library",
            prefabLibrary,
            typeof(LayoutPrefabLibrary),
            false
        );

        EditorGUILayout.Space(8);
        uiCanvasPrefab = (GameObject)EditorGUILayout.ObjectField(
            "UI Canvas Prefab",
            uiCanvasPrefab,
            typeof(GameObject),
            false
        );

        EditorGUILayout.Space(8);
        scale = EditorGUILayout.FloatField("Scale (coords & sizes)", scale);
        boxHeight = EditorGUILayout.FloatField("Box Height (Y)", boxHeight);
        labelHeight = EditorGUILayout.FloatField("Label Height Offset", labelHeight);
        labelSize = EditorGUILayout.FloatField("Label Size", labelSize);

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("Relation Connectors", EditorStyles.boldLabel);
        connectorHeight = EditorGUILayout.FloatField("Base Connector Height", connectorHeight);
        connectorThickness = EditorGUILayout.FloatField("Connector Thickness", connectorThickness);
        minRelationWeight = EditorGUILayout.IntSlider("Minimum Relation Weight", minRelationWeight, 1, 10);
        showLegend = EditorGUILayout.Toggle("Show Legend", showLegend);

        EditorGUILayout.Space(12);

        using (new EditorGUI.DisabledScope(string.IsNullOrEmpty(jsonPath)))
        {
            if (GUILayout.Button("Import → Create Scene → Save → Open", GUILayout.Height(34)))
            {
                try
                {
                    ImportAndCreateScene(
                        jsonPath,
                        outScenePath,
                        prefabLibrary,
                        scale,
                        boxHeight,
                        labelHeight,
                        labelSize,
                        connectorHeight,
                        connectorThickness,
                        minRelationWeight,
                        showLegend,
                        uiCanvasPrefab
                    );
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                    EditorUtility.DisplayDialog("Import failed", e.Message, "OK");
                }
            }
        }

        EditorGUILayout.HelpBox(
            "Uses the Prefab Library to replace matching JSON nodes with prefabs. If no mapping exists, the node becomes a cube.",
            MessageType.Info
        );

        EditorGUILayout.EndScrollView();
    }

    private static void ImportAndCreateScene(
        string layoutJsonPath,
        string outSceneAssetPath,
        LayoutPrefabLibrary prefabLibrary,
        float scale,
        float yBox,
        float labelHeight,
        float labelSize,
        float connectorHeight,
        float connectorThickness,
        int minRelationWeight,
        bool showLegend,
        GameObject uiCanvasPrefab)
    {
        if (!outSceneAssetPath.StartsWith("Assets/"))
            throw new Exception("Output scene path must start with 'Assets/'.");

        if (!File.Exists(layoutJsonPath))
            throw new Exception("JSON file not found: " + layoutJsonPath);

        EnsureUnityFoldersExist(Path.GetDirectoryName(outSceneAssetPath)?.Replace("\\", "/"));

        string json = File.ReadAllText(layoutJsonPath);
        Root root = JsonUtility.FromJson<Root>(json);

        if (root?.solution?.production_graph?.nodes == null || root.solution.production_graph.nodes.Length == 0)
            throw new Exception("No nodes found at solution.production_graph.nodes");

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        var rootGo = new GameObject("GeneratedLayout");

        var relationManagerGO = new GameObject("RelationManager");
        relationManagerGO.transform.SetParent(rootGo.transform, false);
        RelationManager relationManager = relationManagerGO.AddComponent<RelationManager>();

        var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.SetParent(rootGo.transform, false);
        ground.transform.position = Vector3.zero;
        ground.transform.localScale = new Vector3(100f, 100f, 100f);

        var lightGo = new GameObject("Directional Light");
        var light = lightGo.AddComponent<Light>();
        light.type = LightType.Directional;
        light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        lightGo.transform.SetParent(rootGo.transform, false);

        var camGo = new GameObject("Main Camera");
        var cam = camGo.AddComponent<Camera>();
        camGo.tag = "MainCamera";
        cam.orthographic = false;
        cam.fieldOfView = 60f;
        camGo.transform.SetParent(rootGo.transform, false);
        camGo.transform.position = new Vector3(0f, 3f, -20f);
        camGo.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        camGo.AddComponent<FreeFlyCameraController>();
        camGo.AddComponent<MachineClickManager>();

        if (uiCanvasPrefab != null)
        {
            GameObject ui = (GameObject)PrefabUtility.InstantiatePrefab(uiCanvasPrefab);
            ui.name = "RelationUI_Canvas";
        }

        var nodesParent = new GameObject("Nodes");
        nodesParent.transform.SetParent(rootGo.transform, false);

        var edgesParent = new GameObject("Edges");
        edgesParent.transform.SetParent(rootGo.transform, false);

        var clickProxiesParent = new GameObject("ClickProxies");
        clickProxiesParent.transform.SetParent(rootGo.transform, false);

        Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);
        bool hasBounds = false;

        Dictionary<string, GameObject> nodeObjectsById = new Dictionary<string, GameObject>();

        foreach (var n in root.solution.production_graph.nodes)
        {
            if (n == null) continue;

            string label = !string.IsNullOrWhiteSpace(n.machine_name) ? n.machine_name : n.id;
            if (string.IsNullOrWhiteSpace(label)) label = "Unnamed";

            Vector3 basePos = new Vector3(n.x * scale, 0f, n.y * scale);

            float w = Mathf.Max(0.2f, n.approx_width * scale);
            float d = Mathf.Max(0.2f, n.approx_depth * scale);
            float h = Mathf.Max(0.2f, yBox);

            GameObject go;
            bool usedPrefab = false;

            PrefabMapping mapping = FindMapping(prefabLibrary, n.id, n.machine_name);

            if (mapping != null && mapping.prefab != null)
            {
                go = (GameObject)PrefabUtility.InstantiatePrefab(mapping.prefab);
                go.name = "Prefab_" + SanitizeName(label);
                go.transform.SetParent(nodesParent.transform, false);
                go.transform.rotation = Quaternion.Euler(mapping.rotation);

                FitAndPlacePrefab(go, basePos, w, d, mapping.extraScale, mapping.offset);
                usedPrefab = true;
            }
            else
            {
                go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.name = $"Box_{SanitizeName(label)}";
                go.transform.SetParent(nodesParent.transform, false);
                go.transform.position = basePos + new Vector3(0f, h * 0.5f, 0f);
                go.transform.localScale = new Vector3(w, h, d);
            }

            nodeObjectsById[n.id] = go;

            var tag = go.AddComponent<GeneratedComponentTag_PrefabTest>();
            tag.id = n.id;
            tag.machine_name = n.machine_name;
            tag.process_step = n.process_step;
            tag.type = n.type;

            ApplyTagToHierarchy(go, n.id, n.machine_name, n.process_step, n.type);
            RemoveChildColliders(go);
            EnsureClickableCollider(go);
            CreateClickProxy(clickProxiesParent.transform, go, n.id, n.machine_name, n.process_step, n.type);

            var textGo = new GameObject($"Label_{SanitizeName(label)}");
            textGo.transform.SetParent(go.transform, false);

            float labelY = usedPrefab ? labelHeight : (h * 0.5f) + labelHeight;
            textGo.transform.localPosition = new Vector3(0f, labelY, 0f);
            textGo.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

            var tm = textGo.AddComponent<TextMesh>();
            tm.text = label;
            tm.characterSize = labelSize;
            tm.fontSize = 64;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.alignment = TextAlignment.Center;

            var proximityLabel = textGo.AddComponent<MachineProximityLabel>();
            proximityLabel.showDistance = 12f;
            proximityLabel.hideDistance = 14f;

            Bounds b;
            if (TryGetObjectBounds(go, out b))
            {
                if (!hasBounds) { bounds = b; hasBounds = true; }
                else bounds.Encapsulate(b);
            }
            else
            {
                b = new Bounds(go.transform.position, usedPrefab ? new Vector3(w, h, d) : go.transform.localScale);
                if (!hasBounds) { bounds = b; hasBounds = true; }
                else bounds.Encapsulate(b);
            }
        }

        if (root.solution.production_graph.edges != null)
        {
            int edgeIndex = 0;

            foreach (var e in root.solution.production_graph.edges)
            {
                if (e == null) continue;
                if (string.IsNullOrWhiteSpace(e.from) || string.IsNullOrWhiteSpace(e.to)) continue;
                if (!nodeObjectsById.ContainsKey(e.from) || !nodeObjectsById.ContainsKey(e.to)) continue;

                int roundedWeight = Mathf.RoundToInt(Mathf.Clamp(e.weight, 1f, 10f));
                if (roundedWeight < minRelationWeight) continue;

                GameObject fromGo = nodeObjectsById[e.from];
                GameObject toGo = nodeObjectsById[e.to];

                Vector3 start = GetTopCenter(fromGo);
                Vector3 end = GetTopCenter(toGo);

                Color edgeColor = GetWeightColorDiscrete(e.weight);
                float relationHeight = connectorHeight + GetRelationHeightOffset(e.relation);
                float indexOffset = (edgeIndex % 5) * 0.12f;

                GameObject connectorRoot = CreateThreeSegmentConnector(
                    edgesParent.transform,
                    $"Edge_{SanitizeName(e.from)}_to_{SanitizeName(e.to)}_{edgeIndex}",
                    start,
                    end,
                    relationHeight + indexOffset,
                    connectorThickness,
                    edgeColor
                );

                var visual = connectorRoot.GetComponent<RelationVisual>();
                if (visual != null)
                    visual.Setup(e.from, e.to, edgeColor);

                var info = connectorRoot.AddComponent<RelationInfo>();
                info.from = e.from;
                info.to = e.to;
                info.relation = e.relation;
                info.weight = e.weight;
                info.note = e.note;

                relationManager.RegisterRelation(e.from, e.to, connectorRoot);

                edgeIndex++;
            }
        }

        if (showLegend)
            CreateLegend(rootGo.transform, bounds, hasBounds);

        bool saved = EditorSceneManager.SaveScene(scene, outSceneAssetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        if (!saved)
            throw new Exception("Failed to save scene: " + outSceneAssetPath);

        EditorSceneManager.OpenScene(outSceneAssetPath, OpenSceneMode.Single);
        Debug.Log("Scene generated & opened: " + outSceneAssetPath);
    }

    private static PrefabMapping FindMapping(LayoutPrefabLibrary library, string nodeId, string machineName)
    {
        if (library == null || library.mappings == null) return null;

        foreach (var m in library.mappings)
        {
            if (m == null) continue;
            if (!string.IsNullOrWhiteSpace(m.nodeId) && m.nodeId == nodeId)
                return m;
        }

        foreach (var m in library.mappings)
        {
            if (m == null) continue;
            if (!string.IsNullOrWhiteSpace(m.machineName) && m.machineName == machineName)
                return m;
        }

        return null;
    }

    private static Vector3 FixZeroScale(Vector3 scale)
    {
        return new Vector3(
            scale.x == 0f ? 1f : scale.x,
            scale.y == 0f ? 1f : scale.y,
            scale.z == 0f ? 1f : scale.z
        );
    }

    private static void FitAndPlacePrefab(GameObject go, Vector3 basePos, float targetWidth, float targetDepth, Vector3 extraScale, Vector3 extraOffset)
    {
        extraScale = FixZeroScale(extraScale);

        Bounds bounds;
        if (!TryGetObjectBounds(go, out bounds))
        {
            go.transform.position = basePos + extraOffset;
            go.transform.localScale = extraScale;
            return;
        }

        float sourceWidth = Mathf.Max(0.001f, bounds.size.x);
        float sourceDepth = Mathf.Max(0.001f, bounds.size.z);

        float fitScaleX = targetWidth / sourceWidth;
        float fitScaleZ = targetDepth / sourceDepth;

        float uniformFit = Mathf.Min(fitScaleX, fitScaleZ);
        go.transform.localScale = Vector3.Scale(Vector3.one * uniformFit, extraScale);

        if (!TryGetObjectBounds(go, out bounds))
        {
            go.transform.position = basePos + extraOffset;
            return;
        }

        Vector3 currentCenter = bounds.center;
        float bottomY = bounds.min.y;

        Vector3 correction = new Vector3(
            basePos.x - currentCenter.x,
            0f - bottomY,
            basePos.z - currentCenter.z
        );

        go.transform.position += correction + extraOffset;
    }

    private static GameObject CreateThreeSegmentConnector(Transform parent, string name, Vector3 start, Vector3 end, float topHeight, float thickness, Color color)
    {
        GameObject connectorRoot = new GameObject(name);
        connectorRoot.transform.SetParent(parent, false);

        float elevatedY = Mathf.Max(start.y, end.y) + topHeight;

        Vector3 p0 = start;
        Vector3 p1 = new Vector3(start.x, elevatedY, start.z);
        Vector3 p2 = new Vector3(end.x, elevatedY, end.z);
        Vector3 p3 = end;

        Renderer segA = CreateCylinderBetween(connectorRoot.transform, "Seg_A", p0, p1, thickness, color);
        Renderer segB = CreateCylinderBetween(connectorRoot.transform, "Seg_B", p1, p2, thickness, color);
        Renderer segC = CreateCylinderBetween(connectorRoot.transform, "Seg_C", p2, p3, thickness, color);

        var visual = connectorRoot.AddComponent<RelationVisual>();
        visual.segA = segA;
        visual.segB = segB;
        visual.segC = segC;

        TextMesh startLabel = CreateWorldLabel(
            connectorRoot.transform,
            "StartLabel",
            "",
            p1 + new Vector3(0.25f, 0.25f, 0.25f)
        );

        TextMesh endLabel = CreateWorldLabel(
            connectorRoot.transform,
            "EndLabel",
            "",
            p2 + new Vector3(0.25f, 0.25f, 0.25f)
        );

        startLabel.gameObject.SetActive(false);
        endLabel.gameObject.SetActive(false);

        visual.startLabel = startLabel;
        visual.endLabel = endLabel;

        return connectorRoot;
    }

    private static Renderer CreateCylinderBetween(Transform parent, string name, Vector3 a, Vector3 b, float thickness, Color color)
    {
        Vector3 delta = b - a;
        float length = delta.magnitude;
        if (length < 0.001f) return null;

        GameObject cyl = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cyl.name = name;
        cyl.transform.SetParent(parent, false);

        cyl.transform.position = (a + b) * 0.5f;
        cyl.transform.up = delta.normalized;
        cyl.transform.localScale = new Vector3(thickness, length * 0.5f, thickness);

        var renderer = cyl.GetComponent<Renderer>();
        if (renderer != null)
            renderer.sharedMaterial = CreateRelationMaterial(color);

        return renderer;
    }

    private static TextMesh CreateWorldLabel(Transform parent, string name, string text, Vector3 worldPosition)
    {
        GameObject labelGo = new GameObject(name);
        labelGo.transform.SetParent(parent, false);
        labelGo.transform.position = worldPosition;

        var tm = labelGo.AddComponent<TextMesh>();
        tm.text = text;
        tm.characterSize = 0.12f;
        tm.fontSize = 48;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.alignment = TextAlignment.Center;
        tm.color = Color.black;

        labelGo.AddComponent<BillboardLabel>();
        return tm;
    }

    private static Material CreateRelationMaterial(Color color)
    {
        Shader shader = Shader.Find("Sprites/Default");

        if (shader == null)
            shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null)
            shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
            shader = Shader.Find("Standard");

        Material mat = new Material(shader);

        if (mat.HasProperty("_BaseColor"))
            mat.SetColor("_BaseColor", color);

        if (mat.HasProperty("_Color"))
            mat.SetColor("_Color", color);

        mat.color = color;
        return mat;
    }

    private static void CreateLegend(Transform parent, Bounds bounds, bool hasBounds)
    {
        GameObject legendRoot = new GameObject("Legend");
        legendRoot.transform.SetParent(parent, false);

        Vector3 anchor = hasBounds
            ? new Vector3(bounds.min.x - 6f, 0.1f, bounds.max.z + 2f)
            : new Vector3(-6f, 0.1f, 6f);

        for (int weight = 10; weight >= 1; weight--)
        {
            float row = 10 - weight;
            Vector3 rowPos = anchor + new Vector3(0f, 0f, -row * 0.7f);

            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = $"Legend_Color_{weight}";
            cube.transform.SetParent(legendRoot.transform, false);
            cube.transform.position = rowPos;
            cube.transform.localScale = new Vector3(0.35f, 0.35f, 0.35f);

            var renderer = cube.GetComponent<Renderer>();
            if (renderer != null)
                renderer.sharedMaterial = CreateRelationMaterial(GetWeightColorDiscrete(weight));

            GameObject label = new GameObject($"Legend_Label_{weight}");
            label.transform.SetParent(legendRoot.transform, false);
            label.transform.position = rowPos + new Vector3(0.8f, 0f, 0f);
            label.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

            var tm = label.AddComponent<TextMesh>();
            tm.text = $"Weight {weight}";
            tm.characterSize = 0.14f;
            tm.fontSize = 42;
            tm.anchor = TextAnchor.MiddleLeft;
            tm.alignment = TextAlignment.Left;
        }
    }

    private static Color GetWeightColorDiscrete(float weight)
    {
        int w = Mathf.RoundToInt(Mathf.Clamp(weight, 1f, 10f));

        switch (w)
        {
            case 10: return Color.red;
            case 9: return new Color(1f, 0.4f, 0.7f, 1f);
            case 8: return new Color(1f, 0.55f, 0f, 1f);
            case 7: return Color.yellow;
            case 6: return Color.green;
            case 5: return Color.blue;
            case 4: return new Color(0.45f, 0.85f, 1f, 1f);
            case 3: return Color.gray;
            case 2: return new Color(1f, 0.96f, 0.82f, 1f);
            case 1: return Color.white;
            default: return Color.white;
        }
    }

    private static float GetRelationHeightOffset(string relation)
    {
        switch (relation)
        {
            case "material_flow": return 0.0f;
            case "order_along_axis": return 0.5f;
            case "same_line": return 1.0f;
            case "adjacency_preferred": return 1.5f;
            case "distance": return 2.0f;
            case "keep_out": return 2.5f;
            default: return 3.0f;
        }
    }

    private static Vector3 GetTopCenter(GameObject go)
    {
        Bounds b;
        if (TryGetObjectBounds(go, out b))
            return new Vector3(b.center.x, b.max.y, b.center.z);

        return go.transform.position;
    }

    private static bool TryGetObjectBounds(GameObject go, out Bounds bounds)
    {
        var renderers = go.GetComponentsInChildren<Renderer>();

        if (renderers == null || renderers.Length == 0)
        {
            bounds = default;
            return false;
        }

        bounds = renderers[0].bounds;

        for (int i = 1; i < renderers.Length; i++)
            bounds.Encapsulate(renderers[i].bounds);

        return true;
    }

    private static void EnsureClickableCollider(GameObject go)
    {
        Collider rootCollider = go.GetComponent<Collider>();
        if (rootCollider != null)
            return;

        Bounds b;
        if (!TryGetObjectBounds(go, out b))
            return;

        BoxCollider box = go.AddComponent<BoxCollider>();

        Vector3 localCenter = go.transform.InverseTransformPoint(b.center);
        Vector3 localSize = go.transform.InverseTransformVector(b.size);

        box.center = localCenter;
        box.size = new Vector3(
            Mathf.Abs(localSize.x),
            Mathf.Abs(localSize.y),
            Mathf.Abs(localSize.z)
        );
    }

    private static void RemoveChildColliders(GameObject root)
    {
        Collider[] colliders = root.GetComponentsInChildren<Collider>(true);

        foreach (Collider col in colliders)
        {
            if (col.gameObject != root)
                DestroyImmediate(col);
        }
    }

    private static void CreateClickProxy(
        Transform parent,
        GameObject target,
        string id,
        string machineName,
        string processStep,
        string type)
    {
        Bounds b;
        if (!TryGetObjectBounds(target, out b))
            return;

        GameObject proxy = new GameObject("ClickProxy_" + SanitizeName(id));
        proxy.transform.SetParent(parent, true);
        proxy.transform.position = b.center;
        proxy.transform.rotation = Quaternion.identity;
        proxy.transform.localScale = Vector3.one;

        BoxCollider box = proxy.AddComponent<BoxCollider>();
        box.center = Vector3.zero;
        box.size = b.size + new Vector3(0.5f, 0.5f, 0.5f);

        var tag = proxy.AddComponent<GeneratedComponentTag_PrefabTest>();
        tag.id = id;
        tag.machine_name = machineName;
        tag.process_step = processStep;
        tag.type = type;
    }

    private static void ApplyTagToHierarchy(GameObject root, string id, string machineName, string processStep, string type)
    {
        foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
        {
            var tag = t.GetComponent<GeneratedComponentTag_PrefabTest>();

            if (tag == null)
                tag = t.gameObject.AddComponent<GeneratedComponentTag_PrefabTest>();

            tag.id = id;
            tag.machine_name = machineName;
            tag.process_step = processStep;
            tag.type = type;
        }
    }

    private static string SanitizeName(string s)
    {
        if (string.IsNullOrEmpty(s)) return "Unnamed";

        foreach (char c in Path.GetInvalidFileNameChars())
            s = s.Replace(c.ToString(), "_");

        return s.Replace(" ", "_");
    }

    private static void EnsureUnityFoldersExist(string dir)
    {
        if (string.IsNullOrEmpty(dir)) return;

        dir = dir.Replace("\\", "/");

        if (dir == "Assets") return;
        if (!dir.StartsWith("Assets/")) return;

        string[] parts = dir.Split('/');
        string current = "Assets";

        for (int i = 1; i < parts.Length; i++)
        {
            string next = current + "/" + parts[i];

            if (!AssetDatabase.IsValidFolder(next))
                AssetDatabase.CreateFolder(current, parts[i]);

            current = next;
        }
    }
}
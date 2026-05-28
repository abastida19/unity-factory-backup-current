using UnityEngine;

public class RelationFlowAnimator : MonoBehaviour
{
    public Transform segA;
    public Transform segB;
    public Transform segC;

    public float speed = 4f;
    public float dotSize = 0.25f;
    public Color dotColor = new Color(1f, 1f, 0.2f, 1f);

    private GameObject dot;
    private Vector3[] points;
    private float[] segmentLengths;
    private float totalLength;
    private float distanceTravelled;
    private bool isPlaying;

    void Awake()
    {
        BuildPath();
        CreateDot();
        StopFlow();
    }

    void Update()
    {
        if (!isPlaying || dot == null || points == null || points.Length < 4)
            return;

        distanceTravelled += speed * Time.deltaTime;

        if (distanceTravelled > totalLength)
            distanceTravelled = 0f;

        dot.transform.position = GetPointOnPath(distanceTravelled);
    }

    public void StartFlow()
    {
        BuildPath();

        if (dot == null)
            CreateDot();

        if (dot != null)
            dot.SetActive(true);

        distanceTravelled = 0f;
        isPlaying = true;
    }

    public void StopFlow()
    {
        isPlaying = false;

        if (dot != null)
            dot.SetActive(false);
    }

    private void BuildPath()
    {
        if (segA == null || segB == null || segC == null)
            return;

        Vector3 a0, a1, b0, b1, c0, c1;

        GetCylinderEndpoints(segA, out a0, out a1);
        GetCylinderEndpoints(segB, out b0, out b1);
        GetCylinderEndpoints(segC, out c0, out c1);

        // Direction is:
        // from machine → up Seg_A → across Seg_B → down Seg_C → to machine
        points = new Vector3[]
        {
            a0,
            a1,
            b1,
            c1
        };

        segmentLengths = new float[3];
        totalLength = 0f;

        for (int i = 0; i < 3; i++)
        {
            segmentLengths[i] = Vector3.Distance(points[i], points[i + 1]);
            totalLength += segmentLengths[i];
        }
    }

    private void GetCylinderEndpoints(Transform cylinder, out Vector3 start, out Vector3 end)
    {
        float halfLength = cylinder.lossyScale.y;

        start = cylinder.position - cylinder.up * halfLength;
        end = cylinder.position + cylinder.up * halfLength;
    }

    private Vector3 GetPointOnPath(float distance)
    {
        if (distance <= segmentLengths[0])
        {
            float t = distance / segmentLengths[0];
            return Vector3.Lerp(points[0], points[1], t);
        }

        distance -= segmentLengths[0];

        if (distance <= segmentLengths[1])
        {
            float t = distance / segmentLengths[1];
            return Vector3.Lerp(points[1], points[2], t);
        }

        distance -= segmentLengths[1];

        float finalT = distance / segmentLengths[2];
        return Vector3.Lerp(points[2], points[3], finalT);
    }

    private void CreateDot()
    {
        dot = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        dot.name = "FlowDot";
        dot.transform.SetParent(transform, true);
        dot.transform.localScale = Vector3.one * dotSize;

        Collider col = dot.GetComponent<Collider>();
        if (col != null)
            Destroy(col);

        Renderer r = dot.GetComponent<Renderer>();
        if (r != null)
            r.sharedMaterial = CreateMaterial(dotColor);
    }

    private Material CreateMaterial(Color color)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Unlit");

        if (shader == null)
            shader = Shader.Find("Sprites/Default");

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
}
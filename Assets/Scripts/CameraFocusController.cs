using UnityEngine;

public class CameraFocusController : MonoBehaviour
{
    public static CameraFocusController Instance;

    [Header("General Focus")]
    public float rotationSpeed = 3.5f;
    public float moveSpeed = 2.5f;
    public float focusDuration = 1.8f;

    [Header("Relation View")]
    public float relationDistanceMultiplier = 2.2f;
    public float relationMinDistance = 12f;
    public float relationMaxDistance = 45f;
    public float relationViewHeight = 10f;

    private Transform camTransform;

    private Vector3 targetPoint;
    private Vector3 targetPosition;

    private bool rotateOnly = true;
    private bool isFocusing = false;
    private float timer = 0f;

    void Awake()
    {
        Instance = this;
        camTransform = transform;
    }

    void Update()
    {
        if (!isFocusing)
            return;

        timer += Time.deltaTime;

        if (!rotateOnly)
        {
            camTransform.position = Vector3.Lerp(
                camTransform.position,
                targetPosition,
                Time.deltaTime * moveSpeed
            );
        }

        Vector3 direction = targetPoint - camTransform.position;

        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);

            camTransform.rotation = Quaternion.Slerp(
                camTransform.rotation,
                targetRotation,
                Time.deltaTime * rotationSpeed
            );
        }

        if (timer >= focusDuration)
            isFocusing = false;
    }

    public void FocusOn(GameObject target)
    {
        if (target == null)
            return;

        Bounds machineBounds;

        if (!TryGetBounds(target, out machineBounds))
        {
            targetPoint = target.transform.position;
            rotateOnly = true;
            timer = 0f;
            isFocusing = true;
            return;
        }

        // Look at the upper-middle part of the machine
        targetPoint = new Vector3(
            machineBounds.center.x,
            Mathf.Lerp(machineBounds.center.y, machineBounds.max.y, 0.65f),
            machineBounds.center.z
        );

        // Machine size determines how far the camera should stand away
        float machineSize = Mathf.Max(
            machineBounds.size.x,
            machineBounds.size.z,
            machineBounds.size.y
        );

        // Good inspection distance
        float distance = Mathf.Clamp(machineSize * 3.9f, 14f, 40f);

        // Slightly above the highest point of the machine
        float height = Mathf.Clamp(machineBounds.size.y * 2.5f, 9f, 26f);

        // Camera comes from a diagonal side, not directly front/top
        Vector3 sideDirection = new Vector3(-0.7f, 0f, 0.7f).normalized;

        targetPosition = targetPoint + sideDirection * distance;
        targetPosition.y = machineBounds.max.y + height;

        rotateOnly = false;
        timer = 0f;
        isFocusing = true;
    }

    public void FocusOnRelation(GameObject relationObject)
    {
        if (relationObject == null)
            return;

        Bounds b;
        if (!TryGetBounds(relationObject, out b))
        {
            FocusOn(relationObject);
            return;
        }

        Vector3 center = b.center;

        // Look at the center of the relation footprint.
        // Lower Y makes the camera frame the whole vertical + horizontal relation better.
        targetPoint = new Vector3(
            center.x,
            Mathf.Lerp(b.min.y, b.max.y, 0.35f),
            center.z
        );

        // Determine the main direction of the relation.
        // We choose a camera position that gives a flatter, more diagram-like view.
        bool relationIsMostlyHorizontalX = b.size.x >= b.size.z;

        Vector3 viewDirection = relationIsMostlyHorizontalX
            ? new Vector3(0f, 0f, -1f)
            : new Vector3(-1f, 0f, 0f);

        float relationSize = Mathf.Max(b.size.x, b.size.z);

        // Further away than before.
        float distance = Mathf.Clamp(
            relationSize * 1.6f,
            relationMinDistance,
            relationMaxDistance + 10f
        );

        // Higher camera position to avoid machines blocking the view.
        float height = Mathf.Clamp(
            relationSize * 0.8f,
            10f,
            28f
        );

        targetPosition = targetPoint + viewDirection * distance;
        targetPosition.y = targetPoint.y + height;

        rotateOnly = false;
        timer = 0f;
        isFocusing = true;
    }

    private bool TryGetBounds(GameObject obj, out Bounds bounds)
    {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>(true);

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
}
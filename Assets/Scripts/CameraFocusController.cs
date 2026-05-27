using UnityEngine;

public class CameraFocusController : MonoBehaviour
{
    public static CameraFocusController Instance;

    public float rotationSpeed = 4f;
    public float focusDuration = 1.2f;

    private Transform camTransform;
    private Vector3 targetPoint;
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

        Vector3 direction = targetPoint - camTransform.position;

        if (direction.sqrMagnitude < 0.001f)
        {
            isFocusing = false;
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);

        camTransform.rotation = Quaternion.Slerp(
            camTransform.rotation,
            targetRotation,
            Time.deltaTime * rotationSpeed
        );

        if (timer >= focusDuration)
            isFocusing = false;
    }

    public void FocusOn(GameObject target)
    {
        if (target == null)
            return;

        Bounds b;
        if (TryGetBounds(target, out b))
            targetPoint = b.center;
        else
            targetPoint = target.transform.position;

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

    public void FocusOnRelation(GameObject relationObject)
    {
        if (relationObject == null)
            return;

        Bounds b;
        if (TryGetBounds(relationObject, out b))
        {
            // Look slightly below the center of the relation,
            // so the camera gets a more horizontal view instead of only looking at the top line.
            targetPoint = new Vector3(
                b.center.x,
                Mathf.Lerp(b.min.y, b.max.y, 0.45f),
                b.center.z
            );
        }
        else
        {
            targetPoint = relationObject.transform.position;
        }

        timer = 0f;
        isFocusing = true;
    }
}
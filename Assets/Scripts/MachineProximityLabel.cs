using UnityEngine;

public class MachineProximityLabel : MonoBehaviour
{
    public float showDistance = 12f;
    public float hideDistance = 14f;

    private Camera cam;
    private Renderer labelRenderer;
    private bool isVisible = false;

    void Start()
    {
        cam = Camera.main;
        labelRenderer = GetComponent<Renderer>();

        SetVisible(false);
    }

    void LateUpdate()
    {
        if (cam == null)
            cam = Camera.main;

        if (cam == null || labelRenderer == null)
            return;

        Transform target = transform.parent != null ? transform.parent : transform;

        float distance = Vector3.Distance(cam.transform.position, target.position);

        if (!isVisible && distance <= showDistance)
            SetVisible(true);

        if (isVisible && distance >= hideDistance)
            SetVisible(false);

        if (isVisible)
        {
            transform.forward = cam.transform.forward;
        }
    }

    private void SetVisible(bool visible)
    {
        isVisible = visible;

        if (labelRenderer != null)
            labelRenderer.enabled = visible;
    }
}

using UnityEngine;

public class BillboardLabel : MonoBehaviour
{
    void LateUpdate()
    {
        if (Camera.main == null) return;

        transform.forward = Camera.main.transform.forward;
    }
}

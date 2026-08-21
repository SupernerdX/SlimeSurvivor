using UnityEngine;

public class IgnoreParentScale : MonoBehaviour
{
    private Vector3 desiredWorldScale;

    void Awake()
    {
        desiredWorldScale = transform.lossyScale;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        Vector3 parentScale = transform.parent.lossyScale;
        transform.localScale = new Vector3(
        desiredWorldScale.x / parentScale.x,
        desiredWorldScale.y / parentScale.y,
        desiredWorldScale.z / parentScale.z
        );
        
    }
}

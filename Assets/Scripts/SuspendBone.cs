using UnityEngine;

[RequireComponent(typeof(SpringFollow))]
public class SuspendBone : MonoBehaviour
{
    [SerializeField] private float heightRatio;

    private SpringFollow springFollow;

    private MeshDeformation meshDeformeration;
    private Vector3 restLocalPosition;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        springFollow = GetComponent<SpringFollow>();
        restLocalPosition = transform.localPosition;
        meshDeformeration = GetComponentInParent<MeshDeformation>();
    }

    void LateUpdate()
    {
        Bounds bounds = meshDeformeration.GetBounds();

        // Calculate the position of the bone relative to the root bounding box
        Vector3 slimeRealtivePos = meshDeformeration.transform.InverseTransformPoint(transform.position);

        // Clamp the position within the bounds of the root bounding box
        slimeRealtivePos.y = Mathf.Clamp(slimeRealtivePos.y, bounds.min.y, bounds.max.y);
        // convert the clamped position back to world space 
        Vector3 pos = meshDeformeration.transform.TransformPoint(slimeRealtivePos);
        // Convert the world position to the local position of the bone's parent
        Vector3 slotRelativePos = transform.parent.InverseTransformPoint(pos);

        Debug.Log(gameObject.name + " bounds min: " + bounds.min.y + " max: " + bounds.max.y);
        springFollow.SetLocalPosition(slotRelativePos);
    }
    public float GetHeightRatio() => heightRatio;
    public Vector3 GetRestPosition() => restLocalPosition;

    public void SnapTo(Vector3 position) => springFollow.SnapToTarget(position);
    public void BeginRelease() => springFollow.BeginRelease();
}

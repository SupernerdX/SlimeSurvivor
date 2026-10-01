using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(SpringFollow))]
public class SuspendBone : MonoBehaviour
{
    [SerializeField] private float heightRatio;


    private SpringFollow springFollow;

    private MeshDeformation meshDeformeration;
    private SuspendedItemSpawner suspendedItemSpawner;
    private Vector3 restLocalPosition;
    private float meshHalfHeight;
    private float distanceToTop;
    private float distanceToBottom;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        springFollow = GetComponent<SpringFollow>();
        restLocalPosition = transform.localPosition;
        meshDeformeration = GetComponentInParent<MeshDeformation>();
        suspendedItemSpawner = GetComponentInParent<SuspendedItemSpawner>();
       
    }
    void LateUpdate()
    {
       // if(suspendedItemSpawner == null) return;
        Bounds bounds = suspendedItemSpawner.GetSpawnerBounds();

        // Calculate the position of the bone relative to the root bounding box
        Vector3 slimeRealtivePos = meshDeformeration.transform.InverseTransformPoint(transform.position);

        // Clamp the position within the bounds of the root bounding box
        slimeRealtivePos.y = Mathf.Clamp(slimeRealtivePos.y, bounds.min.y + distanceToBottom, bounds.max.y - distanceToTop);

        // convert the clamped position back to world space 
        Vector3 pos = meshDeformeration.transform.TransformPoint(slimeRealtivePos);
        // Convert the world position to the local position of the bone's parent
        //Debug.Log(slimeRealtivePos.y + " bounds: " + bounds.min.y + " to " + bounds.max.y);
        Vector3 slotRelativePos = transform.parent.InverseTransformPoint(pos);

        
        springFollow.SetLocalPosition(slotRelativePos);
    }

    public void OnModelAttached()
    {
        Renderer renderer = GetComponentInChildren<Renderer>();
        if(renderer != null)
        {
            distanceToTop = renderer.bounds.max.y - transform.position.y;
            distanceToBottom = transform.position.y - renderer.bounds.min.y;
        }
        else
        {
            Debug.LogError(gameObject.name + " OnModelAttached: NO RENDERER FOUND");
    
        }
    }

    private void OnDrawGizmos()
    {
        Renderer renderer = GetComponentInChildren<Renderer>();
        if(renderer == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(renderer.bounds.center, renderer.bounds.size);
    }
    public float GetHeightRatio() => heightRatio;
    public Vector3 GetRestPosition() => restLocalPosition;


    public void SnapTo(Vector3 position) => springFollow.SnapToTarget(position);
    public void BeginRelease() => springFollow.BeginRelease();
}

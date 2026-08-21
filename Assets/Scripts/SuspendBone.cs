using UnityEngine;

[RequireComponent(typeof(SpringFollow))]
public class SuspendBone : MonoBehaviour
{
    [SerializeField] private float heightRatio;

    private SpringFollow springFollow;
    private Bounds slimeBounds;
    private Vector3 restLocalPosition;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        springFollow = GetComponent<SpringFollow>();
        restLocalPosition = transform.localPosition;
    }

    public void SetBounds(Bounds bounds)
    {
        slimeBounds = bounds;
    }

    void LateUpdate()
    {
        
        Vector3 pos = springFollow.GetLocalPosition();
        pos.y = Mathf.Clamp(pos.y, slimeBounds.min.y, slimeBounds.max.y);
        Debug.Log(gameObject.name + " bounds min: " + slimeBounds.min.y + " max: " + slimeBounds.max.y);
        springFollow.SetLocalPosition(pos);
    }
    public float GetHeightRatio() => heightRatio;
    public Vector3 GetRestPosition() => restLocalPosition;

    public void SnapTo(Vector3 position) => springFollow.SnapToTarget(position);
    public void BeginRelease() => springFollow.BeginRelease();
}

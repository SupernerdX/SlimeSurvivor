using UnityEngine;

[RequireComponent(typeof(SpringFollow))]
public class SuspendBone : MonoBehaviour
{
    [SerializeField] private float heightRatio;

    private SpringFollow springFollow;
    private float minY, maxY;
    private Vector3 restLocalPosition;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        springFollow = GetComponent<SpringFollow>();
        restLocalPosition = transform.localPosition;
    }

    public void SetBounds(float min, float max)
    {
        minY = min; 
        maxY = max;
    }

    void LateUpdate()
    {
        Vector3 pos = springFollow.GetLocalPosition();
        pos.y = Mathf.Clamp(pos.y, minY, maxY);
        springFollow.SetLocalPosition(pos);
    }
    public float GetHeightRatio() => heightRatio;
    public Vector3 GetRestPosition() => restLocalPosition;

    public void SnapTo(Vector3 position) => springFollow.SnapToTarget(position);
    public void BeginRelease() => springFollow.BeginRelease();
}

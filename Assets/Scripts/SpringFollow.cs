using UnityEngine;

public class SpringFollow : MonoBehaviour
{
    [Range(0f, 200f)] [SerializeField] private float dragStifness;
    [Range(0f, 1f)] [SerializeField] private float dragDamping;


    private Vector3 velocity;
    private Vector3 intialLocalPosition;
    private bool isReleasing;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        intialLocalPosition = transform.localPosition;

    }

    // Update is called once per frame
    void LateUpdate()
    {
        UpdateDrag();
    }   

    private void UpdateDrag()
    {
        if(!isReleasing) return;

        Vector3 gap = intialLocalPosition  - transform.localPosition;
        velocity += dragStifness * gap * Time.deltaTime;
        velocity *= Mathf.Exp(-dragDamping * Time.deltaTime);
        transform.localPosition += velocity * Time.deltaTime; 
    }
    public void SnapToTarget(Vector3 position)
    {
        isReleasing = false;
        transform.localPosition = position;
        velocity = Vector3.zero;
    }
    public void BeginRelease()
    {
        isReleasing = true;
    }
    public void ZeroVelocityY()
    {
        velocity.y = 0f;
    }

    public Vector3 GetLocalPosition() => transform.localPosition;
    public void SetLocalPosition(Vector3 position) => transform.localPosition = position;
}

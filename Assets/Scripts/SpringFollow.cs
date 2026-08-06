using UnityEngine;

public class SpringFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [Range(0f, 200f)] [SerializeField] private float dragStifness;
    [Range(0f, 30f)] [SerializeField] private float dragDamping;
    [SerializeField] private float SuspensionHeight = 0.5f;

    private Vector3 velocity;
    private Vector3 restOffset;
    private Vector3 intialLocalPosition;
    private Vector3 intialTargetScale;
  

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        intialLocalPosition = transform.localPosition;
        intialTargetScale = target.localScale;

    }

    // Update is called once per frame
    void LateUpdate()
    {
        UpdateDrag();
    }   

    private void UpdateDrag()
    {
        float scaleRatio = target.localScale.y / intialTargetScale.y;
        float verticalAdjustment = (scaleRatio - 1f) * SuspensionHeight;
        Vector3 desiredLocalPosition = intialLocalPosition;
        desiredLocalPosition.y += verticalAdjustment;
        Vector3 gap = desiredLocalPosition  - transform.localPosition;
        velocity += dragStifness * gap * Time.deltaTime;
        velocity *= Mathf.Exp(-dragDamping * Time.deltaTime);
        transform.localPosition += velocity * Time.deltaTime; 
    }
}

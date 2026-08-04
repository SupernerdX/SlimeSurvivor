using UnityEngine;

public class SpringFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [Range(0f, 200f)] [SerializeField] private float dragStifness;
    [Range(0f, 1f)] [SerializeField] private float dragDamping;
    
    private Vector3 velocity;
    private Vector3 restOffset;
    private Vector3 intialScale;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        restOffset = transform.position - target.position;
        intialScale = target.localScale;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateDrag();
        Debug.Log("scaleY: " + target.localScale.y);
    }

    private void UpdateDrag()
    {
        float scaleRatio = transform.localScale.y / intialScale.y;
        Vector3 scaledOffset = new Vector3 (restOffset.x, restOffset.y * scaleRatio, restOffset.z);
        Vector3 gap = target.position + scaledOffset - transform.position;
        velocity += dragStifness * Time.deltaTime * gap;
        velocity *= 1f - dragDamping;
        transform.position += velocity * Time.deltaTime; 
    }
}

using UnityEngine;

public class BodyContactHandler : MonoBehaviour
{
    [Header("Body Contact")]
    [SerializeField, Min(0.05f)] private float _contactRadius = 1.3f; 
    [SerializeField, Min(0.0f)] private float _pushAcceleration = 12f;
    [SerializeField] LayerMask _contactLayerMask;

    private int _effectiveLayerMask;
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _contactRadius);
    }

    void Awake()
    {
        _effectiveLayerMask = _contactLayerMask & ~(1 << gameObject.layer);
    }

    private void FixedUpdate()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, _contactRadius, _effectiveLayerMask);
        foreach (Collider collider in colliders)
        {
            Vector3 offset = collider.transform.position - transform.position;
            var pushDirection = offset.sqrMagnitude > 0.01f ? offset.normalized : transform.forward;
            if(pushDirection.y < 0f)
            {
                pushDirection.y = 0f;
            }
            Rigidbody rb = collider.attachedRigidbody;
            if(rb != null)
            {
                rb.AddForce(pushDirection * _pushAcceleration, ForceMode.Acceleration);
            }

        }
            
        
    }
    

}

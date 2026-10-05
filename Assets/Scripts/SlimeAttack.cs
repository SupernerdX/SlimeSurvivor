using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeAttack : MonoBehaviour
{
    [Header("Landing Attack")]
    [SerializeField, Min(0.05f)] private float _landingHitRadius = 0.9f;
    [SerializeField, Min(0.0f)] private float _landingHitDepth = 1.5f;
    [SerializeField, Min(0.0f)] private float _landingHitAboveTolerance = 0.25f;
   // [SerializeField, Min(0.05f)] private float _contactDamageInterval = 1.0f;
    [SerializeField, Min(0.0f)] private float _contactDamage = 1f; 
    [SerializeField] private LayerMask _damageableLayers;
    [SerializeField] private GameObject _slimeTrailPrefab;
    [SerializeField, Min(0.05f)] private float _slimeTrailCooldownDuration = 1.0f;

    [Header("Debug")]
    [SerializeField, Min(0.05f)] private float _debugRadius = 1.3f; 

    private GridMovement _gridMovement;
    private SuspendedItemSpawner _suspendedItemsSpawner;
    private MeshDeformation _meshDeformation;
    private Transform meshTransform;
    private float _nextSlimeTrailTime;

    //private Dictionary<IDamageable, float> _contactDamageTimers = new();

    void Awake()
    {   _gridMovement = GetComponent<GridMovement>();
        _suspendedItemsSpawner = GetComponentInChildren<SuspendedItemSpawner>();
        _meshDeformation = GetComponentInChildren<MeshDeformation>();
        meshTransform  = gameObject.GetComponentInChildren<MeshRenderer>().transform;
        _gridMovement.OnLanded += HandleLanded; 
    }

    void OnDestroy()
    {
        if (_gridMovement != null)
        {
            _gridMovement.OnLanded -= HandleLanded;
        }

    }
    /*
    private void FixedUpdate()
    {
         if(_gridMovement.GetIsMoving()) 
            return; 

        Collider[] colliders = GetNerabyColliders();
        foreach (Collider collider in colliders)
        {
        
        if(!collider.TryGetComponent(out IDamageable playerTarget)) 
            continue;
        _contactDamageTimers.TryGetValue( playerTarget, out float nextAllowedTime);

        if(Time.time >= nextAllowedTime && TryApplyDamageToTarget(playerTarget, _contactDamage))
            {
                _contactDamageTimers[playerTarget] = Time.time + _contactDamageInterval;
            }
        
        }
    }
    */

    private Collider[] GetNerabyColliders()
    {
        return Physics.OverlapSphere(transform.position, _debugRadius, _damageableLayers);
    }

    private bool TryApplyDamageToTarget(IDamageable target, float contactDamage)
    {
        if (target == null) return false;
        target.TakeDamage(contactDamage);
        return true;
    }

    private void HandleLanded()
    {
        Collider[] colliders = GetNerabyColliders();
        foreach (Collider collider in colliders)
        {
             if(!IsInsideLandingHitArea(collider.transform.position))
                continue;

        collider.TryGetComponent(out IDamageable target);
        if(TryApplyDamageToTarget(target, _contactDamage))
            _suspendedItemsSpawner?.CreateSlots(1);
            if (_meshDeformation != null)
                _meshDeformation.TryToGrow();
        
        }
        
        if (_slimeTrailPrefab != null && Time.time >= _nextSlimeTrailTime)
        {
            GameObject slimeTrail = Instantiate(_slimeTrailPrefab, transform.position, transform.rotation);
            slimeTrail.transform.localScale = new Vector3(meshTransform.localScale.x, slimeTrail.transform.localScale.y, meshTransform.localScale.z);
            _nextSlimeTrailTime = Time.time + _slimeTrailCooldownDuration;
        }
        
    
    }

    private bool IsInsideLandingHitArea(Vector3 targetPosition)
    {
        Vector3 offset = targetPosition - transform.position;
        if (offset.y > _landingHitAboveTolerance || offset.y < -_landingHitDepth)
            return false;

        offset.y = 0.0f;
        return offset.sqrMagnitude <= _landingHitRadius * _landingHitRadius;
    }

    void OnDrawGizmos()
    {
        DrawAttackGizmo();
    }

    void DrawAttackGizmo()
    {
        float radius = Mathf.Max(0.05f, _landingHitRadius);
        Vector3 topCenter = transform.position + Vector3.up * _landingHitAboveTolerance;
        Vector3 bottomCenter = transform.position - Vector3.up * _landingHitDepth;

        DrawWireDisc(topCenter, radius, 32);
        DrawWireDisc(bottomCenter, radius, 32);
        Gizmos.DrawLine(topCenter + Vector3.forward * radius, bottomCenter + Vector3.forward * radius);
        Gizmos.DrawLine(topCenter + Vector3.back * radius, bottomCenter + Vector3.back * radius);
        Gizmos.DrawLine(topCenter + Vector3.right * radius, bottomCenter + Vector3.right * radius);
        Gizmos.DrawLine(topCenter + Vector3.left * radius, bottomCenter + Vector3.left * radius);

        Color previousColor = Gizmos.color;
        Gizmos.color = Color.cyan;
        DrawWireDisc(transform.position, Mathf.Max(0.05f, _debugRadius), 32);
        Gizmos.color = previousColor;
    }

    private void DrawWireDisc(Vector3 center, float radius, int segments)
    {
            Vector3 previous = center + Vector3.forward * radius;
        for (int index = 1; index <= segments; index++)
        {
            float angle = index * 360.0f / segments;
            Vector3 next = center + Quaternion.AngleAxis(angle, Vector3.up) * Vector3.forward * radius;
            Gizmos.DrawLine(previous, next);
            previous = next;
        }
    }
}


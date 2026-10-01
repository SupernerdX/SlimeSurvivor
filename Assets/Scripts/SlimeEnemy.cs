using System;
using System.Collections;

using UnityEngine;

public class SlimeEnemy : MonoBehaviour
{
    [SerializeField] private GridMovement _gridMovement;
    [SerializeField] private SuspendedItemSpawner _suspendedItemsSpawner;

    [Header("Landing Attack")]
    [SerializeField, Min(0.05f)] private float _landingHitRadius = 0.9f;
    [SerializeField, Min(0.0f)] private float _landingHitDepth = 1.5f;
    [SerializeField, Min(0.0f)] private float _landingHitAboveTolerance = 0.25f;

    [Header("Body Contact")]
    [SerializeField, Min(0.05f)] private float _contactRadius = 1.3f; 
    [SerializeField, Min(0.0f)] private float _pushAcceleration = 12f;
    [SerializeField, Min(0.0f)] private float _contactDamage = 1f; 
    [SerializeField, Min(0.05f)] private float _contactDamageInterval = 1.0f;

    private bool _isPlayerInContact; 
    private float _nextContactDamageTime; 
    private Vector3 _lastPushDirection; 


        void Awake()
    {
        _gridMovement.OnLanded += HandleLanded;
        _lastPushDirection = transform.forward; 
    }

    void OnDestroy()
    {
        if (_gridMovement != null)
        {
            _gridMovement.OnLanded -= HandleLanded;
        }

    }

    private void FixedUpdate()
    {
        if(_gridMovement.GetIsMoving()) 
            return;  

        /*
        if(!TryGetCurrentTargetPosition(CombatTarget.Player, out Vector3 playerPos))
        {
            _isPlayerInContact = false;
            return;
        }

        Vector3 offset = playerPos - transform.position;
        offset.y = 0;

        float sqrDistance = offset.sqrMagnitude;
        if(sqrDistance > _contactRadius * _contactRadius)
        {
            
        _isPlayerInContact = false; 
        return;

        }
        
        
        Vector3 dir = sqrDistance > 0.0001f ? offset.normalized : _lastPushDirection; 
        _lastPushDirection = dir; 


       // _gameManager.TryAddForceToPlayer(dir* _pushAcceleration, ForceMode.Acceleration);
        
        bool justEntered = !_isPlayerInContact;
        _isPlayerInContact = true;

        if((justEntered || Time.time >= _nextContactDamageTime) &&
        TryApplyDamageToTarget(CombatTarget.Player, _contactDamage))
            _nextContactDamageTime = Time.time + _contactDamageInterval;

        */

    }
    
    private void HandleLanded()
    {
        /*
        CombatTarget playerTarget = CombatTarget.Player;
        if (!TryGetCurrentTargetPosition(playerTarget, out Vector3 targetPosition) ||
            !IsInsideLandingHitArea(targetPosition))
            return;

        if (TryApplyDamageToTarget(playerTarget))
            _suspendedItemsSpawner?.CreateSlots(1);
        */
    }

    private bool IsInsideLandingHitArea(Vector3 targetPosition)
    {
        Vector3 offset = targetPosition - transform.position;
        if (offset.y > _landingHitAboveTolerance || offset.y < -_landingHitDepth)
            return false;

        offset.y = 0.0f;
        return offset.sqrMagnitude <= _landingHitRadius * _landingHitRadius;
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
        DrawWireDisc(transform.position, Mathf.Max(0.05f, _contactRadius), 32);
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


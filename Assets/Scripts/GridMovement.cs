using System;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

#if UNITY_EDITOR
using UnityEditor; 
#endif
public class GridMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private int gridSize = 1;
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float groundPivotOffset = 1.0f;
    [SerializeField, Min(0.1f)] private float _navMeshSnapDistance = 4.0f;
    [SerializeField] float jumpSpeedMultiplyer = 1f;
    [SerializeField] bool showFineTuneControlls;

    [Header("Fine Tuning Jump")]
    [SerializeField] private float moveDuration = 0.5f;
    [SerializeField] private float jumpAnticipationDuration = 0.1f;
    [SerializeField] private float landingRecoverDuration = 0.08f;
    

    [Header("Whole mesh squash and stretch")]
    [Tooltip("Whole-object squash/stretch driven by this script's own movement timing (jump, anticipation, landing). Separate from MeshDeformation's per-vertex squash/stretch multipliers, which weight and refine this same value further.")]
    [SerializeField] private float strechFactor = 0.2f;

    [SerializeField] private InputAction moveAction;
    
    private bool hasTarget;
    private Vector2 CurrentLocation; 
    private Vector2 TargetLocation; 
    private Vector2 PendingDirection;
    private Vector3 baseScale;
    private float moveAlpha;
    private float BaseHeight;

    private float targetBaseHeight; 
    private float currnetGroundPivotOffset; 
    private bool isIntialized;

    private float currentStretchAmount;
    private float jumpAnticipationAlpha;
    private float anticipationStartStretch;
    private float landingRecoverAlpha;
    private float landingStartStretch;
    

    private MeshDeformation meshDeformation;

    private bool IsMoving; 
    private bool HasPendingInput;
    private bool isAnticipating;
    private bool isRecoveringFromLanding;

    public event Action<Vector3> LandingApproaching;
    public event Action OnLanded;

    // Start is called once before the first execution of Update after the MonoBehaviour is created

    void Awake()
    {
        currnetGroundPivotOffset = groundPivotOffset;
    }
    void OnEnable()
    {
        moveAction.Enable();
        moveAction.performed += OnMoved;
    }
    void OnDisable()
    {
        moveAction.Disable();
        moveAction.performed -= OnMoved;
    }
    void Start()
    {
       
         baseScale = transform.localScale;
         CurrentLocation = new Vector2(Mathf.Round(transform.position.x / gridSize), 
         Mathf.Round(transform.position.z / gridSize)
         );
         meshDeformation = GetComponentInChildren<MeshDeformation>();
         currentStretchAmount = 0f;
         ApplyScale(0f);
        
        isIntialized = true; 

        if(TryGetGroundSurface(transform.position, out Vector3 groundSurface))
        {
            ResetToGround(groundSurface);
        }
        else
        {
            BaseHeight = transform.position.y;
            targetBaseHeight = BaseHeight;
        }
    }

    // Update is called once per frame
    void Update()
    {

        if(isAnticipating)
        {
            UpdateJumpAnticipation();
            return;
        }

        if(!IsMoving)
        {
            UpdateLandingRecovery();
            return;
        }
        float effectiveDuration = moveDuration / jumpSpeedMultiplyer; 
        moveAlpha += Time.deltaTime / effectiveDuration;
        moveAlpha = Mathf.Clamp(moveAlpha, 0, 1);
        float smoothMoveAlpha = Mathf.SmoothStep(0f, 1f, moveAlpha);
        Vector3 newLocation = Vector3.Lerp(GridToWorld(CurrentLocation), GridToWorld(TargetLocation), smoothMoveAlpha);
        float groundHeight = Mathf.Lerp(BaseHeight,targetBaseHeight, smoothMoveAlpha);
        newLocation.y = groundHeight + Mathf.Sin(moveAlpha * Mathf.PI) * jumpHeight;
        currentStretchAmount = -Mathf.Cos((1- moveAlpha) * Mathf.PI * 2f) * strechFactor; 
        ApplyScale(currentStretchAmount);
        transform.position = newLocation; 

        if (moveAlpha >= 0.5f)
        {
            Vector3 landingPosition = GridToWorld(TargetLocation);
            landingPosition.y = targetBaseHeight;
            LandingApproaching?.Invoke(landingPosition);
        }

        if(moveAlpha >= 1)
        {
            Vector3 landingPosition = GridToWorld(TargetLocation);
            landingPosition.y = targetBaseHeight;
            transform.position = landingPosition;
            BaseHeight = targetBaseHeight;
            CurrentLocation = TargetLocation;
            IsMoving = false; 
            BeginLandingRecovery();
            if(HasPendingInput)
            {
                Vector2 queuedDirection = PendingDirection;
                HasPendingInput = false;
                BeginJumpAnticipation(queuedDirection);
            }
        }
    }

    
    public void UpdateBaseScale(Vector3 newBaseScale)
    {
        if(meshDeformation != null)
        {
            float deltaY = -meshDeformation.MinY *(newBaseScale.y - baseScale.y);
            currnetGroundPivotOffset += deltaY;
            transform.position += new Vector3 (0f, deltaY, 0f);
            
            if(IsMoving)
            {
                BaseHeight += deltaY;
                targetBaseHeight += deltaY;
            }
        }

        baseScale = newBaseScale;
        float strechAmount = 0f;
        if(IsMoving)
        {
            strechAmount = -Mathf.Cos((1f - moveAlpha) * Mathf.PI * 2f) * strechFactor;
        }
        else if(isAnticipating)
        {
            strechAmount = currentStretchAmount;
        }
        else if(isRecoveringFromLanding)
        {
            strechAmount = currentStretchAmount;
        }

        ApplyScale(strechAmount);
    }

    private bool TryGetGroundSurface(Vector3 rootPosition, out Vector3 groundSurface)
    {
        groundSurface = default; 


        Vector3 samplePosition = rootPosition;
        samplePosition.y -= currnetGroundPivotOffset;

        return NavMesh.SamplePosition(
            samplePosition,
            out NavMeshHit hit,
            _navMeshSnapDistance, 
            NavMesh.AllAreas) 
            && SetGroundSurface(hit.position, out groundSurface);
        
    }
    private static bool SetGroundSurface(Vector3 position, out Vector3 groundSurface)
    {
        groundSurface = position;
        return true;
    }

    public void ResetToGround(Vector3 navMeshSurfacePosition)
    {
        Vector3 rootPosition = navMeshSurfacePosition;
        rootPosition.y += currnetGroundPivotOffset; 
        transform.position = rootPosition;

        CurrentLocation = new Vector2(
            Mathf.Round(rootPosition.x / gridSize), 
            Mathf.Round(rootPosition.z / gridSize));

        TargetLocation = CurrentLocation;
        BaseHeight = rootPosition.y; 
        targetBaseHeight = rootPosition.y; 

        moveAlpha = 0.0f; 
        jumpAnticipationAlpha = 0.0f; 
        landingRecoverAlpha = 0.0f; 
        currentStretchAmount = 0.0f;

        IsMoving = false;
        HasPendingInput = false;
        isAnticipating = false;
        isRecoveringFromLanding = false;
        hasTarget = false;

        if (isIntialized)
        ApplyScale(0.0f); 

    }

    public Vector3 GetBaseScale() => baseScale;

    public Vector3 GetRootPosition()
    {
        Vector3 rootPosition =GridToWorld(CurrentLocation);
        rootPosition.y = BaseHeight;
        return rootPosition;
    } 

    public float GetStretchAmount() => currentStretchAmount;

    public bool GetIsMoving() => IsMoving;
    public bool GetIsRecoveringFromLanding() =>  isRecoveringFromLanding;


    private Vector3 GridToWorld(Vector2 gridLocation)
    {
         Vector3 newWorldLocation = new Vector3(gridLocation.x * gridSize, 0f, gridLocation.y * gridSize);

         return newWorldLocation;
    }

    private void BeginJumpAnticipation(Vector2 direction)
    {
        if(direction == Vector2.zero) return;

        TargetLocation = CurrentLocation + direction;
        if(TryGetGroundSurface(transform.position, out Vector3 startSurface))
        {
            BaseHeight = startSurface.y + currnetGroundPivotOffset;
        }
        else
        {
            BaseHeight = transform.position.y;
        }
        Vector3 targetWorldPosition = GridToWorld(TargetLocation);
        targetWorldPosition.y = BaseHeight;

        if(TryGetGroundSurface(targetWorldPosition, out Vector3 targetSurface))
        {
            targetBaseHeight = targetSurface.y + currnetGroundPivotOffset;
        }
        else
        {
            targetBaseHeight = BaseHeight;
        }

        anticipationStartStretch = currentStretchAmount;
        jumpAnticipationAlpha = 0f;
        IsMoving = false;
        isAnticipating = true;
        isRecoveringFromLanding = false;
    }

    private void UpdateJumpAnticipation()
    {
        float effectiveDuration = jumpAnticipationDuration / jumpSpeedMultiplyer;
        float safeAnticipationDuration = Mathf.Max(effectiveDuration, Mathf.Epsilon);
        jumpAnticipationAlpha += Time.deltaTime / safeAnticipationDuration;
        jumpAnticipationAlpha = Mathf.Clamp01(jumpAnticipationAlpha);

        float smoothAlpha = Mathf.SmoothStep(0f, 1f, jumpAnticipationAlpha);
        currentStretchAmount = Mathf.Lerp(anticipationStartStretch, -strechFactor, smoothAlpha);
        ApplyScale(currentStretchAmount);

        if(jumpAnticipationAlpha >= 1f)
        {
            isAnticipating = false;
            IsMoving = true;
            moveAlpha = 0f;
        }
    }

    private void ApplyScale(float strechAmount)
    {
        transform.localScale = new Vector3(
        baseScale.x - strechAmount,
        baseScale.y + strechAmount, 
        baseScale.z - strechAmount);
    }

    private void BeginLandingRecovery()
    {
        landingStartStretch = currentStretchAmount;
        landingRecoverAlpha = 0f;
        isRecoveringFromLanding = true;
        meshDeformation?.TriggerWaveRippleEffect();
        OnLanded?.Invoke();
    }

    private void UpdateLandingRecovery()
    {
        if(!isRecoveringFromLanding) return;
        float effectiveDuration = landingRecoverDuration / jumpSpeedMultiplyer;
        float rippleDuration = meshDeformation != null ? meshDeformation.GetWaveTravelDuration() : 0f;
        float safeLandingRecoverDuration = Mathf.Max(effectiveDuration, rippleDuration, Mathf.Epsilon);
        landingRecoverAlpha += Time.deltaTime / safeLandingRecoverDuration;
        landingRecoverAlpha = Mathf.Clamp01(landingRecoverAlpha);
        currentStretchAmount = Mathf.Lerp(landingStartStretch, 0f, landingRecoverAlpha);
        ApplyScale(currentStretchAmount);

        if(landingRecoverAlpha >= 1f)
        {
            currentStretchAmount = 0f;
            isRecoveringFromLanding = false;
            ApplyScale(0f);
        }
    }

    private void OnMoved(InputAction.CallbackContext context)
    {
        Vector2 rawInput = context.ReadValue<Vector2>();
        Vector2 moveInput = SnapToGridDirection(rawInput.x, rawInput.y);
        if(moveInput == Vector2.zero) return;

        if(!IsMoving && !isAnticipating && !HasPendingInput)
        {
            BeginJumpAnticipation(moveInput); 
        }
        else
        {
            PendingDirection = moveInput; 
            HasPendingInput = true;
        }
    }

    Vector2 SnapToGridDirection(float x, float z)
    {
        float absX = Mathf.Abs(x);
        float absZ = Mathf.Abs(z); 

        bool xIsMeaningful = absX  > absZ * 0.5f;
        bool zIsMeaningful = absZ > absX * 0.5f; 

        float snappedX = xIsMeaningful ? Mathf.Sign(x) : 0f;
        float snappedZ = zIsMeaningful ? Mathf.Sign(z) : 0f;

        return new Vector2(snappedX, snappedZ);
            
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(GridMovement))]
public class GridMovementEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("gridSize"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("jumpHeight"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("groundPivotOffset"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_navMeshSnapDistance"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("jumpSpeedMultiplyer"));
        
        EditorGUILayout.PropertyField(serializedObject.FindProperty("strechFactor"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("moveAction"));
        


        SerializedProperty showAdvanced = serializedObject.FindProperty("showFineTuneControlls");
        EditorGUILayout.PropertyField(showAdvanced); 

        if(showAdvanced.boolValue)
        {
             
            EditorGUILayout.PropertyField(serializedObject.FindProperty("moveDuration"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("jumpAnticipationDuration"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("landingRecoverDuration"));
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif
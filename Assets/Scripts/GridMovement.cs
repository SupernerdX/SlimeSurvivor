using UnityEngine;
using UnityEngine.InputSystem;

public class GridMovement : MonoBehaviour
{
    
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private int gridSize = 1;
    [SerializeField] private float moveDuration = 0.5f;
    [SerializeField] private float jumpAnticipationDuration = 0.1f;
    [SerializeField] private float landingRecoverDuration = 0.08f;

    [Header("Whole mesh squash and stretch")]
    [Tooltip("Whole-object squash/stretch driven by this script's own movement timing (jump, anticipation, landing). Separate from MeshDeformation's per-vertex squash/stretch multipliers, which weight and refine this same value further.")]
    [SerializeField] private float strechFactor = 0.2f;
    
    [SerializeField] private InputAction moveAction;

    private Vector2 CurrentLocation; 
    private Vector2 TargetLocation;
    private float moveAlpha;
    private float BaseHeight;
    private Vector3 baseScale;
    private float currentStretchAmount;
    private float jumpAnticipationAlpha;
    private float anticipationStartStretch;
    private float landingRecoverAlpha;
    private float landingStartStretch;
    private Vector2 PendingDirection;
    private MeshDeformation meshDeformation;

    private bool IsMoving; 
    private bool HasPendingInput;
    private bool isAnticipating;
    private bool isRecoveringFromLanding;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void OnEnable()
    {
        moveAction.Enable(); 
        moveAction.performed += OnMoved;
    }

    private void OnDisable()
    {
        moveAction.Disable(); 
        moveAction.performed -= OnMoved;
    }

    void Start()
    {
       // Debug.Log(GetComponent<MeshFilter>().mesh.vertexCount);
         baseScale = transform.localScale;
         CurrentLocation = transform.localPosition;
         meshDeformation = GetComponentInChildren<MeshDeformation>();
         currentStretchAmount = 0f;
         ApplyScale(0f);
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
        
        moveAlpha += Time.deltaTime / moveDuration;
        moveAlpha = Mathf.Clamp(moveAlpha, 0, 1);
        float smoothMoveAlpha = Mathf.SmoothStep(0f, 1f, moveAlpha);
        Vector3 newLocation = Vector3.Lerp(GridToWorld(CurrentLocation), GridToWorld(TargetLocation), smoothMoveAlpha);
        newLocation.y =BaseHeight + Mathf.Sin(moveAlpha * Mathf.PI) * jumpHeight;
        currentStretchAmount = -Mathf.Cos((1- moveAlpha) * Mathf.PI * 2f) * strechFactor; 
        ApplyScale(currentStretchAmount);
        transform.position = newLocation; 

        if(moveAlpha >= 1)
        {
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

    public Vector3 GetBaseScale()
    {
        return baseScale;
    }

    public Vector3 GetRootPosition()
    {
        return GridToWorld(CurrentLocation);
    }

    public float GetStretchAmount()
    {
        return currentStretchAmount;
    }

    private Vector3 GridToWorld(Vector2 gridLocation)
    {
         Vector3 newWorldLocation = new Vector3(gridLocation.x * gridSize, 0f, gridLocation.y * gridSize);

         return newWorldLocation;
    }

    private void BeginJumpAnticipation(Vector2 direction)
    {
        if(direction == Vector2.zero) return;

        TargetLocation = CurrentLocation + direction;
        BaseHeight = transform.position.y;
        anticipationStartStretch = currentStretchAmount;
        jumpAnticipationAlpha = 0f;
        IsMoving = false;
        isAnticipating = true;
        isRecoveringFromLanding = false;
    }

    private void UpdateJumpAnticipation()
    {
        float safeAnticipationDuration = Mathf.Max(jumpAnticipationDuration, Mathf.Epsilon);
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
    }

    private void UpdateLandingRecovery()
    {
        if(!isRecoveringFromLanding) return;

        float rippleDuration = meshDeformation != null ? meshDeformation.GetWaveTravelDuration() : 0f;
        float safeLandingRecoverDuration = Mathf.Max(landingRecoverDuration, rippleDuration, Mathf.Epsilon);
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
        Vector2 moveInput = context.ReadValue<Vector2>();
        if(moveInput == Vector2.zero) return;

        if (!IsMoving && !isAnticipating && !HasPendingInput)
        {
            BeginJumpAnticipation(moveInput);
        } 
        else
        {
            PendingDirection = moveInput;
            HasPendingInput = true;
        }

    }
}

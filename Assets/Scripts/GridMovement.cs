using UnityEngine;
using UnityEngine.InputSystem;

public class GridMovement : MonoBehaviour
{
    
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private int gridSize = 1;
    [SerializeField] private float moveDuration = 0.5f;
    [SerializeField] private float strechFactor = 0.2f;
    [SerializeField] private float landingRecoverDuration = 0.08f;
    
    [SerializeField] private InputAction moveAction;

    private Vector2 CurrentLocation; 
    private Vector2 TargetLocation;
    private float moveAlpha;
    private float BaseHeight;
    private Vector3 baseScale;
    private float currentStretchAmount;
    private float landingRecoverAlpha;
    private float landingStartStretch;
    private Vector2 PendingDirection;

    private bool IsMoving; 
    private bool HasPendingInput;
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
         currentStretchAmount = 0f;
         ApplyScale(0f);
    }

    // Update is called once per frame
    void Update()
    {
        if(!IsMoving)
        {
            UpdateLandingRecovery();
            return;
        }
        
        moveAlpha += Time.deltaTime / moveDuration;
        moveAlpha = Mathf.Clamp(moveAlpha, 0, 1);
        Vector3 newLocation = Vector3.Lerp(GridToWorld(CurrentLocation), GridToWorld(TargetLocation), moveAlpha);
        newLocation.y =BaseHeight + Mathf.Sin(moveAlpha * Mathf.PI) * jumpHeight;
        float strechAmount = -Mathf.Cos((1- moveAlpha) * Mathf.PI * 2f) * strechFactor; 
        currentStretchAmount = strechAmount;
        ApplyScale(strechAmount);
        transform.position = newLocation; 

        if(moveAlpha >= 1)
        {
            CurrentLocation = TargetLocation;
            IsMoving = false; 
            BeginLandingRecovery();
            if(HasPendingInput)
            {
                TargetLocation = CurrentLocation + PendingDirection;
                BaseHeight = transform.position.y;
                IsMoving = true; 
                HasPendingInput = false;
                isRecoveringFromLanding = false;
                moveAlpha = 0;
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

    private Vector3 GridToWorld(Vector2 gridLocation)
    {
         Vector3 newWorldLocation = new Vector3(gridLocation.x * gridSize, 0f, gridLocation.y * gridSize);

         return newWorldLocation;
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
    }

    private void UpdateLandingRecovery()
    {
        if(!isRecoveringFromLanding) return;

        float safeLandingRecoverDuration = Mathf.Max(landingRecoverDuration, Mathf.Epsilon);
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
        if (!IsMoving && !HasPendingInput)
        {
        Vector2 moveInput = context.ReadValue<Vector2>();
        TargetLocation = CurrentLocation + moveInput;
        BaseHeight = transform.position.y;
        IsMoving = true; 
        isRecoveringFromLanding = false;
        moveAlpha = 0;
        } 
        else
        {
            PendingDirection = context.ReadValue<Vector2>();
            HasPendingInput = true;
        }

    }
}

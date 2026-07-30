using UnityEngine;
using UnityEngine.InputSystem;

public class GridMovement : MonoBehaviour
{
    
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private int gridSize = 1;
    [SerializeField] private float moveDuration = 0.5f;
    [SerializeField] private float strechFactor = 0.2f;
    
    [SerializeField] private InputAction moveAction;

    private Vector2 CurrentLocation; 
    private Vector2 TargetLocation;
    private float moveAlpha;
    private float BaseHeight;
    private Vector3 baseScale;
    private Vector2 PendingDirection;

    private bool IsMoving; 
    private bool HasPendingInput;

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
         baseScale = transform.localScale;
    }

    // Update is called once per frame
    void Update()
    {
        if(!IsMoving) return;
        
        moveAlpha += Time.deltaTime / moveDuration;
        moveAlpha = Mathf.Clamp(moveAlpha, 0, 1);
        Vector3 newLocation = Vector3.Lerp(GridToWorld(CurrentLocation), GridToWorld(TargetLocation), moveAlpha);
        newLocation.y =BaseHeight + Mathf.Sin(moveAlpha * Mathf.PI) * jumpHeight;
        float strechAmount = -Mathf.Cos((1- moveAlpha) * Mathf.PI * 2f) * strechFactor; 
        transform.localScale = new Vector3(
        baseScale.x - strechAmount,
        baseScale.y + strechAmount, 
        baseScale.z - strechAmount);
        transform.position = newLocation; 

        if(moveAlpha >= 1)
        {
            CurrentLocation = TargetLocation;
            IsMoving = false; 
            transform.localScale = baseScale;
            if(HasPendingInput)
            {
                TargetLocation = CurrentLocation + PendingDirection;
                BaseHeight = transform.position.y;
                IsMoving = true; 
                HasPendingInput = false;
                moveAlpha = 0;
            }
        }
    }

    private Vector3 GridToWorld(Vector2 gridLocation)
    {
         Vector3 newWorldLocation = new Vector3(gridLocation.x * gridSize, 0f, gridLocation.y * gridSize);

         return newWorldLocation;
    }

    private void OnMoved(InputAction.CallbackContext context)
    {
        if (!IsMoving && !HasPendingInput)
        {
        Vector2 moveInput = context.ReadValue<Vector2>();
        TargetLocation = CurrentLocation + moveInput;
        BaseHeight = transform.position.y;
        IsMoving = true; 
        moveAlpha = 0;
        } 
        else
        {
            PendingDirection = context.ReadValue<Vector2>();
            HasPendingInput = true;
        }

    }
}

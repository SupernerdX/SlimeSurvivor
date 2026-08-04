using UnityEngine;

public class MeshDeformation : MonoBehaviour
{
    [SerializeField] private Transform otherSlime;
    [SerializeField] private float StrechThreshold;
    [SerializeField] private float consumeDuration;
    [SerializeField] private float relaxDuration;
    [SerializeField] private bool isGrowing;
    [SerializeField] private float growthMultiplier;
    
    [Header("Squash and Stretch Multipliers")]
    [SerializeField] private float squashEffectMultiplier = 1f;
    [SerializeField] private float stretchEffectMultiplier = 1f;
    [Range(0f, 200f)] [SerializeField] private float dragStifness = 50f;
    [Range(0f, 1f)] [SerializeField] private float dragDamping = 0.5f;

    [Header("wave ripple effect")]
    [SerializeField, Min(0.01f)] private float waveTravelRate = 1.2f;
    [Range(0f, 2f)] [SerializeField] private float waveWidth = 0.25f;
    [Range(0f, 1f)] [SerializeField] private float waveStrength = 0.1f;

    private Mesh mesh;
    private GridMovement gridMovement;
    private bool HasBeenConsumed;
    private bool isRelaxing;
    private bool hasDeformedVertices;

    private float minY; 
    private float maxY;
    private float maxHorizontalDistance;

    private float waveFront; 
    private bool isWaving;



    private float scaleAlpha;
    private float stretchAlpha;
    private float relaxAlpha;
    private float bounceAlpha;
    private Vector3 startScale;
    private Vector3 targetScale;

    private Vector3 [] orginalVerticePositions;
    private Vector3 [] StretchedVerticePositions;
    private Vector3 [] RelaxedVerticePositions;
    
    private Vector3 trailingVelocity;
    private Vector3 trailingPosition;
    private Vector3 dragOffset;


    // Start is called once before the first execution of Update after the MonoBehaviour is created

    void Start()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        gridMovement = GetComponentInParent<GridMovement>();
        orginalVerticePositions = mesh.vertices;
        trailingPosition = gridMovement != null ? gridMovement.transform.position : Vector3.zero;

        StretchedVerticePositions = (Vector3[])orginalVerticePositions.Clone();
        RelaxedVerticePositions = (Vector3[])orginalVerticePositions.Clone();

        minY = orginalVerticePositions[0].y;
        maxY = orginalVerticePositions[0].y;

        for(int i = 1; i < orginalVerticePositions.Length; i++)
        {
            minY = Mathf.Min(minY, orginalVerticePositions[i].y);
            maxY = Mathf.Max(maxY, orginalVerticePositions[i].y);
        }
        

        maxHorizontalDistance = 0f;
        for (int i = 0; i < orginalVerticePositions.Length; i++)
        {
            float horizontalDist = Vector2.Distance(new Vector2(orginalVerticePositions[i].x, orginalVerticePositions[i].z), Vector2.zero);
            maxHorizontalDistance = Mathf.Max(maxHorizontalDistance, horizontalDist);
        }

        startScale = GetCurrentBaseScale();
        targetScale = Vector3.zero;
    }

    void Update()
    {
        bounceAlpha = UpdateRelaxState();
        DeformVertices(bounceAlpha);
        UpdateWaveFront();
        UpdateDragOffset();


        if(isRelaxing && relaxAlpha >= 1f)
        {
            isRelaxing = false;
            hasDeformedVertices = false;
        }

        mesh.vertices = StretchedVerticePositions;
        mesh.RecalculateNormals();

        if(HasBeenConsumed)
        {
            scaleAlpha += Time.deltaTime / consumeDuration;
            scaleAlpha = Mathf.Clamp01(scaleAlpha);
            Vector3 newLocalScale = Vector3.Lerp(startScale, targetScale, scaleAlpha); 
            ApplyBaseScale(newLocalScale);
            if(scaleAlpha >= 1f)
            {
                Destroy(gridMovement != null ? gridMovement.gameObject : gameObject);
            }
        }
        else if(isGrowing)
        {
            scaleAlpha += Time.deltaTime / consumeDuration;
            scaleAlpha = Mathf.Clamp01(scaleAlpha);
            Vector3 newLocalScale = Vector3.Lerp(startScale, targetScale, scaleAlpha); 
            ApplyBaseScale(newLocalScale);
            if(scaleAlpha >= 1f)
            {
                isGrowing = false;
            }
        }
    }

   

    void OnTriggerEnter(Collider other)
    {

        if(HasBeenConsumed) return;

        MeshDeformation otherSlimeScript = other.GetComponent<MeshDeformation>();
        if(otherSlimeScript == null) return;

        float thisSlimeScale = GetCurrentBaseScale().magnitude;
        float otherSlimeScale = otherSlimeScript.GetCurrentBaseScale().magnitude;
      
        if ( otherSlimeScale > thisSlimeScale)
        {
            HasBeenConsumed = true;
        }
        else if(thisSlimeScale > otherSlimeScale)
        {
            isGrowing = true;
            scaleAlpha = 0f; 
            startScale = GetCurrentBaseScale();
            targetScale = startScale * growthMultiplier;
        }

    }

    public Vector3 GetCurrentBaseScale()
    {
        if(gridMovement != null)
        {
            return gridMovement.GetBaseScale();
        }

        return transform.localScale;
    }

    private void ApplyBaseScale(Vector3 newScale)
    {
        if(gridMovement != null)
        {
            gridMovement.UpdateBaseScale(newScale);
            return;
        }

        transform.localScale = newScale;
    }
    private float UpdateRelaxState()
    {
        if(otherSlime != null)
        {
            isRelaxing = false;
            hasDeformedVertices = true;
            return 0f;
        }
        if(!hasDeformedVertices)
        {
            return 0f;
        }
        if(!isRelaxing)
        {
            RelaxedVerticePositions = (Vector3[])StretchedVerticePositions.Clone();
            relaxAlpha = 0f;
            isRelaxing = true;
        }

            float safeRelaxDuration = Mathf.Max(relaxDuration, Mathf.Epsilon);
            relaxAlpha += Time.deltaTime / safeRelaxDuration;
            relaxAlpha = Mathf.Clamp01(relaxAlpha);

            float t = relaxAlpha;
            float overshoot = 1.70158f;
            float c3 = overshoot + 1f;
            bounceAlpha = 1 + c3 * Mathf.Pow(t - 1, 3) + overshoot * Mathf.Pow(t - 1, 2);
            return bounceAlpha;
    }

    private void DeformVertices(float bounceAlpha)
    {
        for(int i = 0; i < orginalVerticePositions.Length; i++ )
        {
            Vector3 localVertexPosition = orginalVerticePositions[i];
            StretchedVerticePositions[i] = GetBaseVertexPosition(i, localVertexPosition, bounceAlpha);

            if(gridMovement != null)
            {
                ApplyMovementDeformation(localVertexPosition, i);
            }
           
        }
    }

    private Vector3 GetBaseVertexPosition(int index, Vector3 localVertexPosition, float bounceAlpha)
    {
        if(otherSlime != null)
            {
             Vector3 vertexWorldPos = transform.TransformPoint(localVertexPosition);
             float distance = Vector3.Distance(vertexWorldPos, otherSlime.transform.position);
             stretchAlpha = Mathf.Clamp01(1f - Mathf.Pow((distance / StrechThreshold), 2));
             Vector3 newVertexPos = Vector3.Lerp(vertexWorldPos, otherSlime.transform.position, stretchAlpha);
             return transform.InverseTransformPoint(newVertexPos);
            }
            else if(isRelaxing)
            {
                return Vector3.Lerp(RelaxedVerticePositions[index], localVertexPosition, bounceAlpha);
            }
            else
            {
                return localVertexPosition;
            }
    }

    private void ApplyMovementDeformation(Vector3 localVertexPosition, int i)
    {
        
        float stretchAmount = gridMovement.GetStretchAmount();
        float heightRange = Mathf.Max(maxY - minY, Mathf.Epsilon);
        float heightRatio = (localVertexPosition.y - minY) / heightRange;

        //wave ripple effect
        float waveIntensity = 0f;
        if (isWaving)
        {
            float waveDistance = Mathf.Abs(heightRatio - waveFront);
            waveIntensity = Mathf.Max(0f, 1f - waveDistance / waveWidth);
        }


        // Center falloff — same for both squash and stretch, purely horizontal
        float horizontalRange = Mathf.Max(maxHorizontalDistance, Mathf.Epsilon);
        float horizontalDistance = new Vector2(localVertexPosition.x, localVertexPosition.z).magnitude;
        float centerFalloff = 1f - (horizontalDistance / horizontalRange);
        float adjustedCenterFalloff = Mathf.Lerp(0.3f, 1f, centerFalloff);
        
        Vector3 vertex = StretchedVerticePositions[i];
        // Apply the drag offset to the vertex position, scaled by height ratio
        vertex += dragOffset * heightRatio;

        if (stretchAmount < 0f)
        {
            // Squashing — TOP-center caves DOWN more than top-corners
            float combinedSquashFalloff = heightRatio * adjustedCenterFalloff;
            vertex.y -= combinedSquashFalloff * Mathf.Abs(stretchAmount) * squashEffectMultiplier;
        
            // Push the current height band radially outward as the wave travels
            Vector2 horizontalDirection = new Vector2(localVertexPosition.x, localVertexPosition.z);
            if (horizontalDirection.sqrMagnitude > Mathf.Epsilon)
            {
                horizontalDirection.Normalize();
                vertex.x += horizontalDirection.x * waveIntensity * waveStrength;
                //Horizontal direction is a 2D vector, so we use horizontalDirection.y for the z-axis movement
                vertex.z += horizontalDirection.y * waveIntensity * waveStrength;
            }
        }
        else if (stretchAmount > 0f)
        {
            // Stretching (apex) — top-center pushes UP more than top-corners
            float combinedStretchFalloff = heightRatio * adjustedCenterFalloff;
            vertex.y += combinedStretchFalloff * stretchAmount * stretchEffectMultiplier;
        }

        StretchedVerticePositions[i] = vertex;
                
    }            
    public void TriggerWaveRippleEffect()
    {
        waveFront = 0f;
        isWaving = true;
    }

    public float GetWaveTravelDuration()
    {
        float safeWaveTravelRate = Mathf.Max(waveTravelRate, 0.01f);
        return (1f + waveWidth) / safeWaveTravelRate;
    }

    private void UpdateWaveFront()
    {
        if (isWaving)
        {
            float safeWaveTravelRate = Mathf.Max(waveTravelRate, 0.01f);
            waveFront += Time.deltaTime * safeWaveTravelRate;

            // Let the complete band move past the top before ending the effect.
            if (waveFront > 1f + waveWidth)
            {
                isWaving = false;
            }
        }

    }

    private void UpdateDragOffset()
    {
        if (gridMovement != null)
        {
            trailingPosition = UpdateDrag(gridMovement.transform.position);
            dragOffset = trailingPosition - gridMovement.transform.position;
        }
    }


    private Vector3 UpdateDrag(Vector3 rootPostion)
    {
        Vector3 gap = rootPostion - trailingPosition;
        trailingVelocity += dragStifness * Time.deltaTime * gap;
        trailingVelocity *= 1f - dragDamping;
        trailingPosition += trailingVelocity * Time.deltaTime;
        return trailingPosition; 
    }
}

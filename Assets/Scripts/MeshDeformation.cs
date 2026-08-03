using UnityEngine;

public class MeshDeformation : MonoBehaviour
{
    [SerializeField] private Transform otherSlime;
    [SerializeField] private float StrechThreshold;
    [SerializeField] private float consumeDuration;
    [SerializeField] private float relaxDuration;
    [SerializeField] private bool isGrowing;
    [SerializeField] private float growthMultiplier;
    [SerializeField] private float squashEffectMultiplier = 1f;
    [SerializeField] private float stretchEffectMultiplier = 1f;


    private Mesh mesh;
    private GridMovement gridMovement;
    private bool HasBeenConsumed;
    private bool isRelaxing;
    private bool hasDeformedVertices;

    private float minY; 
    private float maxY;
    private float maxHorizontalDistance;



    private float scaleAlpha;
    private float stretchAlpha;
    private float relaxAlpha;
    private Vector3 startScale;
    private Vector3 targetScale;

    private Vector3 [] orginalVerticePostions;
    private Vector3 [] StretchedVerticePostions;
    private Vector3 [] RelaxedVerticePostions;


    // Start is called once before the first execution of Update after the MonoBehaviour is created

    void Start()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        gridMovement = GetComponentInParent<GridMovement>();
        orginalVerticePostions = mesh.vertices;
        Debug.Log("Vertex count: " + orginalVerticePostions.Length + " | First vertex: " + orginalVerticePostions[0] + " | Local scale: " + transform.localScale);
        StretchedVerticePostions = (Vector3[])orginalVerticePostions.Clone();
        RelaxedVerticePostions = (Vector3[])orginalVerticePostions.Clone();

        minY = orginalVerticePostions[0].y;
        maxY = orginalVerticePostions[0].y;

        for(int i = 1; i < orginalVerticePostions.Length; i++)
        {
            minY = Mathf.Min(minY, orginalVerticePostions[i].y);
            maxY = Mathf.Max(maxY, orginalVerticePostions[i].y);
        }
        Debug.Log("minY: " + minY + " maxY: " + maxY + " range: " + (maxY - minY));

        maxHorizontalDistance = 0f;
        for (int i = 0; i < orginalVerticePostions.Length; i++)
        {
            float horizontalDist = Vector2.Distance(new Vector2(orginalVerticePostions[i].x, orginalVerticePostions[i].z), Vector2.zero);
            maxHorizontalDistance = Mathf.Max(maxHorizontalDistance, horizontalDist);
        }

        startScale = GetCurrentBaseScale();
        targetScale = Vector3.zero;
    }

    // Update is called once per frame
    /* 
    1. vertexWorldPos = SlimeA.transform.TransformPoint(localVertexPosition)
    2. distance = Vector3.Distance(vertexWorldPos, SlimeB.transform.position)
    3. alpha = 1 - (distance / threshold), clamped 0 to 1
    4. newWorldPos = Lerp(vertexWorldPos, SlimeB.transform.position, alpha)
    5. newLocalPos = SlimeA.transform.InverseTransformPoint(newWorldPos)
    */
    void Update()
    {
        float bounceAlpha = 0f;

        if(otherSlime == null && hasDeformedVertices)
        {
            if(!isRelaxing)
            {
                RelaxedVerticePostions = (Vector3[])StretchedVerticePostions.Clone();
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
        }
        else if(otherSlime != null)
        {
            isRelaxing = false;
            hasDeformedVertices = true;
        }

        for(int i = 0; i < orginalVerticePostions.Length; i++ )
        {
            Vector3 localVertexPosition = orginalVerticePostions[i];
            Vector3 VertexWorldPos = transform.TransformPoint(localVertexPosition);

            if(otherSlime != null)
            {
            
             float distance = Vector3.Distance(VertexWorldPos, otherSlime.transform.position);
             stretchAlpha = 1 - Mathf.Pow((distance / StrechThreshold), 2);
             stretchAlpha = Mathf.Clamp(stretchAlpha, 0, 1);
             Vector3 newVertexPos = Vector3.Lerp(VertexWorldPos, otherSlime.transform.position, stretchAlpha);
             Vector3 newLocalPos = gameObject.transform.InverseTransformPoint(newVertexPos);
             StretchedVerticePostions[i] = newLocalPos;
            }
            else if(isRelaxing)
            {
                StretchedVerticePostions[i] = Vector3.Lerp(RelaxedVerticePostions[i], localVertexPosition, bounceAlpha);
            }
            else
            {
                StretchedVerticePostions[i] = localVertexPosition;
            }

           if (gridMovement != null)
            {
                float stretchAmount = gridMovement.GetStretchAmount();
                float heightRatio = (localVertexPosition.y - minY) / (maxY - minY);

                // Center falloff — same for both squash and stretch, purely horizontal
                float horizontalDistance = Vector2.Distance(new Vector2(localVertexPosition.x, localVertexPosition.z), Vector2.zero);
                float centerFalloff = 1f - (horizontalDistance / maxHorizontalDistance);
                float minCenterEffect = 0.3f;
                float adjustedCenterFalloff = Mathf.Lerp(minCenterEffect, 1f, centerFalloff);

                if (stretchAmount < 0f)
                {
                        // Squashing — TOP-center caves DOWN more than top-corners
                        float topWeight = heightRatio; // same shape as stretchFalloff — strong at top (maxY), weak at bottom (minY)
                        float combinedSquashFalloff = topWeight * adjustedCenterFalloff;
                        StretchedVerticePostions[i].y -= combinedSquashFalloff * Mathf.Abs(stretchAmount) * squashEffectMultiplier;

                    // (your existing horizontal squash-push code for the bottom-widening effect can stay separate, still using plain squashFalloff)
                    }
                    else if (stretchAmount > 0f)
                    {
                        // Stretching (apex) — top-center pushes UP more than top-corners
                        float stretchFalloff = heightRatio;
                        float combinedStretchFalloff = stretchFalloff * adjustedCenterFalloff;
                        StretchedVerticePostions[i].y += combinedStretchFalloff * stretchAmount * stretchEffectMultiplier;
                    }
                }
            }


        if(isRelaxing && relaxAlpha >= 1f)
        {
            isRelaxing = false;
            hasDeformedVertices = false;
        }

        mesh.vertices = StretchedVerticePostions;
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
}

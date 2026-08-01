using UnityEngine;

public class MeshDeformation : MonoBehaviour
{
    [SerializeField] private Transform otherSlime;
    [SerializeField] private float StrechThreshold;
    [SerializeField] private float consumeDuration;
    [SerializeField] private float relaxDuration;
    [SerializeField] private bool isGrowing;
    [SerializeField] private float growthMultiplier;

    [Header("Jiggle Settings")]
    [SerializeField] private float jiggleFrequency = 1f;
    [SerializeField] private float jiggleSpeed = 1f;
    [SerializeField] private float jiggleStrength = 0.02f;

    private Mesh mesh;
    private GridMovement gridMovement;
    private bool HasBeenConsumed;
    private bool isRelaxing;
    private bool hasDeformedVertices;

    private float scaleAlpha;
    private float stretchAlpha;
    private float relaxAlpha;
    private Vector3 startScale;
    private Vector3 targetScale;

    private Vector3 [] orginalVerticePostions;
    private Vector3 [] StrechedVerticePostions;
    private Vector3 [] RelaxedVerticePostions;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        gridMovement = GetComponent<GridMovement>();
        orginalVerticePostions = mesh.vertices;
        StrechedVerticePostions = (Vector3[])orginalVerticePostions.Clone();
        RelaxedVerticePostions = (Vector3[])orginalVerticePostions.Clone();

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
                RelaxedVerticePostions = (Vector3[])StrechedVerticePostions.Clone();
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
             StrechedVerticePostions[i] = newLocalPos;
            }
            else if(isRelaxing)
            {
                StrechedVerticePostions[i] = Vector3.Lerp(RelaxedVerticePostions[i], localVertexPosition, bounceAlpha);
            }
            else
            {
                StrechedVerticePostions[i] = localVertexPosition;
            }

            /*
            float noiseX = Mathf.PerlinNoise(localVertexPosition.x * jiggleFrequency + Time.time * jiggleSpeed, localVertexPosition.z * jiggleFrequency); 
            float noiseY = Mathf.PerlinNoise(localVertexPosition.x * jiggleFrequency + Time.time * jiggleSpeed +100f, localVertexPosition.z * jiggleFrequency +100f);
            float noiseZ = Mathf.PerlinNoise(localVertexPosition.x * jiggleFrequency + Time.time * jiggleSpeed +200f, localVertexPosition.z * jiggleFrequency +200f);

            Vector3 jiggleOffset = new Vector3(
                (noiseX - 0.5f) * jiggleStrength,
                (noiseY - 0.5f) * jiggleStrength,
                (noiseZ - 0.5f) * jiggleStrength
            );

            StrechedVerticePostions[i] += jiggleOffset;
            */
        }

        if(isRelaxing && relaxAlpha >= 1f)
        {
            isRelaxing = false;
            hasDeformedVertices = false;
        }

        mesh.vertices = StrechedVerticePostions;
        mesh.RecalculateNormals();

        if(HasBeenConsumed)
        {
            scaleAlpha += Time.deltaTime / consumeDuration;
            scaleAlpha = Mathf.Clamp01(scaleAlpha);
            Vector3 newLocalScale = Vector3.Lerp(startScale, targetScale, scaleAlpha); 
            ApplyBaseScale(newLocalScale);
            if(scaleAlpha >= 1f)
            {
                Destroy(gameObject);
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

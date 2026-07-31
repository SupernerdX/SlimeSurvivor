using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class MeshDeformation : MonoBehaviour
{
    [SerializeField] private Transform otherSlime;
    [SerializeField] private float StrechThreshold;
    [SerializeField] private float consumeDuration;

    private Mesh mesh;
    private bool HasBeenConsumed;

    private float scaleAlpha;
    private Vector3 targetScale;

    private Vector3 [] orginalVerticePostions;
    private Vector3 [] StrechedVerticePostions;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        orginalVerticePostions = mesh.vertices;
        StrechedVerticePostions = new Vector3[orginalVerticePostions.Length];

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
        for(int i = 0; i < orginalVerticePostions.Length; i++ )
        {
            Vector3 localVertexPosition = orginalVerticePostions[i];
             Vector3 VertexWorldPos = transform.TransformPoint(localVertexPosition);
             float distance = Vector3.Distance(VertexWorldPos, otherSlime.transform.position);
             float strechAlpha = 1 - Mathf.Pow((distance / StrechThreshold), 2);
             strechAlpha = Mathf.Clamp(strechAlpha, 0, 1);
             Vector3 newVertexPos = Vector3.Lerp(VertexWorldPos, otherSlime.transform.position, strechAlpha);
             Vector3 newLocalPos = gameObject.transform.InverseTransformPoint(newVertexPos);
             StrechedVerticePostions[i] = newLocalPos;
        }

        mesh.vertices = StrechedVerticePostions;
        mesh.RecalculateNormals();

        if(HasBeenConsumed)
        {
            scaleAlpha += Time.deltaTime / consumeDuration;
            scaleAlpha = Mathf.Clamp01(scaleAlpha);
            Vector3 newLocalScale = Vector3.Lerp(gameObject.transform.localScale, targetScale, scaleAlpha); 
            gameObject.transform.localScale = newLocalScale; 
            if(scaleAlpha >= 1f)
            {
                Destroy(gameObject);
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if(HasBeenConsumed) return;

        float thisSlimeScale = gameObject.transform.localScale.magnitude;
        float otherSlimeScale = other.gameObject.transform.localScale.magnitude;
        if ( thisSlimeScale < otherSlimeScale)
        {
            HasBeenConsumed = true;
        }

            


    
        
    }
}

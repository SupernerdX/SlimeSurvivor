using UnityEngine;

public class MeshDeformation : MonoBehaviour
{
    [SerializeField] private Transform otherSlime;
    [SerializeField] private float StrechThreshold;

    private Mesh mesh;
    private Vector3 [] orginalVerticePostions;
    private Vector3 [] StrechedVerticePostions;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        orginalVerticePostions = mesh.vertices;
        StrechedVerticePostions = new Vector3[orginalVerticePostions.Length];
    }

    // Update is called once per frame
    void Update()
    {
        foreach (var localVertexPostion in orginalVerticePostions)
        {
             Vector3 VertexWorldPos = transform.TransformPoint(localVertexPostion);
        }
       
    }
}

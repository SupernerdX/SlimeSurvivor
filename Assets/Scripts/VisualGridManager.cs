using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine.Rendering;

public static class VisualGridPrefrence
{
   public const string PreferenceKey =
        "ScriptingPractice.VisualGridEnabled";
}

[AddComponentMenu("ScriptingPractice/Visual Grid")]
[ExecuteAlways]
public class VisualGridManager : MonoBehaviour
{
    [SerializeField] private int gridWidth;
    [SerializeField] private int gridHeight;
    [SerializeField] private float gridSize;
    
    private void OnDrawGizmos()
    {
        #if UNITY_EDITOR
            if(!UnityEditor.EditorPrefs.GetBool(VisualGridPrefrence.PreferenceKey, true))
            {
                return;
            }
        #endif
            Handles.color = Color.red; 
            Handles.zTest = CompareFunction.Always;
            DrawGrid();
            
    }

    private void DrawGrid()
    {
        Gizmos.color = Color.red;
       Vector3 origin = transform.position - 
       new Vector3(gridWidth * gridSize / 2f, 0f, gridHeight * gridSize / 2f);

        for (int x = 0; x <= gridWidth; x++)
        {
            Vector3 start = origin + new Vector3(x * gridSize , 0f, 0f);
            Vector3 end = start + new Vector3 (0f, 0f, gridHeight * gridSize);
            Handles.DrawLine(start, end);
        }
        for (int z = 0; z <= gridHeight; z++)
        {
            Vector3 start = origin + new Vector3(0f, 0f, z * gridSize );
            Vector3 end = start + new Vector3 (gridWidth * gridSize, 0f, 0f);
            Handles.DrawLine(start, end);
        }
    }
}

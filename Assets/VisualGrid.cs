using UnityEngine;


public static class VisualGridPreferences
{
    public const string PreferenceKey =
        "ScriptingPractice.VisualGridEnabled";
}

[AddComponentMenu("Scripting Practice/Visual Grid")]
[ExecuteAlways]
public class VisualGrid : MonoBehaviour
{
    [Min(1)]
    public int width = 10;

    [Min(1)]
    public int height = 10;

    [Min(0.01f)]
    public float cellSize = 1f;

    public Vector3 origin = Vector3.zero;

    public Color lineColor = new Color(1f, 1f, 1f, 0.35f);

    private void OnDrawGizmos()
    {
#if UNITY_EDITOR
        if (!UnityEditor.EditorPrefs.GetBool(
                VisualGridPreferences.PreferenceKey,
                true))
        {
            return;
        }
#endif

        DrawGrid();
    }

    private void DrawGrid()
    {
        var gridWidth = Mathf.Max(1, width);
        var gridHeight = Mathf.Max(1, height);
        var size = Mathf.Max(0.01f, cellSize);
        var start = transform.position + origin;
        var right = transform.right;
        var forward = transform.forward;

        Gizmos.color = lineColor;

        for (var x = 0; x <= gridWidth; x++)
        {
            var offset = right * (x * size);
            var from = start + offset;
            var to = from + (forward * (gridHeight * size));
            Gizmos.DrawLine(from, to);
        }

        for (var y = 0; y <= gridHeight; y++)
        {
            var offset = forward * (y * size);
            var from = start + offset;
            var to = from + (right * (gridWidth * size));
            Gizmos.DrawLine(from, to);
        }
    }
}

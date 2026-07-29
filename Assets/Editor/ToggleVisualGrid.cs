using UnityEngine;
using UnityEditor;
using UnityEditor.Toolbars;
using UnityEditor.Overlays;
using UnityEngine.UI;
using System;

public class ToggleVisualGrid
{
    public const string ToolbarButtonID = "ScriptingPractice/ToggleVisualGrid";

    [MenuItem("Tools/Toggle Grid", false)]
    static void ToggleVG()
    {
        Toggle();
        
    }

    private static void Toggle()
    {
        Debug.Log("Toolbar grid button clicked");
    }

    
}

using System;
using UnityEditor;
using UnityEditor.Toolbars;
using UnityEngine;


public static class ToggleVisualGrid
{
    private const string PreferenceKey =
        "ScriptingPractice.VisualGridEnabled";

    public static bool Enabled =>
        EditorPrefs.GetBool(PreferenceKey, true);

    [MainToolbarElement(
        "ScriptingPractice/Toggle Visual Grid",
        defaultDockPosition = MainToolbarDockPosition.Right)]
    public static MainToolbarElement CreateGridButton()
    {
        var content = new MainToolbarContent(
            "Grid",
            "Toggle the visual grid");

        return new MainToolbarButton(content, ToggleGridButton);
    }

    private static void ToggleGridButton()
    {
        EditorPrefs.SetBool(PreferenceKey, !Enabled);
        SceneView.RepaintAll();

        BuildGridOverlay.RefreshGrid();
        
    }
public static class BuildGridOverlay
    {
    public static void RefreshGrid()
    {
        
    }
    
    }
    
}
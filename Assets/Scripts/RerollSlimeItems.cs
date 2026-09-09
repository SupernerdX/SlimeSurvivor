using UnityEngine;
using UnityEditor;
using UnityEditor.Toolbars;

public static class RerollSlimeItems
{

    [MainToolbarElement(
        "ScriptingPractice/Reroll Slime Items",
        defaultDockPosition = MainToolbarDockPosition.Right)]

    public static MainToolbarElement CreateRerollButton()
    {
        var content = new MainToolbarContent(
            "Reroll Slime Items",
            "Reroll the slime items in this set");

            return new MainToolbarButton(content, RerollSlimeItemsButton);
    }

    private static void RerollSlimeItemsButton()
    {
        if (!EditorApplication.isPlaying)
        {
            Debug.LogWarning("Reroll only works in Play mode.");
            return;
        }
        SuspendedItemSpawner spawner = Object.FindAnyObjectByType<SuspendedItemSpawner>();
        if(spawner != null)
        {
            spawner.ResetSpawner();
        }
        else
        {
            Debug.LogWarning("No SuspenedItemSpawner found in this Scene");
        }
    }
}

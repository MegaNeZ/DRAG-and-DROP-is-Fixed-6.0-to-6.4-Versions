# Unity 6.0 to 6.4 or above Hierarchy Drag and Drop Bug Fix
A lightweight Unity 6 editor script that fixes the broken Hierarchy drag-and-drop bug caused by the native System.UInt64 to System.Int32 casting exception.
## The Problem
When dragging assets or prefabs into the Hierarchy in certain Unity 6 builds, the editor throws a persistent casting error and completely blocks the operation:
`ArgumentException: Object of type 'System.UInt64' cannot be converted to type 'System.Int32'.`
This happens because Unity 6 upgraded internal instance identifiers to 64-bit (`UnityEngine.EntityId`), but some legacy internal drop handlers inside the engine still expect a 32-bit integer (`Int32`), causing a native crash in the UI interface.
## The Solution (Open Source Code)
Since the bug is hardcoded inside Unity's native DLLs, this script creates a global fallback handler. It safely intercepts the drag event on the hierarchy and instantiates the dragged prefab programmatically, bypassing the broken native code completely.
## How to Install
> ⚠️ **CRITICAL REQUIREMENT:** This script uses the `UnityEditor` namespace. It **MUST** be placed inside a folder named **`Editor`** anywhere in your project, or Unity will throw compilation errors when you try to build your game.
1. Inside your Unity Project window, look for an existing folder named **`Editor`**. If you don't have one, right-click inside `Assets` and create a new folder named exactly **`Editor`**.
2. Go inside that `Editor` folder.
3. Right-click, select **Create > C# Script**, and name it exactly **`FixHierarchyDrop.cs`**.
4. Open the script, delete all the default template code, and paste the open-source code provided above.
5. Save the file (**Ctrl + S**), return to Unity, clear your console, and you're good to go!
Just copy and paste the code below directly into your project:

```csharp
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class FixHierarchyDrop
{
    static FixHierarchyDrop()
    {
        // Monitors the global hierarchy change event in the editor
        EditorApplication.hierarchyChanged += OnHierarchyChanged;
    }

    private static void OnHierarchyChanged()
    {
        // Checks if there are objects being dragged and if the mouse button was released (DragPerform)
        if (DragAndDrop.objectReferences.Length > 0 && Event.current != null && Event.current.type == EventType.DragPerform)
        {
            // Tells the editor that the drag operation is accepted
            DragAndDrop.AcceptDrag();
            
            foreach (var obj in DragAndDrop.objectReferences)
            {
                if (obj is GameObject prefab)
                {
                    // Safely instantiates the prefab into the current active scene
                    GameObject instantiated = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                    
                    // Registers the action in the Undo system so Ctrl+Z works properly
                    Undo.RegisterCreatedObjectUndo(instantiated, "Fix Drag Drop");
                    
                    // Automatically selects the newly created object in the scene
                    Selection.activeObject = instantiated;
                }
            }
            
            // Consumes the event to prevent Unity's broken native code from triggering the casting bug
            Event.current.Use();
        }
    }
}

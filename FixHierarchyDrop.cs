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

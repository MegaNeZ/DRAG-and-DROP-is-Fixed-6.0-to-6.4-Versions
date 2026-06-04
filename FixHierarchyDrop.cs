using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class FixHierarchyDrop
{
    static FixHierarchyDrop()
    {
        // Monitora o evento global de arrastar e soltar no editor
        EditorApplication.hierarchyChanged += OnHierarchyChanged;
    }

    private static void OnHierarchyChanged()
    {
        // Se houver um objeto sendo arrastado e o mouse for solto
        if (DragAndDrop.objectReferences.Length > 0 && Event.current != null && Event.current.type == EventType.DragPerform)
        {
            DragAndDrop.AcceptDrag();
            
            foreach (var obj in DragAndDrop.objectReferences)
            {
                if (obj is GameObject prefab)
                {
                    // Instancia o prefab de forma limpa na cena atual
                    GameObject instantiated = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                    Undo.RegisterCreatedObjectUndo(instantiated, "Fix Drag Drop");
                    Selection.activeObject = instantiated;
                }
            }
            
            // Avisa o editor que o evento foi tratado para não disparar o bug interno
            Event.current.Use();
        }
    }
}
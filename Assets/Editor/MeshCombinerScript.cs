using UnityEditor;
using UnityEngine;

public class MeshCombinerEditor : EditorWindow
{
    [MenuItem("Tools/Mesh Combiner")]
    public static void ShowWindow()
    {
        GetWindow<MeshCombinerEditor>("Mesh Combiner");
    }

    private void OnGUI()
    {
        if (GUILayout.Button("Combine Selected Meshes"))
        {
            CombineSelectedMeshes();
        }
    }

    private void CombineSelectedMeshes()
    {
        GameObject[] selectedObjects = Selection.gameObjects;
        if (selectedObjects.Length == 0)
        {
            Debug.LogWarning("No GameObjects selected!");
            return;
        }

        MeshFilter[] meshFilters = new MeshFilter[selectedObjects.Length];
        for (int i = 0; i < selectedObjects.Length; i++)
        {
            meshFilters[i] = selectedObjects[i].GetComponent<MeshFilter>();
        }

        CombineInstance[] combine = new CombineInstance[meshFilters.Length];
        for (int i = 0; i < meshFilters.Length; i++)
        {
            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
        }

        Mesh combinedMesh = new Mesh();
        combinedMesh.CombineMeshes(combine);

        GameObject combinedObject = new GameObject("Combined Mesh");
        MeshFilter filter = combinedObject.AddComponent<MeshFilter>();
        filter.mesh = combinedMesh;
        combinedObject.AddComponent<MeshRenderer>();

        Debug.Log("Meshes combined successfully!");
    }
}

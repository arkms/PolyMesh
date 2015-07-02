using UnityEngine;
using UnityEditor;
using System.Collections;

public class PolyToMesh : EditorWindow
{
    GameObject go_withcol;
    bool CreateNewGameObject = true;
    bool CreateLikeChildren = true;
    float depth= 5.0f;

    [MenuItem("Arj2D/Poly2Mesh")]
    static void Init()
    {
        EditorWindow.GetWindow(typeof(PolyToMesh), true, "Poly to Mesh");
    }

    void OnGUI()
    {
        go_withcol = (GameObject)EditorGUILayout.ObjectField("GameObject:", go_withcol, typeof(GameObject), true);
        CreateNewGameObject = EditorGUILayout.ToggleLeft("Create new GameObject", CreateNewGameObject);
        float newdepth = EditorGUILayout.FloatField("Depth", depth);
        if (newdepth < 0.0f)
        {
            newdepth = 0.1f;
        }
        depth = newdepth;

        if (CreateNewGameObject)
        {
            CreateLikeChildren = EditorGUILayout.ToggleLeft("Crete like a children the new GameObject", CreateLikeChildren);
            EditorGUILayout.HelpBox("The current PolyCollider is going to be deleted", MessageType.Warning);
        }

        if (GUILayout.Button("Convert") && go_withcol != null)
        {
            PolygonCollider2D polyCol = go_withcol.GetComponent<PolygonCollider2D>();
            if (polyCol == null)
            {
                EditorUtility.DisplayDialog("Error", "Please select a GameObject with Polygon Collider 2D", "Ok");
                return;
            }
            System.Collections.Generic.List<Vector3> pointsPoly = new System.Collections.Generic.List<Vector3>();
            for (int i = 0; i < polyCol.points.Length; i++)
            {
                pointsPoly.Add(polyCol.points[i]);
            }

            //Build vertices array
            Vector3 offset = new Vector3(0, 0, depth / 2.0f);
            var newvertices = new System.Collections.Generic.List<Vector3>();
            var newtriangles = new System.Collections.Generic.List<int>();
            for (int i = 0; i < pointsPoly.Count; i++)
            {
                newvertices.Add(pointsPoly[i] + offset);
                newvertices.Add(pointsPoly[i] - offset);
            }

            //Build triangles array
            for (int a = 0; a < newvertices.Count; a += 2)
            {
                var b = (a + 1) % newvertices.Count;
                var c = (a + 2) % newvertices.Count;
                var d = (a + 3) % newvertices.Count;
                newtriangles.Add(a);
                newtriangles.Add(c);
                newtriangles.Add(b);
                newtriangles.Add(c);
                newtriangles.Add(d);
                newtriangles.Add(b);
            }
            var mesh = new Mesh();
            mesh.name = go_withcol.name + "_col";

            //Update the mesh
            mesh.Clear();
            mesh.vertices = newvertices.ToArray();
            mesh.triangles = newtriangles.ToArray();
            mesh.RecalculateNormals();
            mesh.Optimize();

            //uvs
            Vector2[] uvs = new Vector2[mesh.vertexCount];
            for (int i = 0; i < uvs.Length; i++)
            {
                uvs[i] = new Vector2(mesh.vertices[i].x, mesh.vertices[i].z);
            }
            mesh.uv = uvs;

            //save mesh
            string path = EditorUtility.SaveFilePanel("Save Mesh Collider", "Assets/", mesh.name, "asset");
            if (string.IsNullOrEmpty(path))
            {
                EditorUtility.DisplayDialog("Error", "Please select a folder where to save mesh collider", "Ok");
                return;
            }

            path = FileUtil.GetProjectRelativePath(path);
            Mesh meshToSave = Object.Instantiate(mesh) as Mesh;
            AssetDatabase.CreateAsset(meshToSave, path);
            AssetDatabase.SaveAssets();
           

            if (CreateNewGameObject)
            {
                GameObject go = new GameObject(go_withcol.name + "_mesh");
                go.transform.position = go_withcol.transform.position;
                go.transform.rotation = go_withcol.transform.rotation;
                if (CreateLikeChildren)
                    go.transform.parent = go_withcol.transform;
                MeshCollider meshcol = go.AddComponent<MeshCollider>();
                meshcol.sharedMesh = mesh;
            }
            else
            {
                DestroyImmediate(polyCol);
                MeshCollider meshcol = go_withcol.AddComponent<MeshCollider>();
                meshcol.sharedMesh = mesh;
            }
        }
    }
}
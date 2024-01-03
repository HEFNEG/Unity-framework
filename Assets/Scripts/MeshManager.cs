using System.Collections.Generic;

using Autodesk.Fbx;

using DG.Tweening;

using Unity.Mathematics;

using UnityEngine;

public class MeshManager : MonoBehaviour {
    public static MeshManager Instance;

    private void Awake() {
        Instance = this;
    }

    void Start() { }

    private void Update() { }

    public void MeshSlice(Transform meshObject, Vector3 pos, Vector3 tangent, Vector3 depth) {
        var meshFilter = meshObject.GetComponent<MeshFilter>();
        if (meshFilter == null || meshFilter.mesh == null) {
            return;
        }

        var normal = math.normalize(math.cross(tangent, depth));
        var worldToLocal = meshObject.worldToLocalMatrix;
        var localPos = math.mul(worldToLocal, new float4(pos.x, pos.y, pos.z, 1f)).xyz;
        var localNormal = math.normalize(math.mul(worldToLocal, new float4(normal.x, normal.y, normal.z, 0f)).xyz);
        GenerateMeshByPlane(meshFilter.mesh, localPos, localNormal, out Mesh bigMesh, out Mesh smallMesh);
        if (bigMesh.vertexCount < 3 || smallMesh.vertexCount < 3) {
            return;
        }

        meshFilter.mesh = bigMesh;
        var newInstance = GameObject.Instantiate(meshObject.gameObject,meshObject.parent,true);
        newInstance.GetComponent<MeshFilter>().mesh = smallMesh;

        meshObject.DOMove(meshObject.position + Vector3.right, 0.5f);
        newInstance.transform.DOMove(newInstance.transform.position + Vector3.left, 0.5f);
        //ExportScene( Application.streamingAssetsPath +"/abc");
        //PrefabUtility.SaveAsPrefabAsset(newInstance, Application.streamingAssetsPath);
    }

    /// <summary>
    /// 在网格的局部空间下进行切割
    /// </summary>
    /// <param name="mesh"></param>
    /// <param name="pos"></param>
    /// <param name="normal"></param>
    /// <param name="bigMesh"></param>
    /// <param name="smallMesh"></param>
    /// <returns></returns>
    private bool GenerateMeshByPlane(Mesh mesh, Vector3 pos, Vector3 normal, out Mesh bigMesh, out Mesh smallMesh) {
        bigMesh = new Mesh();
        smallMesh = new Mesh();

        var verticesA = new List<Vector3>();
        var trianglesA = new List<int>();
        var normalsA = new List<Vector3>();

        var verticesB = new List<Vector3>();
        var trianglesB = new List<int>();
        var normalsB = new List<Vector3>();

        var vertices = mesh.vertices;
        var triangles = mesh.triangles;
        var normals = mesh.normals;
        for (int i = 0; i < triangles.Length; i += 3) {
            var a = vertices[triangles[i]];
            var b = vertices[triangles[i + 1]];
            var c = vertices[triangles[i + 2]];
            byte flag = 0;

            if (TryGetCrossPoint(a, b, pos, normal, out var pointAB)) {
                flag |= 1;
            }

            if (TryGetCrossPoint(a, c, pos, normal, out var pointAC)) {
                flag |= 2;
            }

            if (TryGetCrossPoint(b, c, pos, normal, out var pointBC)) {
                flag |= 4;
            }

            Vector3Int indexs;
            switch (flag) {
                case 3:
                    // A-BC
                    indexs = new Vector3Int(triangles[i], triangles[i + 1], triangles[i + 2]);
                    if (math.dot(a - pos, normal) > 0) {
                        AddTriangle(vertices, triangles, normals, verticesA, trianglesA, normalsA, verticesB, trianglesB, normalsB, indexs, pointAB, pointAC);
                    } else {
                        AddTriangle(vertices, triangles, normals, verticesB, trianglesB, normalsB, verticesA, trianglesA, normalsA, indexs, pointAB, pointAC);
                    }

                    break;
                case 5:
                    // B-CA
                    indexs = new Vector3Int(triangles[i + 1], triangles[i + 2],triangles[i]);
                    if (math.dot(b - pos, normal) > 0) {
                        AddTriangle(vertices, triangles, normals, verticesA, trianglesA, normalsA, verticesB, trianglesB, normalsB, indexs, pointBC, pointAB);
                    } else {
                        AddTriangle(vertices, triangles, normals, verticesB, trianglesB, normalsB, verticesA, trianglesA, normalsA, indexs, pointBC, pointAB);
                    }

                    break;
                case 6:
                    // C-AB
                    indexs = new Vector3Int(triangles[i + 2], triangles[i], triangles[i + 1]);
                    if (math.dot(a - pos, normal) > 0) {
                        AddTriangle(vertices, triangles, normals, verticesA, trianglesA, normalsA, verticesB, trianglesB, normalsB, indexs, pointAC, pointBC);
                    } else {
                        AddTriangle(vertices, triangles, normals, verticesB, trianglesB, normalsB, verticesA, trianglesA, normalsA, indexs, pointAC, pointBC);
                    }

                    break;
                default:
                    if (math.dot(a - pos, normal) > 0) {
                        var indexA = verticesA.Count;
                        verticesA.AddRange(new[] { a, b, c });
                        normalsA.AddRange(new[] { normals[triangles[i]], normals[triangles[i + 1]], normals[triangles[i + 2]] });
                        trianglesA.AddRange(new[] { indexA, indexA + 1, indexA + 2 });
                    } else {
                        var indexB = verticesB.Count;
                        verticesB.AddRange(new[] { a, b, c });
                        normalsB.AddRange(new[] { normals[triangles[i]], normals[triangles[i + 1]], normals[triangles[i + 2]] });
                        trianglesB.AddRange(new[] { indexB, indexB + 1, indexB + 2 });
                    }

                    break;
            }
        }

        bigMesh.SetVertices(verticesA);
        bigMesh.SetTriangles(trianglesA, 0);
        bigMesh.SetNormals(normalsA);
        smallMesh.SetVertices(verticesB);
        smallMesh.SetTriangles(trianglesB, 0);
        smallMesh.SetNormals(normalsB);

        return true;
    }

    /// <summary>
    /// 对边进行切割
    /// </summary>
    /// <param name="line"></param>
    /// <param name="pos"></param>
    /// <param name="normal"></param>
    /// <param name="point"></param>
    /// <returns></returns>
    private bool TryGetCrossPoint(Vector3 start, Vector3 end, Vector3 pos, Vector3 normal, out Vector3 point) {
        point = Vector3.zero;
        var line = end - start;
        var d = math.dot(pos - start, normal) / math.dot(line, normal);
        if (math.abs(d) > math.length(line)) {
            return false;
        }

        point = start + d * line;
        return true;
    }

    /// <summary>
    /// 在两个mesh中添加新的面，index[0] 被划分到 meshA，其余两个点被划分到 meshB
    /// </summary>
    /// <param name="verticesA"></param>
    /// <param name="trianglesA"></param>
    /// <param name="verticesB"></param>
    /// <param name="trianglesB"></param>
    /// <param name="index"></param>
    /// <param name="newPointB"></param>
    /// <param name="newPointC"></param>
    private void AddTriangle(Vector3[] originVertices, int[] originTriangles, Vector3[] originNormals, List<Vector3> verticesA, List<int> trianglesA, List<Vector3> normalsA, List<Vector3> verticesB, List<int> trianglesB, List<Vector3> normalsB, Vector3Int index, Vector3 newPointB, Vector3 newPointC) {
        var newNormalB = (Vector3)math.normalize(originNormals[index[0]] + originNormals[index[1]]);
        var newNormalC = (Vector3)math.normalize(originNormals[index[0]] + originNormals[index[2]]);
        // MeshA
        var indexA = verticesA.Count;
        verticesA.AddRange(new[] { originVertices[index[0]], newPointB, newPointC });
        normalsA.AddRange(new[] { originNormals[index[0]], newNormalB, newNormalC });
        trianglesA.AddRange(new[] { indexA, indexA + 1, indexA + 2 });
        // MeshB
        var indexB = verticesB.Count;
        verticesB.AddRange(new[] { originVertices[index[1]], originVertices[index[2]], newPointB, newPointC });
        normalsB.AddRange(new[] { originNormals[index[1]], originNormals[index[2]], newNormalB, newNormalC });
        trianglesB.AddRange(new[] {indexB + 2,indexB, indexB + 1});

        // 这里有时候法线会反过来，需要判断一次
        /*var tempNormal = math.cross(originVertices[index[2]] - newPointB, newPointC - newPointB);
        if (math.dot(tempNormal, newNormalC) > 0) {*/
            trianglesB.AddRange(new[] { indexB + 2,  indexB + 1, indexB + 3 });
        /*} else {
            trianglesB.AddRange(new[] { indexB + 2,  indexB + 3, indexB + 1 });
        }*/

    }

    protected void ExportScene (string fileName)
    {
        using(FbxManager fbxManager = FbxManager.Create ()){
            // configure IO settings.
            fbxManager.SetIOSettings (FbxIOSettings.Create (fbxManager, Globals.EXP_FBX_EMBEDDED));

            // Export the scene
            using (FbxExporter exporter = FbxExporter.Create (fbxManager, "myExporter")) {

                // Initialize the exporter.
                bool status = exporter.Initialize (fileName, -1, fbxManager.GetIOSettings ());

                // Create a new scene to export
                FbxScene scene = FbxScene.Create (fbxManager, "SampleScene");

                // Export the scene to the file.
                exporter.Export (scene);
            }
        }
    }
}
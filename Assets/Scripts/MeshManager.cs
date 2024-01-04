using System;
using System.Collections.Generic;
using System.Linq;

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
        var newInstance = GameObject.Instantiate(meshObject.gameObject, meshObject.parent, true);
        newInstance.GetComponent<MeshFilter>().mesh = smallMesh;

        meshObject.DOMove(meshObject.position + Vector3.right, 0.5f);
        newInstance.transform.DOMove(newInstance.transform.position + Vector3.left, 0.5f);
    }

    /// <summary>
    /// 在网格的局部空间下进行切割,已知切分细网格时会有问题
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

        var slicePoints = new Dictionary<Vector3, List<Vector3>>();

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
                        AddTriangle(vertices, triangles, normals, verticesA, trianglesA, normalsA, verticesB, trianglesB, normalsB, indexs, pointAB, pointAC, slicePoints);
                    } else {
                        AddTriangle(vertices, triangles, normals, verticesB, trianglesB, normalsB, verticesA, trianglesA, normalsA, indexs, pointAB, pointAC, slicePoints);
                    }

                    break;
                case 5:
                    // B-CA
                    indexs = new Vector3Int(triangles[i + 1], triangles[i + 2], triangles[i]);

                    if (math.dot(b - pos, normal) > 0) {
                        AddTriangle(vertices, triangles, normals, verticesA, trianglesA, normalsA, verticesB, trianglesB, normalsB, indexs, pointBC, pointAB, slicePoints);
                    } else {
                        AddTriangle(vertices, triangles, normals, verticesB, trianglesB, normalsB, verticesA, trianglesA, normalsA, indexs, pointBC, pointAB, slicePoints);
                    }

                    break;
                case 6:
                    // C-AB
                    indexs = new Vector3Int(triangles[i + 2], triangles[i], triangles[i + 1]);

                    if (math.dot(a - pos, normal) > 0) {
                        AddTriangle(vertices, triangles, normals, verticesA, trianglesA, normalsA, verticesB, trianglesB, normalsB, indexs, pointAC, pointBC, slicePoints);
                    } else {
                        AddTriangle(vertices, triangles, normals, verticesB, trianglesB, normalsB, verticesA, trianglesA, normalsA, indexs, pointAC, pointBC, slicePoints);
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

        CompleteFace(verticesA, trianglesA, normalsA, verticesB, trianglesB, normalsB, slicePoints);
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
    private void AddTriangle(Vector3[] originVertices, int[] originTriangles, Vector3[] originNormals, List<Vector3> verticesA, List<int> trianglesA, List<Vector3> normalsA, List<Vector3> verticesB, List<int> trianglesB, List<Vector3> normalsB, Vector3Int index, Vector3 newPointB, Vector3 newPointC, Dictionary<Vector3, List<Vector3>> slicePoints) {
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
        trianglesB.AddRange(new[] { indexB + 2, indexB, indexB + 1 });
        trianglesB.AddRange(new[] { indexB + 2, indexB + 1, indexB + 3 });

        if (!slicePoints.ContainsKey(newPointB)) {
            slicePoints.Add(newPointB, new List<Vector3>());
        }

        if (!slicePoints.ContainsKey(newPointC)) {
            slicePoints.Add(newPointC, new List<Vector3>());
        }

        slicePoints[newPointB].Add(newPointC);
        slicePoints[newPointC].Add(newPointB);
    }

    /// <summary>
    /// 补全缺失的面
    /// </summary>
    /// <param name="verticesA"></param>
    /// <param name="trianglesA"></param>
    /// <param name="normalsA"></param>
    /// <param name="verticesB"></param>
    /// <param name="trianglesB"></param>
    /// <param name="normalsB"></param>
    /// <param name="slicePoints"></param>
    private void CompleteFace(List<Vector3> verticesA, List<int> trianglesA, List<Vector3> normalsA, List<Vector3> verticesB, List<int> trianglesB, List<Vector3> normalsB, Dictionary<Vector3, List<Vector3>> slicePoints) {
        if (slicePoints.Count <= 0) {
            return;
        }

        Queue<Vector3> queue = new Queue<Vector3>();
        queue.Enqueue(slicePoints.Keys.ToArray()[0]);
        var boundsA = GetMeshBounds(verticesA);
        var boundsB = GetMeshBounds(verticesB);
        while (queue.Count != 0) {
            var current = queue.Dequeue();
            if (!slicePoints.ContainsKey(current)) {
                continue;
            }
            var neighbor = slicePoints[current];
            if (neighbor.Count != 2) {
                Debug.LogError($"neighbor error,number is {neighbor.Count}");
                slicePoints.Remove(current);
                queue.Enqueue(neighbor[0]);
                continue;
            }
            var pointA = neighbor[0];
            var pointB = neighbor[1];
            queue.Enqueue(pointA);
            queue.Enqueue(pointB);

            var indexA = verticesA.Count;
            var indexB = verticesB.Count;
            var clockWiseNormal = (Vector3)math.normalize(math.cross(pointA - current, pointB - current));
            var antiClockWiseNormal = (Vector3)math.normalize(math.cross(pointB - current, pointA - current));

            verticesA.AddRange(new[] { current, pointA, pointB });
            verticesB.AddRange(new[] { current, pointA, pointB });

            if (math.dot(clockWiseNormal, current - boundsA.center) > 0) {
                trianglesA.AddRange(new[] { indexA, indexA + 1, indexA + 2 });
                //trianglesA.AddRange(new[] { indexA, indexA + 2, indexA + 1 }); //
                normalsA.AddRange(new []{clockWiseNormal,clockWiseNormal,clockWiseNormal});
                trianglesB.AddRange(new[] { indexB, indexB + 2, indexB + 1 });
                //trianglesB.AddRange(new[] { indexB, indexB + 1, indexB + 2 }); //
                normalsB.AddRange(new []{antiClockWiseNormal,antiClockWiseNormal,antiClockWiseNormal});
            } else {
                trianglesA.AddRange(new[] { indexA, indexA + 2, indexA + 1 });
                //trianglesA.AddRange(new[] { indexA, indexA + 1, indexA + 2 }); //
                normalsA.AddRange(new []{antiClockWiseNormal,antiClockWiseNormal,antiClockWiseNormal});
                trianglesB.AddRange(new[] { indexB, indexB + 1, indexB + 2 });
                //trianglesB.AddRange(new[] { indexB, indexB + 2, indexB + 1 }); //
                normalsB.AddRange(new []{clockWiseNormal,clockWiseNormal,clockWiseNormal});
            }

            // 更新顶点信息
            slicePoints.Remove(current);
            if (slicePoints.TryGetValue(pointA, out var neighborsA)) {
                neighborsA.Remove(current);
                neighborsA.Add(pointB);
            }
            if (slicePoints.TryGetValue(pointB, out var neighborsB)) {
                neighborsB.Remove(current);
                neighborsB.Add(pointA);
            }

            if (slicePoints.Count<3) {
                break;
            }
        }
    }

    private Bounds GetMeshBounds(List<Vector3> vertices) {
        Vector3 max = Vector3.zero;
        Vector3 min = Vector3.zero;
        foreach (var vertice in vertices) {
            max.x = math.max(vertice.x, max.x);
            max.y = math.max(vertice.y, max.y);
            max.z = math.max(vertice.z, max.z);
            min.x = math.min(vertice.x, max.x);
            min.y = math.min(vertice.y, max.y);
            min.z = math.min(vertice.z, max.z);
        }

        return new Bounds((max + min) / 2, max - min);
    }
}
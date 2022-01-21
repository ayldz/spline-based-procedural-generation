using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class QuadRing : MonoBehaviour
{
    [SerializeField, Range(0.01f, 1f)]
    private float _innerRadius;

    [SerializeField, Range(0.01f, 2f)]
    private float _thickness;

    private float RadiusOuter { get { return _innerRadius + _thickness; } }

    [SerializeField, Range(3, 32)]
    private int _angularSegments = 3;

    private int VertexCount { get { return _angularSegments * 2; } }

    private Mesh mesh;

    private void OnDrawGizmosSelected()
    {
        GizmosScript.DrawWireCircle(transform.position, transform.rotation, _innerRadius, _angularSegments);
        GizmosScript.DrawWireCircle(transform.position, transform.rotation, RadiusOuter, _angularSegments);
    }

    private void Awake()
    {
        mesh = new Mesh();
        mesh.name = "Quad Ring";

        GetComponent<MeshFilter>().sharedMesh = mesh;
    }

    private void Update() => GenerateMesh();

    private void GenerateMesh()
    {
        mesh.Clear();

        int vCount = VertexCount;
        List<Vector3> vertices = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();

        for (int i = 0; i < _angularSegments; i++)
        {
            float t = i / (float)_angularSegments;
            float angRad = t * MathScript.TAU;
            Vector2 dir = MathScript.GetUnitVectorByAngle(angRad);
            vertices.Add(dir * RadiusOuter);
            vertices.Add(dir * _innerRadius);
            normals.Add(Vector3.forward); // new Vector3(0,0,1);
            normals.Add(Vector3.forward);

        }

        List<int> triangles = new List<int>();
        for (int i = 0; i < _angularSegments; i++)
        {
            int indexRoot = i * 2;
            int indexInnerRoot = indexRoot + 1;
            int indexOuterNext = (indexRoot + 2) % vCount;
            int indexInnerNext = (indexRoot + 3) % vCount;

            triangles.Add(indexRoot);
            triangles.Add(indexOuterNext);
            triangles.Add(indexInnerNext);

            triangles.Add(indexRoot);
            triangles.Add(indexInnerNext);
            triangles.Add(indexInnerRoot);
        }

        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.SetNormals(normals);
    }
}
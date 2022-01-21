using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class RopeSegment : MonoBehaviour
{
    [SerializeField]
    private Mesh2D _shape2D;

    [SerializeField, Range(1,32)]
    private int _edgeRingCount = 8;

    [SerializeField]
    private Transform[] _controlPoints = new Transform[4];

    private float t = 0f;

    private Vector3 GetPos(int i) => _controlPoints[i].position;

    private Mesh _mesh;

    private void Awake()
    {
        _mesh = new Mesh();
        _mesh.name = "Segment";

        GetComponent<MeshFilter>().sharedMesh = _mesh; 
    }

    private void Update() => GenerateMesh();

    private void GenerateMesh() 
    {
        _mesh.Clear();

        // vertices

        List<Vector3> vertices = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();

        for (int ring = 0; ring < _edgeRingCount + 1; ring++)
        {
            float t = ring / (_edgeRingCount - 1f);

            OrientedPoint op = GetBezierOrientedPoint(t);

            for (int i = 0; i < _shape2D.vertices.Length; i++)
            {
                vertices.Add(op.LocalToWorldPos(_shape2D.vertices[i].point));
                normals.Add(op.LocalToWorldVector(_shape2D.vertices[i].normal));
            }
        }

        // triangles

        List<int> triangles = new List<int>();

        for (int ring = 0; ring < _edgeRingCount - 1; ring++)
        {
            int indexRoot = ring * _shape2D.vertices.Length;
            int indexRootNext = (ring + 1) * _shape2D.vertices.Length;

            for (int line = 0; line < _shape2D.lineIndices.Length; line+=2)
            {
                int lineIndexA = _shape2D.lineIndices[line];
                int lineIndexB = _shape2D.lineIndices[line + 1];

                int currentA = indexRoot + lineIndexA;
                int currentB = indexRoot + lineIndexB;
                int nextA = indexRootNext + lineIndexA;
                int nextB = indexRootNext + lineIndexB;
                
                triangles.Add(currentA);
                triangles.Add(nextA);
                triangles.Add(nextB);

                triangles.Add(currentA);
                triangles.Add(nextB);
                triangles.Add(currentB);
            }
        }

        _mesh.SetVertices(vertices);
        _mesh.SetTriangles(triangles, 0);
        _mesh.SetNormals(normals);
    }

    //private void OnDrawGizmos()
    //{
    //    for (int i = 0; i < 4; i++)
    //    {
    //        Gizmos.DrawSphere(GetPos(i), 0.2f);
    //    }

    //    Handles.DrawBezier(GetPos(0), GetPos(3), GetPos(1), GetPos(2), Color.white, EditorGUIUtility.whiteTexture, 1f);

    //    OrientedPoint testPoint = GetBezierOrientedPoint(t);

    //    Handles.PositionHandle(testPoint.pos, testPoint.rot);

    //    //void DrawPoint(Vector2 localPos) => Gizmos.DrawSphere(testPoint.LocalToWorld(localPos), 0.1f);

    //    Vector3[] verts = _shape2D.vertices.Select(v => testPoint.LocalToWorldPos(v.point)).ToArray();

    //    for (int i = 0; i < _shape2D.lineIndices.Length; i += 2)
    //    {
    //        Vector3 a = verts[_shape2D.lineIndices[i]];
    //        Vector3 b = verts[_shape2D.lineIndices[i + 1]];
    //        Gizmos.DrawLine(a, b);

    //    }
    //}

    private OrientedPoint GetBezierOrientedPoint(float t) 
    {
        Vector3 p0 = GetPos(0);
        Vector3 p1 = GetPos(1);
        Vector3 p2 = GetPos(2);
        Vector3 p3 = GetPos(3);

        Vector3 a = Vector3.Lerp(p0, p1, t);
        Vector3 b = Vector3.Lerp(p1, p2, t);
        Vector3 c = Vector3.Lerp(p2, p3, t);

        Vector3 d = Vector3.Lerp(a, b, t);
        Vector3 e = Vector3.Lerp(b, c, t);

        Vector3 pos = Vector3.Lerp(d, e, t);
        Vector3 tangent = (e - d).normalized;

        return new OrientedPoint(pos, tangent);
    }
}

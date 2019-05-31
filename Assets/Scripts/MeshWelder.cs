using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace B83.MeshHelper
{
    /*This was created by a user known as Bunny83 on the Unity3D forums at: http://answers.unity3d.com/questions/1382854/welding-vertices-at-runtime.html
     * I simply slotted it into my own project in an attempt to alleviate tile seam collisions.
     * Bunny83's original upload can be found at: https://www.dropbox.com/s/u0wfq42441pkoat/MeshWelder.cs?dl=0
     * I take no credit for the work done here*/

    public enum EVertexAttribute
    {
        Position = 0x0001,
        Normal = 0x0002,
        Tangent = 0x0004,
        Color = 0x0008,
        UV1 = 0x0010,
        UV2 = 0x0020,
        UV3 = 0x0040,
        UV4 = 0x0080,
        BoneWeight = 0x0100,
    }

    public class Vertex
    {
        public Vector3 pos;
        public Vector3 normal;
        public Vector4 tangent;
        public Color color;
        public Vector2 uv1;
        public Vector2 uv2;
        public Vector2 uv3;
        public Vector2 uv4;
        public BoneWeight bWeight;
        public Vertex(Vector3 aPos)
        {
            pos = aPos;
        }
    }

    public class MeshWelder
    {

        Vertex[] vertices;
        List<Vertex> newVerts;
        int[] map;

        EVertexAttribute m_Attributes;
        Mesh m_Mesh;
        public float MaxUVDelta = 0.0001f;
        public float MaxPositionDelta = 0.0001f;
        public float MaxAngleDelta = 0.01f;
        public float MaxColorDelta = 1f / 255f;
        public float MaxBWeightDelta = 0.01f;

        public MeshWelder(Mesh aMesh)
        {
            m_Mesh = aMesh;
        }

        private bool HasAttr(EVertexAttribute aAttr)
        {
            return (m_Attributes & aAttr) != 0;
        }
        private bool CompareColor(Color c1, Color c2)
        {
            return
                (Mathf.Abs(c1.r - c2.r) <= MaxColorDelta) &&
                (Mathf.Abs(c1.g - c2.g) <= MaxColorDelta) &&
                (Mathf.Abs(c1.b - c2.b) <= MaxColorDelta) &&
                (Mathf.Abs(c1.a - c2.a) <= MaxColorDelta);
        }

        private bool CompareBoneWeight(BoneWeight v1, BoneWeight v2)
        {
            if (v1.boneIndex0 != v2.boneIndex0 || v1.boneIndex1 != v2.boneIndex1 ||
                v1.boneIndex2 != v2.boneIndex2 || v1.boneIndex3 != v2.boneIndex3) return false;
            if (Mathf.Abs(v1.weight0 - v2.weight0) > MaxBWeightDelta) return false;
            if (Mathf.Abs(v1.weight1 - v2.weight1) > MaxBWeightDelta) return false;
            if (Mathf.Abs(v1.weight2 - v2.weight2) > MaxBWeightDelta) return false;
            if (Mathf.Abs(v1.weight3 - v2.weight3) > MaxBWeightDelta) return false;
            return true;
        }

        private bool Compare(Vertex v1, Vertex v2)
        {
            if ((v1.pos - v2.pos).sqrMagnitude > MaxPositionDelta) return false;
            if (HasAttr(EVertexAttribute.Normal) && Vector3.Angle(v1.normal, v2.normal) > MaxAngleDelta) return false;
            if (HasAttr(EVertexAttribute.Tangent) && Vector3.Angle(v1.tangent, v2.tangent) > MaxAngleDelta || v1.tangent.w != v2.tangent.w) return false;
            if (HasAttr(EVertexAttribute.Color) && !CompareColor(v1.color, v2.color)) return false;
            if (HasAttr(EVertexAttribute.UV1) && (v1.uv1 - v2.uv1).sqrMagnitude > MaxUVDelta) return false;
            if (HasAttr(EVertexAttribute.UV2) && (v1.uv2 - v2.uv2).sqrMagnitude > MaxUVDelta) return false;
            if (HasAttr(EVertexAttribute.UV3) && (v1.uv3 - v2.uv3).sqrMagnitude > MaxUVDelta) return false;
            if (HasAttr(EVertexAttribute.UV4) && (v1.uv4 - v2.uv4).sqrMagnitude > MaxUVDelta) return false;
            if (HasAttr(EVertexAttribute.BoneWeight) && !CompareBoneWeight(v1.bWeight, v2.bWeight)) return false;
            return true;
        }

        private void CreateVertexList()
        {
            var Positions = m_Mesh.vertices;
            var Normals = m_Mesh.normals;
            var Tangents = m_Mesh.tangents;
            var Colors = m_Mesh.colors;
            var Uv1 = m_Mesh.uv;
            var Uv2 = m_Mesh.uv2;
            var Uv3 = m_Mesh.uv3;
            var Uv4 = m_Mesh.uv4;
            var BWeights = m_Mesh.boneWeights;
            m_Attributes = EVertexAttribute.Position;
            if (Normals != null && Normals.Length > 0) m_Attributes |= EVertexAttribute.Normal;
            if (Tangents != null && Tangents.Length > 0) m_Attributes |= EVertexAttribute.Tangent;
            if (Colors != null && Colors.Length > 0) m_Attributes |= EVertexAttribute.Color;
            if (Uv1 != null && Uv1.Length > 0) m_Attributes |= EVertexAttribute.UV1;
            if (Uv2 != null && Uv2.Length > 0) m_Attributes |= EVertexAttribute.UV2;
            if (Uv3 != null && Uv3.Length > 0) m_Attributes |= EVertexAttribute.UV3;
            if (Uv4 != null && Uv4.Length > 0) m_Attributes |= EVertexAttribute.UV4;
            if (BWeights != null && BWeights.Length > 0) m_Attributes |= EVertexAttribute.BoneWeight;

            vertices = new Vertex[Positions.Length];
            for (int i = 0; i < Positions.Length; i++)
            {
                var v = new Vertex(Positions[i]);
                if (HasAttr(EVertexAttribute.Normal)) v.normal = Normals[i];
                if (HasAttr(EVertexAttribute.Tangent)) v.tangent = Tangents[i];
                if (HasAttr(EVertexAttribute.Color)) v.color = Colors[i];
                if (HasAttr(EVertexAttribute.UV1)) v.uv1 = Uv1[i];
                if (HasAttr(EVertexAttribute.UV2)) v.uv2 = Uv2[i];
                if (HasAttr(EVertexAttribute.UV3)) v.uv3 = Uv3[i];
                if (HasAttr(EVertexAttribute.UV4)) v.uv4 = Uv4[i];
                if (HasAttr(EVertexAttribute.BoneWeight)) v.bWeight = BWeights[i];
                vertices[i] = v;
            }
        }
        private void RemoveDuplicates()
        {
            map = new int[vertices.Length];
            newVerts = new List<Vertex>();
            for (int i = 0; i < vertices.Length; i++)
            {
                var v = vertices[i];
                bool dup = false;
                for (int i2 = 0; i2 < newVerts.Count; i2++)
                {
                    if (Compare(v, newVerts[i2]))
                    {
                        map[i] = i2;
                        dup = true;
                        break;
                    }
                }
                if (!dup)
                {
                    map[i] = newVerts.Count;
                    newVerts.Add(v);
                }
            }
        }
        private void AssignNewVertexArrays()
        {
            m_Mesh.vertices = newVerts.Select(v => v.pos).ToArray();
            if (HasAttr(EVertexAttribute.Normal))
                m_Mesh.normals = newVerts.Select(v => v.normal).ToArray();
            if (HasAttr(EVertexAttribute.Tangent))
                m_Mesh.tangents = newVerts.Select(v => v.tangent).ToArray();
            if (HasAttr(EVertexAttribute.Color))
                m_Mesh.colors = newVerts.Select(v => v.color).ToArray();
            if (HasAttr(EVertexAttribute.UV1))
                m_Mesh.uv = newVerts.Select(v => v.uv1).ToArray();
            if (HasAttr(EVertexAttribute.UV2))
                m_Mesh.uv2 = newVerts.Select(v => v.uv2).ToArray();
            if (HasAttr(EVertexAttribute.UV3))
                m_Mesh.uv3 = newVerts.Select(v => v.uv3).ToArray();
            if (HasAttr(EVertexAttribute.UV4))
                m_Mesh.uv4 = newVerts.Select(v => v.uv4).ToArray();
            if (HasAttr(EVertexAttribute.BoneWeight))
                m_Mesh.boneWeights = newVerts.Select(v => v.bWeight).ToArray();
        }

        private void RemapTriangles()
        {
            for (int n = 0; n < m_Mesh.subMeshCount; n++)
            {
                var tris = m_Mesh.GetTriangles(n);
                for (int i = 0; i < tris.Length; i++)
                {
                    tris[i] = map[tris[i]];
                }
                m_Mesh.SetTriangles(tris, n);
            }
        }
        public void Weld()
        {
            CreateVertexList();
            RemoveDuplicates();
            RemapTriangles();
            AssignNewVertexArrays();
        }
    }
}
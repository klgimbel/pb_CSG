using System;
using System.Collections.Generic;
using UnityEngine;

namespace Parabox.CSG
{
	static class VertexUtility
	{
        /// <summary>
        /// Allocate and fill all attribute arrays. This method will fill all arrays, regardless of whether or not real data populates the values (check what attributes a Vertex contains with HasAttribute()).
        /// </summary>
        /// <remarks>
        /// If you are using this function to rebuild a mesh, use SetMesh instead. SetMesh handles setting null arrays where appropriate for you.
        /// </remarks>
        /// <seealso cref="SetMesh"/>
        /// <param name="vertices">The source vertices.</param>
        /// <param name="position">A new array of the vertex position values.</param>
        /// <param name="color">A new array of the vertex color values.</param>
        /// <param name="uv0">A new array of the vertex uv0 values.</param>
        /// <param name="normal">A new array of the vertex normal values.</param>
        /// <param name="tangent">A new array of the vertex tangent values.</param>
        /// <param name="uv2">A new array of the vertex uv2 values.</param>
        /// <param name="uv3">A new array of the vertex uv3 values.</param>
        /// <param name="uv4">A new array of the vertex uv4 values.</param>
        public static void GetArrays(
            IList<Vertex> vertices,
            out Vector3[] position,
            out Color[] color,
            out Vector2[] uv0,
            out Vector3[] normal,
            out Vector4[] tangent,
            out Vector2[] uv2,
            out List<Vector4> uv3,
            out List<Vector4> uv4,
            Matrix4x4? worldToLocal = null)
        {
            GetArrays(vertices, out position, out color, out uv0, out normal, out tangent, out uv2, out uv3, out uv4, VertexAttributes.All, worldToLocal);
        }

        /// <summary>
        /// Allocate and fill the requested attribute arrays.
        /// </summary>
        /// <remarks>
        /// If you are using this function to rebuild a mesh, use SetMesh instead. SetMesh handles setting null arrays where appropriate for you.
        /// </remarks>
        /// <seealso cref="SetMesh"/>
        /// <param name="vertices">The source vertices.</param>
        /// <param name="position">A new array of the vertex position values if requested by the attributes parameter, or null.</param>
        /// <param name="color">A new array of the vertex color values if requested by the attributes parameter, or null.</param>
        /// <param name="uv0">A new array of the vertex uv0 values if requested by the attributes parameter, or null.</param>
        /// <param name="normal">A new array of the vertex normal values if requested by the attributes parameter, or null.</param>
        /// <param name="tangent">A new array of the vertex tangent values if requested by the attributes parameter, or null.</param>
        /// <param name="uv2">A new array of the vertex uv2 values if requested by the attributes parameter, or null.</param>
        /// <param name="uv3">A new array of the vertex uv3 values if requested by the attributes parameter, or null.</param>
        /// <param name="uv4">A new array of the vertex uv4 values if requested by the attributes parameter, or null.</param>
        /// <param name="attributes">A flag with the MeshAttributes requested.</param>
        /// <seealso cref="HasArrays"/>
        public static void GetArrays(
            IList<Vertex> vertices,
            out Vector3[] position,
            out Color[] color,
            out Vector2[] uv0,
            out Vector3[] normal,
            out Vector4[] tangent,
            out Vector2[] uv2,
            out List<Vector4> uv3,
            out List<Vector4> uv4,
            VertexAttributes attributes, 
            Matrix4x4? worldToLocal = null)
        {
            if (vertices == null)
                throw new ArgumentNullException("vertices");

            int vc = vertices.Count;
            var first = vc < 1 ? new Vertex() : vertices[0];

            bool hasPosition = ((attributes & VertexAttributes.Position) == VertexAttributes.Position) && first.hasPosition;
            bool hasColor = ((attributes & VertexAttributes.Color) == VertexAttributes.Color) && first.hasColor;
            bool hasUv0 = ((attributes & VertexAttributes.Texture0) == VertexAttributes.Texture0) && first.hasUV0;
            bool hasNormal = ((attributes & VertexAttributes.Normal) == VertexAttributes.Normal) && first.hasNormal;
            bool hasTangent = ((attributes & VertexAttributes.Tangent) == VertexAttributes.Tangent) && first.hasTangent;
            bool hasUv2 = ((attributes & VertexAttributes.Texture1) == VertexAttributes.Texture1) && first.hasUV2;
            bool hasUv3 = ((attributes & VertexAttributes.Texture2) == VertexAttributes.Texture2) && first.hasUV3;
            bool hasUv4 = ((attributes & VertexAttributes.Texture3) == VertexAttributes.Texture3) && first.hasUV4;

            position = hasPosition ? new Vector3[vc] : null;
            color = hasColor ? new Color[vc] : null;
            uv0 = hasUv0 ? new Vector2[vc] : null;
            normal = hasNormal ? new Vector3[vc] : null;
            tangent = hasTangent ? new Vector4[vc] : null;
            uv2 = hasUv2 ? new Vector2[vc] : null;
            uv3 = hasUv3 ? new List<Vector4>(vc) : null;
            uv4 = hasUv4 ? new List<Vector4>(vc) : null;

            for (int i = 0; i < vc; i++)
            {
                if (hasPosition)
                    position[i] = worldToLocal.HasValue ? worldToLocal.Value.MultiplyPoint(vertices[i].position) : vertices[i].position;
                if (hasColor)
                    color[i] = vertices[i].color;
                if (hasUv0)
                    uv0[i] = vertices[i].uv0;
                if (hasNormal)
                    normal[i] = worldToLocal.HasValue ? worldToLocal.Value.MultiplyVector(vertices[i].normal) : vertices[i].normal;
                if (hasTangent)
                    tangent[i] = worldToLocal.HasValue ? worldToLocal.Value.rotation * vertices[i].tangent : vertices[i].tangent;
                if (hasUv2)
                    uv2[i] = vertices[i].uv2;
                if (hasUv3)
                    uv3.Add(vertices[i].uv3);
                if (hasUv4)
                    uv4.Add(vertices[i].uv4);
            }
        }

        public static List<Vertex> GetVertices(this Mesh mesh, Matrix4x4? localToWorld = null)
        {
            if (mesh == null)
                return null;

            int vertexCount = mesh.vertexCount;

            Vector3[] positions = mesh.vertices;
            Color[] colors = mesh.colors;
            Vector3[] normals = mesh.normals;
            Vector4[] tangents = mesh.tangents;
            Vector2[] uv0s = mesh.uv;
            Vector2[] uv2s = mesh.uv2;
            List<Vector4> uv3s = new List<Vector4>();
            List<Vector4> uv4s = new List<Vector4>();
            mesh.GetUVs(2, uv3s);
            mesh.GetUVs(3, uv4s);

            bool _hasPositions = positions != null && positions.Length == vertexCount;
            bool _hasColors = colors != null && colors.Length == vertexCount;
            bool _hasNormals = normals != null && normals.Length == vertexCount;
            bool _hasTangents = tangents != null && tangents.Length == vertexCount;
            bool _hasUv0 = uv0s != null && uv0s.Length == vertexCount;
            bool _hasUv2 = uv2s != null && uv2s.Length == vertexCount;
            bool _hasUv3 = uv3s.Count == vertexCount;
            bool _hasUv4 = uv4s.Count == vertexCount;

            List<Vertex> v = new List<Vertex>(vertexCount);
            for (int i = 0; i < vertexCount; i++)
            {
                var vn = new Vertex();

                if (_hasPositions)
                    vn.position = localToWorld.HasValue ? localToWorld.Value.MultiplyPoint(positions[i]) : positions[i];

                if (_hasColors)
                    vn.color = colors[i];

                if (_hasNormals)
                    vn.normal = localToWorld.HasValue ? localToWorld.Value.MultiplyVector(normals[i]) : normals[i];

                if (_hasTangents)
                    vn.tangent = localToWorld.HasValue ? localToWorld.Value.rotation * tangents[i] : tangents[i];

                if (_hasUv0)
                    vn.uv0 = uv0s[i];

                if (_hasUv2)
                    vn.uv2 = uv2s[i];

                if (_hasUv3)
                    vn.uv3 = uv3s[i];

                if (_hasUv4)
                    vn.uv4 = uv4s[i];

                v.Add(vn);
            }

            return v;
        }

        /// <summary>
        /// Replace mesh values with vertex array. Mesh is cleared during this function, so be sure to set the triangles after calling.
        /// </summary>
        /// <param name="mesh">The target mesh.</param>
        /// <param name="vertices">The vertices to replace the mesh attributes with.</param>
        public static void SetMesh(Mesh mesh, IList<Vertex> vertices, Matrix4x4? worldToLocal = null)
        {
            if (mesh == null)
                throw new ArgumentNullException("mesh");

            if (vertices == null)
                throw new ArgumentNullException("vertices");

            Vector3[] positions = null;
            Color[] colors = null;
            Vector2[] uv0s = null;
            Vector3[] normals = null;
            Vector4[] tangents = null;
            Vector2[] uv2s = null;
            List<Vector4> uv3s = null;
            List<Vector4> uv4s = null;

            GetArrays(vertices, out positions,
                out colors,
                out uv0s,
                out normals,
                out tangents,
                out uv2s,
                out uv3s,
                out uv4s,
                worldToLocal);

            mesh.Clear();

            Vertex first = vertices[0];

            if (first.hasPosition) mesh.vertices = positions;
            if (first.hasColor) mesh.colors = colors;
            if (first.hasUV0) mesh.uv = uv0s;
            if (first.hasNormal) mesh.normals = normals;
            if (first.hasTangent) mesh.tangents = tangents;
            if (first.hasUV2) mesh.uv2 = uv2s;
            if (first.hasUV3)
                if (uv3s != null)
                    mesh.SetUVs(2, uv3s);
            if (first.hasUV4)
                if (uv4s != null)
                    mesh.SetUVs(3, uv4s);
        }

        /// <summary>
        /// Linearly interpolate between two vertices.
        /// </summary>
        /// <param name="x">Left parameter.</param>
        /// <param name="y">Right parameter.</param>
        /// <param name="weight">The weight of the interpolation. 0 is fully x, 1 is fully y.</param>
        /// <returns>A new vertex interpolated by weight between x and y.</returns>
        public static Vertex Mix(this Vertex x, Vertex y, float weight)
        {
            float i = 1f - weight;

            Vertex v = new Vertex();

            v.position = x.position * i + y.position * weight;

            if (x.hasColor && y.hasColor)
                v.color = x.color * i + y.color * weight;
            else if (x.hasColor)
                v.color = x.color;
            else if (y.hasColor)
                v.color = y.color;

            if (x.hasNormal && y.hasNormal)
                v.normal = x.normal * i + y.normal * weight;
            else if (x.hasNormal)
                v.normal = x.normal;
            else if (y.hasNormal)
                v.normal = y.normal;

            if (x.hasTangent && y.hasTangent)
                v.tangent = x.tangent * i + y.tangent * weight;
            else if (x.hasTangent)
                v.tangent = x.tangent;
            else if (y.hasTangent)
                v.tangent = y.tangent;

            if (x.hasUV0 && y.hasUV0)
                v.uv0 = x.uv0 * i + y.uv0 * weight;
            else if (x.hasUV0)
                v.uv0 = x.uv0;
            else if (y.hasUV0)
                v.uv0 = y.uv0;

            if (x.hasUV2 && y.hasUV2)
                v.uv2 = x.uv2 * i + y.uv2 * weight;
            else if (x.hasUV2)
                v.uv2 = x.uv2;
            else if (y.hasUV2)
                v.uv2 = y.uv2;

            if (x.hasUV3 && y.hasUV3)
                v.uv3 = x.uv3 * i + y.uv3 * weight;
            else if (x.hasUV3)
                v.uv3 = x.uv3;
            else if (y.hasUV3)
                v.uv3 = y.uv3;

            if (x.hasUV4 && y.hasUV4)
                v.uv4 = x.uv4 * i + y.uv4 * weight;
            else if (x.hasUV4)
                v.uv4 = x.uv4;
            else if (y.hasUV4)
                v.uv4 = y.uv4;

            return v;
        }
	}
}

using System;
using System.Collections.Generic;
using CUE4Parse.UE4.Assets.Exports.Animation;
using CUE4Parse.UE4.Objects.Core.Math;
using OpenTK.Graphics.OpenGL4;

namespace FModel.Views.Snooper.Models;

public class Morph : IDisposable
{
    private int _handle;

    public static readonly int VertexSize = 6; // Position + Tangent

    public readonly string Name;
    public readonly float[] Vertices;

    public Morph(float[] vertices, int vertexSize, UMorphTarget morphTarget)
    {
        Name = morphTarget.Name;
        Vertices = new float[vertices.Length / vertexSize * VertexSize];

        var morphLookup = new Dictionary<uint, (FVector positionDelta, FVector tangentDelta)>();
        foreach (var vertex in morphTarget.MorphLODModels[0].Vertices)
            morphLookup[vertex.SourceIdx] = (vertex.PositionDelta, vertex.TangentZDelta);

        for (int i = 0; i < vertices.Length; i += vertexSize)
        {
            var baseIndex = i / vertexSize * VertexSize;
            if (morphLookup.TryGetValue((uint) vertices[i], out var delta))
            {
                Vertices[baseIndex + 0] = vertices[i + 1] + delta.positionDelta.X * Constants.SCALE_DOWN_RATIO;
                Vertices[baseIndex + 1] = vertices[i + 2] + delta.positionDelta.Z * Constants.SCALE_DOWN_RATIO;
                Vertices[baseIndex + 2] = vertices[i + 3] + delta.positionDelta.Y * Constants.SCALE_DOWN_RATIO;
                Vertices[baseIndex + 3] = vertices[i + 7] + delta.tangentDelta.X;
                Vertices[baseIndex + 4] = vertices[i + 8] + delta.tangentDelta.Z;
                Vertices[baseIndex + 5] = vertices[i + 9] + delta.tangentDelta.Y;
            }
            else
            {
                Vertices[baseIndex + 0] = vertices[i + 1];
                Vertices[baseIndex + 1] = vertices[i + 2];
                Vertices[baseIndex + 2] = vertices[i + 3];
                Vertices[baseIndex + 3] = vertices[i + 7];
                Vertices[baseIndex + 4] = vertices[i + 8];
                Vertices[baseIndex + 5] = vertices[i + 9];
            }
        }
    }

    public Morph(float[] vertices, Dictionary<uint, int> dict, UMorphTarget morphTarget)
    {
        Name = morphTarget.Name;
        Vertices = new float[vertices.Length];
        Array.Copy(vertices, Vertices, vertices.Length);

        foreach (var vert in morphTarget.MorphLODModels[0].Vertices)
        {
            var count = 0;
            if (dict.TryGetValue(vert.SourceIdx, out var baseIndex))
            {
                Vertices[baseIndex + count++] += vert.PositionDelta.X * Constants.SCALE_DOWN_RATIO;
                Vertices[baseIndex + count++] += vert.PositionDelta.Z * Constants.SCALE_DOWN_RATIO;
                Vertices[baseIndex + count++] += vert.PositionDelta.Y * Constants.SCALE_DOWN_RATIO;
                Vertices[baseIndex + count++] += vert.TangentZDelta.X;
                Vertices[baseIndex + count++] += vert.TangentZDelta.Z;
                Vertices[baseIndex + count++] += vert.TangentZDelta.Y;
            }
        }
    }

    public void Setup()
    {
        _handle = GL.CreateProgram();
    }

    public void Dispose()
    {
        GL.DeleteProgram(_handle);
    }
}

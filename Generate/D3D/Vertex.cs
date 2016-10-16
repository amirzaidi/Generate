using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System.Runtime.InteropServices;

namespace Generate.D3D
{
    [StructLayout(LayoutKind.Sequential)]
    struct Vertex
    {
        internal Vector4 Position;
        internal Vector2 TexCoords;
        internal Vector3 Normal;

        internal static InputElement[] Layout = new[]
        {
            new InputElement("POSITION", 0, Format.R32G32B32A32_Float, 0, 0, InputClassification.PerVertexData, 0),
            new InputElement("TEXCOORD", 0, Format.R32G32_Float, InputElement.AppendAligned, 0, InputClassification.PerVertexData, 0),
            new InputElement("NORMAL", 0, Format.R32G32B32_Float, InputElement.AppendAligned, 0, InputClassification.PerVertexData, 0)
        };

        internal static void CalcSurfaceNormal(ref Vertex[] Vertices, int Start = 0, int End = 0)
        {
            if (End == 0)
            {
                End = Vertices.Length;
            }

            var U = Vertices[Start + 1].Position - Vertices[Start].Position;
            var V = Vertices[Start + 2].Position - Vertices[Start].Position;

            Vertices[Start].Normal.X = U.Y * V.Z - U.Z * V.Y;
            Vertices[Start].Normal.Y = U.Z * V.X - U.X * V.Z;
            Vertices[Start].Normal.Z = U.X * V.Y - U.Y * V.X;

            for (int i = Start + 1; i < End; i++)
            {
                Vertices[i].Normal = Vertices[0].Normal;
            }
        }
    }
}

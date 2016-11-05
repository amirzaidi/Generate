using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;
using System;

namespace Generate.D3D
{
    abstract class IShader : IDisposable
    {
        protected DeviceContext Context;
        protected Matrix Projection;

        protected ShaderSignature Signature;
        protected VertexShader VS;
        protected PixelShader PS;

        protected InputLayout InputLayout;
        protected DataStream BufferStream;

        internal virtual void Prepare()
        {
            Context.InputAssembler.InputLayout = InputLayout;
            Context.VertexShader.Set(VS);
            Context.PixelShader.Set(PS);
        }

        internal abstract void UpdateWorld(Matrix World);

        internal abstract void End();

        public virtual void Dispose()
        {
            Utilities.Dispose(ref BufferStream);
            Utilities.Dispose(ref InputLayout);
            Utilities.Dispose(ref PS);
            Utilities.Dispose(ref VS);
            Utilities.Dispose(ref Signature);
        }
    }
}

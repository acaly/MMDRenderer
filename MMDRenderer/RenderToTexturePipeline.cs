using LightDx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MMDRenderer
{
    class RenderToTexturePipeline
    {
        private struct VSConstants
        {
            public Matrix4x4 World;
            public Matrix4x4 View;
            public Matrix4x4 Projection;
        }

        private readonly LightDevice _device;
        private readonly Pipeline _pipeline;
        private readonly ConstantBuffer<VSConstants> _constantBufferVS;

        public ref Matrix4x4 World => ref _constantBufferVS.Value.World;
        public ref Matrix4x4 View => ref _constantBufferVS.Value.View;
        public ref Matrix4x4 Projection => ref _constantBufferVS.Value.Projection;

        public LightDevice Device => _device;

        public RenderToTexturePipeline(LightDevice device)
        {
            _device = device;

            //pipeline
            _pipeline = device.CompilePipeline(InputTopology.Triangle,
                 ShaderSource.FromResource("RenderToTextureVS.fx", ShaderType.Vertex),
                 ShaderSource.FromResource("RenderToTexturePS.fx", ShaderType.Pixel));
            _pipeline.SetBlender(Blender.AlphaBlender);

            //constant (VS)
            _constantBufferVS = _pipeline.CreateConstantBuffer<VSConstants>();
            _pipeline.SetConstant(ShaderType.Vertex, 0, _constantBufferVS);
            _constantBufferVS.Value.World = Matrix4x4.Identity;

            device.ResolutionChanged += (sender, e) => SetupProjMatrix();
            SetupProjMatrix();
        }

        private void SetupProjMatrix()
        {
            _constantBufferVS.Value.Projection =
                _device.CreatePerspectiveFieldOfView((float)Math.PI / 4).Transpose();
        }

        public VertexDataProcessor<T> CreateVertexDataProcessor<T>() where T : unmanaged
        {
            return _pipeline.CreateVertexDataProcessor<T>();
        }

        public void SetTexture(Texture2D tex)
        {
            _pipeline.SetResource(0, tex);
        }

        public void UpdateConstants() => _constantBufferVS.Update();

        public void Apply()
        {
            _pipeline.Apply();
        }
    }
}

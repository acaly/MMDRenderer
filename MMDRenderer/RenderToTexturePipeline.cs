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
        private readonly Pipeline _pipeline1;
        private readonly Pipeline _pipeline2;
        private readonly ConstantBuffer<VSConstants> _constantBufferVS;

        public ref Matrix4x4 World => ref _constantBufferVS.Value.World;
        public ref Matrix4x4 View => ref _constantBufferVS.Value.View;
        public ref Matrix4x4 Projection => ref _constantBufferVS.Value.Projection;

        public LightDevice Device => _device;

        public RenderToTexturePipeline(LightDevice device)
        {
            _device = device;

            //pipeline
            _pipeline1 = device.CompilePipeline(InputTopology.Triangle,
                 ShaderSource.FromResource("RenderToTextureVS.fx", ShaderType.Vertex),
                 ShaderSource.FromResource("RenderToTexturePS.fx", ShaderType.Pixel));
            _pipeline1.SetBlender(Blender.AlphaBlender);

            _pipeline2 = device.CompilePipeline(InputTopology.Triangle,
                 ShaderSource.FromResource("RenderToTextureVS.fx", ShaderType.Vertex),
                 ShaderSource.FromResource("ForwardRenderingPS.fx", ShaderType.Pixel));
            _pipeline2.SetBlender(Blender.AlphaBlender);

            //constant (VS)
            _constantBufferVS = _pipeline1.CreateConstantBuffer<VSConstants>();
            _pipeline1.SetConstant(ShaderType.Vertex, 0, _constantBufferVS);
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
            return _pipeline1.CreateVertexDataProcessor<T>();
        }

        public void SetTexture(Texture2D tex)
        {
            if (_pipeline1.IsActive)
            {
                _pipeline1.SetResource(0, tex);
            }
            else
            {
                _pipeline2.SetResource(0, tex);
            }
        }

        public void UpdateConstants() => _constantBufferVS.Update();

        public void ApplyDeferred()
        {
            _pipeline1.Apply();
        }

        public void ApplyForward()
        {
            _pipeline2.Apply();
        }
    }
}

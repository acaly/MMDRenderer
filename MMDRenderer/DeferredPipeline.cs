using LightDx;
using LightDx.InputAttributes;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MMDRenderer
{
    class DeferredPipeline
    {
        private struct PSConstants
        {
            public Vector4 ClearColor;
        }

        private struct Vertex
        {
            [Position]
            public Vector2 Position;
        }

        private readonly LightDevice _device;
        private readonly RenderTargetList _target;
        private readonly Pipeline _pipeline;
        private readonly ConstantBuffer<PSConstants> _psConstants;
        private readonly VertexBuffer _vertexBuffer;

        public ref Vector4 ClearColor => ref _psConstants.Value.ClearColor;

        public DeferredPipeline(LightDevice device, DeferredTarget target)
        {
            _device = device;

            //target
            var screenTarget = device.GetDefaultTarget();
            _target = new RenderTargetList(screenTarget);

            //pipeline
            _pipeline = device.CompilePipeline(InputTopology.Triangle,
                ShaderSource.FromResource("Deferred.fx", ShaderType.Vertex | ShaderType.Pixel));

            _psConstants = _pipeline.CreateConstantBuffer<PSConstants>();
            _psConstants.Value.ClearColor = Color.AliceBlue.WithAlpha(1);
            _pipeline.SetConstant(ShaderType.Pixel, 0, _psConstants);

            _pipeline.SetResource(0, target.ColorData);
            _pipeline.SetResource(1, target.DepthData);
            _pipeline.SetResource(2, target.NormalData);

            //vb
            var processor = _pipeline.CreateVertexDataProcessor<Vertex>();
            var data = new Vertex[]
            {
                    new Vertex { Position = new Vector2(0, 0) },
                    new Vertex { Position = new Vector2(1, 0) },
                    new Vertex { Position = new Vector2(0, 1) },
                    new Vertex { Position = new Vector2(0, 1) },
                    new Vertex { Position = new Vector2(1, 0) },
                    new Vertex { Position = new Vector2(1, 1) },
            };
            _vertexBuffer = processor.CreateImmutableBuffer(data);
        }

        public void Render()
        {
            _psConstants.Update();
            _target.ClearAll();
            _target.Apply();
            _pipeline.Apply();
            _vertexBuffer.DrawAll();
        }
    }
}

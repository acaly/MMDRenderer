using LightDx;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMDRenderer
{
    class DeferredTarget
    {
        private readonly LightDevice _device;
        private readonly RenderTargetList _renderTarget1;
        private readonly RenderTargetList _renderTarget2;
        private readonly RenderTargetObject _normalTargetObj;
        private readonly Texture2D _colorData, _depthData, _normalData;
        public Texture2D ColorData => _colorData;
        public Texture2D DepthData => _depthData;
        public Texture2D NormalData => _normalData;

        public DeferredTarget(LightDevice device)
        {
            _device = device;

            //target & output
            var screenTarget = device.GetDefaultTarget();
            var colorTarget = device.CreateTextureTarget();
            var normalTarget = device.CreateTextureTarget();
            var depthTarget = device.CreateDepthStencilTarget();
            _normalTargetObj = normalTarget;

            _renderTarget1 = new RenderTargetList(colorTarget, normalTarget, depthTarget);
            _renderTarget2 = new RenderTargetList(screenTarget, normalTarget, depthTarget);

            colorTarget.ClearColor = Color.Black.WithAlpha(0);

            _colorData = colorTarget.GetTexture2D();
            _depthData = depthTarget.GetTexture2D();
            _normalData = normalTarget.GetTexture2D();
        }

        public void ClearAll() => _renderTarget1.ClearAll();
        public void ClearDeferred() => _normalTargetObj.Clear();
        public void ApplyDeferred() => _renderTarget1.Apply();
        public void ApplyForward() => _renderTarget2.Apply();
    }
}

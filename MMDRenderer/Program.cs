using LightDx;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MMDRenderer
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            string filename;
            using (var dialog = new OpenFileDialog { Filter = "*.pmx|*.pmx" })
            {
                if (dialog.ShowDialog() == DialogResult.Cancel) return;
                filename = dialog.FileName;
            }

            var form = new Form
            {
                ClientSize = new Size(800, 600),
                Text = "Pmx Viewer",
            };
            var device = LightDevice.Create(form);

            var renderToTexturePipeline = new RenderToTexturePipeline(device);
            var deferredTarget = new DeferredTarget(device);
            var deferredRendering1 = new DeferredRenderer(device, "Deferred1.fx",
                new[] { deferredTarget.ColorData, deferredTarget.NormalData, deferredTarget.DepthData }, false);
            var deferredRendering2 = new DeferredRenderer(device, "Deferred2.fx",
                new[] { deferredTarget.NormalData, deferredTarget.DepthData }, true);
            deferredRendering2.ClearColor = Color.Black.WithAlpha(0);

            var model = new PmxModel(renderToTexturePipeline, filename);
            model.HideMaterial(29); //Hide the shadow of hair. We don't have diffuse color in shader.
            model.SetTransparent(26);
            var camera = new Camera(form);

            form.Show();
            form.Activate();
            var frameCounter = new FrameCounter();
            frameCounter.Start();

            device.RunMultithreadLoop(delegate ()
            {
                var time = frameCounter.NextFrame() / 1000;

                camera.Update();

                renderToTexturePipeline.View = camera.GetViewMatrix().Transpose();
                renderToTexturePipeline.UpdateConstants();

                deferredTarget.ClearAll();
                deferredTarget.ApplyDeferred();
                renderToTexturePipeline.ApplyDeferred();
                model.DrawSolid();

                deferredRendering1.ClearAll();
                deferredRendering1.Render();

                deferredTarget.ClearDeferred();
                deferredTarget.ApplyForward();
                renderToTexturePipeline.ApplyForward();
                model.DrawTransparent();
                
                deferredRendering2.Render();

                device.Present(true);
            });
        }
    }
}

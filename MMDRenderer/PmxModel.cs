using LightDx;
using LightDx.InputAttributes;
using MMDataIO.Pmx;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MMDRenderer
{
    class PmxModel
    {
        private struct Vertex
        {
            [Position]
            public Vector3 Position;
            [TexCoord]
            public Vector2 TexCoord;
            [Normal]
            public Vector3 Normal;
        }

        private readonly RenderToTexturePipeline _pipeline;
        private readonly PmxModelData _model;
        private readonly Texture2D[] _textures;
        private readonly VertexBuffer[] _vertexBuffers;
        private readonly bool[] _isTransparent;

        public PmxModel(RenderToTexturePipeline pipeline, string filename)
        {
            _pipeline = pipeline;

            _model = new PmxModelData();
            using (var r = new BinaryReader(File.OpenRead(filename)))
            {
                _model.Read(r);
            }

            _textures = new Texture2D[_model.TextureFiles.Length];
            var path = Path.GetDirectoryName(filename);
            var device = pipeline.Device;
            for (int i = 0; i < _textures.Length; ++i)
            {
                var src = _model.TextureFiles[i];
                var fullName = Path.Combine(path, src);
                if (Path.GetExtension(src) == ".tga")
                {
                    using (var tga = TgaDecoder.FromFile(fullName))
                    {
                        _textures[i] = device.CreateTexture2D(tga);
                    }
                }
                else
                {
                    using (var textureFile = File.OpenRead(fullName))
                    {
                        _textures[i] = device.CreateTexture2D(textureFile);
                    }
                }
            }

            var bufferProcesser = pipeline.CreateVertexDataProcessor<Vertex>();
            _vertexBuffers = new VertexBuffer[_model.MaterialArray.Length];
            for (int i = 0, indexOffset = 0; i < _model.MaterialArray.Length; ++i)
            {
                var mat = _model.MaterialArray[i];
                var vertexData = new Vertex[mat.FaceCount];
                for (int j = 0; j < mat.FaceCount; ++j)
                {
                    var index = _model.VertexIndices[indexOffset + j];
                    var vertex = _model.VertexArray[Math.Abs(index)];
                    vertexData[j].Position = vertex.Pos;
                    vertexData[j].TexCoord = vertex.Uv;
                    vertexData[j].Normal = vertex.Normal;
                }
                indexOffset += mat.FaceCount;
                _vertexBuffers[i] = bufferProcesser.CreateImmutableBuffer(vertexData);
            }

            _isTransparent = new bool[_vertexBuffers.Length];
        }

        public void HideMaterial(int id)
        {
            _vertexBuffers[id] = null;
        }

        public void SetTransparent(int id)
        {
            _isTransparent[id] = true;
        }

        public void DrawSolid() => DrawGroup(false);
        public void DrawTransparent() => DrawGroup(true);

        private void DrawGroup(bool isTransparent)
        {
            for (int i = 0; i < _vertexBuffers.Length; ++i)
            {
                if (_vertexBuffers[i] == null) continue;
                if (_isTransparent[i] != isTransparent) continue;
                if (_model.MaterialArray[i].TextureId >= _textures.Length)
                {
                    _pipeline.SetTexture(null);
                }
                else
                {
                    _pipeline.SetTexture(_textures[_model.MaterialArray[i].TextureId]);
                }
                _vertexBuffers[i].DrawAll();
            }
        }
    }
}

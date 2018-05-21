using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MMDRenderer
{
    class TgaDecoder
    {
        [StructLayout(LayoutKind.Explicit)]
        private struct TgaHeader
        {
            [FieldOffset(0)] public byte IdFieldLength;
            [FieldOffset(1)] public byte ColorMapType;
            [FieldOffset(2)] public byte ImageType;
            [FieldOffset(3)] public ushort ColorMapIndex;
            [FieldOffset(5)] public ushort ColorMapLength;
            [FieldOffset(7)] public byte ColorMapDepth;
            [FieldOffset(8)] public ushort ImageOriginX;
            [FieldOffset(10)] public ushort ImageOriginY;
            [FieldOffset(12)] public ushort ImageWidth;
            [FieldOffset(14)] public ushort ImageHeight;
            [FieldOffset(16)] public byte BitPerPixel;
            [FieldOffset(17)] public byte Descriptor;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct StructAsByteArray
        {
            [FieldOffset(0)]
            private TgaHeader[] HeaderData;
            [FieldOffset(0)]
            private byte[] ByteData;

            public StructAsByteArray(int i)
            {
                HeaderData = null;
                ByteData = new byte[Marshal.SizeOf<TgaHeader>()];
            }

            public ref TgaHeader Data => ref HeaderData[0];
            public byte[] Buffer => ByteData;
        }

        public static Bitmap FromFile(string filename)
        {
            using (var stream = File.OpenRead(filename))
            {
                return FromStream(stream);
            }
        }
        
        public static Bitmap FromStream(Stream stream)
        {
            var header = new StructAsByteArray(0);
            var headerBuffer = header.Buffer;
            ref var headerData = ref header.Data;
            stream.Read(headerBuffer, 0, headerBuffer.Length);
            if (headerData.ColorMapType != 0 ||
                headerData.ImageType != 2 && headerData.ImageType != 10 ||
                headerData.BitPerPixel != 24 && headerData.BitPerPixel != 32)
            {
                throw new NotImplementedException();
            }

            var colorData = new byte[stream.Length - stream.Position];
            stream.Read(colorData, 0, colorData.Length);
            var ret = new Bitmap(headerData.ImageWidth, headerData.ImageHeight, PixelFormat.Format32bppArgb);

            int elementCount = headerData.BitPerPixel / 8;
            if (headerData.ImageType == 9 || headerData.ImageType == 10 || headerData.ImageType == 11)
            {
                byte[] elements = new byte[elementCount];
                int decodeBufferLength = elementCount * ret.Width * ret.Height;
                byte[] decodeBuffer = new byte[decodeBufferLength];
                int decoded = 0;
                int offset = 0;
                while (decoded < decodeBufferLength)
                {
                    int packet = colorData[offset++];
                    if ((packet & 0x80) != 0)
                    {
                        for (int i = 0; i < elementCount; i++)
                        {
                            elements[i] = colorData[offset++];
                        }
                        int count = (packet & 0x7F) + 1;
                        for (int i = 0; i < count; i++)
                        {
                            for (int j = 0; j < elementCount; j++)
                            {
                                decodeBuffer[decoded++] = elements[j];
                            }
                        }
                    }
                    else
                    {
                        int count = (packet + 1) * elementCount;
                        for (int i = 0; i < count; i++)
                        {
                            decodeBuffer[decoded++] = colorData[offset++];
                        }
                    }
                }
                colorData = decodeBuffer;
            }

            var bitmapData = ret.LockBits(new Rectangle(0, 0, ret.Width, ret.Height),
                ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            try
            {
                for (int y = 0; y < ret.Height; ++y)
                {
                    var dy = (headerData.Descriptor & 0x20) == 0 ? (ret.Height - 1 - y) : y;
                    dy *= (ret.Width * elementCount);
                    for (int x = 0; x < ret.Width; ++x)
                    {
                        var dx = (headerData.Descriptor & 0x10) == 0 ? x : (ret.Width - 1 - x);
                        dx *= elementCount;
                        var index = dx + dy;

                        int b = colorData[index + 0];
                        int g = colorData[index + 1];
                        int r = colorData[index + 2];
                        int a = 255;
                        if (elementCount == 4) a = colorData[index + 3];
                        int color = (a << 24) | (r << 16) | (g << 8) | b;

                        Marshal.WriteInt32(bitmapData.Scan0, bitmapData.Stride * y + 4 * x, color);
                    }
                }
            }
            finally
            {
                ret.UnlockBits(bitmapData);
            }
            return ret;
        }
    }
}

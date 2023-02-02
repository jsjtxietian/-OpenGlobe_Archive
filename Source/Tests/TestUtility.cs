﻿#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using System.Drawing;
using NUnit.Framework;
using OpenGlobe.Core;
using OpenGlobe.Renderer;

namespace OpenGlobe
{
    public static class TestUtility
    {
        /// <summary>
        /// Creates a frame buffer with a 1x1 RGB color attachment.
        /// </summary>
        public static Framebuffer CreateFramebuffer(Context context)
        {
            Framebuffer framebuffer = context.CreateFramebuffer();
            framebuffer.ColorAttachments[0] = Device.CreateTexture2D(
                new Texture2DDescription(1, 1, TextureFormat.RedGreenBlue8, false));
            framebuffer.DepthAttachment = Device.CreateTexture2D(
                new Texture2DDescription(1, 1, TextureFormat.Depth24, false));

            return framebuffer;
        }

        /// <summary>
        /// Creates a 1x1 RGBA8 texture
        /// </summary>
        public static Texture2D CreateTexture(BlittableRGBA rgba)
        {
            Texture2DDescription description = new Texture2DDescription(1, 1, TextureFormat.RedGreenBlueAlpha8, false);
            Texture2D texture = Device.CreateTexture2D(description);

            BlittableRGBA[] pixels = new BlittableRGBA[] { rgba };

            using (WritePixelBuffer writePixelBuffer = Device.CreateWritePixelBuffer(PixelBufferHint.Stream, ArraySizeInBytes.Size(pixels)))
            {
                writePixelBuffer.CopyFromSystemMemory(pixels);
                texture.CopyFromBuffer(writePixelBuffer, ImageFormat.RedGreenBlueAlpha, ImageDatatype.UnsignedByte);
            }

            return texture;
        }

        public static VertexArray CreateVertexArray(Context context, int positionLocation)
        {
            Vector4F[] positions = new[] { new Vector4F(0, 0, 0, 1) };
            VertexBuffer positionsBuffer = Device.CreateVertexBuffer(BufferHint.StaticDraw, ArraySizeInBytes.Size(positions));
            positionsBuffer.CopyFromSystemMemory(positions);
            
            VertexArray va = context.CreateVertexArray();
            va.Attributes[positionLocation] = new VertexBufferAttribute(positionsBuffer, ComponentDatatype.Float, 4);
            va.DisposeBuffers = true;

            return va;
        }

        public static RenderState CreateRenderStateWithoutDepthTest()
        {
            RenderState rs = new RenderState();
            rs.DepthTest.Enabled = false;
            return rs;
        }

        public static void ValidateColor(Texture2D colorTexture, byte red, byte green, byte blue)
        {
            using (ReadPixelBuffer readPixelBuffer = colorTexture.CopyToBuffer(ImageFormat.RedGreenBlue, ImageDatatype.UnsignedByte, 1))
            {
                byte[] color = readPixelBuffer.CopyToSystemMemory<byte>();
                Assert.AreEqual(red, color[0], "Red does not match");
                Assert.AreEqual(green, color[1], "Green does not match");
                Assert.AreEqual(blue, color[2], "Blue does not match");
            }
        }
    }
}

﻿#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using System.Drawing;
using System.Runtime.InteropServices;
using NUnit.Framework;
using OpenGlobe.Core;

namespace OpenGlobe.Renderer
{
    /// <summary>
    /// System tests for OpenGlobe.Renderer.  System tests are higher level 
    /// than unit tests; instead of validating a single class, system tests 
    /// use multiple classes to validate a more complicated task.
    /// </summary>
    [TestFixture]
    public class SystemTests
    {
        [Test]
        public void ClearColorDepth()
        {
            using (GraphicsWindow window = Device.CreateWindow(1, 1))
            using (Framebuffer framebuffer = TestUtility.CreateFramebuffer(window.Context))
            using (Texture2D depthTexture = Device.CreateTexture2D(new Texture2DDescription(1, 1, TextureFormat.Depth32f, false)))
            {
                framebuffer.DepthAttachment = depthTexture;

                window.Context.Framebuffer = framebuffer;
                window.Context.Clear(new ClearState() { Buffers = ClearBuffers.All, Color = Color.Red, Depth = 0.5f });
                TestUtility.ValidateColor(framebuffer.ColorAttachments[0], 255, 0, 0);
                ValidateDepth(framebuffer.DepthAttachment, 0.5f);

                //
                // Scissor out window and verify clear doesn't modify contents
                //
                ScissorTest scissorTest = new ScissorTest();
                scissorTest.Enabled = true;
                scissorTest.Rectangle = new Rectangle(0, 0, 0, 0);

                window.Context.Clear(new ClearState() { ScissorTest = scissorTest, Buffers = ClearBuffers.All, Color = Color.Blue, Depth = 1 });
                TestUtility.ValidateColor(framebuffer.ColorAttachments[0], 255, 0, 0);
                ValidateDepth(framebuffer.DepthAttachment, 0.5f);
            }
        }

        [Test]
        public void RenderPoint()
        {
            using (GraphicsWindow window = Device.CreateWindow(1, 1))
            using (Framebuffer framebuffer = TestUtility.CreateFramebuffer(window.Context))
            using (ShaderProgram sp = Device.CreateShaderProgram(ShaderSources.PassThroughVertexShader(), ShaderSources.PassThroughFragmentShader()))
            using (VertexArray va = TestUtility.CreateVertexArray(window.Context, sp.VertexAttributes["position"].Location))
            {
                window.Context.Framebuffer = framebuffer;
                window.Context.Draw(PrimitiveType.Points, 0, 1, new DrawState(TestUtility.CreateRenderStateWithoutDepthTest(), sp, va), new SceneState());
                TestUtility.ValidateColor(framebuffer.ColorAttachments[0], 255, 0, 0);
            }
        }

        [Test]
        public void RenderTriangle()
        {
            Vector4F[] positions = new[] 
            { 
                new Vector4F(-0.5f, -0.5f, 0, 1),
                new Vector4F(0.5f, -0.5f, 0, 1),
                new Vector4F(0.5f, 0.5f, 0, 1) 
            };

            ushort[] indices = new ushort[] 
            { 
                0, 1, 2
            };

            using (GraphicsWindow window = Device.CreateWindow(1, 1))
            using (Framebuffer framebuffer = TestUtility.CreateFramebuffer(window.Context))
            using (ShaderProgram sp = Device.CreateShaderProgram(ShaderSources.PassThroughVertexShader(), ShaderSources.PassThroughFragmentShader()))
            using (VertexBuffer positionsBuffer = Device.CreateVertexBuffer(BufferHint.StaticDraw, ArraySizeInBytes.Size(positions)))
            using (IndexBuffer indexBuffer = Device.CreateIndexBuffer(BufferHint.StaticDraw, indices.Length * sizeof(ushort)))
            using (VertexArray va = window.Context.CreateVertexArray())
            {
                positionsBuffer.CopyFromSystemMemory(positions);
                indexBuffer.CopyFromSystemMemory(indices);

                va.Attributes[sp.VertexAttributes["position"].Location] =
                    new VertexBufferAttribute(positionsBuffer, ComponentDatatype.Float, 4);
                va.IndexBuffer = indexBuffer;

                window.Context.Framebuffer = framebuffer;
                window.Context.Draw(PrimitiveType.Triangles, 0, 3, new DrawState(TestUtility.CreateRenderStateWithoutDepthTest(), sp, va), new SceneState());
                TestUtility.ValidateColor(framebuffer.ColorAttachments[0], 255, 0, 0);

                //
                // Verify detach
                //
                window.Context.Clear(new ClearState() { Buffers = ClearBuffers.ColorBuffer, Color = Color.FromArgb(0, 255, 0) });
                va.Attributes[sp.VertexAttributes["position"].Location] = null;
                va.IndexBuffer = null;
                window.Context.Draw(PrimitiveType.Triangles, 0, 0, new DrawState(TestUtility.CreateRenderStateWithoutDepthTest(), sp, va), new SceneState());
                TestUtility.ValidateColor(framebuffer.ColorAttachments[0], 0, 255, 0);

                //
                // Verify rendering without indices
                //
                va.Attributes[sp.VertexAttributes["position"].Location] =
                    new VertexBufferAttribute(positionsBuffer, ComponentDatatype.Float, 4);
                window.Context.Draw(PrimitiveType.Triangles, 0, 3, new DrawState(TestUtility.CreateRenderStateWithoutDepthTest(), sp, va), new SceneState());
                TestUtility.ValidateColor(framebuffer.ColorAttachments[0], 255, 0, 0);
            }
        }

        /// <summary>
        /// Renders one point with a 1x1 texture.
        /// </summary>
        [Test]
        public void RenderTexturedPoint()
        {
            string fs =
                @"#version 330
                 
                  uniform sampler2D textureUnit;
                  out vec4 FragColor;

                  void main()
                  {
                      FragColor = texture(textureUnit, vec2(0, 0));
                  }";

            using (GraphicsWindow window = Device.CreateWindow(1, 1))
            using (Framebuffer framebuffer = TestUtility.CreateFramebuffer(window.Context))
            using (ShaderProgram sp = Device.CreateShaderProgram(ShaderSources.PassThroughVertexShader(), fs))
            using (Texture2D texture = TestUtility.CreateTexture(new BlittableRGBA(Color.FromArgb(0, 255, 0, 0))))
            using (VertexArray va = TestUtility.CreateVertexArray(window.Context, sp.VertexAttributes["position"].Location))
            {
                Uniform<int> textureUniform = (Uniform<int>)sp.Uniforms["textureUnit"];
                textureUniform.Value = 0;

                window.Context.TextureUnits[0].Texture = texture;
                window.Context.TextureUnits[0].TextureSampler = Device.TextureSamplers.NearestClamp;
                window.Context.Framebuffer = framebuffer;
                window.Context.Draw(PrimitiveType.Points, 0, 1, new DrawState(TestUtility.CreateRenderStateWithoutDepthTest(), sp, va), new SceneState());

                TestUtility.ValidateColor(framebuffer.ColorAttachments[0], 255, 0, 0);
            }
        }

        [Test]
        public void RenderPointMultipleColorAttachments()
        {
            Texture2DDescription description = new Texture2DDescription(1, 1, TextureFormat.RedGreenBlue8, false);

            string fs =
                @"#version 330
                 
                  out vec3 RedColor;
                  out vec3 GreenColor;

                  void main()
                  {
                      RedColor = vec3(1.0, 0.0, 0.0);
                      GreenColor = vec3(0.0, 1.0, 0.0);
                  }";

            using (GraphicsWindow window = Device.CreateWindow(1, 1))
            using (Framebuffer framebuffer = window.Context.CreateFramebuffer())
            using (Texture2D redTexture = Device.CreateTexture2D(description))
            using (Texture2D greenTexture = Device.CreateTexture2D(description))
            using (ShaderProgram sp = Device.CreateShaderProgram(ShaderSources.PassThroughVertexShader(), fs))
            using (VertexArray va = TestUtility.CreateVertexArray(window.Context, sp.VertexAttributes["position"].Location))
            {
                Assert.AreNotEqual(sp.FragmentOutputs["RedColor"], sp.FragmentOutputs["GreenColor"]);
                framebuffer.ColorAttachments[sp.FragmentOutputs["RedColor"]] = redTexture;
                framebuffer.ColorAttachments[sp.FragmentOutputs["GreenColor"]] = greenTexture;

                window.Context.Framebuffer = framebuffer;
                window.Context.Draw(PrimitiveType.Points, 0, 1, new DrawState(TestUtility.CreateRenderStateWithoutDepthTest(), sp, va), new SceneState());

                TestUtility.ValidateColor(redTexture, 255, 0, 0);
                TestUtility.ValidateColor(greenTexture, 0, 255, 0);
            }
        }

        /// <summary>
        /// Renders one point with two 1x1 textures.
        /// </summary>
        [Test]
        public void RenderMultitexturedPoint()
        {
            using (GraphicsWindow window = Device.CreateWindow(1, 1))
            using (Framebuffer framebuffer = TestUtility.CreateFramebuffer(window.Context))
            using (ShaderProgram sp = Device.CreateShaderProgram(ShaderSources.PassThroughVertexShader(), ShaderSources.MultitextureFragmentShader()))
            using (Texture2D texture0 = TestUtility.CreateTexture(new BlittableRGBA(Color.FromArgb(0, 255, 0, 0))))
            using (Texture2D texture1 = TestUtility.CreateTexture(new BlittableRGBA(Color.FromArgb(0, 0, 255, 0))))
            using (VertexArray va = TestUtility.CreateVertexArray(window.Context, sp.VertexAttributes["position"].Location))
            {
                window.Context.TextureUnits[0].Texture = texture0;
                window.Context.TextureUnits[0].TextureSampler = Device.TextureSamplers.NearestClamp;
                window.Context.TextureUnits[1].Texture = texture1;
                window.Context.TextureUnits[1].TextureSampler = Device.TextureSamplers.NearestClamp;
                window.Context.Framebuffer = framebuffer;
                window.Context.Draw(PrimitiveType.Points, 0, 1, new DrawState(TestUtility.CreateRenderStateWithoutDepthTest(), sp, va), new SceneState());

                TestUtility.ValidateColor(framebuffer.ColorAttachments[0], 255, 255, 0);
            }
        }

        /// <summary>
        /// Renders a point to color and stencil buffers
        /// </summary>
        [Test]
        public void RenderPointWithStencil()
        {
            using (GraphicsWindow window = Device.CreateWindow(1, 1))
            using (Framebuffer framebuffer = TestUtility.CreateFramebuffer(window.Context))
            using (Texture2D depthStencilTexture = Device.CreateTexture2D(new Texture2DDescription(1, 1, TextureFormat.Depth24Stencil8, false)))
            using (ShaderProgram sp = Device.CreateShaderProgram(ShaderSources.PassThroughVertexShader(), ShaderSources.PassThroughFragmentShader()))
            using (VertexArray va = TestUtility.CreateVertexArray(window.Context, sp.VertexAttributes["position"].Location))
            {
                framebuffer.DepthStencilAttachment = depthStencilTexture;

                StencilTest stencilTest = new StencilTest();
                stencilTest.Enabled = true;
                stencilTest.FrontFace.DepthFailStencilPassOperation = StencilOperation.Replace;
                stencilTest.FrontFace.DepthPassStencilPassOperation = StencilOperation.Replace;
                stencilTest.FrontFace.StencilFailOperation = StencilOperation.Replace;
                stencilTest.FrontFace.ReferenceValue = 2;

                RenderState renderState = new RenderState();
                renderState.StencilTest = stencilTest;

                window.Context.Framebuffer = framebuffer;
                window.Context.Clear(new ClearState());
                window.Context.Draw(PrimitiveType.Points, 0, 1, new DrawState(renderState, sp, va), new SceneState());

                TestUtility.ValidateColor(framebuffer.ColorAttachments[0], 255, 0, 0);

                using (ReadPixelBuffer readPixelBuffer = depthStencilTexture.CopyToBuffer(ImageFormat.DepthStencil, ImageDatatype.UnsignedInt248, 1))
                {
                    byte[] depthStencil = readPixelBuffer.CopyToSystemMemory<byte>();
                    Assert.AreEqual(stencilTest.FrontFace.ReferenceValue, depthStencil[0]);
                }
            }
        }

        [Test]
        public void RenderPointWithDrawAutomaticUniforms0()
        {
            string vs =
                @"#version 330

                  layout(location = og_positionVertexLocation) in vec4 position;
                  uniform mat4 og_viewMatrix;
                  uniform mat4 og_modelMatrix;
                  uniform mat4 og_perspectiveMatrix;

                  void main()
                  {
                      gl_Position = og_perspectiveMatrix * og_viewMatrix * og_modelMatrix * position; 
                  }";

            RenderPoint(vs);
        }

        [Test]
        public void RenderPointWithDrawAutomaticUniforms1()
        {
            string vs =
                @"#version 330

                  layout(location = og_positionVertexLocation) in vec4 position;
                  uniform mat4 og_modelViewPerspectiveMatrix;

                  void main()
                  {
                      gl_Position = og_modelViewPerspectiveMatrix * position; 
                  }";

            RenderPoint(vs);
        }

        private void RenderPoint(string vs)
        {
            using (GraphicsWindow window = Device.CreateWindow(1, 1))
            using (Framebuffer framebuffer = TestUtility.CreateFramebuffer(window.Context))
            using (ShaderProgram sp = Device.CreateShaderProgram(vs, ShaderSources.PassThroughFragmentShader()))
            using (VertexArray va = TestUtility.CreateVertexArray(window.Context, sp.VertexAttributes["position"].Location))
            {
                SceneState sceneState = new SceneState();
                sceneState.Camera.Eye = 2 * Vector3D.UnitX;
                sceneState.Camera.Target = Vector3D.Zero;
                sceneState.Camera.Up = Vector3D.UnitZ;

                window.Context.Framebuffer = framebuffer;
                window.Context.Draw(PrimitiveType.Points, new DrawState(TestUtility.CreateRenderStateWithoutDepthTest(), sp, va), sceneState);
                TestUtility.ValidateColor(framebuffer.ColorAttachments[0], 255, 0, 0);
            }
        }

        [Test]
        public void RenderNonInterleavedVertexBuffer()
        {
            Vector4F[] positions = new[] { new Vector4F(0, 0, 0, 1) };
            BlittableRGBA[] colors = new[] { new BlittableRGBA(Color.Red) };
            string vs = 
                @"#version 330

                  layout(location = og_positionVertexLocation) in vec4 position;               
                  layout(location = og_colorVertexLocation) in vec4 color;
                  out vec4 fsColor;

                  void main()                     
                  {
                      gl_Position = position; 
                      fsColor = color;
                  }";
            string fs = 
                @"#version 330
                 
                  in vec4 fsColor;
                  out vec4 FragColor;

                  void main()
                  {
                      FragColor = fsColor;
                  }";

            using (GraphicsWindow window = Device.CreateWindow(1, 1))
            using (Framebuffer framebuffer = TestUtility.CreateFramebuffer(window.Context))
            using (ShaderProgram sp = Device.CreateShaderProgram(vs, fs))
            using (VertexBuffer vertexBuffer = Device.CreateVertexBuffer(BufferHint.StaticDraw,
                    ArraySizeInBytes.Size(positions) + ArraySizeInBytes.Size(colors)))
            using (VertexArray va = window.Context.CreateVertexArray())
            {
                int colorsOffset = ArraySizeInBytes.Size(positions);
                vertexBuffer.CopyFromSystemMemory(positions);
                vertexBuffer.CopyFromSystemMemory(colors, colorsOffset);

                va.Attributes[sp.VertexAttributes["position"].Location] =
                    new VertexBufferAttribute(vertexBuffer, ComponentDatatype.Float, 4);
                va.Attributes[sp.VertexAttributes["color"].Location] =
                    new VertexBufferAttribute(vertexBuffer, ComponentDatatype.UnsignedByte, 4, true, colorsOffset, 0);
            
                window.Context.Framebuffer = framebuffer;
                window.Context.Draw(PrimitiveType.Points, 0, 1, new DrawState(TestUtility.CreateRenderStateWithoutDepthTest(), sp, va), new SceneState());

                TestUtility.ValidateColor(framebuffer.ColorAttachments[0], 255, 0, 0);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct InterleavedVertex
        {
            public Vector4F Position { get; set; }
            public BlittableRGBA Color { get; set; }
        }

        [Test]
        public void RenderInterleavedVertexBuffer()
        {
            string vs =
                @"#version 330

                  layout(location = og_positionVertexLocation) in vec4 position;               
                  layout(location = og_colorVertexLocation) in vec4 color;

                  out vec4 fsColor;

                  void main()                     
                  {
                      gl_Position = position; 
                      fsColor = color;
                  }";
            string fs = 
                @"#version 330
                 
                  in vec4 fsColor;
                  out vec4 FragColor;

                  void main()
                  {
                      FragColor = fsColor;
                  }";

            InterleavedVertex[] vertices = new InterleavedVertex[]
            {
                new InterleavedVertex()
                {
                    Position = new Vector4F(0, 0, 0, 1),
                    Color = new BlittableRGBA(Color.Red)
                },
                new InterleavedVertex()
                {
                    Position = new Vector4F(0, 0, 0, 1),
                    Color = new BlittableRGBA(Color.FromArgb(255, 0, 255, 0))
                }
            };

            GraphicsWindow window = Device.CreateWindow(1, 1);
            Framebuffer framebuffer = TestUtility.CreateFramebuffer(window.Context);
            ShaderProgram sp = Device.CreateShaderProgram(vs, fs);
            VertexBuffer vertexBuffer = Device.CreateVertexBuffer(BufferHint.StaticDraw, ArraySizeInBytes.Size(vertices));
            VertexArray va = window.Context.CreateVertexArray();
            {
                int colorOffset = SizeInBytes<Vector4F>.Value;
                vertexBuffer.CopyFromSystemMemory(vertices);

                va.Attributes[sp.VertexAttributes["position"].Location] =
                    new VertexBufferAttribute(vertexBuffer, ComponentDatatype.Float, 4, false, 0, SizeInBytes<InterleavedVertex>.Value);
                va.Attributes[sp.VertexAttributes["color"].Location] =
                    new VertexBufferAttribute(vertexBuffer, ComponentDatatype.UnsignedByte, 4, true, colorOffset, SizeInBytes<InterleavedVertex>.Value);

                window.Context.Framebuffer = framebuffer;
                window.Context.Draw(PrimitiveType.Points, 0, 1, new DrawState(TestUtility.CreateRenderStateWithoutDepthTest(), sp, va), new SceneState());
                TestUtility.ValidateColor(framebuffer.ColorAttachments[0], 255, 0, 0);

                window.Context.Draw(PrimitiveType.Points, 1, 1, new DrawState(TestUtility.CreateRenderStateWithoutDepthTest(), sp, va), new SceneState());
                TestUtility.ValidateColor(framebuffer.ColorAttachments[0], 0, 255, 0);
            }
        }

        ///////////////////////////////////////////////////////////////////////

        private static void ValidateDepth(Texture2D depthTexture, float depth)
        {
            using (ReadPixelBuffer readPixelBuffer = depthTexture.CopyToBuffer(ImageFormat.DepthComponent, ImageDatatype.Float, 1))
            {
                float[] readDepth = readPixelBuffer.CopyToSystemMemory<float>();
                Assert.AreEqual (depth, readDepth[0]);
            }
        }
    }
}

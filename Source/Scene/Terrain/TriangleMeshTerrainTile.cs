﻿#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using System;
using System.Drawing;
using OpenGlobe.Core;
using OpenGlobe.Scene;
using OpenGlobe.Renderer;
using System.Collections.Generic;

namespace OpenGlobe.Scene
{
    public sealed class TriangleMeshTerrainTile : IDisposable
    {
        public TriangleMeshTerrainTile(Context context, TerrainTile tile)
        {
            ShaderProgram silhouetteSP = Device.CreateShaderProgram(
                EmbeddedResources.GetText("OpenGlobe.Scene.Terrain.TriangleMeshTerrainTile.SilhouetteVS.glsl"),
                EmbeddedResources.GetText("OpenGlobe.Scene.Terrain.TriangleMeshTerrainTile.SilhouetteGS.glsl"),
                EmbeddedResources.GetText("OpenGlobe.Scene.Terrain.TriangleMeshTerrainTile.SilhouetteFS.glsl"));

            Uniform<float> fillDistance = (Uniform<float>)silhouetteSP.Uniforms["u_fillDistance"];
            fillDistance.Value = 1.5f;
            _silhouetteHeightExaggeration = (Uniform<float>)silhouetteSP.Uniforms["u_heightExaggeration"];

            ShaderProgram sp = Device.CreateShaderProgram(
                EmbeddedResources.GetText("OpenGlobe.Scene.Terrain.TriangleMeshTerrainTile.TerrainVS.glsl"),
                EmbeddedResources.GetText("OpenGlobe.Scene.Terrain.TriangleMeshTerrainTile.TerrainFS.glsl"));

            _tileMinimumHeight = tile.MinimumHeight;
            _tileMaximumHeight = tile.MaximumHeight;

            _heightExaggeration = (Uniform<float>)sp.Uniforms["u_heightExaggeration"];
            _minimumHeight = (Uniform<float>)sp.Uniforms["u_minimumHeight"];
            _maximumHeight = (Uniform<float>)sp.Uniforms["u_maximumHeight"];
            HeightExaggeration = 1;

            ///////////////////////////////////////////////////////////////////

            Mesh mesh = new Mesh();
            mesh.PrimitiveType = PrimitiveType.Triangles;
            mesh.FrontFaceWindingOrder = WindingOrder.Counterclockwise;

            int numberOfPositions = tile.Resolution.X * tile.Resolution.Y;
            VertexAttributeDoubleVector3 positionsAttribute = new VertexAttributeDoubleVector3("position", numberOfPositions);
            IList<Vector3D> positions = positionsAttribute.Values;
            mesh.Attributes.Add(positionsAttribute);

            int numberOfPartitionsX = tile.Resolution.X - 1;
            int numberOfPartitionsY = tile.Resolution.Y - 1;
            int numberOfIndices = (numberOfPartitionsX * numberOfPartitionsY) * 6;
            IndicesUnsignedInt indices = new IndicesUnsignedInt(numberOfIndices);
            mesh.Indices = indices;

            //
            // Positions
            //
            Vector2D lowerLeft = tile.Extent.LowerLeft;
            Vector2D toUpperRight = tile.Extent.UpperRight - lowerLeft;

            int heightIndex = 0;
            for (int y = 0; y <= numberOfPartitionsY; ++y)
            {
                double deltaY = y / (double)numberOfPartitionsY;
                double currentY = lowerLeft.Y + (deltaY * toUpperRight.Y);

                for (int x = 0; x <= numberOfPartitionsX; ++x)
                {
                    double deltaX = x / (double)numberOfPartitionsX;
                    double currentX = lowerLeft.X + (deltaX * toUpperRight.X);
                    positions.Add(new Vector3D(currentX, currentY, tile.Heights[heightIndex++]));
                }
            }

            //
            // Indices
            //
            int rowDelta = numberOfPartitionsX + 1;
            int i = 0;
            for (int y = 0; y < numberOfPartitionsY; ++y)
            {
                for (int x = 0; x < numberOfPartitionsX; ++x)
                {
                    indices.AddTriangle(new TriangleIndicesUnsignedInt(i, i + 1, rowDelta + (i + 1)));
                    indices.AddTriangle(new TriangleIndicesUnsignedInt(i, rowDelta + (i + 1), rowDelta + i));
                    i += 1;
                }
                i += 1;
            }

            _drawState = new DrawState();
            _drawState.RenderState.FacetCulling.FrontFaceWindingOrder = mesh.FrontFaceWindingOrder;
            _drawState.ShaderProgram = sp;
            _drawState.VertexArray = context.CreateVertexArray(mesh, sp.VertexAttributes, BufferHint.StaticDraw);

            _silhouetteDrawState = new DrawState();
            _silhouetteDrawState.RenderState.FacetCulling.Enabled = false;
            _silhouetteDrawState.RenderState.DepthMask = false;
            _silhouetteDrawState.VertexArray = _drawState.VertexArray;
            _silhouetteDrawState.ShaderProgram = silhouetteSP;

            _primitiveType = mesh.PrimitiveType;

            _clearColor = new ClearState();
            _clearColor.Buffers = ClearBuffers.ColorBuffer;
            
            //
            // Only depth needs to be cleared but include stencil for speed.
            //
            _clearDepthStencil = new ClearState();
            _clearDepthStencil.Buffers = ClearBuffers.DepthBuffer | ClearBuffers.StencilBuffer;
        }

        public void Render(Context context, SceneState sceneState)
        {
            Verify.ThrowIfNull(context);

            context.Draw(_primitiveType, _drawState, sceneState);
        }

        public void RenderDepthAndSilhouetteTextures(Context context, SceneState sceneState, bool silhouette)
        {
            Verify.ThrowIfNull(context);
            Verify.ThrowIfNull(sceneState);
            
            CreateDepthAndSilhouetteData(context);

            //
            // Depth texture
            //
            context.Framebuffer = _terrainFramebuffer;
            context.Clear(_clearDepthStencil);
            Render(context, sceneState);

            //
            // Silhouette texture
            //
            context.Framebuffer = _silhouetteFramebuffer;
            context.Clear(_clearColor);
            if (silhouette)
            {
                context.Draw(_primitiveType, _silhouetteDrawState, sceneState);
            }
        }

        private void CreateDepthAndSilhouetteData(Context context)
        {
            if ((_depthTexture == null) || (_depthTexture.Description.Width != context.Viewport.Width) ||
                (_depthTexture.Description.Height != context.Viewport.Height))
            {
                //
                // Dispose as necessary
                //
                if (_depthTexture != null)
                {
                    _depthTexture.Dispose();
                }
                if (_silhouetteTexture != null)
                {
                    _silhouetteTexture.Dispose();
                }
                if (_colorTexture != null)
                {
                    _colorTexture.Dispose();
                }

                //
                // Textures
                //
                _depthTexture = Device.CreateTexture2D(new Texture2DDescription(context.Viewport.Width, context.Viewport.Height, TextureFormat.Depth24));
                _silhouetteTexture = Device.CreateTexture2D(new Texture2DDescription(context.Viewport.Width, context.Viewport.Height, TextureFormat.Red8));
                _colorTexture = Device.CreateTexture2D(new Texture2DDescription(context.Viewport.Width, context.Viewport.Height, TextureFormat.RedGreenBlue8));

                //
                // Terrain FBO
                //
                if (_terrainFramebuffer == null)
                {
                    _terrainFramebuffer = context.CreateFramebuffer();
                }
                _terrainFramebuffer.DepthAttachment = _depthTexture;
                _terrainFramebuffer.ColorAttachments[0] = _colorTexture;

                //
                // Silhouette FBO
                //
                if (_silhouetteFramebuffer == null)
                {
                    _silhouetteFramebuffer = context.CreateFramebuffer();
                }
                _silhouetteFramebuffer.DepthAttachment = _depthTexture;
                _silhouetteFramebuffer.ColorAttachments[0] = _silhouetteTexture;
            }
        }

        public float HeightExaggeration
        {
            get { return _heightExaggeration.Value; }
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException("HeightExaggeration", "HeightExaggeration must be greater than zero.");
                }

                if (_heightExaggeration.Value != value)
                {
                    //
                    // TEXEL_SPACE_TODO:  If one of the AABB z planes is not 0, the
                    // scale will incorrectly move the entire tile.
                    //
                    _heightExaggeration.Value = value;
                    _minimumHeight.Value = _tileMinimumHeight * value;
                    _maximumHeight.Value = _tileMaximumHeight * value;
                    _silhouetteHeightExaggeration.Value = value;
                }
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            _drawState.ShaderProgram.Dispose();
            _drawState.VertexArray.Dispose();
            _silhouetteDrawState.ShaderProgram.Dispose();
            _silhouetteDrawState.VertexArray.Dispose();
            if (_depthTexture != null)
            {
                _depthTexture.Dispose();
            }
            if (_silhouetteTexture != null)
            {
                _silhouetteTexture.Dispose();
            }
            if (_colorTexture != null)
            {
                _colorTexture.Dispose();
            }
            if (_silhouetteFramebuffer != null)
            {
                _silhouetteFramebuffer.Dispose();
            }
            if (_terrainFramebuffer != null)
            {
                _terrainFramebuffer.Dispose();
            }
        }

        #endregion

        public Texture2D DepthTexture
        {
            get { return _depthTexture; }
        }

        public Texture2D SilhouetteTexture
        {
            get { return _silhouetteTexture; }
        }

        private readonly DrawState _drawState;
        private readonly DrawState _silhouetteDrawState;

        private Framebuffer _silhouetteFramebuffer;
        private Framebuffer _terrainFramebuffer;

        private readonly Uniform<float> _heightExaggeration;
        private readonly Uniform<float> _minimumHeight;
        private readonly Uniform<float> _maximumHeight;
        private readonly Uniform<float> _silhouetteHeightExaggeration;

        private readonly float _tileMinimumHeight;
        private readonly float _tileMaximumHeight;

        private Texture2D _depthTexture;
        private Texture2D _silhouetteTexture;
        private Texture2D _colorTexture;

        private readonly ClearState _clearColor;
        private readonly ClearState _clearDepthStencil;

        private readonly PrimitiveType _primitiveType;
    }
}
﻿#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using System;
using OpenGlobe.Core;
using OpenGlobe.Renderer;

namespace OpenGlobe.Scene
{
    public sealed class OutlinedPolylineGeometryShader : IDisposable
    {
        public OutlinedPolylineGeometryShader()
        {
            _drawState = new DrawState();
            _drawState.RenderState.FacetCulling.Enabled = false;

            Width = 1;
            OutlineWidth = 1;
        }

        public void Set(Context context, Mesh mesh)
        {
            Verify.ThrowIfNull(context);

            if (mesh == null)
            {
                throw new ArgumentNullException("mesh");
            }

            if (mesh.PrimitiveType != PrimitiveType.Lines &&
                mesh.PrimitiveType != PrimitiveType.LineLoop &&
                mesh.PrimitiveType != PrimitiveType.LineStrip)
            {
                throw new ArgumentException("mesh.PrimitiveType must be Lines, LineLoop, or LineStrip.", "mesh");
            }

            if (!mesh.Attributes.Contains("position") &&
                !mesh.Attributes.Contains("color") &&
                !mesh.Attributes.Contains("outlineColor"))
            {
                throw new ArgumentException("mesh.Attributes should contain attributes named \"position\", \"color\", and \"outlineColor\".", "mesh");
            }

            if (_drawState.ShaderProgram == null)
            {
                _drawState.ShaderProgram = Device.CreateShaderProgram(
                    EmbeddedResources.GetText("OpenGlobe.Scene.Renderables.Polyline.OutlinedPolylineGeometryShader.PolylineVS.glsl"),
                    EmbeddedResources.GetText("OpenGlobe.Scene.Renderables.Polyline.OutlinedPolylineGeometryShader.PolylineGS.glsl"),
                    EmbeddedResources.GetText("OpenGlobe.Scene.Renderables.Polyline.OutlinedPolylineGeometryShader.PolylineFS.glsl"));
                _fillDistance = (Uniform<float>)_drawState.ShaderProgram.Uniforms["u_fillDistance"];
                _outlineDistance = (Uniform<float>)_drawState.ShaderProgram.Uniforms["u_outlineDistance"];
            }

            ///////////////////////////////////////////////////////////////////
            _drawState.VertexArray = context.CreateVertexArray(mesh, _drawState.ShaderProgram.VertexAttributes, BufferHint.StaticDraw);
            _primitiveType = mesh.PrimitiveType;
        }

        public void Render(Context context, SceneState sceneState)
        {
            Verify.ThrowIfNull(context);
            Verify.ThrowIfNull(sceneState);

            if (_drawState.ShaderProgram != null)
            {
                double fillDistance = Width * 0.5 * sceneState.HighResolutionSnapScale;
                _fillDistance.Value = (float)(fillDistance);
                _outlineDistance.Value = (float)(fillDistance + (OutlineWidth * sceneState.HighResolutionSnapScale));

                context.Draw(_primitiveType, _drawState, sceneState);
            }
        }

        public double Width { get; set; }

        public double OutlineWidth { get; set; }

        public bool Wireframe
        {
            get { return _drawState.RenderState.RasterizationMode == RasterizationMode.Line; }
            set { _drawState.RenderState.RasterizationMode = value ? RasterizationMode.Line : RasterizationMode.Fill; }
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (_drawState.ShaderProgram != null)
            {
                _drawState.ShaderProgram.Dispose();
            }

            if (_drawState.VertexArray != null)
            {
                _drawState.VertexArray.Dispose();
            }
        }

        #endregion

        private readonly DrawState _drawState;
        private Uniform<float> _fillDistance;
        private Uniform<float> _outlineDistance;
        private PrimitiveType _primitiveType;
    }
}
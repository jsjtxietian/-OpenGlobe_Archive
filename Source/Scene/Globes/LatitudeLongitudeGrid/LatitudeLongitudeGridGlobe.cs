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
using System.Collections.Generic;

namespace OpenGlobe.Scene
{
    public sealed class LatitudeLongitudeGridGlobe : IDisposable
    {
        public LatitudeLongitudeGridGlobe(Context context)
        {
            Verify.ThrowIfNull(context);

            ShaderProgram sp = Device.CreateShaderProgram(
                EmbeddedResources.GetText("OpenGlobe.Scene.Globes.LatitudeLongitudeGrid.Shaders.GlobeVS.glsl"),
                EmbeddedResources.GetText("OpenGlobe.Scene.Globes.LatitudeLongitudeGrid.Shaders.GlobeFS.glsl"));
            _gridWidth = (Uniform<Vector2F>)sp.Uniforms["u_gridLineWidth"];
            _gridResolution = (Uniform<Vector2F>)sp.Uniforms["u_gridResolution"];
            _globeOneOverRadiiSquared = (Uniform<Vector3F>)sp.Uniforms["u_globeOneOverRadiiSquared"];

            _drawState = new DrawState();
            _drawState.ShaderProgram = sp;

            Shape = Ellipsoid.ScaledWgs84;
        }

        private void Clean(Context context)
        {
            if (_dirty)
            {
                if (_drawState.VertexArray != null)
                {
                    _drawState.VertexArray.Dispose();
                    _drawState.VertexArray = null;
                }

                Mesh mesh = GeographicGridEllipsoidTessellator.Compute(_shape, 64, 32, GeographicGridEllipsoidVertexAttributes.Position);
                _drawState.VertexArray = context.CreateVertexArray(mesh, _drawState.ShaderProgram.VertexAttributes, BufferHint.StaticDraw);
                _primitiveType = mesh.PrimitiveType;

                _drawState.RenderState.FacetCulling.FrontFaceWindingOrder = mesh.FrontFaceWindingOrder;

                _globeOneOverRadiiSquared.Value = _shape.OneOverRadiiSquared.ToVector3F();

                _dirty = false;
            }
        }

        public void Render(Context context, SceneState sceneState)
        {
            Verify.ThrowIfNull(context);
            Verify.ThrowIfNull(sceneState);
            Verify.ThrowInvalidOperationIfNull(Texture, "Texture");

            if (GridResolutions == null)
            {
                throw new InvalidOperationException("GridResolutions");
            }

            Clean(context);

            //
            // This could be improved to exploit temporal coherence as described in section x.x.
            //
            double height = sceneState.Camera.Height(_shape);
            for (int i = 0; i < GridResolutions.Count; ++i)
            {
                if (GridResolutions[i].Interval.Contains(height))
                {
                    _gridResolution.Value = GridResolutions[i].Resolution.ToVector2F();
                    break;
                }
            }

            float width = (float)sceneState.HighResolutionSnapScale;
            _gridWidth.Value = new Vector2F(width, width);

            context.TextureUnits[0].Texture = Texture;
            context.TextureUnits[0].TextureSampler = Device.TextureSamplers.LinearClamp;
            context.Draw(_primitiveType, _drawState, sceneState);
        }

        public Texture2D Texture { get; set; }

        public GridResolutionCollection GridResolutions { get; set; }

        public Ellipsoid Shape
        {
            get { return _shape; }
            set
            {
                _dirty = true;
                _shape = value;
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            _drawState.ShaderProgram.Dispose();
            _drawState.VertexArray.Dispose();
        }

        #endregion

        private readonly DrawState _drawState;
        private readonly Uniform<Vector2F> _gridWidth;
        private readonly Uniform<Vector2F> _gridResolution;
        private readonly Uniform<Vector3F> _globeOneOverRadiiSquared;

        private PrimitiveType _primitiveType;

        private Ellipsoid _shape;
        private bool _dirty;
    }
}
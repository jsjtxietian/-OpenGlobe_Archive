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
    public sealed class RayCastedGlobe : IDisposable
    {
        public RayCastedGlobe(Context context)
        {
            Verify.ThrowIfNull(context);

            _renderState = new RenderState();
            _renderState.FacetCulling.Face = CullFace.Front;
            string vs = EmbeddedResources.GetText("OpenGlobe.Scene.Globes.RayCasted.Shaders.GlobeVS.glsl");

            ShaderProgram sp = Device.CreateShaderProgram(vs, EmbeddedResources.GetText("OpenGlobe.Scene.Globes.RayCasted.Shaders.GlobeFS.glsl"));
            _cameraEyeSquared = (Uniform<Vector3F>)sp.Uniforms["u_cameraEyeSquared"];
            _useAverageDepth = (Uniform<bool>)sp.Uniforms["u_useAverageDepth"];

            ShaderProgram solidSP = Device.CreateShaderProgram(vs, EmbeddedResources.GetText("OpenGlobe.Scene.Globes.RayCasted.Shaders.SolidShadedGlobeFS.glsl"));
            _cameraEyeSquaredSolid = (Uniform<Vector3F>)solidSP.Uniforms["u_cameraEyeSquared"];
            _useAverageDepthSolid = (Uniform<bool>)solidSP.Uniforms["u_useAverageDepth"];

            _drawState = new DrawState(_renderState, sp, null);
            _drawStateSolid = new DrawState(_renderState, solidSP, null);

            Shape = Ellipsoid.ScaledWgs84;
            Shade = true;
            ShowGlobe = true;
        }

        private void Clean(Context context)
        {
            if (_dirty)
            {
                if (_va != null)
                {
                    _va.Dispose();
                    _va = null;
                    _drawState.VertexArray = null;
                    _drawStateSolid.VertexArray = null;
                }

                Mesh mesh = BoxTessellator.Compute(2 * _shape.Radii);
                _va = context.CreateVertexArray(mesh, _drawState.ShaderProgram.VertexAttributes, BufferHint.StaticDraw);
                _drawState.VertexArray = _va;
                _drawStateSolid.VertexArray = _va;
                _primitiveType = mesh.PrimitiveType;

                _renderState.FacetCulling.FrontFaceWindingOrder = mesh.FrontFaceWindingOrder;

                ((Uniform<Vector3F>)_drawState.ShaderProgram.Uniforms["u_globeOneOverRadiiSquared"]).Value = _shape.OneOverRadiiSquared.ToVector3F();
                ((Uniform<Vector3F>)_drawStateSolid.ShaderProgram.Uniforms["u_globeOneOverRadiiSquared"]).Value = _shape.OneOverRadiiSquared.ToVector3F();

                if (_wireframe != null)
                {
                    _wireframe.Dispose();
                    _wireframe = null;
                }
                _wireframe = new Wireframe(context, mesh);
                _wireframe.FacetCullingFace = CullFace.Front;
                _wireframe.Width = 3;

                _dirty = false;
            }
        }

        public void Render(Context context, SceneState sceneState)
        {
            Verify.ThrowIfNull(context);
            Verify.ThrowIfNull(sceneState);
            Verify.ThrowInvalidOperationIfNull(Texture, "Texture");

            Clean(context);

            if (ShowGlobe)
            {
                Vector3D eye = sceneState.Camera.Eye;
                Vector3F cameraEyeSquared = eye.MultiplyComponents(eye).ToVector3F();

                if (Shade)
                {
                    context.TextureUnits[0].Texture = Texture;
                    context.TextureUnits[0].TextureSampler = Device.TextureSamplers.LinearClamp;
                    _cameraEyeSquared.Value = cameraEyeSquared;
                    context.Draw(_primitiveType, _drawState, sceneState);
                }
                else
                {
                    _cameraEyeSquaredSolid.Value = cameraEyeSquared;
                    context.Draw(_primitiveType, _drawStateSolid, sceneState);
                }
            }

            if (ShowWireframeBoundingBox)
            {
                _wireframe.Render(context, sceneState);
            }
        }

        public bool UseAverageDepth
        {
            get { return _useAverageDepth.Value; }
            set 
            { 
                _useAverageDepth.Value = value;
                _useAverageDepthSolid.Value = value;
            }
        }

        public Ellipsoid Shape
        {
            get { return _shape; }
            set
            {
                _dirty = true;
                _shape = value;
            }
        }

        public bool Shade { get; set; }
        public bool ShowGlobe { get; set; }
        public bool ShowWireframeBoundingBox { get; set; }
        public Texture2D Texture { get; set; }

        #region IDisposable Members

        public void Dispose()
        {
            _drawState.ShaderProgram.Dispose();
            _drawStateSolid.ShaderProgram.Dispose();

            if (_va != null)
            {
                _va.Dispose();
            }

            if (_wireframe != null)
            {
                _wireframe.Dispose();
            }
        }

        #endregion

        private readonly DrawState _drawState;
        private readonly Uniform<Vector3F> _cameraEyeSquared;
        private readonly DrawState _drawStateSolid;
        private readonly Uniform<Vector3F> _cameraEyeSquaredSolid;

        private readonly Uniform<bool> _useAverageDepth;
        private readonly Uniform<bool> _useAverageDepthSolid;
        
        private readonly RenderState _renderState;
        private VertexArray _va;
        private PrimitiveType _primitiveType;

        private Wireframe _wireframe;

        private Ellipsoid _shape;
        private bool _dirty;
    }
}
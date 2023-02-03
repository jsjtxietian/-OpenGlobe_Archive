#region License
//
// (C) Copyright 2010 Patrick Cozzi and Kevin Ring
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using System;
using System.Collections.Generic;
using System.Drawing;
using OpenGlobe.Core;
using OpenGlobe.Renderer;
using OpenGlobe.Scene;

namespace OpenGlobe.Examples
{
    /// <summary>
    /// The same as GPURelativeToEye, except with a different 
    /// implementation of DoubleToTwoFloats() and a different vertex shader
    /// based on emulated doubles from the DSFUN90 Fortran library.
    /// </summary>
    sealed class GPURelativeToEyeDSFUN90 : IDisposable, IRenderable
    {
        public GPURelativeToEyeDSFUN90(Context context, Vector3D[] positions, byte[] colors)
        {
            _sp = Device.CreateShaderProgram(
                EmbeddedResources.GetText("OpenGlobe.Examples.GPURelativeToEyeDSFUN90.Shaders.VS.glsl"),
                EmbeddedResources.GetText("OpenGlobe.Examples.Shaders.FS.glsl"));
            _cameraEyeHigh = (Uniform<Vector3F>)_sp.Uniforms["u_cameraEyeHigh"];
            _cameraEyeLow = (Uniform<Vector3F>)_sp.Uniforms["u_cameraEyeLow"];
            _modelViewPerspectiveMatrixRelativeToEye = (Uniform<Matrix4F>)(_sp.Uniforms["u_modelViewPerspectiveMatrixRelativeToEye"]);
            _pointSize = (Uniform<float>)_sp.Uniforms["u_pointSize"];

            Mesh mesh = new Mesh();
            VertexAttributeFloatVector3 positionsHighAttribute = new VertexAttributeFloatVector3("positionHigh", positions.Length);
            VertexAttributeFloatVector3 positionsLowAttribute = new VertexAttributeFloatVector3("positionLow", positions.Length);
            VertexAttributeRGB colorAttribute = new VertexAttributeRGB("color", positions.Length);
            mesh.Attributes.Add(positionsHighAttribute);
            mesh.Attributes.Add(positionsLowAttribute);
            mesh.Attributes.Add(colorAttribute);

            for (int i = 0; i < positions.Length; ++i)
            {
                Vector3F positionHigh;
                Vector3F positionLow;
                Vector3DToTwoVector3F(positions[i], out positionHigh, out positionLow);

                positionsHighAttribute.Values.Add(positionHigh);
                positionsLowAttribute.Values.Add(positionLow);
            }

            for (int i = 0; i < colors.Length; ++i)
            {
                colorAttribute.Values.Add(colors[i]);
            }

            _va = context.CreateVertexArray(mesh, _sp.VertexAttributes, BufferHint.StaticDraw);

            RenderState renderState = new RenderState();
            renderState.FacetCulling.Enabled = false;
            renderState.DepthTest.Enabled = false;
            renderState.ProgramPointSize = ProgramPointSize.Enabled;

            _drawState = new DrawState(renderState, _sp, _va);
        }

        private static void DoubleToTwoFloats(double value, out float high, out float low)
        {
            high = (float)value;
            low = (float)(value - high);
        }

        private static void Vector3DToTwoVector3F(Vector3D value, out Vector3F high, out Vector3F low)
        {
            float highX;
            float highY;
            float highZ;

            float lowX;
            float lowY;
            float lowZ;

            DoubleToTwoFloats(value.X, out highX, out lowX);
            DoubleToTwoFloats(value.Y, out highY, out lowY);
            DoubleToTwoFloats(value.Z, out highZ, out lowZ);

            high = new Vector3F(highX, highY, highZ);
            low = new Vector3F(lowX, lowY, lowZ);
        }
        
        private void Update(SceneState sceneState)
        {
            Vector3D eye = sceneState.Camera.Eye;

            if (_eye != eye)
            {
                _eye = eye;

                Vector3F eyeHigh;
                Vector3F eyeLow;
                Vector3DToTwoVector3F(eye, out eyeHigh, out eyeLow);
                _cameraEyeHigh.Value = eyeHigh;
                _cameraEyeLow.Value = eyeLow;

                Matrix4D m = sceneState.ModelViewMatrix;
                Matrix4D mv = new Matrix4D(
                    m.Column0Row0, m.Column1Row0, m.Column2Row0, 0.0,
                    m.Column0Row1, m.Column1Row1, m.Column2Row1, 0.0,
                    m.Column0Row2, m.Column1Row2, m.Column2Row2, 0.0,
                    m.Column0Row3, m.Column1Row3, m.Column2Row3, m.Column3Row3);

                _modelViewPerspectiveMatrixRelativeToEye.Value =
                    (sceneState.PerspectiveMatrix * mv).ToMatrix4F();
            }

            _pointSize.Value = (float)(8.0 * sceneState.HighResolutionSnapScale);
        }

        #region IRenderable Members

        public void Render(Context context, SceneState sceneState)
        {
            Update(sceneState);

            context.Draw(PrimitiveType.Triangles, 0, 6, _drawState, sceneState);
            context.Draw(PrimitiveType.Points, 6, 1, _drawState, sceneState);
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            _va.Dispose();
            _sp.Dispose();
        }

        #endregion

        private readonly VertexArray _va;
        private readonly ShaderProgram _sp;
        private readonly Uniform<Vector3F> _cameraEyeHigh;
        private readonly Uniform<Vector3F> _cameraEyeLow;
        private readonly Uniform<Matrix4F> _modelViewPerspectiveMatrixRelativeToEye;
        private readonly Uniform<float> _pointSize;
        private readonly DrawState _drawState;
        private Vector3D _eye;
    }
}
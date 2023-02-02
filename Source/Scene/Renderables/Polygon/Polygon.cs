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
using System.Collections.Generic;
using OpenGlobe.Core;
using OpenGlobe.Renderer;

namespace OpenGlobe.Scene
{
    public sealed class Polygon : IDisposable
    {
        public Polygon(Context context, Ellipsoid globeShape, IEnumerable<Vector3D> positions)
        {
            Verify.ThrowIfNull(context);
            Verify.ThrowIfNull(globeShape);

            //
            // Pipeline Stage 1a:  Clean up - Remove duplicate positions
            //
            List<Vector3D> cleanPositions = (List<Vector3D>)SimplePolygonAlgorithms.Cleanup(positions);

            //
            // Pipeline Stage 1b:  Clean up - Swap winding order
            //
            EllipsoidTangentPlane plane = new EllipsoidTangentPlane(globeShape, cleanPositions);
            ICollection<Vector2D> positionsOnPlane = plane.ComputePositionsOnPlane(cleanPositions);
            if (SimplePolygonAlgorithms.ComputeWindingOrder(positionsOnPlane) == PolygonWindingOrder.Clockwise)
            {
                cleanPositions.Reverse();
                //((List<Vector2D>)positionsOnPlane).Reverse();
            }
            
            //
            // Pipeline Stage 2:  Triangulate
            //
            IndicesUnsignedInt indices = EarClippingOnEllipsoid.Triangulate(cleanPositions);
            //IndicesInt32 indices = EarClipping.Triangulate(positionsOnPlane);

            //
            // Pipeline Stage 3:  Subdivide
            //
            TriangleMeshSubdivisionResult result = TriangleMeshSubdivision.Compute(cleanPositions, indices, Trig.ToRadians(1));
            
            //
            // Pipeline Stage 4:  Set height
            //
            VertexAttributeDoubleVector3 positionsAttribute = new VertexAttributeDoubleVector3(
                "position", (result.Indices.Values.Count / 3) + 2);
            foreach (Vector3D position in result.Positions)
            {
                positionsAttribute.Values.Add(globeShape.ScaleToGeocentricSurface(position));
            }

            Mesh mesh = new Mesh();
            mesh.PrimitiveType = PrimitiveType.Triangles;
            mesh.FrontFaceWindingOrder = WindingOrder.Counterclockwise;
            mesh.Attributes.Add(positionsAttribute);
            mesh.Indices = result.Indices;

            ShaderProgram sp = Device.CreateShaderProgram(
                EmbeddedResources.GetText("OpenGlobe.Scene.Renderables.Polygon.Shaders.PolygonVS.glsl"),
                EmbeddedResources.GetText("OpenGlobe.Scene.Renderables.Polygon.Shaders.PolygonFS.glsl"));
            ((Uniform<Vector3F>)sp.Uniforms["u_globeOneOverRadiiSquared"]).Value = globeShape.OneOverRadiiSquared.ToVector3F();
            _colorUniform = (Uniform<Vector4F>)sp.Uniforms["u_color"];

            _drawState = new DrawState();
            _drawState.RenderState.Blending.Enabled = true;
            _drawState.RenderState.Blending.SourceRGBFactor = SourceBlendingFactor.SourceAlpha;
            _drawState.RenderState.Blending.SourceAlphaFactor = SourceBlendingFactor.SourceAlpha;
            _drawState.RenderState.Blending.DestinationRGBFactor = DestinationBlendingFactor.OneMinusSourceAlpha;
            _drawState.RenderState.Blending.DestinationAlphaFactor = DestinationBlendingFactor.OneMinusSourceAlpha;
            _drawState.ShaderProgram = sp;
            _meshBuffers = Device.CreateMeshBuffers(mesh, _drawState.ShaderProgram.VertexAttributes, BufferHint.StaticDraw);

            _primitiveType = mesh.PrimitiveType;

            Color = Color.White;
        }

        private void Update(Context context)
        {
            if (_meshBuffers != null)
            {
                if (_drawState.VertexArray != null)
                {
                    _drawState.VertexArray.Dispose();
                    _drawState.VertexArray = null;
                }

                _drawState.VertexArray = context.CreateVertexArray(_meshBuffers);
                _meshBuffers = null;
            }
        }

        public void Render(Context context, SceneState sceneState)
        {
            Verify.ThrowIfNull(context);
            Verify.ThrowIfNull(sceneState);

            Update(context);
            context.Draw(_primitiveType, _drawState, sceneState);
        }

        public Color Color
        {
            get { return _color; }

            set
            {
                _color = value;
                _colorUniform.Value = new Vector4F(_color.R / 255.0f, _color.G / 255.0f, _color.B / 255.0f, _color.A / 255.0f);
            }
        }

        public bool Wireframe
        {
            get { return _drawState.RenderState.RasterizationMode == RasterizationMode.Line; }
            set { _drawState.RenderState.RasterizationMode = value ? RasterizationMode.Line : RasterizationMode.Fill; }
        }

        public bool BackfaceCulling
        {
            get { return _drawState.RenderState.FacetCulling.Enabled; }
            set { _drawState.RenderState.FacetCulling.Enabled = value; }
        }

        public bool DepthWrite
        {
            get { return _drawState.RenderState.DepthMask; }
            set { _drawState.RenderState.DepthMask = value; }
        }

        #region IDisposable Members

        public void Dispose()
        {
            _drawState.ShaderProgram.Dispose();
            _drawState.VertexArray.Dispose();
        }

        #endregion

        private Color _color;
        private readonly Uniform<Vector4F> _colorUniform;
        private readonly DrawState _drawState;
        private readonly PrimitiveType _primitiveType;
        private MeshBuffers _meshBuffers;       // For passing between threads
    }
}
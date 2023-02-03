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
using OpenGlobe.Scene;

namespace OpenGlobe.Examples
{
    sealed class LatitudeLongitudeGrid : IDisposable
    {
        public LatitudeLongitudeGrid()
        {
            Ellipsoid globeShape = Ellipsoid.Wgs84;
            _window = Device.CreateWindow(800, 600, "Chapter 4:  Latitude Longitude Grid");
            _window.Resize += OnResize;
            _window.RenderFrame += OnRenderFrame;
            _sceneState = new SceneState();
            _camera = new CameraLookAtPoint(_sceneState.Camera, _window, globeShape);
            _clearState = new ClearState();

            _sceneState.Camera.PerspectiveNearPlaneDistance = 0.01 * globeShape.MaximumRadius;
            _sceneState.Camera.PerspectiveFarPlaneDistance = 10.0 * globeShape.MaximumRadius;
            _sceneState.Camera.ZoomToTarget(globeShape.MaximumRadius);

            ///////////////////////////////////////////////////////////////////

            IList<GridResolution> gridResolutions = new List<GridResolution>();
            gridResolutions.Add(new GridResolution(
                new Interval(0, 1000000, IntervalEndpoint.Closed, IntervalEndpoint.Open),
                new Vector2D(0.005, 0.005)));
            gridResolutions.Add(new GridResolution(
                new Interval(1000000, 2000000, IntervalEndpoint.Closed, IntervalEndpoint.Open),
                new Vector2D(0.01, 0.01)));
            gridResolutions.Add(new GridResolution(
                new Interval(2000000, 20000000, IntervalEndpoint.Closed, IntervalEndpoint.Open),
                new Vector2D(0.05, 0.05)));
            gridResolutions.Add(new GridResolution(
                new Interval(20000000, double.MaxValue, IntervalEndpoint.Closed, IntervalEndpoint.Open),
                new Vector2D(0.1, 0.1)));

            _globe = new LatitudeLongitudeGridGlobe(_window.Context);
            _globe.Texture = Device.CreateTexture2D(new Bitmap("NE2_50M_SR_W_4096.jpg"), TextureFormat.RedGreenBlue8, false);
            _globe.Shape = globeShape;
            _globe.GridResolutions = new GridResolutionCollection(gridResolutions);

            ///////////////////////////////////////////////////////////////////

            Vector3D vancouver = globeShape.ToVector3D(new Geodetic3D(Trig.ToRadians(-123.06), Trig.ToRadians(49.13), 0));

            TextureAtlas atlas = new TextureAtlas(new Bitmap[]
            {
                new Bitmap("building.png"),
                Device.CreateBitmapFromText("Vancouver", new Font("Arial", 24))
            });

            _vancouverLabel = new BillboardCollection(_window.Context);
            _vancouverLabel.Texture = Device.CreateTexture2D(atlas.Bitmap, TextureFormat.RedGreenBlueAlpha8, false);
            _vancouverLabel.DepthTestEnabled = false;
            _vancouverLabel.Add(new Billboard()
            {
                Position = vancouver,
                TextureCoordinates = atlas.TextureCoordinates[0]
            });
            _vancouverLabel.Add(new Billboard()
            {
                Position = vancouver,
                TextureCoordinates = atlas.TextureCoordinates[1],
                HorizontalOrigin = HorizontalOrigin.Left
            });

            atlas.Dispose();
        }

        private void OnResize()
        {
            _window.Context.Viewport = new Rectangle(0, 0, _window.Width, _window.Height);
            _sceneState.Camera.AspectRatio = _window.Width / (double)_window.Height;
        }

        private void OnRenderFrame()
        {
            Context context = _window.Context;
            context.Clear(_clearState);

            _globe.Render(context, _sceneState);
            _vancouverLabel.Render(context, _sceneState);
        }

        #region IDisposable Members

        public void Dispose()
        {
            _camera.Dispose();
            _globe.Texture.Dispose();
            _globe.Dispose();
            _vancouverLabel.Texture.Dispose();
            _vancouverLabel.Dispose();
            _window.Dispose();
        }

        #endregion

        private void Run(double updateRate)
        {
            _window.Run(updateRate);
        }

        static void Main()
        {
            using (LatitudeLongitudeGrid example = new LatitudeLongitudeGrid())
            {
                example.Run(30.0);
            }
        }

        private readonly GraphicsWindow _window;
        private readonly SceneState _sceneState;
        private readonly CameraLookAtPoint _camera;
        private readonly ClearState _clearState;

        private readonly LatitudeLongitudeGridGlobe _globe;
        private readonly BillboardCollection _vancouverLabel;
    }
}
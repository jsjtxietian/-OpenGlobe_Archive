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
using OpenGlobe.Renderer;
using OpenGlobe.Scene;

namespace OpenGlobe.Examples
{
    sealed class ClipmapTerrainOnGlobe : IDisposable
    {
        public ClipmapTerrainOnGlobe()
        {
            _window = Device.CreateWindow(800, 600, "Chapter 13:  Clipmap Terrain on a Globe");

            _ellipsoid = Ellipsoid.Wgs84;

            WorldWindTerrainSource terrainSource = new WorldWindTerrainSource();
            EsriRestImagery imagery = new EsriRestImagery();
            _clipmap = new GlobeClipmapTerrain(_window.Context, terrainSource, imagery, _ellipsoid, 511);
            _clipmap.HeightExaggeration = 1.0f;

            _sceneState = new SceneState();
            _sceneState.DiffuseIntensity = 0.90f;
            _sceneState.SpecularIntensity = 0.05f;
            _sceneState.AmbientIntensity = 0.05f;
            _sceneState.Camera.FieldOfViewY = Math.PI / 3.0;

            _clearState = new ClearState();
            _clearState.Color = Color.White;

            _sceneState.Camera.PerspectiveNearPlaneDistance = 0.000001 * _ellipsoid.MaximumRadius;
            _sceneState.Camera.PerspectiveFarPlaneDistance = 10.0 * _ellipsoid.MaximumRadius;
            _sceneState.SunPosition = new Vector3D(200000, 300000, 200000) * _ellipsoid.MaximumRadius;

             _lookCamera = new CameraLookAtPoint(_sceneState.Camera, _window, _ellipsoid);
             _lookCamera.Range = 1.5 * _ellipsoid.MaximumRadius;

             _globe = new RayCastedGlobe(_window.Context);
             _globe.Shape = _ellipsoid;
             Bitmap bitmap = new Bitmap("NE2_50M_SR_W_4096.jpg");
             _globe.Texture = Device.CreateTexture2D(bitmap, TextureFormat.RedGreenBlue8, false);

             _clearDepth = new ClearState();
             _clearDepth.Buffers = ClearBuffers.DepthBuffer | ClearBuffers.StencilBuffer;

            _window.Keyboard.KeyDown += OnKeyDown;

            _window.Resize += OnResize;
            _window.RenderFrame += OnRenderFrame;
            _window.PreRenderFrame += OnPreRenderFrame;

            _hudFont = new Font("Arial", 16);
            _hud = new HeadsUpDisplay();
            _hud.Color = Color.Blue;
            UpdateHUD();
        }

        private void OnResize()
        {
            _window.Context.Viewport = new Rectangle(0, 0, _window.Width, _window.Height);
            _sceneState.Camera.AspectRatio = _window.Width / (double)_window.Height;
        }

        private void OnKeyDown(object sender, KeyboardKeyEventArgs e)
        {
            if (e.Key == KeyboardKey.U)
            {
                _sceneState.SunPosition = _sceneState.Camera.Eye;
            }
            else if (e.Key == KeyboardKey.W)
            {
                _clipmap.Wireframe = !_clipmap.Wireframe;
                UpdateHUD();
            }
            else if (e.Key == KeyboardKey.B)
            {
                if (!_clipmap.BlendRegionsEnabled)
                {
                    _clipmap.BlendRegionsEnabled = true;
                    _clipmap.ShowBlendRegions = false;
                }
                else if (_clipmap.ShowBlendRegions)
                {
                    _clipmap.BlendRegionsEnabled = false;
                }
                else
                {
                    _clipmap.ShowBlendRegions = true;
                }
                UpdateHUD();
            }
            else if (e.Key == KeyboardKey.L)
            {
                _clipmap.LodUpdateEnabled = !_clipmap.LodUpdateEnabled;
                UpdateHUD();
            }
            else if (e.Key == KeyboardKey.C)
            {
                _clipmap.ColorClipmapLevels = !_clipmap.ColorClipmapLevels;
                if (_clipmap.ColorClipmapLevels)
                {
                    _clipmap.ShowImagery = false;
                    _clipmap.Lighting = true;
                }
                UpdateHUD();
            }
            else if (e.Key == KeyboardKey.I)
            {
                _clipmap.ShowImagery = !_clipmap.ShowImagery;
                _clipmap.Lighting = !_clipmap.ShowImagery;
                if (_clipmap.ShowImagery)
                {
                    _clipmap.ColorClipmapLevels = false;
                }
                UpdateHUD();
            }
            else if (e.Key == KeyboardKey.S)
            {
                _clipmap.Lighting = !_clipmap.Lighting;
                UpdateHUD();
            }
            else if (e.Key == KeyboardKey.Z)
            {
                if (_lookCamera != null)
                {
                    double longitude = -119.5326056;
                    double latitude = 37.74451389;
                    Geodetic3D halfDome = new Geodetic3D(Trig.ToRadians(longitude), Trig.ToRadians(latitude), 2700.0);
                    _lookCamera.ViewPoint(_ellipsoid, halfDome);
                    _lookCamera.Azimuth = 0.0;
                    _lookCamera.Elevation = Trig.ToRadians(30.0);
                    _lookCamera.Range = 10000.0;
                }
            }
            else if (e.Key == KeyboardKey.F)
            {
                if (_lookCamera != null)
                {
                    _lookCamera.Dispose();
                    _lookCamera = null;
                    _flyCamera = new CameraFly(_sceneState.Camera, _window);
                    _flyCamera.MovementRate = 1200.0;
                }
                else if (_flyCamera != null)
                {
                    _flyCamera.Dispose();
                    _flyCamera = null;
                    _sceneState.Camera.Target = new Vector3D(0.0, 0.0, 0.0);
                    _lookCamera = new CameraLookAtPoint(_sceneState.Camera, _window, _ellipsoid);
                    _lookCamera.UpdateParametersFromCamera();
                }
                UpdateHUD();
            }
            else if (_flyCamera != null && (e.Key == KeyboardKey.Plus || e.Key == KeyboardKey.KeypadPlus))
            {
                _flyCamera.MovementRate *= 2.0;
                UpdateHUD();
            }
            else if (_flyCamera != null && (e.Key == KeyboardKey.Minus || e.Key == KeyboardKey.KeypadMinus))
            {
                _flyCamera.MovementRate *= 0.5;
                UpdateHUD();
            }
            else if (e.Key == KeyboardKey.E)
            {
                if (_clipmap.Ellipsoid.MaximumRadius == _clipmap.Ellipsoid.MinimumRadius)
                {
                    _clipmap.Ellipsoid = Ellipsoid.Wgs84;
                    _globe.Shape = Ellipsoid.Wgs84;
                }
                else
                {
                    double radius = Ellipsoid.Wgs84.MaximumRadius;
                    _clipmap.Ellipsoid = new Ellipsoid(radius, radius, radius);
                    _globe.Shape = _clipmap.Ellipsoid;
                }
            }
        }

        private void OnRenderFrame()
        {
            Context context = _window.Context;
            context.Clear(_clearState);

            _globe.Render(context, _sceneState);

            context.Clear(_clearDepth);

            _clipmap.Render(context, _sceneState);

            if (_hud != null)
            {
                _hud.Render(context, _sceneState);
            }
        }

        private void OnPreRenderFrame()
        {
            Context context = _window.Context;
            _clipmap.PreRender(context, _sceneState);
        }

        private void UpdateHUD()
        {
            if (_hud == null)
                return;

            string text;

            text = "Blending: " + GetBlendingString() + " (B)\n";
            text += "Imagery: " + (_clipmap.ShowImagery ? "Enabled" : "Disabled") + " (I)\n";
            text += "Lighting: " + (_clipmap.Lighting ? "Enabled" : "Disabled") + " (S)\n";
            text += "Wireframe: " + (_clipmap.Wireframe ? "Enabled" : "Disabled") + " (W)\n";
            text += "LOD Update: " + (_clipmap.LodUpdateEnabled ? "Enabled" : "Disabled") + " (L)\n";
            text += "Color Clipmap Levels: " + (_clipmap.ColorClipmapLevels ? "Enabled" : "Disabled") + " (C)\n";
            text += "Camera: " + (_lookCamera != null ? "Look At" : "Fly") + " (F)\n";

            if (_flyCamera != null)
            {
                text += "Speed: " + _flyCamera.MovementRate + "m/s (+/-)\n";
            }

            if (_hud.Texture != null)
            {
                _hud.Texture.Dispose();
                _hud.Texture = null;
            }
            _hud.Texture = Device.CreateTexture2D(
                Device.CreateBitmapFromText(text, _hudFont),
                TextureFormat.RedGreenBlueAlpha8, false);
        }

        private string GetBlendingString()
        {
            if (!_clipmap.BlendRegionsEnabled)
                return "Disabled";
            else if (_clipmap.ShowBlendRegions)
                return "Enabled and Shown";
            else
                return "Enabled";
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (_lookCamera != null)
                _lookCamera.Dispose();
            if (_flyCamera != null)
                _flyCamera.Dispose();
            _clipmap.Dispose();
            if (_hudFont != null)
                _hudFont.Dispose();
            if (_hud != null)
            {
                _hud.Texture.Dispose();
                _hud.Dispose();
            }
            _globe.Dispose();
            _window.Dispose();
        }

        #endregion

        private void Run(double updateRate)
        {
            _window.Run(updateRate);
        }

        static void Main()
        {
            using (ClipmapTerrainOnGlobe example = new ClipmapTerrainOnGlobe())
            {
                example.Run(30.0);
            }
        }

        private readonly GraphicsWindow _window;
        private readonly SceneState _sceneState;
        private CameraLookAtPoint _lookCamera;
        private CameraFly _flyCamera;
        private readonly ClearState _clearState;
        private readonly GlobeClipmapTerrain _clipmap;
        private HeadsUpDisplay _hud;
        private Font _hudFont;
        private RayCastedGlobe _globe;
        private ClearState _clearDepth;
        private Ellipsoid _ellipsoid;
    }
}
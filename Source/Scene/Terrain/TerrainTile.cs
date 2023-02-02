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

namespace OpenGlobe.Scene
{
    public class TerrainTile
    {
        public static TerrainTile FromBitmap(Bitmap bitmap)
        {
            if (bitmap == null)
            {
                throw new ArgumentNullException("bitmap");
            }

            float[] heights = new float[bitmap.Width * bitmap.Height];
            float minHeight = float.MaxValue;
            float maxHeight = float.MinValue;

            int k = 0;
            for (int j = bitmap.Height - 1; j >= 0; --j)
            {
                for (int i = 0; i < bitmap.Width; ++i)
                {
                    float height = (float)(bitmap.GetPixel(i, j).R / 255.0);
                    heights[k++] = height;
                    minHeight = Math.Min(height, minHeight);
                    maxHeight = Math.Max(height, maxHeight);
                }
            }

            return new TerrainTile(
                new RectangleD(new Vector2D(0.5, 0.5), new Vector2D((double)bitmap.Width - 0.5, (double)bitmap.Height - 0.5)),
                new Vector2I(bitmap.Width, bitmap.Height),
                heights, minHeight, maxHeight);
        }

        public TerrainTile(
            RectangleD extent,
            Vector2I resolution,
            float[] heights,
            float minimumHeight,
            float maximumHeight)
        {
            if (extent.UpperRight.X <= extent.LowerLeft.X ||
                extent.UpperRight.Y <= extent.LowerLeft.Y)
            {
                throw new ArgumentOutOfRangeException("extent");
            }

            if (resolution.X < 0 || resolution.Y < 0)
            {
                throw new ArgumentOutOfRangeException("resolution");
            }

            if (heights == null)
            {
                throw new ArgumentNullException("heights");
            }

            if (heights.Length != resolution.X * resolution.Y)
            {
                throw new ArgumentException("heights.Length != resolution.Width * resolution.Height");
            }

            if (minimumHeight > maximumHeight)
            {
                throw new ArgumentOutOfRangeException("minimumHeight", "minimumHeight > maximumHeight");
            }

            _extent = extent;
            _resolution = resolution;
            _heights = heights;
            _minimumHeight = minimumHeight;
            _maximumHeight = maximumHeight;
        }

        public RectangleD Extent
        {
            get { return _extent; }
        }

        public Vector2I Resolution
        {
            get { return _resolution; }
        }

        public float[] Heights
        {
            get { return _heights; }
        }

        public float MinimumHeight
        {
            get { return _minimumHeight; }
        }

        public float MaximumHeight
        {
            get { return _maximumHeight; }
        }

        private readonly RectangleD _extent;
        private readonly Vector2I _resolution;
        private readonly float[] _heights;
        private readonly float _minimumHeight;
        private readonly float _maximumHeight;
    }
}
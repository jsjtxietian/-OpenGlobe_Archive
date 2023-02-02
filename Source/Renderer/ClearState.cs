﻿#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using System.Drawing;

namespace OpenGlobe.Renderer
{
    public class ClearState
    {
        public ClearState()
        {
            ScissorTest = new ScissorTest();
            ColorMask = new ColorMask(true, true, true, true);
            DepthMask = true;
            FrontStencilMask = ~0;
            BackStencilMask = ~0;

            Buffers = ClearBuffers.All;
            Color = Color.White;
            Depth = 1;
            Stencil = 0;
        }

        public ScissorTest ScissorTest { get; set; }
        public ColorMask ColorMask { get; set; }
        public bool DepthMask { get; set; }
        public int FrontStencilMask { get; set; }
        public int BackStencilMask { get; set; }
        
        public ClearBuffers Buffers { get; set; }
        public Color Color { get; set; }
        public float Depth { get; set; }
        public int Stencil { get; set; }
    }
}

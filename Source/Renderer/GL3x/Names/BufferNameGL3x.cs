﻿#region License
//
// (C) Copyright 2009 Patrick Cozzi, Deron Ohlarik, and Kevin Ring
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using System;
using OpenTK.Graphics.OpenGL;

namespace OpenGlobe.Renderer.GL3x
{
    internal sealed class BufferNameGL3x : IDisposable
    {
        public BufferNameGL3x()
        {
            GL.GenBuffers(1, out _value);
        }

        ~BufferNameGL3x()
        {
            FinalizerThreadContextGL3x.RunFinalizer(Dispose);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public int Value
        {
            get { return _value; }
        }

        private void Dispose(bool disposing)
        {
            if (_value != 0)
            {
                GL.DeleteBuffers(1, ref _value);
                _value = 0;
            }
        }

        private int _value;
    }
}

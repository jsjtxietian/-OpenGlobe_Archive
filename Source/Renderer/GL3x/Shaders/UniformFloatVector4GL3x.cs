﻿#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenGlobe.Core;
using OpenGlobe.Renderer;

namespace OpenGlobe.Renderer.GL3x
{
    internal class UniformFloatVector4GL3x : Uniform<Vector4F>, ICleanable
    {
        internal UniformFloatVector4GL3x(string name, int location, ICleanableObserver observer)
            : base(name, UniformType.FloatVector4)
        {
            _location = location;
            _dirty = true;
            _observer = observer;
            _observer.NotifyDirty(this);
        }

        #region Uniform<> Members

        public override Vector4F Value
        {
            set
            {
                if (!_dirty && (_value != value))
                {
                    _dirty = true;
                    _observer.NotifyDirty(this);
                }

                _value = value;
            }

            get { return _value; }
        }

        #endregion

        #region ICleanable Members

        public void Clean()
        {
            GL.Uniform4(_location, _value.X, _value.Y, _value.Z, _value.W);
            _dirty = false;
        }

        #endregion

        private int _location;
        private Vector4F _value;
        private bool _dirty;
        private readonly ICleanableObserver _observer;
    }
}

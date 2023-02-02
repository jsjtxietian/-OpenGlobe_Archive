﻿#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using OpenGlobe.Core;
using OpenTK.Graphics.OpenGL;

namespace OpenGlobe.Renderer.GL3x
{
    internal class UniformBoolVector2GL3x : Uniform<Vector2B>, ICleanable
    {
        internal UniformBoolVector2GL3x(string name, int location, ICleanableObserver observer)
            : base(name, UniformType.BoolVector2)
        {
            _location = location;
            _dirty = true;
            _observer = observer;
            _observer.NotifyDirty(this);
        }

        #region Uniform<> Members

        public override Vector2B Value
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
            GL.Uniform2(_location, _value.X ? 1 : 0, _value.Y ? 1 : 0);
            _dirty = false;
        }

        #endregion

        private int _location;
        private Vector2B _value;
        private bool _dirty;
        private readonly ICleanableObserver _observer;
    }
}

#region License
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
    internal class UniformFloatMatrix32GL3x : Uniform<Matrix32<float>>, ICleanable
    {
        internal UniformFloatMatrix32GL3x(string name, int location, ICleanableObserver observer)
            : base(name, UniformType.FloatMatrix32)
        {
            _location = location;
            _value = new Matrix32<float>();
            _dirty = true;
            _observer = observer;
            _observer.NotifyDirty(this);
        }

        #region Uniform<> Members

        public override Matrix32<float> Value
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
            GL.UniformMatrix3x2(_location, 1, false, _value.ReadOnlyColumnMajorValues);
            _dirty = false;
        }

        #endregion

        private int _location;
        private Matrix32<float> _value;
        private bool _dirty;
        private readonly ICleanableObserver _observer;
    }
}

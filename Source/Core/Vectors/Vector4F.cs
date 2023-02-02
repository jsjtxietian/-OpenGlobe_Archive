﻿#region License
//
// (C) Copyright 2010 Patrick Cozzi, Deron Ohlarik, and Kevin Ring
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Globalization;

namespace OpenGlobe.Core
{
    /// <summary>
    /// A set of 4-dimensional cartesian coordinates where the four components,
    /// <see cref="X"/>, <see cref="Y"/>, <see cref="Z"/>, and <see cref="W"/>
    /// are represented as single-precision (32-bit) floating point numbers.
    /// </summary>
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct Vector4F : IEquatable<Vector4F>
    {
        public static Vector4F Zero
        {
            get { return new Vector4F(0.0f, 0.0f, 0.0f, 0.0f); }
        }

        public static Vector4F UnitX
        {
            get { return new Vector4F(1.0f, 0.0f, 0.0f, 0.0f); }
        }

        public static Vector4F UnitY
        {
            get { return new Vector4F(0.0f, 1.0f, 0.0f, 0.0f); }
        }

        public static Vector4F UnitZ
        {
            get { return new Vector4F(0.0f, 0.0f, 1.0f, 0.0f); }
        }

        public static Vector4F UnitW
        {
            get { return new Vector4F(0.0f, 0.0f, 0.0f, 1.0f); }
        }

        public static Vector4F Undefined
        {
            get { return new Vector4F(float.NaN, float.NaN, float.NaN, float.NaN); }
        }

        public Vector4F(float x, float y, float z, float w)
        {
            _x = x;
            _y = y;
            _z = z;
            _w = w;
        }

        public Vector4F(Vector3F v, float w)
        {
            _x = v.X;
            _y = v.Y;
            _z = v.Z;
            _w = w;
        }

        public Vector4F(Vector2F v, float z, float w)
        {
            _x = v.X;
            _y = v.Y;
            _z = z;
            _w = w;
        }

        public float X
        {
            get { return _x; }
        }

        public float Y
        {
            get { return _y; }
        }

        public float Z
        {
            get { return _z; }
        }

        public float W
        {
            get { return _w; }
        }

        public Vector2F XY
        {
            get { return new Vector2F(X, Y); }
        }

        public Vector3F XYZ
        {
            get { return new Vector3F(X, Y, Z); }
        }

        public float MagnitudeSquared
        {
            get { return _x * _x + _y * _y + _z * _z + _w * _w; }
        }

        public float Magnitude
        {
            get { return (float)Math.Sqrt(MagnitudeSquared); }
        }

        public bool IsUndefined
        {
            get { return float.IsNaN(_x); }
        }

        public Vector4F Normalize(out float magnitude)
        {
            magnitude = Magnitude;
            return this / magnitude;
        }

        public Vector4F Normalize()
        {
            float magnitude;
            return Normalize(out magnitude);
        }

        public float Dot(Vector4F other)
        {
            return X * other.X + Y * other.Y + Z * other.Z + W * other.W;
        }

        public Vector4F Add(Vector4F addend)
        {
            return this + addend;
        }

        public Vector4F Subtract(Vector4F subtrahend)
        {
            return this - subtrahend;
        }

        public Vector4F Multiply(float scalar)
        {
            return this * scalar;
        }

        public Vector4F MultiplyComponents(Vector4F scale)
        {
            return new Vector4F(X * scale.X, Y * scale.Y, Z * scale.Z, W * scale.W);
        }

        public Vector4F Divide(float scalar)
        {
            return this / scalar;
        }

        public Vector4F MostOrthogonalAxis
        {
            get
            {
                float x = Math.Abs(X);
                float y = Math.Abs(Y);
                float z = Math.Abs(Z);
                float w = Math.Abs(W);

                if ((x < y) && (x < z) && (x < w))
                {
                    return UnitX;
                }
                else if ((y < x) && (y < z) && (y < w))
                {
                    return UnitY;
                }
                else if ((z < x) && (z < y) && (z < w))
                {
                    return UnitZ;
                }
                else
                {
                    return UnitW;
                }
            }
        }

        public Vector4F Negate()
        {
            return -this;
        }

        public bool EqualsEpsilon(Vector4F other, float epsilon)
        {
            return (Math.Abs(_x - other._x) <= epsilon) &&
                   (Math.Abs(_y - other._y) <= epsilon) &&
                   (Math.Abs(_z - other._z) <= epsilon) &&
                   (Math.Abs(_w - other._w) <= epsilon);
        }

        public bool Equals(Vector4F other)
        {
            return _x == other._x && _y == other._y && _z == other._z && _w == other._w;
        }

        public static Vector4F operator -(Vector4F vector)
        {
            return new Vector4F(-vector.X, -vector.Y, -vector.Z, -vector.W);
        }

        public static Vector4F operator +(Vector4F left, Vector4F right)
        {
            return new Vector4F(left._x + right._x, left._y + right._y, left._z + right._z, left._w + right._w);
        }

        public static Vector4F operator -(Vector4F left, Vector4F right)
        {
            return new Vector4F(left._x - right._x, left._y - right._y, left._z - right._z, left._w - right._w);
        }

        public static Vector4F operator *(Vector4F left, float right)
        {
            return new Vector4F(left._x * right, left._y * right, left._z * right, left._w * right);
        }

        public static Vector4F operator *(float left, Vector4F right)
        {
            return right * left;
        }

        public static Vector4F operator /(Vector4F left, float right)
        {
            return new Vector4F(left._x / right, left._y / right, left._z / right, left._w / right);
        }

        public static bool operator ==(Vector4F left, Vector4F right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Vector4F left, Vector4F right)
        {
            return !left.Equals(right);
        }

        public override bool Equals(object obj)
        {
            if (obj is Vector4F)
            {
                return Equals((Vector4F)obj);
            }
            return false;
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "({0}, {1}, {2}, {3})", X, Y, Z, W);
        }

        public override int GetHashCode()
        {
            return _x.GetHashCode() ^ _y.GetHashCode() ^ _z.GetHashCode() ^ _w.GetHashCode();
        }

        public Vector4D ToVector4D()
        {
            return new Vector4D(_x, _y, _z, _w);
        }

        public Vector4I ToVector4I()
        {
            return new Vector4I((int)_x, (int)_y, (int)_z, (int)_w);
        }

        public Vector4H ToVector4H()
        {
            return new Vector4H(_x, _y, _z, _w);
        }

        public Vector4B ToVector4B()
        {
            return new Vector4B(Convert.ToBoolean(_x), Convert.ToBoolean(_y), Convert.ToBoolean(_z), Convert.ToBoolean(_w));
        }

        private readonly float _x;
        private readonly float _y;
        private readonly float _z;
        private readonly float _w;
    }
}

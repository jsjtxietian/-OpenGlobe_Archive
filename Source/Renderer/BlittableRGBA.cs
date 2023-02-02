﻿#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using System;
using System.Drawing;
using System.Globalization;
using System.Runtime.InteropServices;

namespace OpenGlobe.Renderer
{
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct BlittableRGBA : IEquatable<BlittableRGBA>
    {
        public byte R { get; set; }
        public byte G { get; set; }
        public byte B { get; set; }
        public byte A { get; set; }

        public static readonly ImageFormat Format = ImageFormat.RedGreenBlueAlpha;
        public static readonly ImageDatatype Datatype = ImageDatatype.UnsignedByte;

        public BlittableRGBA(Color color)
            : this()
        {
            R = color.R;
            G = color.G;
            B = color.B;
            A = color.A;
        }

        public static bool operator ==(BlittableRGBA left, BlittableRGBA right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(BlittableRGBA left, BlittableRGBA right)
        {
            return !left.Equals(right);
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "Red: {0} Green: {1} Blue: {2} Alpha: {3}", R, G, B, A);
        }

        public override int GetHashCode()
        {
            return R.GetHashCode() ^ G.GetHashCode() ^ B.GetHashCode() ^ A.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is BlittableRGBA))
                return false;

            return this.Equals((BlittableRGBA)obj);
        }

        #region IEquatable<BlittableRGBA> Members

        public bool Equals(BlittableRGBA other)
        {
            return
                (R == other.R) &&
                (G == other.G) &&
                (B == other.B) &&
                (A == other.A);
        }

        #endregion
    }
}

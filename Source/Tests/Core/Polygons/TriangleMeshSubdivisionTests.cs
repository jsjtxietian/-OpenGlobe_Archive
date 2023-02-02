﻿#region License
//
// (C) Copyright 2010 Patrick Cozzi, Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace OpenGlobe.Core
{
    [TestFixture]
    public class TriangleMeshSubdivisionTests
    {
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Null()
        {
            TriangleMeshSubdivision.Compute(null, new IndicesUnsignedInt(), Trig.ToRadians(1));
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Null2()
        {
            TriangleMeshSubdivision.Compute(new Vector3D[] { }, null, Trig.ToRadians(1));
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void OutOfRangeException()
        {
            TriangleMeshSubdivision.Compute(new Vector3D[] { }, new IndicesUnsignedInt(), Trig.ToRadians(1));
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void ArgumentException()
        {
            IndicesUnsignedInt indices = new IndicesUnsignedInt();
            indices.Values.Add(0);
            indices.Values.Add(1);
            indices.Values.Add(2);
            indices.Values.Add(3);

            TriangleMeshSubdivision.Compute(new Vector3D[] { }, indices, Trig.ToRadians(1));
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void OutOfRangeException2()
        {
            Vector3D[] positions = new Vector3D[]
            {
                Vector3D.Zero,
                new Vector3D(0.5, 0, 0),
                new Vector3D(0.5, 0.5, 0)
            };

            IndicesUnsignedInt indices = new IndicesUnsignedInt();
            indices.Values.Add(0);
            indices.Values.Add(1);
            indices.Values.Add(2);

            TriangleMeshSubdivision.Compute(positions, indices, 0);
        }

        [Test]
        public void OneTriangle()
        {
            Vector3D[] positions = new Vector3D[]
            {
                Vector3D.Zero,
                new Vector3D(0.5, 0, 0),
                new Vector3D(0.5, 0.5, 0)
            };

            IndicesUnsignedInt indices = new IndicesUnsignedInt();
            indices.Values.Add(0);
            indices.Values.Add(1);
            indices.Values.Add(2);

            TriangleMeshSubdivisionResult result = TriangleMeshSubdivision.Compute(positions, indices, Trig.ToRadians(1));

            Assert.AreEqual(0, result.Indices.Values[0]);
            Assert.AreEqual(1, result.Indices.Values[1]);
            Assert.AreEqual(2, result.Indices.Values[2]);
        }

        [Test]
        public void TwoTriangles()
        {
            Vector3D[] positions = new Vector3D[]
            {
                Vector3D.Zero,
                new Vector3D(0.5, 0.5, 0),
                new Vector3D(-0.5, 0.5, 0),
                new Vector3D(0, 1, 0)
            };

            IndicesUnsignedInt indices = new IndicesUnsignedInt();
            indices.Values.Add(0);
            indices.Values.Add(1);
            indices.Values.Add(2);
            indices.Values.Add(3);
            indices.Values.Add(2);
            indices.Values.Add(1);

            TriangleMeshSubdivisionResult result = TriangleMeshSubdivision.Compute(positions, indices, Trig.ToRadians(90));

            Assert.AreEqual(0, result.Indices.Values[0]);
            Assert.AreEqual(1, result.Indices.Values[1]);
            Assert.AreEqual(2, result.Indices.Values[2]);

            Assert.AreEqual(3, result.Indices.Values[3]);
            Assert.AreEqual(2, result.Indices.Values[4]);
            Assert.AreEqual(1, result.Indices.Values[5]);
        }

        [Test]
        public void TwoTriangles2()
        {
            Vector3D[] positions = new Vector3D[]
            {
                Vector3D.Zero,
                new Vector3D(0.5, 0.5, 0),
                new Vector3D(-0.5, 0.5, 0),
                new Vector3D(0, 1, 0)
            };

            IndicesUnsignedInt indices = new IndicesUnsignedInt();
            indices.Values.Add(1);
            indices.Values.Add(2);
            indices.Values.Add(0);
            indices.Values.Add(2);
            indices.Values.Add(1);
            indices.Values.Add(3);

            TriangleMeshSubdivisionResult result = TriangleMeshSubdivision.Compute(positions, indices, Trig.ToRadians(90));

            Assert.AreEqual(1, result.Indices.Values[0]);
            Assert.AreEqual(2, result.Indices.Values[1]);
            Assert.AreEqual(0, result.Indices.Values[2]);

            Assert.AreEqual(2, result.Indices.Values[3]);
            Assert.AreEqual(1, result.Indices.Values[4]);
            Assert.AreEqual(3, result.Indices.Values[5]);
        }

        [Test]
        public void TwoTriangles3()
        {
            Vector3D[] positions = new Vector3D[]
            {
                Ellipsoid.UnitSphere.ToVector3D(Trig.ToRadians(new Geodetic2D(0, 44))),
                Ellipsoid.UnitSphere.ToVector3D(Trig.ToRadians(new Geodetic2D(1, 45))),
                Ellipsoid.UnitSphere.ToVector3D(Trig.ToRadians(new Geodetic2D(-1, 45))),
                Ellipsoid.UnitSphere.ToVector3D(Trig.ToRadians(new Geodetic2D(0, 46)))
            };
            
            IndicesUnsignedInt indices = new IndicesUnsignedInt();
            indices.Values.Add(0);
            indices.Values.Add(1);
            indices.Values.Add(2);
            indices.Values.Add(3);
            indices.Values.Add(2);
            indices.Values.Add(1);

            TriangleMeshSubdivisionResult result = TriangleMeshSubdivision.Compute(positions, indices, Trig.ToRadians(1.4));

            Assert.AreEqual(5, result.Positions.Count);
            Assert.AreEqual(12, result.Indices.Values.Count);
        }
    }
}

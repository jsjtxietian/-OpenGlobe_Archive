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
    public class SimplePolygonAlgorithmsTests
    {
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WindingOrderNull()
        {
            SimplePolygonAlgorithms.ComputeWindingOrder(null);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void WindingOrderEmpty()
        {
            SimplePolygonAlgorithms.ComputeWindingOrder(new List<Vector2D>());
        }
                
        [Test]
        public void WindingOrderCounterclockwiseTriangle()
        {
            IList<Vector2D> positions = new List<Vector2D>();
            positions.Add(new Vector2D(0, 0));
            positions.Add(new Vector2D(1, 0)); 
            positions.Add(new Vector2D(1, 1));

            Assert.AreEqual(PolygonWindingOrder.Counterclockwise, SimplePolygonAlgorithms.ComputeWindingOrder(positions));
            Assert.AreEqual(0.5, SimplePolygonAlgorithms.ComputeArea(positions));
        }

        [Test]
        public void WindingOrderClockwiseTriangle()
        {
            IList<Vector2D> positions = new List<Vector2D>();
            positions.Add(new Vector2D(0, 0));
            positions.Add(new Vector2D(1, 1));
            positions.Add(new Vector2D(1, 0));

            Assert.AreEqual(PolygonWindingOrder.Clockwise, SimplePolygonAlgorithms.ComputeWindingOrder(positions));
            Assert.AreEqual(-0.5, SimplePolygonAlgorithms.ComputeArea(positions));
        }

        [Test]
        public void WindingOrderCounterclockwiseDiamond()
        {
            IList<Vector2D> positions = new List<Vector2D>();
            positions.Add(new Vector2D(0, -0.5));
            positions.Add(new Vector2D(0.5, 0));
            positions.Add(new Vector2D(0, 0.5));
            positions.Add(new Vector2D(-0.5, 0));

            Assert.AreEqual(PolygonWindingOrder.Counterclockwise, SimplePolygonAlgorithms.ComputeWindingOrder(positions));
            Assert.AreEqual(0.5, SimplePolygonAlgorithms.ComputeArea(positions));
        }

        [Test]
        public void WindingOrderClockwiseDiamond()
        {
            IList<Vector2D> positions = new List<Vector2D>();
            positions.Add(new Vector2D(0, -0.5));
            positions.Add(new Vector2D(-0.5, 0));
            positions.Add(new Vector2D(0, 0.5));
            positions.Add(new Vector2D(0.5, 0));

            Assert.AreEqual(PolygonWindingOrder.Clockwise, SimplePolygonAlgorithms.ComputeWindingOrder(positions));
            Assert.AreEqual(-0.5, SimplePolygonAlgorithms.ComputeArea(positions));
        }

        [Test]
        public void WindingOrderOnEllipsoid()
        {
            IList<Vector3D> positions = new List<Vector3D>();
            positions.Add(Ellipsoid.UnitSphere.ToVector3D(Trig.ToRadians(new Geodetic3D(0, 0))));
            positions.Add(Ellipsoid.UnitSphere.ToVector3D(Trig.ToRadians(new Geodetic3D(1, 0))));
            positions.Add(Ellipsoid.UnitSphere.ToVector3D(Trig.ToRadians(new Geodetic3D(1, 1))));

            EllipsoidTangentPlane plane = new EllipsoidTangentPlane(Ellipsoid.UnitSphere, positions);
            ICollection<Vector2D> positionsOnPlane = plane.ComputePositionsOnPlane(positions);
            Assert.AreEqual(PolygonWindingOrder.Counterclockwise, SimplePolygonAlgorithms.ComputeWindingOrder(positionsOnPlane));
        }

        [Test]
        public void WindingOrderOnEllipsoid2()
        {
            IList<Vector3D> positions = new List<Vector3D>();
            positions.Add(Ellipsoid.UnitSphere.ToVector3D(Trig.ToRadians(new Geodetic3D(1, 1))));
            positions.Add(Ellipsoid.UnitSphere.ToVector3D(Trig.ToRadians(new Geodetic3D(1, 0))));
            positions.Add(Ellipsoid.UnitSphere.ToVector3D(Trig.ToRadians(new Geodetic3D(0, 0))));

            EllipsoidTangentPlane plane = new EllipsoidTangentPlane(Ellipsoid.UnitSphere, positions);
            ICollection<Vector2D> positionsOnPlane = plane.ComputePositionsOnPlane(positions);
            Assert.AreEqual(PolygonWindingOrder.Clockwise, SimplePolygonAlgorithms.ComputeWindingOrder(positionsOnPlane));
        }


        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CleanupOrderNull()
        {
            SimplePolygonAlgorithms.Cleanup<Vector2D>(null);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void CleanupOrderEmpty()
        {
            SimplePolygonAlgorithms.Cleanup(new List<Vector2D>());
        }

        [Test]
        public void CleanupNotRequired()
        {
            IList<Vector2D> positions = new List<Vector2D>();
            positions.Add(new Vector2D(0, 0));
            positions.Add(new Vector2D(1, 1));
            positions.Add(new Vector2D(1, 0));

            GraphicsAssert.ListsAreEqual(positions, SimplePolygonAlgorithms.Cleanup(positions));
        }

        [Test]
        public void CleanupRequired()
        {
            IList<Vector2D> positions = new List<Vector2D>();
            positions.Add(new Vector2D(0, 0));
            positions.Add(new Vector2D(0, 0));
            positions.Add(new Vector2D(1, 0));
            positions.Add(new Vector2D(1, 1));
            positions.Add(new Vector2D(0, 0));

            IList<Vector2D> cleanedPositions = SimplePolygonAlgorithms.Cleanup(positions);

            Assert.AreEqual(3, cleanedPositions.Count);
            Assert.AreEqual(new Vector2D(1, 0), cleanedPositions[0]);
            Assert.AreEqual(new Vector2D(1, 1), cleanedPositions[1]);
            Assert.AreEqual(new Vector2D(0, 0), cleanedPositions[2]);
        }

    }
}

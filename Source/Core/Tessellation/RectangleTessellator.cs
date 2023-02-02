﻿#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using System;
using System.Collections.Generic;

namespace OpenGlobe.Core
{
    public static class RectangleTessellator
    {
        public static Mesh Compute(RectangleD rectangle, int numberOfPartitionsX, int numberOfPartitionsY)
        {
            if (numberOfPartitionsX < 0)
            {
                throw new ArgumentOutOfRangeException("numberOfPartitionsX");
            }

            if (numberOfPartitionsY < 0)
            {
                throw new ArgumentOutOfRangeException("numberOfPartitionsY");
            }

            Mesh mesh = new Mesh();
            mesh.PrimitiveType = PrimitiveType.Triangles;
            mesh.FrontFaceWindingOrder = WindingOrder.Counterclockwise;

            int numberOfPositions = (numberOfPartitionsX + 1) * (numberOfPartitionsY + 1);
            VertexAttributeFloatVector2 positionsAttribute = new VertexAttributeFloatVector2("position", numberOfPositions);
            IList<Vector2F> positions = positionsAttribute.Values;
            mesh.Attributes.Add(positionsAttribute);

            int numberOfIndices = (numberOfPartitionsX * numberOfPartitionsY) * 6;
            IndicesUnsignedInt indices = new IndicesUnsignedInt(numberOfIndices);
            mesh.Indices = indices;

            //
            // Positions
            //
            Vector2D lowerLeft = rectangle.LowerLeft;
            Vector2D toUpperRight = rectangle.UpperRight - lowerLeft;

            for (int y = 0; y <= numberOfPartitionsY; ++y)
            {
                double deltaY = y / (double)numberOfPartitionsY;
                double currentY = lowerLeft.Y + (deltaY * toUpperRight.Y);

                for (int x = 0; x <= numberOfPartitionsX; ++x)
                {
                    double deltaX = x / (double)numberOfPartitionsX;
                    double currentX = lowerLeft.X + (deltaX * toUpperRight.X);
                    positions.Add(new Vector2D(currentX, currentY).ToVector2F());
                }
            }

            //
            // Indices
            //
            int rowDelta = numberOfPartitionsX + 1;
            int i = 0;
            for (int y = 0; y < numberOfPartitionsY; ++y)
            {
                for (int x = 0; x < numberOfPartitionsX; ++x)
                {
                    indices.AddTriangle(new TriangleIndicesUnsignedInt(i, i + 1, rowDelta + (i + 1)));
                    indices.AddTriangle(new TriangleIndicesUnsignedInt(i, rowDelta + (i + 1), rowDelta + i));
                    i += 1;
                }
                i += 1;
            }

            return mesh;
        }
    }
}
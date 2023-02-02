﻿#region License
//
// (C) Copyright 2010 Patrick Cozzi and Kevin Ring
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

namespace OpenGlobe.Scene
{
    public class RasterTileRegion
    {
        public RasterTileRegion(RasterTile tile, int west, int south, int east, int north)
        {
            _tile = tile;
            _west = west;
            _south = south;
            _east = east;
            _north = north;
        }

        public RasterTile Tile
        {
            get { return _tile; }
        }

        public int West
        {
            get { return _west; }
        }

        public int South
        {
            get { return _south; }
        }

        public int East
        {
            get { return _east; }
        }

        public int North
        {
            get { return _north; }
        }

        private readonly RasterTile _tile;
        private readonly int _west;
        private readonly int _south;
        private readonly int _east;
        private readonly int _north;
    }
}

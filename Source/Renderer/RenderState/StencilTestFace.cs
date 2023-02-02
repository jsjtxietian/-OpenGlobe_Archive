﻿#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

namespace OpenGlobe.Renderer
{
    public enum StencilOperation
    {
        Zero,
        Invert,
        Keep,
        Replace,
        Increment,
        Decrement,
        IncrementWrap,
        DecrementWrap
    }

    public enum StencilTestFunction
    {
        Never,
        Less,
        Equal,
        LessThanOrEqual,
        Greater,
        NotEqual,
        GreaterThanOrEqual,
        Always,
    }

    public class StencilTestFace
    {
        public StencilTestFace()
        {
            StencilFailOperation = StencilOperation.Keep;
            DepthFailStencilPassOperation = StencilOperation.Keep;
            DepthPassStencilPassOperation = StencilOperation.Keep;
            Function = StencilTestFunction.Always;
            ReferenceValue = 0;
            Mask = ~0;
        }

        public StencilOperation StencilFailOperation { get; set; }
        public StencilOperation DepthFailStencilPassOperation { get; set; }
        public StencilOperation DepthPassStencilPassOperation { get; set; }
        
        public StencilTestFunction Function { get; set; }
        public int ReferenceValue { get; set; }
        public int Mask { get; set; }
    }
}

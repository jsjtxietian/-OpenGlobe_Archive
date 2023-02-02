﻿#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

namespace OpenGlobe.Renderer
{
    internal class ViewportOrthographicMatrixUniformFactory : DrawAutomaticUniformFactory
    {
        #region DrawAutomaticUniformFactory Members

        public override string Name
        {
            get { return "og_viewportOrthographicMatrix"; }
        }

        public override DrawAutomaticUniform Create(Uniform uniform)
        {
            return new ViewportOrthographicMatrixUniform(uniform);
        }

        #endregion
    }
}

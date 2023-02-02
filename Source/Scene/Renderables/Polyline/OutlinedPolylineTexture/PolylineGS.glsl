﻿#version 330 
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//

layout(lines) in;
layout(triangle_strip, max_vertices = 4) out;

in vec4 gsColor[];
in vec4 gsOutlineColor[];

flat out vec4 fsColor;
flat out vec4 fsOutlineColor;
out float fsTextureCoordinate;

uniform mat4 og_modelViewPerspectiveMatrix;
uniform mat4 og_viewportTransformationMatrix;
uniform mat4 og_viewportOrthographicMatrix;
uniform float og_perspectiveNearPlaneDistance;
uniform float u_distance;

vec4 ClipToWindowCoordinates(vec4 v, mat4 viewportTransformationMatrix)
{
    v.xyz /= v.w;                                                  // normalized device coordinates
    v.xyz = (viewportTransformationMatrix * vec4(v.xyz, 1.0)).xyz; // window coordinates
    return v;
}

void ClipLineSegmentToNearPlane(
    float nearPlaneDistance, 
    mat4 modelViewPerspectiveMatrix,
    vec4 modelP0, 
    vec4 modelP1, 
    out vec4 clipP0, 
    out vec4 clipP1,
	out bool culledByNearPlane)
{
    clipP0 = modelViewPerspectiveMatrix * modelP0;
    clipP1 = modelViewPerspectiveMatrix * modelP1;
	culledByNearPlane = false;

    float distanceToP0 = clipP0.z + nearPlaneDistance;
    float distanceToP1 = clipP1.z + nearPlaneDistance;

    if ((distanceToP0 * distanceToP1) < 0.0)
    {
        float t = distanceToP0 / (distanceToP0 - distanceToP1);
        vec3 modelV = vec3(modelP0) + t * (vec3(modelP1) - vec3(modelP0));
        vec4 clipV = modelViewPerspectiveMatrix * vec4(modelV, 1);

        if (distanceToP0 < 0.0)
        {
            clipP0 = clipV;
        }
        else
        {
            clipP1 = clipV;
        }
    }
	else if (distanceToP0 < 0.0)
	{
	    culledByNearPlane = true;
	}
}

void main()
{
    vec4 clipP0;
    vec4 clipP1;
	bool culledByNearPlane;
    ClipLineSegmentToNearPlane(og_perspectiveNearPlaneDistance, 
		og_modelViewPerspectiveMatrix,
		gl_in[0].gl_Position, gl_in[1].gl_Position, clipP0, clipP1, culledByNearPlane);

	if (culledByNearPlane)
	{
		return;
	}

	vec4 windowP0 = ClipToWindowCoordinates(clipP0, og_viewportTransformationMatrix);
	vec4 windowP1 = ClipToWindowCoordinates(clipP1, og_viewportTransformationMatrix);

	vec2 direction = windowP1.xy - windowP0.xy;
	vec2 normal = normalize(vec2(direction.y, -direction.x));

	vec4 v0 = vec4(windowP0.xy - (normal * u_distance), -windowP0.z, 1.0);
	vec4 v1 = vec4(windowP1.xy - (normal * u_distance), -windowP1.z, 1.0);
	vec4 v2 = vec4(windowP0.xy + (normal * u_distance), -windowP0.z, 1.0);
	vec4 v3 = vec4(windowP1.xy + (normal * u_distance), -windowP1.z, 1.0);

	gl_Position = og_viewportOrthographicMatrix * v0;
	fsColor = gsColor[0];
	fsOutlineColor = gsOutlineColor[0];
	fsTextureCoordinate = 0.0;
	EmitVertex();

	gl_Position = og_viewportOrthographicMatrix * v1;
	fsColor = gsColor[0];
	fsOutlineColor = gsOutlineColor[0];
	fsTextureCoordinate = 0.0;
	EmitVertex();

	gl_Position = og_viewportOrthographicMatrix * v2;
	fsColor = gsColor[0];
	fsOutlineColor = gsOutlineColor[0];
	fsTextureCoordinate = 1.0;
	EmitVertex();

	gl_Position = og_viewportOrthographicMatrix * v3;
	fsColor = gsColor[0];
	fsOutlineColor = gsOutlineColor[0];
	fsTextureCoordinate = 1.0;
	EmitVertex();
}
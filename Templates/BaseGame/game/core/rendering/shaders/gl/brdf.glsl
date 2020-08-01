//-----------------------------------------------------------------------------
// Copyright (c) 2018 GarageGames, LLC
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to
// deal in the Software without restriction, including without limitation the
// rights to use, copy, modify, merge, publish, distribute, sublicense, and/or
// sell copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS
// IN THE SOFTWARE.
//-----------------------------------------------------------------------------

#ifndef BRDF_HLSL
#define BRDF_HLSL

#include "./torque.glsl"

// BRDF from Frostbite presentation:
// Moving Frostbite to Physically Based Rendering
// S´ebastien Lagarde - Electronic Arts Frostbite
// Charles de Rousiers - Electronic Arts Frostbite
// SIGGRAPH 2014

float pow5(float x) {
    float x2 = x * x;
    return x2 * x2 * x;
}

vec3 F_Schlick(in vec3 f0, in float f90, in float u)
{
	return f0 + (f90 - f0) * pow5(1.f - u);
}

vec3 F_Fresnel(vec3 SpecularColor, float VoH)
{
	vec3 SpecularColorSqrt = sqrt(min(SpecularColor, vec3(0.99, 0.99, 0.99)));
	vec3 n = (1 + SpecularColorSqrt) / (1 - SpecularColorSqrt);
	vec3 g = sqrt(n*n + VoH*VoH - 1);
	return 0.5 * sqr((g - VoH) / (g + VoH)) * (1 + sqr(((g + VoH)*VoH - 1) / ((g - VoH)*VoH + 1)));
}

vec3 FresnelSchlickRoughness(float cosTheta, vec3 F0, float roughness)
{
	vec3 ret = vec3(0.0, 0.0, 0.0);
	float powTheta = pow(1.0 - cosTheta, 5.0);
	float invRough = float(1.0 - roughness);

	ret.x = F0.x + (max(invRough, F0.x) - F0.x) * powTheta;
	ret.y = F0.y + (max(invRough, F0.y) - F0.y) * powTheta;
	ret.z = F0.z + (max(invRough, F0.z) - F0.z) * powTheta;

	return ret;
}

float Fr_DisneyDiffuse(float NdotV, float NdotL, float LdotH, float linearRoughness)
{
	float energyBias = lerp(0.0f, 0.5f, linearRoughness);
	float energyFactor = lerp(1.0f, 1.0f / 1.51f, linearRoughness);
	float fd90 = energyBias + 2.0 * LdotH*LdotH * linearRoughness;
	vec3 f0 = vec3(1.0f, 1.0f, 1.0f);
	float lightScatter = F_Schlick(f0, fd90, NdotL).r;
	float viewScatter = F_Schlick(f0, fd90, NdotV).r;

	return lightScatter * viewScatter * energyFactor;
}

float V_SmithGGXCorrelated(float NdotL, float NdotV, float roughness)
{
	// Original formulation of G_SmithGGX Correlated 
	// lambda_v = (-1 + sqrt(alphaG2 * (1 - NdotL2) / NdotL2 + 1)) * 0.5f; 
	// lambda_l = (-1 + sqrt(alphaG2 * (1 - NdotV2) / NdotV2 + 1)) * 0.5f; 
	// G_SmithGGXCorrelated = 1 / (1 + lambda_v + lambda_l); 
	// V_SmithGGXCorrelated = G_SmithGGXCorrelated / (4.0f * NdotL * NdotV); 


	// This is the optimized version 
	//float alphaG2 = alphaG * alphaG;

	float a2 = roughness * roughness;

    float lambdaV = NdotL * sqrt((NdotV - a2 * NdotV) * NdotV + a2);
    float lambdaL = NdotV * sqrt((NdotL - a2 * NdotL) * NdotL + a2);
    float v = 0.5f / (lambdaV + lambdaL);

	return v;
}

float D_GGX(float NdotH, float roughness)
{
	// Divide by PI is apply later 

	float oneMinusNdotHSquared = 1.0 - NdotH * NdotH;
	float a = NdotH * roughness;
	float k = roughness / (oneMinusNdotHSquared + a * a);
	float d = k * k * M_1OVER_PI_F;
	return d;
}

#endif

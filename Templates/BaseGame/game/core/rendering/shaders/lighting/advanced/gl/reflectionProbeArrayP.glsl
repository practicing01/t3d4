#include "../../../gl/hlslCompat.glsl"
#include "../../../postFX/gl/postFx.glsl"
#include "../../../gl/torque.glsl"
#include "shadergen:/autogenConditioners.h"
#include "../../../gl/lighting.glsl"
#include "../../../gl/brdf.glsl"

#line 7

uniform sampler2D deferredBuffer;
uniform sampler2D colorBuffer;
uniform sampler2D matInfoBuffer;
uniform sampler2D BRDFTexture;

uniform vec4 rtParams0;
uniform vec4 vsFarPlane;
uniform mat4 cameraToWorld;
uniform vec3 eyePosWorld;

//cubemap arrays require all the same size. so shared mips# value
uniform float cubeMips;

uniform int numProbes;

uniform samplerCubeArray specularCubemapAR;
uniform samplerCubeArray irradianceCubemapAR;

#ifdef USE_SSAO_MASK
uniform sampler2D ssaoMask;
uniform vec4 rtParams6;
#endif

uniform vec4    inProbePosArray[MAX_PROBES];
uniform vec4    inRefPosArray[MAX_PROBES];
uniform mat4    worldToObjArray[MAX_PROBES];
uniform vec4    refScaleArray[MAX_PROBES];
uniform vec4    probeConfigData[MAX_PROBES];   //r,g,b/mode,radius,atten

#if DEBUGVIZ_CONTRIB
uniform vec4    probeContribColors[MAX_PROBES];
#endif

uniform int skylightCubemapIdx;

out vec4 OUT_col;

void main()
{
   //unpack normal and linear depth 
   vec4 normDepth = deferredUncondition(deferredBuffer, IN_uv0.xy);

   //create surface
   Surface surface = createSurface(normDepth, colorBuffer, matInfoBuffer, IN_uv0.xy, eyePosWorld, IN_wsEyeRay, cameraToWorld);

   //early out if emissive
   if (getFlag(surface.matFlag, 0))
   {
      OUT_col = vec4(surface.albedo, 0);
      return;
   }
   
   #ifdef USE_SSAO_MASK
      float ssao =  1.0 - texture( ssaoMask, viewportCoordToRenderTarget( IN_uv0.xy, rtParams6 ) ).r;
      surface.ao = min(surface.ao, ssao);  
   #endif


   float alpha = 1;

#if SKYLIGHT_ONLY == 0
   int i = 0;
   float blendFactor[MAX_PROBES];
   float blendSum = 0;
   float blendFacSum = 0;
   float invBlendSum = 0;
   float probehits = 0;
   //Set up our struct data
   float contribution[MAX_PROBES];

   if (alpha > 0)
   {
      //Process prooooobes
      for (i = 0; i < numProbes; ++i)
      {
         contribution[i] = 0;

         if (probeConfigData[i].r == 0) //box
         {
            contribution[i] = defineBoxSpaceInfluence(surface.P, worldToObjArray[i], probeConfigData[i].b);
            if (contribution[i]>0.0)
               probehits++;
         }
         else if (probeConfigData[i].r == 1) //sphere
         {
            contribution[i] = defineSphereSpaceInfluence(surface.P, inProbePosArray[i].xyz, probeConfigData[i].g);
            if (contribution[i]>0.0)
               probehits++;
         }

         contribution[i] = max(contribution[i],0);

         blendSum += contribution[i];
         invBlendSum += (1.0f - contribution[i]);
      }
      // Weight0 = normalized NDF, inverted to have 1 at center, 0 at boundary.
	   // And as we invert, we need to divide by Num-1 to stay normalized (else sum is > 1). 
	   // respect constraint B.
	   // Weight1 = normalized inverted NDF, so we have 1 at center, 0 at boundary
	   // and respect constraint A.
      
      if (probehits > 1.0)
	   {
         for (i = 0; i < numProbes; i++)
         {
               blendFactor[i] = ((contribution[i] / blendSum)) / probehits;
               blendFactor[i] *= ((contribution[i]) / invBlendSum);
               blendFactor[i] = saturate(blendFactor[i]);
               blendFacSum += blendFactor[i];
         }

         // Normalize blendVal
         if (blendFacSum == 0.0f) // Possible with custom weight
         {
               blendFacSum = 1.0f;
         }

         float invBlendSumWeighted = 1.0f / blendFacSum;
         for (i = 0; i < numProbes; ++i)
         {
               blendFactor[i] *= invBlendSumWeighted;
               contribution[i] *= blendFactor[i];
         }
      }
      
#if DEBUGVIZ_ATTENUATION == 1
      float contribAlpha = 1;
      for (i = 0; i < numProbes; ++i)
      {
         contribAlpha -= contribution[i];
      }

      OUT_col = vec4(1 - contribAlpha, 1 - contribAlpha, 1 - contribAlpha, 1);
      return;
#endif

#if DEBUGVIZ_CONTRIB == 1
      vec3 finalContribColor = vec3(0, 0, 0);
      float contribAlpha = 1;
      for (i = 0; i < numProbes; ++i)
      {
         finalContribColor += contribution[i] *probeContribColors[i].rgb;
         contribAlpha -= contribution[i];
      }

      //Skylight coloration for anything not covered by probes above
      if(skylightCubemapIdx != -1)
      	finalContribColor += vec3(0, 1, 0) * contribAlpha;

      OUT_col = vec4(finalContribColor, 1);
      return;
#endif
   }
#endif

   vec3 irradiance = vec3(0, 0, 0);
   vec3 specular = vec3(0, 0, 0);

   // Radiance (Specular)
#if DEBUGVIZ_SPECCUBEMAP == 0
   float lod = roughnessToMipLevel(surface.roughness, cubeMips);
#elif DEBUGVIZ_SPECCUBEMAP == 1
   float lod = 0;
#endif

#if SKYLIGHT_ONLY == 0
   for (i = 0; i < numProbes; ++i)
   {
      float contrib = contribution[i];
      if (contrib > 0.0f)
      {
         float cubemapIdx = probeConfigData[i].a;
         vec3 dir = boxProject(surface.P, surface.R, worldToObjArray[i], refScaleArray[i].xyz, inRefPosArray[i].xyz);

         irradiance += textureLod(irradianceCubemapAR, vec4(dir, cubemapIdx), 0).xyz * contrib;
         specular += textureLod(specularCubemapAR, vec4(dir, cubemapIdx), lod).xyz * contrib;
         alpha -= contrib;
      }
   }
#endif

   if (skylightCubemapIdx != -1 && alpha > 0.001)
   {
      irradiance = lerp(irradiance,textureLod(irradianceCubemapAR, vec4(surface.R, skylightCubemapIdx), 0).xyz,alpha);
      specular = lerp(specular,textureLod(specularCubemapAR, vec4(surface.R, skylightCubemapIdx), lod).xyz,alpha);
   }

#if DEBUGVIZ_SPECCUBEMAP == 1 && DEBUGVIZ_DIFFCUBEMAP == 0
   OUT_col = vec4(specular, 1);
   return;
#elif DEBUGVIZ_DIFFCUBEMAP == 1
   OUT_col = vec4(irradiance, 1);
   return;
#endif


   //energy conservation
   vec3 F = FresnelSchlickRoughness(surface.NdotV, surface.f0, surface.roughness);
   vec3 kD = 1.0f - F;
   kD *= 1.0f - surface.metalness;

   float dfgNdotV = max( surface.NdotV , 0.0009765625f ); //0.5f/512.0f (512 is size of dfg/brdf lookup tex)
   vec2 envBRDF = textureLod(BRDFTexture, vec2(dfgNdotV, surface.roughness),0).rg;
   specular *= F * envBRDF.x + surface.f90 * envBRDF.y;
   irradiance *= kD * surface.baseColor.rgb;

   //AO
   irradiance *= surface.ao;
   specular *= computeSpecOcclusion(surface.NdotV, surface.ao, surface.roughness);

   //http://marmosetco.tumblr.com/post/81245981087
   float horizonOcclusion = 1.3;
   float horizon = saturate( 1 + horizonOcclusion * dot(surface.R, surface.N));
   horizon *= horizon;

   OUT_col = vec4(irradiance + specular, 0);//alpha writes disabled
}

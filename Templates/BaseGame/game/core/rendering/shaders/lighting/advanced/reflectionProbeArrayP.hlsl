#include "../../postFX/postFx.hlsl"
#include "../../shaderModel.hlsl"
#include "../../shaderModelAutoGen.hlsl"
#include "../../lighting.hlsl"
#include "../../brdf.hlsl"

TORQUE_UNIFORM_SAMPLER2D(deferredBuffer, 0);
TORQUE_UNIFORM_SAMPLER2D(colorBuffer, 1);
TORQUE_UNIFORM_SAMPLER2D(matInfoBuffer, 2);
TORQUE_UNIFORM_SAMPLER2D(BRDFTexture, 3);

uniform float4 rtParams0;
uniform float4 vsFarPlane;
uniform float4x4 cameraToWorld;
uniform float3 eyePosWorld;

//cubemap arrays require all the same size. so shared mips# value
uniform float cubeMips;

uniform int numProbes;

TORQUE_UNIFORM_SAMPLERCUBEARRAY(specularCubemapAR, 4);
TORQUE_UNIFORM_SAMPLERCUBEARRAY(irradianceCubemapAR, 5);

#ifdef USE_SSAO_MASK
TORQUE_UNIFORM_SAMPLER2D(ssaoMask, 6);
uniform float4 rtParams6;
#endif

uniform float4    inProbePosArray[MAX_PROBES];
uniform float4    inRefPosArray[MAX_PROBES];
uniform float4x4  worldToObjArray[MAX_PROBES];
uniform float4    refScaleArray[MAX_PROBES];
uniform float4    probeConfigData[MAX_PROBES];   //r,g,b/mode,radius,atten

#if DEBUGVIZ_CONTRIB
uniform float4    probeContribColors[MAX_PROBES];
#endif

uniform int skylightCubemapIdx;

float4 main(PFXVertToPix IN) : SV_TARGET
{
   //unpack normal and linear depth 
   float4 normDepth = TORQUE_DEFERRED_UNCONDITION(deferredBuffer, IN.uv0.xy);

   //create surface
   Surface surface = createSurface(normDepth, TORQUE_SAMPLER2D_MAKEARG(colorBuffer),TORQUE_SAMPLER2D_MAKEARG(matInfoBuffer),
      IN.uv0.xy, eyePosWorld, IN.wsEyeRay, cameraToWorld);

   //early out if emissive
   if (getFlag(surface.matFlag, 0))
   {
      return float4(surface.albedo, 0);
   }

   #ifdef USE_SSAO_MASK
      float ssao =  1.0 - TORQUE_TEX2D( ssaoMask, viewportCoordToRenderTarget( IN.uv0.xy, rtParams6 ) ).r;
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

      return float4(1 - contribAlpha, 1 - contribAlpha, 1 - contribAlpha, 1);
#endif

#if DEBUGVIZ_CONTRIB == 1
      float3 finalContribColor = float3(0, 0, 0);
      float contribAlpha = 1;
      for (i = 0; i < numProbes; ++i)
      {
         finalContribColor += contribution[i] *probeContribColors[i].rgb;
         contribAlpha -= contribution[i];
      }

      //Skylight coloration for anything not covered by probes above
      if(skylightCubemapIdx != -1)
         finalContribColor += float3(0, 1, 0) * contribAlpha;

      return float4(finalContribColor, 1);
#endif
   }
#endif

   float3 irradiance = float3(0, 0, 0);
   float3 specular = float3(0, 0, 0);

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
         int cubemapIdx = probeConfigData[i].a;
         float3 dir = boxProject(surface.P, surface.R, worldToObjArray[i], refScaleArray[i].xyz, inRefPosArray[i].xyz);

         irradiance += TORQUE_TEXCUBEARRAYLOD(irradianceCubemapAR, dir, cubemapIdx, 0).xyz * contrib;
         specular += TORQUE_TEXCUBEARRAYLOD(specularCubemapAR, dir, cubemapIdx, lod).xyz * contrib;
         alpha -= contrib;
      }
   }
#endif

   if(skylightCubemapIdx != -1 && alpha >= 0.001)
   {
      irradiance = lerp(irradiance,TORQUE_TEXCUBEARRAYLOD(irradianceCubemapAR, surface.R, skylightCubemapIdx, 0).xyz,alpha);
      specular = lerp(specular,TORQUE_TEXCUBEARRAYLOD(specularCubemapAR, surface.R, skylightCubemapIdx, lod).xyz,alpha);
   }

#if DEBUGVIZ_SPECCUBEMAP == 1 && DEBUGVIZ_DIFFCUBEMAP == 0
   return float4(specular, 1);
#elif DEBUGVIZ_DIFFCUBEMAP == 1
   return float4(irradiance, 1);
#endif

   //energy conservation
   float3 F = FresnelSchlickRoughness(surface.NdotV, surface.f0, surface.roughness);
   float3 kD = 1.0f - F;
   kD *= 1.0f - surface.metalness;

   float dfgNdotV = max( surface.NdotV , 0.0009765625f ); //0.5f/512.0f (512 is size of dfg/brdf lookup tex)
   float2 envBRDF = TORQUE_TEX2DLOD(BRDFTexture, float4(dfgNdotV, surface.roughness,0,0)).rg;
   specular *= F * envBRDF.x + surface.f90 * envBRDF.y;
   irradiance *= kD * surface.baseColor.rgb;

   //AO
   irradiance *= surface.ao;
   specular *= computeSpecOcclusion(surface.NdotV, surface.ao, surface.roughness);

   //http://marmosetco.tumblr.com/post/81245981087
   float horizonOcclusion = 1.3;
   float horizon = saturate( 1 + horizonOcclusion * dot(surface.R, surface.N));
   horizon *= horizon;

   return float4((irradiance + specular) * horizon, 0);//alpha writes disabled
}

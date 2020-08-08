//-----------------------------------------------------------------------------
// Copyright (c) 2012 GarageGames, LLC
//
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

#include "platform/platform.h"
#include "lighting/advanced/advancedLightBinManager.h"

#include "lighting/advanced/advancedLightManager.h"
#include "lighting/advanced/advancedLightBufferConditioner.h"
#include "lighting/shadowMap/shadowMapManager.h"
#include "lighting/shadowMap/shadowMapPass.h"
#include "lighting/shadowMap/lightShadowMap.h"
#include "lighting/common/lightMapParams.h"
#include "renderInstance/renderDeferredMgr.h"
#include "gfx/gfxTransformSaver.h"
#include "scene/sceneManager.h"
#include "scene/sceneRenderState.h"
#include "materials/materialManager.h"
#include "materials/sceneData.h"
#include "core/util/safeDelete.h"
#include "core/util/rgb2luv.h"
#include "gfx/gfxDebugEvent.h"
#include "math/util/matrixSet.h"
#include "console/consoleTypes.h"
#include "gfx/gfxTextureManager.h"

const RenderInstType AdvancedLightBinManager::RIT_LightInfo( "specularLighting" );
const String AdvancedLightBinManager::smBufferName( "specularLighting" );

ShadowFilterMode AdvancedLightBinManager::smShadowFilterMode = ShadowFilterMode_SoftShadowHighQuality;
bool AdvancedLightBinManager::smPSSMDebugRender = false;
bool AdvancedLightBinManager::smUseSSAOMask = false;
bool AdvancedLightBinManager::smDiffuseLightViz = false;
bool AdvancedLightBinManager::smSpecularLightViz = false;
bool AdvancedLightBinManager::smDetailLightingViz = false;

S32 AdvancedLightBinManager::smMaximumNumOfLights = -1;
bool AdvancedLightBinManager::smUseLightFade = false;
F32 AdvancedLightBinManager::smLightFadeStart = 50;
F32 AdvancedLightBinManager::smLightFadeEnd = 75;

ImplementEnumType( ShadowFilterMode,
   "The shadow filtering modes for Advanced Lighting shadows.\n"
   "@ingroup AdvancedLighting" )

   { ShadowFilterMode_None, "None", 
      "@brief Simple point sampled filtering.\n"
      "This is the fastest and lowest quality mode." },

   { ShadowFilterMode_SoftShadow, "SoftShadow", 
      "@brief A variable tap rotated poisson disk soft shadow filter.\n"
      "It performs 4 taps to classify the point as in shadow, out of shadow, or along a "
      "shadow edge.  Samples on the edge get an additional 8 taps to soften them." },

   { ShadowFilterMode_SoftShadowHighQuality, "SoftShadowHighQuality", 
      "@brief A 12 tap rotated poisson disk soft shadow filter.\n"
      "It performs all the taps for every point without any early rejection." },

EndImplementEnumType;

// NOTE: The order here matches that of the LightInfo::Type enum.
const String AdvancedLightBinManager::smLightMatNames[] =
{
   "AL_PointLightMaterial",   // LightInfo::Point
   "AL_SpotLightMaterial",    // LightInfo::Spot
   "AL_VectorLightMaterial",  // LightInfo::Vector
   "",                        // LightInfo::Ambient
};

// NOTE: The order here matches that of the LightInfo::Type enum.
const GFXVertexFormat* AdvancedLightBinManager::smLightMatVertex[] =
{
   getGFXVertexFormat<AdvancedLightManager::LightVertex>(), // LightInfo::Point
   getGFXVertexFormat<AdvancedLightManager::LightVertex>(), // LightInfo::Spot
   getGFXVertexFormat<FarFrustumQuadVert>(),                // LightInfo::Vector
   NULL,                                                    // LightInfo::Ambient
};

// NOTE: The order here matches that of the ShadowType enum.
const String AdvancedLightBinManager::smShadowTypeMacro[] =
{
   "", // ShadowType_Spot
   "", // ShadowType_PSSM,
   "", // ShadowType_Paraboloid,
   "SHADOW_DUALPARABOLOID_SINGLE_PASS",   // ShadowType_DualParaboloidSinglePass,
   "SHADOW_DUALPARABOLOID",               // ShadowType_DualParaboloid,
   "SHADOW_CUBE",                         // ShadowType_CubeMap,
};

AdvancedLightBinManager::RenderSignal &AdvancedLightBinManager::getRenderSignal()
{
   static RenderSignal theSignal;
   return theSignal;
}

IMPLEMENT_CONOBJECT(AdvancedLightBinManager);

ConsoleDocClass( AdvancedLightBinManager,
   "@brief Rendering Manager responsible for lighting, shadows, and global variables affecing both.\n\n"

   "Should not be exposed to TorqueScript as a game object, meant for internal use only\n\n"

   "@ingroup Lighting"
);

AdvancedLightBinManager::AdvancedLightBinManager( AdvancedLightManager *lm /* = NULL */, 
                                                 ShadowMapManager *sm /* = NULL */, 
                                                 GFXFormat lightBufferFormat /* = GFXFormatR8G8B8A8 */ )
   :  RenderBinManager( RIT_LightInfo, 1.0f, 1.0f ), 
      mNumLightsCulled(0), 
      mLightManager(lm), 
      mShadowManager(sm)
{
   mMRTLightmapsDuringDeferred = true;

   Con::NotifyDelegate callback( this, &AdvancedLightBinManager::_deleteLightMaterials );
   Con::addVariableNotify( "$pref::Shadows::filterMode", callback );
   Con::addVariableNotify( "$AL::PSSMDebugRender", callback );
   Con::addVariableNotify( "$AL::UseSSAOMask", callback );
   Con::addVariableNotify( "$AL::DiffuseLightViz", callback);
   Con::addVariableNotify( "$AL::SpecularLightViz", callback);
   Con::addVariableNotify( "$AL::DetailLightingViz", callback);
}


AdvancedLightBinManager::~AdvancedLightBinManager()
{
   _deleteLightMaterials();

   Con::NotifyDelegate callback( this, &AdvancedLightBinManager::_deleteLightMaterials );
   Con::removeVariableNotify( "$pref::shadows::filterMode", callback );
   Con::removeVariableNotify( "$AL::PSSMDebugRender", callback );
   Con::removeVariableNotify( "$AL::UseSSAOMask", callback );
   Con::removeVariableNotify( "$AL::DiffuseLightViz", callback);
   Con::removeVariableNotify( "$AL::SpecularLightViz", callback);
   Con::removeVariableNotify( "$AL::DetailLightingViz", callback);
}

void AdvancedLightBinManager::consoleInit()
{
   Parent::consoleInit();

   Con::addVariable( "$pref::shadows::filterMode", 
      TYPEID<ShadowFilterMode>(), &smShadowFilterMode,
      "The filter mode to use for shadows.\n"
      "@ingroup AdvancedLighting\n" );

   Con::addVariable( "$AL::UseSSAOMask", TypeBool, &smUseSSAOMask,
      "Used by the SSAO PostEffect to toggle the sampling of ssaomask "
      "texture by the light shaders.\n"
      "@ingroup AdvancedLighting\n" );

   Con::addVariable( "$AL::PSSMDebugRender", TypeBool, &smPSSMDebugRender,
      "Enables debug rendering of the PSSM shadows.\n"
      "@ingroup AdvancedLighting\n" );

   Con::addVariable("$AL::DiffuseLightViz", TypeBool, &smDiffuseLightViz,
      "Enables debug rendering of the PSSM shadows.\n"
      "@ingroup AdvancedLighting\n");

   Con::addVariable("$AL::SpecularLightViz", TypeBool, &smSpecularLightViz,
      "Enables debug rendering of the PSSM shadows.\n"
      "@ingroup AdvancedLighting\n");

   Con::addVariable("$AL::DetailLightingViz", TypeBool, &smDetailLightingViz,
      "Enables debug rendering of the PSSM shadows.\n"
      "@ingroup AdvancedLighting\n");

   Con::addVariable("$pref::maximumNumOfLights",
      TypeS32, &smMaximumNumOfLights,
      "The maximum number of local lights that can be rendered at a time. If set to -1, then no limit.\n");

   Con::addVariable("$pref::useLightFade", TypeBool, &smUseLightFade, "Indicates if local lights should utilize the distance-based object fadeout logic.\n");
   Con::addVariable("$pref::lightFadeStart", TypeF32, &smLightFadeStart, "Distance at which light fading begins if $pref::useLightFade is on.\n");
   Con::addVariable("$pref::lightFadeEnd", TypeF32, &smLightFadeEnd, "Distance at which light fading should have fully faded if $pref::useLightFade is on.\n");

}

bool AdvancedLightBinManager::setTargetSize(const Point2I &newTargetSize)
{
   /*bool ret = Parent::setTargetSize( newTargetSize );

   // We require the viewport to match the default.
   mNamedTarget.setViewport( GFX->getViewport() );

   return ret;*/
   return true;
}

bool AdvancedLightBinManager::_updateTargets()
{
  /* PROFILE_SCOPE(AdvancedLightBinManager_updateTargets);

   bool ret = Parent::_updateTargets();

   mDiffuseLightingTarget = NamedTexTarget::find("diffuseLighting");
   if (mDiffuseLightingTarget.isValid())
   {
      mDiffuseLightingTex = mDiffuseLightingTarget->getTexture();

      for (U32 i = 0; i < mTargetChainLength; i++)
         mTargetChain[i]->attachTexture(GFXTextureTarget::Color1, mDiffuseLightingTex);
   }

   GFX->finalizeReset();

   return ret;*/
   return true;
}

void AdvancedLightBinManager::addLight( LightInfo *light )
{
   // Get the light type.
   const LightInfo::Type lightType = light->getType();

   AssertFatal(   lightType == LightInfo::Point || 
                  lightType == LightInfo::Spot, "Bogus light type." );

   // Find a shadow map for this light, if it has one
   ShadowMapParams *lsp = light->getExtended<ShadowMapParams>();
   LightShadowMap *lsm = lsp->getShadowMap();

   // Get the right shadow type.
   ShadowType shadowType = ShadowType_None;
   if (  light->getCastShadows() &&  
         lsm && lsm->hasShadowTex() &&
         !ShadowMapPass::smDisableShadows )
      shadowType = lsm->getShadowType();

   // Add the entry
   LightBinEntry lEntry;
   lEntry.lightInfo = light;
   lEntry.shadowMap = lsm;
   lEntry.lightMaterial = _getLightMaterial( lightType, shadowType, lsp->hasCookieTex() );

   if( lightType == LightInfo::Spot )
      lEntry.vertBuffer = mLightManager->getConeMesh( lEntry.numPrims, lEntry.primBuffer );
   else
      lEntry.vertBuffer = mLightManager->getSphereMesh( lEntry.numPrims, lEntry.primBuffer );

   // If it's a point light, push front, spot 
   // light, push back. This helps batches.
   Vector<LightBinEntry> &curBin = mLightBin;
   if ( light->getType() == LightInfo::Point )
      curBin.push_front( lEntry );
   else
      curBin.push_back( lEntry );
}

void AdvancedLightBinManager::clearAllLights()
{
   Con::setIntVariable("lightMetrics::activeLights", mLightBin.size());
   Con::setIntVariable("lightMetrics::culledLights", mNumLightsCulled);

   mLightBin.clear();
   mNumLightsCulled = 0;
}

S32 QSORT_CALLBACK AdvancedLightBinManager::_lightScoreCmp(const LightBinEntry* a, const  LightBinEntry* b)
{
   F32 diff = a->lightInfo->getScore() - b->lightInfo->getScore();
   return diff > 0 ? 1 : diff < 0 ? -1 : 0;
}

void AdvancedLightBinManager::_scoreLights(const MatrixF& cameraTrans)
{
   PROFILE_SCOPE(AdvancedLightBinManager_scoreLights);

   if (!LIGHTMGR)
      return;

   // Get all the lights.
   const Point3F lumDot(0.2125f, 0.7154f, 0.0721f);

   for (LightBinIterator itr = mLightBin.begin(); itr != mLightBin.end(); itr++)
   {
      // Get the light.
      LightBinEntry& light = *itr;

      F32 luminace = 0.0f;
      F32 weight = 0.0f;
      F32 score = 0.0f;

      const bool isSpot = light.lightInfo->getType() == LightInfo::Spot;
      const bool isPoint = light.lightInfo->getType() == LightInfo::Point;

      if (isPoint || isSpot)
      {
         Point3F distVec = light.lightInfo->getPosition() - cameraTrans.getPosition();
         F32 dist = distVec.len();

         score = dist;// light.lightInfo->getRange().x / mMax(dist, 1.0f);

         // Get the luminocity.
         luminace = mDot(light.lightInfo->getColor(), lumDot) * light.lightInfo->getBrightness();

         weight = light.lightInfo->getPriority();

         //Distance fading test
         if (smUseLightFade)
         {
            if (dist > smLightFadeStart)
            {
               F32 brightness = light.lightInfo->getBrightness();

               float fadeOutAmt = (dist - smLightFadeStart) / (smLightFadeEnd - smLightFadeStart);
               fadeOutAmt = 1 - fadeOutAmt;

               light.lightInfo->setFadeAmount(fadeOutAmt);
            }
         }
         else
         {
            light.lightInfo->setFadeAmount(1.0);
         }
      }

      light.lightInfo->setScore(score * weight - luminace);
   }

   // Sort them!
   mLightBin.sort(_lightScoreCmp);
}

void AdvancedLightBinManager::render( SceneRenderState *state )
{
   PROFILE_SCOPE( AdvancedLightManager_Render );

   // Take a look at the SceneRenderState and see if we should skip drawing the pre-pass
   if( state->disableAdvancedLightingBins() )
      return;

   // Automagically save & restore our viewport and transforms.
   GFXTransformSaver saver;

   if( !mLightManager )
      return;

   // Get the sunlight. If there's no sun, and no lights in the bins, no draw
   LightInfo *sunLight = mLightManager->getSpecialLight( LightManager::slSunLightType, false );
   GFXDEBUGEVENT_SCOPE( AdvancedLightBinManager_Render, ColorI::RED );

   // Tell the superclass we're about to render
   //if ( !_onPreRender( state ) )
   //   return;

   NamedTexTargetRef sceneColorTargetRef = NamedTexTarget::find("AL_FormatToken");
   if (sceneColorTargetRef.isNull())
      return;

   GFXTextureTargetRef lightingTargetRef = GFX->allocRenderToTextureTarget();

   if (lightingTargetRef.isNull())
      return;

   //Do a quick pass to update our probes if they're dirty
   //PROBEMGR->updateDirtyProbes();

   lightingTargetRef->attachTexture(GFXTextureTarget::Color0, sceneColorTargetRef->getTexture());

   GFX->pushActiveRenderTarget();
   GFX->setActiveRenderTarget(lightingTargetRef);

   GFX->setViewport(sceneColorTargetRef->getViewport());

   // Restore transforms
   MatrixSet &matrixSet = getRenderPass()->getMatrixSet();
   matrixSet.restoreSceneViewProjection();

   // Set up the SG Data
   SceneData sgData;
   sgData.init( state );

   // There are cases where shadow rendering is disabled.
   const bool disableShadows = /*state->isReflectPass() || */ShadowMapPass::smDisableShadows;

   // Pick the right material for rendering the sunlight... we only
   // cast shadows when its enabled and we're not in a reflection.
   LightMaterialInfo *vectorMatInfo;
   if (  sunLight &&
         sunLight->getCastShadows() &&
         !disableShadows &&
         sunLight->getExtended<ShadowMapParams>() )
      vectorMatInfo = _getLightMaterial( LightInfo::Vector, ShadowType_PSSM, false );
   else
      vectorMatInfo = _getLightMaterial( LightInfo::Vector, ShadowType_None, false );

   // Initialize and set the per-frame parameters after getting
   // the vector light material as we use lazy creation.
   _setupPerFrameParameters( state );
   
   // Draw sunlight/ambient
   if ( sunLight && vectorMatInfo )
   {
      GFXDEBUGEVENT_SCOPE( AdvancedLightBinManager_Render_Sunlight, ColorI::RED );

      // Set up SG data
      setupSGData( sgData, state, sunLight );
      vectorMatInfo->setLightParameters( sunLight, state );

      // Set light holds the active shadow map.       
      mShadowManager->setLightShadowMapForLight( sunLight );

      // Set geometry
      GFX->setVertexBuffer( mFarFrustumQuadVerts );
      GFX->setPrimitiveBuffer( NULL );

      vectorMatInfo->matInstance->mSpecialLight = true;

      // Render the material passes
      while( vectorMatInfo->matInstance->setupPass( state, sgData ) )
      {
         vectorMatInfo->matInstance->setSceneInfo( state, sgData );
         vectorMatInfo->matInstance->setTransforms( matrixSet, state );
         GFX->drawPrimitive( GFXTriangleStrip, 0, 2 );
      }
   }

   const Frustum& frustum = state->getCameraFrustum();
   MatrixF invCam(frustum.getTransform());
   invCam.inverse();

   const MatrixF& cameraTrans = frustum.getTransform();

   if(smUseLightFade || smMaximumNumOfLights != -1)
      _scoreLights(cameraTrans);

   S32 lightCount = 0;

   // Blend the lights in the bin to the light buffer
   for( LightBinIterator itr = mLightBin.begin(); itr != mLightBin.end(); itr++ )
   {
      if (smMaximumNumOfLights != -1 && lightCount >= smMaximumNumOfLights)
         break;

      lightCount++;

      LightBinEntry& curEntry = *itr;
      LightInfo *curLightInfo = curEntry.lightInfo;
      if (curEntry.lightInfo->getType() >= LightInfo::Vector)
         continue;
      LightMaterialInfo *curLightMat = curEntry.lightMaterial;
      const U32 numPrims = curEntry.numPrims;
      const U32 numVerts = curEntry.vertBuffer->mNumVerts;

      ShadowMapParams *lsp = curLightInfo->getExtended<ShadowMapParams>();

      // Skip lights which won't affect the scene.
      if (!curLightMat || curLightInfo->getBrightness() * curLightInfo->getFadeAmount() <= 0.001f)
         continue;

      GFXDEBUGEVENT_SCOPE( AdvancedLightBinManager_Render_Light, ColorI::RED );

      setupSGData( sgData, state, curLightInfo );
      curLightMat->setLightParameters( curLightInfo, state );
      mShadowManager->setLightShadowMap( curEntry.shadowMap );

      // Set geometry
      GFX->setVertexBuffer( curEntry.vertBuffer );
      GFX->setPrimitiveBuffer( curEntry.primBuffer );

      lsp->getOcclusionQuery()->begin();

      curLightMat->matInstance->mSpecialLight = false;

      // Render the material passes
      while( curLightMat->matInstance->setupPass( state, sgData ) )
      {
         // Set transforms
         matrixSet.setWorld(*sgData.objTrans);
         curLightMat->matInstance->setTransforms(matrixSet, state);
         curLightMat->matInstance->setSceneInfo(state, sgData);

         if(curEntry.primBuffer)
            GFX->drawIndexedPrimitive(GFXTriangleList, 0, 0, numVerts, 0, numPrims);
         else
            GFX->drawPrimitive(GFXTriangleList, 0, numPrims);
      }

      lsp->getOcclusionQuery()->end();
   }

   // Set NULL for active shadow map (so nothing gets confused)
   mShadowManager->setLightShadowMap(NULL);
   GFX->setVertexBuffer( NULL );
   GFX->setPrimitiveBuffer( NULL );

   // Fire off a signal to let others know that light-bin rendering is ending now
   getRenderSignal().trigger(state, this);

   // Finish up the rendering
   //_onPostRender();
   GFX->popActiveRenderTarget();
}

AdvancedLightBinManager::LightMaterialInfo* AdvancedLightBinManager::_getLightMaterial(   LightInfo::Type lightType, 
                                                                                          ShadowType shadowType, 
                                                                                          bool useCookieTex )
{
   PROFILE_SCOPE( AdvancedLightBinManager_GetLightMaterial );

   // Build the key.
   const LightMatKey key( lightType, shadowType, useCookieTex );

   // See if we've already built this one.
   LightMatTable::Iterator iter = mLightMaterials.find( key );
   if ( iter != mLightMaterials.end() )
      return iter->value;
   
   // If we got here we need to build a material for 
   // this light+shadow combination.

   LightMaterialInfo *info = NULL;

   // First get the light material name and make sure
   // this light has a material in the first place.
   const String &lightMatName = smLightMatNames[ lightType ];
   if ( lightMatName.isNotEmpty() )
   {
      Vector<GFXShaderMacro> shadowMacros;

      // Setup the shadow type macros for this material.
      if ( shadowType == ShadowType_None )
         shadowMacros.push_back( GFXShaderMacro( "NO_SHADOW" ) );
      else
      {
         shadowMacros.push_back( GFXShaderMacro( smShadowTypeMacro[ shadowType ] ) );

         // Do we need to do shadow filtering?
         if ( smShadowFilterMode != ShadowFilterMode_None )
         {
            shadowMacros.push_back( GFXShaderMacro( "SOFTSHADOW" ) );
           
            const F32 SM = GFX->getPixelShaderVersion();
            if ( SM >= 3.0f && smShadowFilterMode == ShadowFilterMode_SoftShadowHighQuality )
               shadowMacros.push_back( GFXShaderMacro( "SOFTSHADOW_HIGH_QUALITY" ) );
         }
      }
   
      if ( useCookieTex )
         shadowMacros.push_back( GFXShaderMacro( "USE_COOKIE_TEX" ) );

      // Its safe to add the PSSM debug macro to all the materials.
      if ( smPSSMDebugRender )
         shadowMacros.push_back( GFXShaderMacro( "PSSM_DEBUG_RENDER" ) );

      if( smDiffuseLightViz )
         shadowMacros.push_back(GFXShaderMacro("DIFFUSE_LIGHT_VIZ"));
      else if (smSpecularLightViz)
         shadowMacros.push_back(GFXShaderMacro("SPECULAR_LIGHT_VIZ"));
      else if (smDetailLightingViz)
         shadowMacros.push_back(GFXShaderMacro("DETAIL_LIGHTING_VIZ"));

      // Now create the material info object.
      info = new LightMaterialInfo( lightMatName, smLightMatVertex[ lightType ], shadowMacros );
   }

   // Push this into the map and return it.
   mLightMaterials.insertUnique( key, info );
   return info;
}

void AdvancedLightBinManager::_deleteLightMaterials()
{
   LightMatTable::Iterator iter = mLightMaterials.begin();
   for ( ; iter != mLightMaterials.end(); iter++ )
      delete iter->value;
      
   mLightMaterials.clear();
}

void AdvancedLightBinManager::_setupPerFrameParameters( const SceneRenderState *state )
{
   PROFILE_SCOPE( AdvancedLightBinManager_SetupPerFrameParameters );
   const Frustum &frustum = state->getCameraFrustum();

   MatrixF invCam( frustum.getTransform() );
   invCam.inverse();

   const Point3F *wsFrustumPoints = frustum.getPoints();
   const Point3F& cameraPos = frustum.getPosition();

   // Perform a camera offset.  We need to manually perform this offset on the sun (or vector) light's
   // polygon, which is at the far plane.
   Point3F cameraOffsetPos = cameraPos;

   // Now build the quad for drawing full-screen vector light
   // passes.... this is a volatile VB and updates every frame.
   FarFrustumQuadVert verts[4];
   {
      verts[0].point.set(wsFrustumPoints[Frustum::FarTopLeft] - cameraPos);
      invCam.mulP(wsFrustumPoints[Frustum::FarTopLeft], &verts[0].normal);
      verts[0].texCoord.set(-1.0, 1.0);
      verts[0].tangent.set(wsFrustumPoints[Frustum::FarTopLeft] - cameraOffsetPos);

      verts[1].point.set(wsFrustumPoints[Frustum::FarTopRight] - cameraPos);
      invCam.mulP(wsFrustumPoints[Frustum::FarTopRight], &verts[1].normal);
      verts[1].texCoord.set(1.0, 1.0);
      verts[1].tangent.set(wsFrustumPoints[Frustum::FarTopRight] - cameraOffsetPos);

      verts[2].point.set(wsFrustumPoints[Frustum::FarBottomLeft] - cameraPos);
      invCam.mulP(wsFrustumPoints[Frustum::FarBottomLeft], &verts[2].normal);
      verts[2].texCoord.set(-1.0, -1.0);
      verts[2].tangent.set(wsFrustumPoints[Frustum::FarBottomLeft] - cameraOffsetPos);

      verts[3].point.set(wsFrustumPoints[Frustum::FarBottomRight] - cameraPos);
      invCam.mulP(wsFrustumPoints[Frustum::FarBottomRight], &verts[3].normal);
      verts[3].texCoord.set(1.0, -1.0);
      verts[3].tangent.set(wsFrustumPoints[Frustum::FarBottomRight] - cameraOffsetPos);
   }
   mFarFrustumQuadVerts.set( GFX, 4 );
   dMemcpy( mFarFrustumQuadVerts.lock(), verts, sizeof( verts ) );
   mFarFrustumQuadVerts.unlock();

   PlaneF farPlane(wsFrustumPoints[Frustum::FarBottomLeft], wsFrustumPoints[Frustum::FarTopLeft], wsFrustumPoints[Frustum::FarTopRight]);
   PlaneF vsFarPlane(verts[0].normal, verts[1].normal, verts[2].normal);

   // Parameters calculated, assign them to the materials
   LightMatTable::Iterator iter = mLightMaterials.begin();
   for ( ; iter != mLightMaterials.end(); iter++ )
   {
      if ( iter->value )
         iter->value->setViewParameters(  frustum.getNearDist(), 
                                          frustum.getFarDist(), 
                                          frustum.getPosition(), 
                                          farPlane, 
                                          vsFarPlane);
   }
}

void AdvancedLightBinManager::setupSGData( SceneData &data, const SceneRenderState* state, LightInfo *light )
{
   PROFILE_SCOPE( AdvancedLightBinManager_setupSGData );

   data.lights[0] = light;
   data.ambientLightColor = state->getAmbientLightColor();
   data.objTrans = &MatrixF::Identity;

   if ( light )
   {
      if ( light->getType() == LightInfo::Point )
      {
         // The point light volume gets some flat spots along
         // the perimiter mostly visible in the constant and 
         // quadradic falloff modes.
         //
         // To account for them slightly increase the scale 
         // instead of greatly increasing the polycount.

         mLightMat = light->getTransform();
         mLightMat.scale( light->getRange() * 1.01f );
         data.objTrans = &mLightMat;
      }
      else if ( light->getType() == LightInfo::Spot )
      {
         mLightMat = light->getTransform();

         // Rotate it to face down the -y axis.
         MatrixF scaleRotateTranslate( EulerF( M_PI_F / -2.0f, 0.0f, 0.0f ) );

         // Calculate the radius based on the range and angle.
         F32 range = light->getRange().x;
         F32 radius = range * mSin( mDegToRad( light->getOuterConeAngle() ) * 0.5f );

         // NOTE: This fudge makes the cone a little bigger
         // to remove the facet egde of the cone geometry.
         radius *= 1.1f;

         // Use the scale to distort the cone to
         // match our radius and range.
         scaleRotateTranslate.scale( Point3F( radius, radius, range ) );

         // Apply the transform and set the position.
         mLightMat *= scaleRotateTranslate;
         mLightMat.setPosition( light->getPosition() );

         data.objTrans = &mLightMat;
      }
   }
}

void AdvancedLightBinManager::MRTLightmapsDuringDeferred( bool val )
{
   // Do not enable if the GFX device can't do MRT's
   if ( GFX->getNumRenderTargets() < 2 )
      val = false;

   if ( mMRTLightmapsDuringDeferred != val )
   {
      mMRTLightmapsDuringDeferred = val;

      // Reload materials to cause a feature recalculation on deferred materials
      if(mLightManager->isActive())
         MATMGR->flushAndReInitInstances();

      RenderDeferredMgr *deferred;
      if ( Sim::findObject( "AL_DeferredBin", deferred ) && deferred->getTargetTexture( 0 ) )
         deferred->updateTargets();
   }
}

AdvancedLightBinManager::LightMaterialInfo::LightMaterialInfo( const String &matName,
                                                               const GFXVertexFormat *vertexFormat,
                                                               const Vector<GFXShaderMacro> &macros )
:  matInstance(NULL),
   zNearFarInvNearFar(NULL),
   farPlane(NULL), 
   vsFarPlane(NULL),
   negFarPlaneDotEye(NULL),
   lightPosition(NULL), 
   lightDirection(NULL), 
   lightColor(NULL), 
   lightRange(NULL),
   lightInvSqrRange(NULL),
   lightAmbient(NULL),
   lightSpotParams(NULL)
{   
   Material *mat = MATMGR->getMaterialDefinitionByName( matName );
   if ( !mat )
      return;

   matInstance = new LightMatInstance( *mat );

   for ( U32 i=0; i < macros.size(); i++ )
      matInstance->addShaderMacro( macros[i].name, macros[i].value );

   matInstance->init( MATMGR->getDefaultFeatures(), vertexFormat );

   lightDirection = matInstance->getMaterialParameterHandle("$lightDirection");
   lightAmbient = matInstance->getMaterialParameterHandle("$lightAmbient");
   lightSpotParams = matInstance->getMaterialParameterHandle("$lightSpotParams");
   lightRange = matInstance->getMaterialParameterHandle("$lightRange");
   lightInvSqrRange = matInstance->getMaterialParameterHandle("$lightInvSqrRange");
   lightPosition = matInstance->getMaterialParameterHandle("$lightPosition");
   farPlane = matInstance->getMaterialParameterHandle("$farPlane");
   vsFarPlane = matInstance->getMaterialParameterHandle("$vsFarPlane");
   negFarPlaneDotEye = matInstance->getMaterialParameterHandle("$negFarPlaneDotEye");
   zNearFarInvNearFar = matInstance->getMaterialParameterHandle("$zNearFarInvNearFar");
   lightColor = matInstance->getMaterialParameterHandle("$lightColor");
   lightBrightness = matInstance->getMaterialParameterHandle("$lightBrightness");
}

AdvancedLightBinManager::LightMaterialInfo::~LightMaterialInfo()
{
   SAFE_DELETE(matInstance);
}

void AdvancedLightBinManager::LightMaterialInfo::setViewParameters(  const F32 _zNear, 
                                                                     const F32 _zFar, 
                                                                     const Point3F &_eyePos, 
                                                                     const PlaneF &_farPlane,
                                                                     const PlaneF &_vsFarPlane)
{
   MaterialParameters *matParams = matInstance->getMaterialParameters();

   matParams->setSafe( farPlane, *((const Point4F *)&_farPlane) );

   matParams->setSafe( vsFarPlane, *((const Point4F *)&_vsFarPlane) );

   if ( negFarPlaneDotEye->isValid() )
   {
      // -dot( farPlane, eyePos )
      const F32 negFarPlaneDotEyeVal = -( mDot( *((const Point3F *)&_farPlane), _eyePos ) + _farPlane.d );
      matParams->set( negFarPlaneDotEye, negFarPlaneDotEyeVal );
   }

   matParams->setSafe( zNearFarInvNearFar, Point4F( _zNear, _zFar, 1.0f / _zNear, 1.0f / _zFar ) );
}

void AdvancedLightBinManager::LightMaterialInfo::setLightParameters( const LightInfo *lightInfo, const SceneRenderState* renderState )
{
   MaterialParameters *matParams = matInstance->getMaterialParameters();

   matParams->setSafe( lightColor, lightInfo->getColor() );
   matParams->setSafe(lightBrightness, lightInfo->getBrightness() * lightInfo->getFadeAmount());

   switch( lightInfo->getType() )
   {
   case LightInfo::Vector:
      {
         matParams->setSafe( lightDirection, lightInfo->getDirection());
         matParams->setSafe( lightAmbient, renderState->getAmbientLightColor());
      }
      break;

   case LightInfo::Spot:
      {
         const F32 outerCone = lightInfo->getOuterConeAngle();
         const F32 innerCone = getMin(lightInfo->getInnerConeAngle(), outerCone);
         const F32 outerCos = mCos(mDegToRad(outerCone / 2.0f));
         const F32 innerCos = mCos(mDegToRad(innerCone / 2.0f));
         Point2F spotParams(outerCos,innerCos - outerCos); 

         matParams->setSafe( lightSpotParams, spotParams );
         matParams->setSafe( lightDirection, lightInfo->getDirection());
         matParams->setSafe( lightPosition, lightInfo->getPosition());

         const F32 radius = lightInfo->getRange().x;
         const F32 invSqrRadius = 1.0f / mSquared(radius);
         matParams->setSafe(lightRange, radius);
         matParams->setSafe(lightInvSqrRange, invSqrRadius);
      }
      break;

   case LightInfo::Point:
      {
         matParams->setSafe(lightPosition, lightInfo->getPosition());

         const F32 radius = lightInfo->getRange().x;
         const F32 invSqrRadius = 1.0f / (radius * radius);
         matParams->setSafe( lightRange, radius);
         matParams->setSafe( lightInvSqrRange, invSqrRadius);  
      }
      break;

   default:
      AssertFatal( false, "Bad light type!" );
      break;
   }
}

bool LightMatInstance::setupPass( SceneRenderState *state, const SceneData &sgData )
{
   // Go no further if the material failed to initialize properly.
   if (  !mProcessedMaterial || 
         mProcessedMaterial->getNumPasses() == 0 )
      return false;

   U32 reflectStatus = Base;
   if (state->isReflectPass())
      reflectStatus = Reflecting;

   // Fetch the lightmap params
   const LightMapParams *lmParams = sgData.lights[0]->getExtended<LightMapParams>();
   
   // If no Lightmap params, let parent handle it
   if(lmParams == NULL)
      return Parent::setupPass(state, sgData);

   // Defaults
   bool bRetVal = true;

   // What render pass is this...
   if(mCurPass == -1)
   {
      // First pass, reset this flag
      mInternalPass = false;

      // Pass call to parent
      bRetVal = Parent::setupPass(state, sgData);
   }
   else
   {
      // If this light is represented in a lightmap, it has already done it's 
      // job for non-lightmapped geometry. Now render the lightmapped geometry
      // pass (specular + shadow-darkening)
      if(!mInternalPass && lmParams->representedInLightmap)
         mInternalPass = true;
      else
         return Parent::setupPass(state, sgData);
   }

   // Set up the shader constants we need to...
   if(mLightMapParamsSC->isValid())
   {
      // If this is an internal pass, special case the parameters
      if(mInternalPass)
      {
         AssertFatal( lmParams->shadowDarkenColor.alpha == -1.0f, "Assumption failed, check unpack code!" );
         getMaterialParameters()->set( mLightMapParamsSC, lmParams->shadowDarkenColor );
      }
      else
         getMaterialParameters()->set( mLightMapParamsSC, LinearColorF::WHITE );
   }

   // Now override stateblock with our own
   if(!mInternalPass)
   {
      // If this is not an internal pass, and this light is represented in lightmaps
      // than only effect non-lightmapped geometry for this pass
      if (lmParams->representedInLightmap)
      {
         GFX->setStateBlock(mLitState[StaticLightNonLMGeometry][reflectStatus]);
      }
      else // This is a normal, dynamic light.
      {
         if (mSpecialLight)
            GFX->setStateBlock(mLitState[SunLight][reflectStatus]);
         else
            GFX->setStateBlock(mLitState[DynamicLight][reflectStatus]);
      }

   }
   else // Internal pass, this is the add-specular/multiply-darken-color pass
      GFX->setStateBlock(mLitState[StaticLightLMGeometry][reflectStatus]);

   return bRetVal;
}

bool LightMatInstance::init( const FeatureSet &features, const GFXVertexFormat *vertexFormat )
{
   bool success = Parent::init(features, vertexFormat);
   
   // If the initialization failed don't continue.
   if ( !success || !mProcessedMaterial || mProcessedMaterial->getNumPasses() == 0 )   
      return false;

   mLightMapParamsSC = getMaterialParameterHandle("$lightMapParams");

   // Grab the state block for the first render pass (since this mat instance
   // inserts a pass after the first pass)
   AssertFatal(mProcessedMaterial->getNumPasses() > 0, "No passes created! Ohnoes");
   const RenderPassData *rpd = mProcessedMaterial->getPass(0);
   AssertFatal(rpd, "No render pass data!");
   AssertFatal(rpd->mRenderStates[0], "No render state 0!");
   
   // Get state block desc for normal (not wireframe, not translucent, not glow, etc)
   // render state
   GFXStateBlockDesc litState = rpd->mRenderStates[0]->getDesc();

   // Create state blocks for each of the 3 possible combos in setupPass

   //DynamicLight State: This will effect lightmapped and non-lightmapped geometry
   // in the same way.

   litState.separateAlphaBlendDefined = true;
   litState.separateAlphaBlendEnable = false;
   litState.stencilMask = RenderDeferredMgr::OpaqueDynamicLitMask | RenderDeferredMgr::OpaqueStaticLitMask;
   litState.setCullMode(GFXCullCW);
   mLitState[DynamicLight][Base] = GFX->createStateBlock(litState);
   litState.setCullMode(GFXCullCCW);
   mLitState[DynamicLight][Reflecting] = GFX->createStateBlock(litState);

   litState.separateAlphaBlendDefined = true;
   litState.separateAlphaBlendEnable = false;
   litState.stencilMask = RenderDeferredMgr::OpaqueDynamicLitMask | RenderDeferredMgr::OpaqueStaticLitMask;
   litState.setCullMode(GFXCullCCW);
   mLitState[SunLight][Base] = GFX->createStateBlock(litState);
   litState.setCullMode(GFXCullCCW);
   mLitState[SunLight][Reflecting] = GFX->createStateBlock(litState);

   // StaticLightNonLMGeometry State: This will treat non-lightmapped geometry
   // in the usual way, but will not effect lightmapped geometry.
   litState.separateAlphaBlendDefined = true;
   litState.separateAlphaBlendEnable = false;
   litState.stencilMask = RenderDeferredMgr::OpaqueDynamicLitMask;
   litState.setCullMode(GFXCullCW);
   mLitState[StaticLightNonLMGeometry][Base] = GFX->createStateBlock(litState);
   litState.setCullMode(GFXCullCCW);
   mLitState[StaticLightNonLMGeometry][Reflecting] = GFX->createStateBlock(litState);

   // StaticLightLMGeometry State: This will add specular information (alpha) but
   // multiply-darken color information. 
   litState.blendDest = GFXBlendSrcColor;
   litState.blendSrc = GFXBlendZero;
   litState.stencilMask = RenderDeferredMgr::OpaqueStaticLitMask;
   litState.separateAlphaBlendDefined = true;
   litState.separateAlphaBlendEnable = true;
   litState.separateAlphaBlendSrc = GFXBlendOne;
   litState.separateAlphaBlendDest = GFXBlendOne;
   litState.separateAlphaBlendOp = GFXBlendOpAdd;
   litState.setCullMode(GFXCullCW);
   mLitState[StaticLightLMGeometry][Base] = GFX->createStateBlock(litState);
   litState.setCullMode(GFXCullCCW);
   mLitState[StaticLightLMGeometry][Reflecting] = GFX->createStateBlock(litState);

   return true;
}

singleton PostEffect( reflectionProbeArrayPostFX )
{
   // Do not allow the selection effect to work in reflection 
   // passes by default so we don't do the extra drawing.
   //allowReflectPass = false;
                  
   renderTime = "PFXAfterBin";
   renderBin = "ProbeBin";
   renderPriority = 9999;
   isEnabled = true;

   shader = PFX_ReflectionProbeArray;
   stateBlock = PFX_ReflectionProbeArrayStateBlock;

   texture[0] = "#deferred";
   texture[1] = "#color";
   texture[2] = "#matinfo";
};

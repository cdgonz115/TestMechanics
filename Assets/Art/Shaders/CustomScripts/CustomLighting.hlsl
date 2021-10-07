void MainLight_float(float3 WorldPos, out float3 Direction, out float3 Color, out float DistanceAtten, out float ShadowAtten)
{
#if SHADERGRAPH_PREVIEW
   Direction = float3(0.5, 0.5, 0.0);
   Color = 1.0;
   DistanceAtten = 1.0;
   ShadowAtten = 1.0;
#else
#if SHADOWS_SCREEN
   float4 clipPos = TransformWorldToHClip(WorldPos);
   float4 shadowCoord = ComputeScreenPos(clipPos);
#else
   float4 shadowCoord = TransformWorldToShadowCoord(WorldPos);
#endif
   Light mainLight = GetMainLight(shadowCoord);
   Direction = mainLight.direction;
   Color = mainLight.color;
   DistanceAtten = mainLight.distanceAttenuation;
   ShadowAtten = mainLight.shadowAttenuation;
#endif
}

void DirectSpecular_float(float3 Specular, float Smoothness, float3 Direction, float3 Color, float3 WorldNormal, float3 WorldView, out float3 Out)
{
#if SHADERGRAPH_PREVIEW
   Out = 0.0;
#else
    Smoothness = exp2(10.0 * Smoothness + 1.0);
    WorldNormal = normalize(WorldNormal);
    WorldView = SafeNormalize(WorldView);
    Out = LightingSpecular(Color, Direction, WorldNormal, WorldView, float4(Specular, 0.0), Smoothness);
#endif
}

void AdditionalLights_float(float3 SpecColor, float Smoothness, float3 WorldPosition, float3 WorldNormal, float3 WorldView, out float3 Diffuse, out float3 Specular)
{
    float3 diffuseColor = 0.0;
    float3 specularColor = 0.0;

#ifndef SHADERGRAPH_PREVIEW
    Smoothness = exp2(10.0 * Smoothness + 1.0);
    WorldNormal = normalize(WorldNormal);
    WorldView = SafeNormalize(WorldView);
    float pixelLightCount = GetAdditionalLightsCount();
    for (float i = 0.0; i < pixelLightCount; ++i)
    {
        Light light = GetAdditionalLight(i, WorldPosition);
        float3 attenuatedLightColor = light.color * (light.distanceAttenuation * light.shadowAttenuation);
        diffuseColor += LightingLambert(attenuatedLightColor, light.direction, WorldNormal);
        specularColor += LightingSpecular(attenuatedLightColor, light.direction, WorldNormal, WorldView, float4(SpecColor, 0.0), Smoothness);
    }
#endif

    Diffuse = diffuseColor;
    Specular = specularColor;
}
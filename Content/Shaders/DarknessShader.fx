sampler uImage0 : register(s0);
sampler uImage1 : register(s1);
sampler uImage2 : register(s2);
sampler uImage3 : register(s3);
float3 uColor;
float3 uSecondaryColor;
float2 uScreenResolution;
float2 uScreenPosition;
float2 uTargetPosition;
float2 uDirection;
float uOpacity;
float uTime;
float uIntensity;
float uProgress;
float2 uImageSize1;
float2 uImageSize2;
float2 uImageSize3;
float2 uImageOffset;
float uSaturation;
float4 uSourceRect;
float2 uZoom;
float distance = 0.05;
float maxGlow = 2;

float4 Darkness(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 color = tex2D(uImage0, coords);
    sampleColor = color;
    float2 coords2 = coords * 2.0 - 1.0;
    float length = sqrt(coords2.x * coords2.x + coords2.y * coords2.y);
    
    float res = distance / length;
    res = clamp(res, 0, maxGlow);
    color.rgb = res;
    return color * sampleColor;
}

technique Tech1
{
    pass Darkness
    {
        PixelShader = compile ps_2_0 Darkness();
    }
}
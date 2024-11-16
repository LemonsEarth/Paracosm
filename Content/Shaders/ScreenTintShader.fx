sampler uImage0 : register(s0);
float3 uColor;
float uProgress;

float4 ScreenTint(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 color = tex2D(uImage0, coords);
    color.rgb *= (uColor / uProgress);
    return color * sampleColor;
}

technique Tech1
{
    pass ScreenTint
    {
        PixelShader = compile ps_2_0 ScreenTint();
    }
}
sampler uImage0 : register(s0);
float3 uColor;

float4 ScreenTint(float sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 color = tex2D(uImage0, coords);
    color.rgb *= uColor;
    return color;
}

technique Tech1
{
    pass ScreenTint
    {
        PixelShader = compile ps_2_0 ScreenTint();
    }
}
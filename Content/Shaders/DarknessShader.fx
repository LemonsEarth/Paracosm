sampler uImage0 : register(s0);
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
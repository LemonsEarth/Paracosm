sampler uImage0 : register(s0);
float2 uImageSize0;
float distance = 0.05;
float maxGlow = 10;
float minGlow = 5;

float4 ProjectileLight(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0, float2 position : SV_Position) : COLOR0
{
    float4 color = tex2D(uImage0, coords);
    float4 color2 = color;
    float length = sqrt(coords.x * coords.x + coords.y * coords.y);
    
    if (length > 0)
    {
        float res = distance / length;
        res = clamp(res, 0, 0);
        color.rgb = res;
        return tex2D(uImage0, coords);
    }
    return tex2D(uImage0, coords);
}

technique Tech1
{
    pass ProjectileLight
    {
        PixelShader = compile ps_2_0 ProjectileLight();
    }
}
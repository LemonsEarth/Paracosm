sampler uImage0 : register(s0);
float2 uImageSize0;
float distance = 0.4;
float maxGlow = 10;
float minGlow = 5;
float time;

float4 ProjectileLight(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0, float2 position : SV_Position) : COLOR0
{
    float4 color = tex2D(uImage0, coords);
    float4 color2 = color;
    float2 coords2 = coords * 2.0 - 1.0; // Coords but set between (-1, -1) and (1, 1)
    float length = sqrt(coords2.x * coords2.x + coords2.y * coords2.y); // Distance of current pixel to center
    
    if (length > 0 && length < 0.8)
    {
        float res = distance / length;
        if (length > distance)
        {
            res -= distance / 2; // Falloff
        }
        res = clamp(res, 0, maxGlow);
        color += (res * (0.25 * sin(4 * time) + 0.5)); //Make the glow oscillate
        return color;
    }
    return color2;
}

technique Tech1
{
    pass ProjectileLight
    {
        PixelShader = compile ps_2_0 ProjectileLight();
    }
}
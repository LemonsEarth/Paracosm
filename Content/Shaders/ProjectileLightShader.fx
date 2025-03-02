sampler uImage0 : register(s0);
float2 uImageSize0;
float4 color = float4(1, 1, 1, 1);
float distance = 0.6;
float maxGlow = 10;
float amplitude = 0.25;
float shineRate = 4;
float verticalShift = 0.5;
float time;

float4 ProjectileLight(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0, float2 position : SV_Position) : COLOR0
{
    float4 texColor = tex2D(uImage0, coords);
    float4 savedTexColor = texColor;
    float2 coords2 = coords * 2.0 - 1.0; // Coords but set between (-1, -1) and (1, 1)
    float length = sqrt(coords2.x * coords2.x + coords2.y * coords2.y); // Distance of current pixel to center
    
    if (length > 0)
    {
        float res = distance / length - distance;
        if (length > distance)
        {
            res -= distance / 4; // Falloff
        }
        else
        {
            res -= distance / 4;
        }
        res = clamp(res, 0, maxGlow);
        texColor = color * (res * (amplitude * sin(shineRate * time) + verticalShift)); //Make the glow oscillate
        return texColor;
    }
    return savedTexColor;
}

technique Tech1
{
    pass ProjectileLight
    {
        PixelShader = compile ps_2_0 ProjectileLight();
    }
}
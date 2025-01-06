sampler uImage0 : register(s0);
sampler uImage1 : register(s1);
float2 uImageSize0;
float2 uImageSize1;
float4 uSourceRect;
float2 desiredPos;
float2 uTargetPosition;
float2 uScreenPosition;
float2 uScreenResolution;
float uTime;

float4 DarknessPos(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0, float4 position : SV_Position) : COLOR0
{
    float4 baseColor = tex2D(uImage0, coords);
    float4 fogColor = tex2D(uImage1, coords);
    sampleColor = baseColor;
    float2 centeredCoords = coords * 2.0 - 1.0;
    float2 targetCoords = (uTargetPosition - uScreenPosition) / uScreenResolution;
    float2 coordsDir = (coords - targetCoords) * (uScreenResolution / uScreenResolution.y); // (uScreenResolution / uScreenResolution.y) turns it into a circle
    float length = sqrt(coordsDir.x * coordsDir.x + coordsDir.y * coordsDir.y); // target to current coords
    float2 desiredCoords = (desiredPos - uScreenPosition) / uScreenResolution;
    float2 desiredCoordsDir = (desiredCoords - targetCoords) * (uScreenResolution / uScreenResolution.y);
    float desiredDistance = sqrt(desiredCoordsDir.x * desiredCoordsDir.x + desiredCoordsDir.y * desiredCoordsDir.y);
    float offset = (sin(4 * uTime) + 5) / 2000; // width of the outline
    if (length > desiredDistance + offset)
    {
        float luminosity = (fogColor.r + fogColor.g + fogColor.b) / 3;
        fogColor *= luminosity;
        fogColor.r *= (sin(7 * uTime) + 1.2) * 0.5;
        fogColor.g *= (sin(5 * uTime) + 1.2) * 0.5;
        fogColor.b *= (sin(3 * uTime) + 1.2) * 0.5;
        return fogColor;
    }
    else if (length > desiredDistance)
    {
        return 1;
    }
    return sampleColor;
    
}

technique Tech1
{
    pass DarknessPos
    {
        PixelShader = compile ps_2_0 DarknessPos();
    }
}
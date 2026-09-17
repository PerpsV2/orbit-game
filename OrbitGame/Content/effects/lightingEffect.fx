#define VS_SHADERMODEL vs_6_0
#define PS_SHADERMODEL ps_6_0

#define DISTANT_FADEOUT_FACTOR 6.0
#define DISTANT_BRIGHTNESS_CAP 0.9
#define STAR_SHAPE_FACTOR 3

matrix Projection;
matrix World;

float2 LightCenter;
float4 LightColour;
float BValue;
float CValue;
float NValue;
float Magnitude;
float LightSize;
float DistanceScale;
float2 ScreenSize;

Texture2D ShadowMask : register(t0);;
SamplerState ShadowMaskSampler : register(s0);;

Texture2D OccluderMask : register(t1);;
SamplerState OccluderMaskSampler : register(s1);;

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float4 TexCoords : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 TexCoords : TEXCOORD0;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput)0;

    output.Position = mul(input.Position, mul(World, Projection));
    output.TexCoords = input.TexCoords;

    return output;
}

float4 MainPS(VertexShaderOutput input) : SV_TARGET
{
    float2 pixel = float2(input.TexCoords.x * ScreenSize.x, input.TexCoords.y * ScreenSize.y);
    float pixelDistance = length(pixel - LightCenter);
    
    float distance = pixelDistance * DistanceScale;
    
    float logDistScale = log10(DistanceScale);
    float apparentLightSize = max(min(
        Magnitude - logDistScale, 
        1.0 / DISTANT_FADEOUT_FACTOR * (Magnitude * Magnitude / logDistScale - Magnitude)
        ), 0);
    
    float pixelAngle = atan2((pixel - LightCenter).y, (pixel - LightCenter).x);
    float starFactor = -sqrt(abs(sin(2 * pixelAngle))) / max(LightSize, STAR_SHAPE_FACTOR) + 1;
    float distantBrightness = min(starFactor * starFactor / pow(pixelDistance / apparentLightSize, 1), 1) * DISTANT_BRIGHTNESS_CAP;
    
    float radiatedBrightness = BValue / pow(distance + CValue, 2);
    
    float objectBrightness = min(1.0 / pow(pixelDistance / LightSize, 2), 1);
    
    float brightness = min(1, max(objectBrightness, radiatedBrightness + distantBrightness));
    return saturate(float4(1, 1, 1, 1) * brightness  *  
        ShadowMask.Sample(ShadowMaskSampler, input.TexCoords) * 
        OccluderMask.Sample(OccluderMaskSampler, input.TexCoords));
}

technique BasicColorDrawing
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};
#if OPENGL
    #define SV_POSITION POSITION
    #define VS_SHADERMODEL vs_3_0
    #define PS_SHADERMODEL ps_3_0
#else
    #define VS_SHADERMODEL vs_4_0_level_9_1
    #define PS_SHADERMODEL ps_4_0_level_9_1
#endif

#define SAMPLES 8
#define TAU 6.28318

matrix Projection;
matrix World;

float2 LightCenter;
float4 LightColour;
//float LightRadius;
float DistanceScale;

float2 ScreenSize;

texture2D OccluderMask;
sampler2D OccluderMaskSampler = sampler_state 
{
    Texture = <OccluderMask>;
};

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

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float2 diffVector = float2(input.TexCoords.x * ScreenSize.x, input.TexCoords.y * ScreenSize.y) - LightCenter;
    float distance = length(diffVector);
    return saturate(LightColour / pow(distance * DistanceScale, 2));
}

technique BasicColorDrawing
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};
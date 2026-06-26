#if OPENGL
    #define SV_POSITION POSITION
    #define VS_SHADERMODEL vs_3_0
    #define PS_SHADERMODEL ps_3_0
#else
    #define VS_SHADERMODEL vs_4_0_level_9_1
    #define PS_SHADERMODEL ps_4_0_level_9_1
#endif

#define SAMPLES 4
#define TAU 6.28318

matrix Projection;
matrix World;
float4 Colour;

float2 LightCenter;
float LightRadius;
float DistanceScale;
float ScreenWidth;
float ScreenHeight;

texture2D MaskTexture;
sampler2D MaskTextureSampler = sampler_state 
{
    Texture = <MaskTexture>;
};

float2 OccluderCenter;
float OccluderRadius;

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

float SDFCircle(float2 pos, float2 center, float radius) 
{
    return length(pos - center) - radius;
}

float CalculateOcclusion(float2 origin, float2 direction, float distance) 
{
    float2 ray = origin;
    while (true) {
        float sdfDistance = SDFCircle(ray, OccluderCenter, OccluderRadius);
        if (sdfDistance <= 0) return 0;
        ray += direction * sdfDistance;
        if (length(ray - origin) >= distance) return 1;
    }
    return 0;
}

float Rand(float2 uv)
{
    return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453123);
}

float4 BasicColourPS(VertexShaderOutput input) : COLOR
{
    float4 maskColour = tex2D(MaskTextureSampler, input.TexCoords);
    float rand = Rand(input.TexCoords);
    float4 result = float4(0, 0, 0, 0);
    for (int i = 0; i < SAMPLES; i++) {
        float2 lightOrigin = LightCenter + float2(LightRadius * cos(rand * TAU), LightRadius * sin(rand * TAU));
        float2 diffVector = float2(input.TexCoords.x * ScreenWidth, input.TexCoords.y * ScreenHeight) - lightOrigin;
        float distance = length(diffVector);
        float2 direction = diffVector / distance;
        float4 baseColour = Colour / pow(distance * DistanceScale, 2) / SAMPLES;
        float occlusion = CalculateOcclusion(lightOrigin, direction, distance);
        result += float4(baseColour.r, baseColour.g, baseColour.b, baseColour.a) * occlusion;
        rand = Rand(input.TexCoords * rand);
    }
    return float4(result.r, result.g, result.b, result.a) * maskColour.a;
}

technique BasicColorDrawing
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL BasicColourPS();
    }
};
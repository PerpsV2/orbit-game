#if OPENGL
    #define SV_POSITION POSITION
    #define VS_SHADERMODEL vs_3_0
    #define PS_SHADERMODEL ps_3_0
#else
    #define VS_SHADERMODEL vs_4_0_level_9_1
    #define PS_SHADERMODEL ps_4_0_level_9_1
#endif

#define TAU 6.28318530718

matrix Projection;
matrix World;

float4 Colour;

float Eccentricity;
float2 Center;
float2 Focus;

float C1;
float C2;
float C3;
float C4;
float C5;
float C6;

float ConicSymmetryAngle;
float StartAngle;
float EndAngle;

float2 TexelSize;

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float2 TexCoords : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float2 TexCoords : TEXCOORD0;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput)0;

    output.Position = mul(input.Position, mul(World, Projection));
    output.TexCoords = input.TexCoords;

    return output;
}

float Mod(float x, float y) {
    return x - y * floor(x / y);
}

float4 DefaultPS(VertexShaderOutput input) : COLOR
{
    return Colour;
}

float WithinConic(float2 coords) {
    return C1 * coords.x * coords.x +
           C2 * coords.x * coords.y + 
           C3 * coords.y * coords.y +
           C4 * coords.x + 
           C5 * coords.y + 
           C6 <= 0;
}

bool WithinLineOfSymmetry(float2 coords) {
    float modAngle = Mod(ConicSymmetryAngle, TAU);
    if (modAngle < TAU / 4 || modAngle > 3 * TAU / 4) return coords.y > tan(modAngle) * coords.x;
    else return coords.y < tan(modAngle) * coords.x;
}

float4 EllipsePS(VertexShaderOutput input) : COLOR
{
    if (Eccentricity > 1) {
        float2 centerCoords = input.TexCoords - Center;
        if (WithinLineOfSymmetry(centerCoords)) return float4(0, 0, 0, 0);
    }
    if (EndAngle - StartAngle < TAU) {
        float2 focusCoords = input.TexCoords - Focus;
        float texelAngle = StartAngle - Focus.x;//float texelAngle = Mod(atan2(focusCoords.y, focusCoords.x), TAU);
        if (texelAngle < StartAngle || texelAngle > EndAngle) return float4(0, 0, 0, 0);
    }

    float2 rightTexel = input.TexCoords + float2(TexelSize.x, 0);
    float2 topTexel = input.TexCoords + float2(0, TexelSize.y);
    float2 leftTexel = input.TexCoords + float2(-TexelSize.x, 0);
    float2 bottomTexel = input.TexCoords + float2(0, -TexelSize.y);
    
    if (WithinConic(rightTexel) && WithinConic(topTexel) && WithinConic(leftTexel) && WithinConic(bottomTexel))
        return float4(0, 0, 0, 0);
    if (!WithinConic(rightTexel) && !WithinConic(topTexel) && !WithinConic(leftTexel) && !WithinConic(bottomTexel))
        return float4(0, 0, 0, 0);

    return Colour;
}

technique BasicOrbitDrawing
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL EllipsePS();
    }
};
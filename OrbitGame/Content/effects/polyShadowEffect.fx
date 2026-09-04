#if OPENGL
    #define SV_POSITION POSITION
    #define VS_SHADERMODEL vs_3_0
    #define PS_SHADERMODEL ps_3_0
#else
    #define VS_SHADERMODEL vs_6_0
    #define PS_SHADERMODEL ps_6_0
#endif

#define PI 3.14159265359
#define TAU 6.28318530718

matrix Projection;
matrix World;

float2 LightCenter;
float LightRadius;
float2 OccluderVertices[16];
int VerticesActiveCount;

float2 ScreenSize;

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

float Mod(float value, float mod) 
{
    return value - mod * floor(value / mod);
}

float Wrap(float value)
{
    if (value >= 0 && value < TAU) return value;
    return Mod(value, TAU);
}

bool IsInFrontLight(float2 p, float2 v, float lightDistance, float tangentAngle1, float tangentAngle2) 
{
    float pointAngle = Wrap(atan2(v.y, v.x));
    
    if (Wrap(pointAngle - tangentAngle2) <= Wrap(tangentAngle1 - tangentAngle2)) {
        return pow(v.x - LightCenter.x + p.x, 2) + pow(v.y - LightCenter.y + p.y, 2) >= LightRadius * LightRadius &&
               pow(v.x, 2) + pow(v.y, 2) <= pow(lightDistance, 2) - LightRadius * LightRadius;
    }
    return true;
}

bool IntervalIsSubset(float s1, float e1, float s2, float e2)
{
    float a = Wrap(s1 - s2);
    float b = Wrap(e2 - s2);
    float w = Wrap(e1 - s1);
    return a <= b && a + w <= b;
}

bool IntervalIsDisjoint(float s1, float e1, float s2, float e2)
{
    float a = Wrap(s1 - s2);
    float b = Wrap(e2 - s2);
    float w = Wrap(e1 - s1);
    return a >= b && a + w <= TAU;
}

bool AngleInAngularInterval(float angle, float start, float end)
{
    if (start < end) return start <= angle && angle <= end;
    return angle >= start || angle <= end;
}

float CalculateOcclusion(float2 tex) 
{
    float2 pixel = float2(tex.x * ScreenSize.x, tex.y * ScreenSize.y);
    
    float2 lightRelPos = LightCenter - pixel;
    float lightDistance = length(lightRelPos);
    float lightHalfAngularRadius = asin(LightRadius / lightDistance);
    
    float lightCenterAngle = Wrap(atan2(lightRelPos.y, lightRelPos.x));
    float tangentAngle1 = Wrap(lightCenterAngle + lightHalfAngularRadius);
    float tangentAngle2 = Wrap(lightCenterAngle - lightHalfAngularRadius);
    
    float midAngle1 = lightHalfAngularRadius * 2;
    float midAngle2 = lightHalfAngularRadius * 2;
    
    float topAngle = 0;
    float bottomAngle = 0;
    
    for (int c = 0; c < VerticesActiveCount; ++c) {
        int n = Mod(c + 1, VerticesActiveCount);
        float2 p1 = OccluderVertices[c] - pixel;
        float2 p2 = OccluderVertices[n] - pixel;
        
        float pointAngle1 = Wrap(atan2(p1.y, p1.x));
        float pointAngle2 = Wrap(atan2(p2.y, p2.x));
        
        if (Wrap(pointAngle1 - pointAngle2) > PI) 
        {
            float temp = pointAngle1;
            pointAngle1 = pointAngle2;
            pointAngle2 = temp;
        }
        
        if (!IsInFrontLight(pixel, p2, lightDistance, tangentAngle1, tangentAngle2) || 
            !IsInFrontLight(pixel, p1, lightDistance, tangentAngle1, tangentAngle2)) continue;
            
        if (IntervalIsSubset(tangentAngle2, tangentAngle1, pointAngle2, pointAngle1)) return 1;

        if (IntervalIsSubset(pointAngle2, pointAngle1, tangentAngle2, tangentAngle1)) {
            float m1 = Wrap(tangentAngle1 - pointAngle1);
            float m2 = Wrap(pointAngle2 - tangentAngle2);
            
            if (m1 < midAngle1) midAngle1 = m1;
            if (m2 < midAngle2) midAngle2 = m2;
        }
        
        else if (!IntervalIsDisjoint(tangentAngle2, tangentAngle1, pointAngle2, pointAngle1)) {
            if (AngleInAngularInterval(pointAngle1, tangentAngle2, tangentAngle1)) {
                float m = Wrap(pointAngle1 - tangentAngle2);
                if (m > bottomAngle) bottomAngle = m;
            }
            else {
                float m = Wrap(tangentAngle1 - pointAngle2);
                if (m > topAngle) topAngle = m;
            }
        }
    }
    
    if (midAngle1 < topAngle) {
        topAngle = 0;
        midAngle1 = 0;
    }
    
    if (midAngle2 < bottomAngle) {
        bottomAngle = 0;
        midAngle2 = 0;
    }
    
    midAngle2 = lightHalfAngularRadius * 2 - midAngle2;
    
    float occludedAngle = topAngle + bottomAngle + max(midAngle2 - midAngle1, 0);
    return occludedAngle / (2 * lightHalfAngularRadius);
}

float4 MainPS(VertexShaderOutput input) : SV_TARGET
{
    float occlusion = CalculateOcclusion(input.TexCoords);
    return float4(0, 0, 0, saturate(occlusion));
}

technique BasicColorDrawing
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};
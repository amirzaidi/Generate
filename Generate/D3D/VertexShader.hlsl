cbuffer Matrices
{
    float4x4 World;
    float4x4 View;
    float4x4 Projection;
    float4x4 WVP;

    float4x4 LightView;
    float4x4 LightProjection;
};

cbuffer VertexLight
{
    float3 LightPosition;
    float UseLight;
};

struct Vertex
{
    float3 Position : POSITION;
    float2 TexCoord : TEXCOORD0;
    float3 Normal : NORMAL;
};

struct Pixel
{
    float4 Position : SV_POSITION;
    float2 TexCoord : TEXCOORD0;
    float3 Normal : NORMAL;
    float4 LightViewPosition : TEXCOORD1;
    float3 LightPos : TEXCOORD2;
};

Pixel VS(Vertex Input)
{
    Pixel Output;
    
    Output.TexCoord = Input.TexCoord;
    Output.Normal = normalize(mul(Input.Normal, (float3x3) World));
    Output.Position = mul(mul(mul(float4(Input.Position, 1), World), LightView), LightProjection);

    if (UseLight == 1)
    {
        Output.LightViewPosition = Output.Position;
        Output.Position = mul(float4(Input.Position, 1), WVP);
        Output.LightPos = normalize(LightPosition.xyz - mul(Input.Position, (float3x3) World));
    }
    
    return Output;
}
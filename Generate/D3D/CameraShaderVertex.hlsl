cbuffer CameraMatrices
{
    float4x4 World;
    float4x4 CameraWVP;
};

cbuffer VertexLight
{
    float4x4 LightVP;
    float4 LightPosition;
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
    float3x3 WorldRotation = (float3x3) World;
    Pixel Output;
    
    Output.TexCoord = Input.TexCoord;
    Output.Normal = normalize(mul(Input.Normal, WorldRotation));
    Output.Position = mul(float4(Input.Position, 1), CameraWVP);
    
    Output.LightViewPosition = mul(mul(float4(Input.Position, 1), World), LightVP);
    Output.LightPos = normalize(LightPosition.xyz - mul(Input.Position, WorldRotation));
    
    return Output;
}
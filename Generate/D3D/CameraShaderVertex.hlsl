cbuffer CameraMatrices
{
    float4x4 World;
    float4x4 CameraWVP;
};

cbuffer VertexLight
{
    float4x4 LightVP;
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
};

Pixel VS(Vertex Input)
{
    Pixel Output;
    
    Output.TexCoord = Input.TexCoord;
    Output.Normal = normalize(mul(Input.Normal, (float3x3) World));
    Output.Position = mul(float4(Input.Position, 1), CameraWVP);
    
    Output.LightViewPosition = mul(mul(float4(Input.Position, 1), World), LightVP);
    
    return Output;
}
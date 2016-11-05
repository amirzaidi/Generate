cbuffer Matrices
{
    float4x4 LightWVP;
};

struct Vertex
{
    float3 Position : POSITION;
    float2 TexCoord : TEXCOORD0;
    float3 Normal : NORMAL;
};

struct Pixel
{
    float4 Depth : SV_Position;
};

Pixel VS(Vertex Input)
{
    Pixel Output;
    Output.Depth = mul(float4(Input.Position, 1), LightWVP);
    
    return Output;
}
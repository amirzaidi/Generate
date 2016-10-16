cbuffer Matrices
{
    float4x4 World;
    float4x4 View;
    float4x4 Projection;
    float4x4 WVP;
};

struct Vertex
{
    float4 Position : POSITION;
    float2 TexCoord : TEXCOORD0;
    float3 Normal : NORMAL;
};

struct Pixel
{
    float4 Position : SV_POSITION;
    float2 TexCoord : TEXCOORD0;
    float3 Normal : NORMAL;
};

Pixel VS(Vertex Input)
{
    Pixel Output;

	// Change the position vector to be 4 units for proper matrix calculations.
    Input.Position.w = 1.0f;

	// Calculate the position of the vertex against the world, view, and projection matrices.
    //Output.Position = mul(Input.Position, World);
	//Output.Position = mul(Output.Position, View);
	//Output.Position = mul(Output.Position, Projection);
    Output.Position = mul(Input.Position, WVP);

	// Store the input color for the pixel shader to use.
    Output.TexCoord = Input.TexCoord;
    Output.Normal = normalize(mul(Input.Normal, (float3x3) World));
    
    return Output;
}
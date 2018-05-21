struct PixelInputType
{
	float4 position : SV_POSITION;
	float2 tex : TEXCOORD0;
	float3 normal : NORMAL;
};

struct PixelOutputType
{
	float4 color : SV_TARGET0;
	float4 normal : SV_TARGET1;
};

Texture2D shaderTexture;
SamplerState SampleType;

PixelOutputType PS(PixelInputType input)
{
	PixelOutputType output;

	output.color = shaderTexture.Sample(SampleType, input.tex);
	output.normal.xyz = float3(0.5, 0.5, 0.5) + 0.5 * input.normal;
	output.normal.w = 1;

	return output;
}

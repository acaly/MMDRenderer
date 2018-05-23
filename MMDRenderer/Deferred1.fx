struct VS_IN
{
	float2 pos : POSITION;
};

struct PS_IN
{
	float4 pos : SV_POSITION;
	float2 tex : TEXCOORD;
};

cbuffer PS_CONSTANT_BUFFER : register(b0)
{
	float4 clearColor;
};

PS_IN VS(VS_IN input)
{
	PS_IN output = (PS_IN)0;

	output.pos.x = (input.pos.x * 2) - 1;
	output.pos.y = 1 - (input.pos.y * 2);
	output.pos.w = 1;
	output.tex = input.pos.xy;

	return output;
}

SamplerState textureSampler : register(s0);
Texture2D colorTexture : register(t0);
Texture2D normalTexture : register(t1);
Texture2D depthTexture : register(t2);

float4 PS(PS_IN input) : SV_Target
{
	float2 tex = input.tex.xy;

	float depth = depthTexture.Sample(textureSampler, tex).r;
	float4 color = colorTexture.Sample(textureSampler, tex);
	float4 normalCol = normalTexture.Sample(textureSampler, tex);
	float3 normal = (normalCol - float3(0.5, 0.5, 0.5)) * 2;

	if (normalCol.w == 0)
	{
		return clearColor;
	}
	return color;
}

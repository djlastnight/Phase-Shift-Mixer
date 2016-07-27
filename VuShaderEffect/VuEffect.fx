//--------------------------------------------------------------------------------------
// 
// WPF ShaderEffect HLSL Template
//
//--------------------------------------------------------------------------------------

//-----------------------------------------------------------------------------------------
// Shader constant register mappings (scalars - float, double, Point, Color, Point3D, etc.)
//-----------------------------------------------------------------------------------------

float level : register(C0);

//--------------------------------------------------------------------------------------
// Sampler Inputs (Brushes, including ImplicitInput)
//--------------------------------------------------------------------------------------

sampler2D implicitInputSampler : register(S0);


//--------------------------------------------------------------------------------------
// Pixel Shader
//--------------------------------------------------------------------------------------

float4 main(float2 uv : TEXCOORD) : COLOR
{
	float y = frac(1 - uv.y);
	float x = uv.x;
	float alpha = 1.0f;

	if (level > y)
	{
		if (y <= 0.25)
		{
			// dark green
			return float4(0.3, 0.8, 0.3, alpha);
		}

		if (y <= 0.50)
		{
			// green
			return float4(0, 1, 0, alpha);
		}

		if (y <= 0.75)
		{
			// orange
			return float4(1, 0.7, 0, alpha);
		}

		// red
		return float4(1, 0, 0, alpha);
	}

	// original color
	return tex2D(implicitInputSampler, uv);
}



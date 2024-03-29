Shader "Unlit/URP_GRASS"
{
    Properties
    { 
		_NoiseTex("Noise texture", 2D) = "white" {}
		_WaterColor("Grass (RGB)", Color) = (1.0, 1.0, 1.0, 1.0)
		_Height("Height", Range(0.0001,5.0)) = 1.0
        _InvFade ("Soft Factor", Range(0.01,3.0)) = 1.0
        _depthPow ("Depth pow", float) = 1.0
        _depthFactor ("Depth Factor", float) = 1.0
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalRenderPipeline" }
		
		///
		LOD 300
		ZWrite On
        Blend SrcAlpha OneMinusSrcAlpha
		///


        Pass
        {
            HLSLPROGRAM
			
		//input
			sampler2D _NoiseTex;
			sampler2D _GrassTex;
			sampler2D _StoneTex;
			float4 _WaterColor;
			float _Height;

            #pragma vertex vert
            #pragma fragment frag

			
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"            

            struct appdata
            {
                float4 positionOS : POSITION;                 
                float4 vertex : POSITION;
                float3 normal : TEXCOORD1;
			    float4 tangent: TEXCOORD2;
            };

            struct v2f
            {
                float4 positionHCS  : SV_POSITION;
			    float3 tangentViewDir: TEXCOORD1;
			    float4 tangent: TEXCOORD2;
				float2 uv_NoiseTex:TEXCOORD3;
            };            

            v2f vert(appdata v)
            {
                v2f OUT;
                OUT.positionHCS = TransformObjectToHClip(v.positionOS.xyz);

                float3 worldVertexPos = mul(unity_ObjectToWorld, v.vertex).xyz;
			    float3 worldViewDir = worldVertexPos - _WorldSpaceCameraPos;

			    //To convert from world space to tangent space we need the following
			    //https://docs.unity3d.com/Manual/SL-VertexFragmentShaderExamples.html
			    float3 worldNormal = TransformObjectToWorldNormal(v.normal);
			    float3 worldTangent = TransformObjectToWorldDir(v.tangent.xyz);
			    float3 worldBitangent = cross(worldNormal, worldTangent) * v.tangent.w * unity_WorldTransformParams.w;

			    //Use dot products instead of building the matrix
			    OUT.tangentViewDir = float3(
				    dot(worldViewDir, worldTangent),
				    dot(worldViewDir, worldNormal),
				    dot(worldViewDir, worldBitangent)
				);

                return OUT;
            }
            float2 animateUV(float2 texturePos)
		{
			//texturePos.x += sin(_Time.y*texturePos.x*.75*(1-texturePos.y))*.0008;//_Time.x * 5;
			texturePos*=400;
			return texturePos;
		}


		//Get the height from a uv position
		float getHeight(float2 texturePos)
		{
			
			texturePos = animateUV(texturePos);

			//Multiply with 0.2 to make the landscape flatter
			float4 colorNoise = tex2Dlod(_NoiseTex, float4(texturePos * 0.1, 0.0, 0.0));

			//Calculate the height at this uv coordinate
			//Just use r because r = g = b  because color is grayscale
			//(1-color.r) because black should be low
			//-1 because the ray is going down so the ray's y-coordinate will be negative
			float height = ((colorNoise.r) * - 1.0 * _Height);

			return height;
		}


		//Combine stone and grass depending on grayscale color
		float4 getBlendTexture(float2 texturePos, float height)
		{						
			texturePos = animateUV(texturePos);

			float textureSize = .1;

			float4 colorGrass = tex2Dlod(_GrassTex, float4(texturePos * textureSize, 0.0, 0.0));
			float4 colorStone = tex2Dlod(_StoneTex, float4(texturePos * textureSize, 0.0, 0.0));

			float colorGrayscale = 1 - (abs(height) / _Height);
			colorGrayscale = (-abs(height) / _Height);
			
			float4 mixedColor = 0.0;//lerp(colorGrass, colorStone, colorGrayscale);

			if (colorGrayscale < 0.4)
			{
				mixedColor = clamp(.00625/tex2Dlod(_NoiseTex, float4(texturePos * 0.1, 0.0, 0.0)),0.0 , 0.7)*.5 * (_WaterColor);
			}

			return mixedColor;
		}


		float2 getWeightedTexPos(float3 rayPos, float3 rayDir, float stepDistance)
		{
			float3 oldPos = rayPos - stepDistance * rayDir;

			float oldHeight = getHeight(oldPos.xz);

			float oldDistToTerrain = abs(oldHeight - oldPos.y);

			float currentHeight = getHeight(rayPos.xz);

			float currentDistToTerrain = rayPos.y - currentHeight;

			float weight = currentDistToTerrain / (currentDistToTerrain - oldDistToTerrain);

			float2 weightedTexPos = oldPos.xz * weight + rayPos.xz * (1 - weight);
			
			return currentHeight.xx;//weightedTexPos;
		}

            half4 frag(v2f IN) : SV_Target
            {
                half4 customColor;

			 //   float3 rayDir = normalize(IN.tangentViewDir);
                //customColor= float4(abs(rayDir),1);

				float3 rayPos = float3(IN.uv_NoiseTex.x, 0.0, IN.uv_NoiseTex.y);

			float3 rayDir = normalize(IN.tangentViewDir);

			float modif = pow(abs(rayDir.y), 0.753);
			
			int STEPS = (int)(100.0 * modif);
			float stepDistance = 0.0001;
			stepDistance *= modif;

			float4 finalColor = _WaterColor*modif*.1;
			
			float lastheight=0;
			float2 pt = float2(0,0);
			float shdw=0;
			//for (int i = 10; i < STEPS; i++)
			for (int i = 10; i < STEPS; i++)
			{
			//	stepDistance*=1-rayDir.y;
				//Get the current height at this uv coordinate
				float height = getHeight(rayPos.xz);
				
				//If the ray is below the surface
				if (rayPos.y < height)
				{
					//Get the texture position by interpolation between the position where we hit terrain and the position before
					lastheight=height;
					float2 weightedTex = getWeightedTexPos(rayPos, rayDir, stepDistance);
					
					float height = getHeight(weightedTex);
					pt=weightedTex;
					finalColor = getBlendTexture(weightedTex, height);
					float gh=getHeight(weightedTex);
					break;
				}
				rayPos += stepDistance * rayDir;
			}

                return float4(abs(rayDir),1);
            }
            ENDHLSL
        }
    }
}
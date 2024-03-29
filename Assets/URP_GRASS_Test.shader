Shader "Unlit/URP_GRASS"
{
    Properties
    { 
		_NoiseTex("Noise texture", 2D) = "white" {}
		_GrassTex("Grass (RGB)", 2D) = "white" {}
		_StoneTex("Stone (RGB)", 2D) = "white" {}
		_WaterColor("Grass (RGB)", Color) = (1.0, 1.0, 1.0, 1.0)
		_Height("Height", Range(0.0001,5.0)) = 1.0
        _InvFade ("Soft Factor", Range(0.01,3.0)) = 1.0
        _depthPow ("Depth pow", float) = 1.0
        _depthFactor ("Depth Factor", float) = 1.0
    }

    SubShader
    {
        Tags { 
			//"RenderType" = "Opaque" "RenderPipeline" = "UniversalRenderPipeline" 

		    "RenderPipeline" = "UniversalPipeline"
			"RenderType"="Opaque"
            "UniversalMaterialType" = "Lit"
            "Queue"="Geometry"

            //"RenderType"="Opaque"
            //"UniversalMaterialType" = "Lit"
            //"Queue"="Transparent"
		
		}
		LOD 100

		
        Pass
        {
			
            HLSLPROGRAM
			/////
			//#pragma multi_compile _ _MAIN_LIGHT_SHADOWS
   //         #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
   //         #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
   //         #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
   //         #pragma multi_compile_fragment _ _SHADOWS_SOFT
			/////

			#pragma target 2.0

			#pragma vertex vert
            #pragma fragment frag
			
		//input
			sampler2D _NoiseTex;
			sampler2D _GrassTex;
			sampler2D _StoneTex;
			half4 _WaterColor;
			half _Height;

			 //#include "UnityCG.cginc"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"            
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct appdata
            {
                float4 positionOS : POSITION;                 
                float4 vertex : POSITION;
                half3 normal : TEXCOORD1;
			    half4 tangent: TEXCOORD2;
            };

            struct v2f
            {
                half4 positionHCS  : SV_POSITION;
			    half3 tangentViewDir: TEXCOORD1;
			    half4 tangent: TEXCOORD2;
				half2 uv_NoiseTex:TEXCOORD3;
            };            


			//////
			//CBUFFER_START(UnityPerMaterial)
   //         float4 _BaseMap_ST;
   //         float4 _NormalMap_ST;
   //         float4 _HeightMap_ST;
   //         float _Height;
   //         CBUFFER_END
			/////


            v2f vert(appdata v)
            {
                v2f OUT;
                OUT.positionHCS = TransformObjectToHClip(v.positionOS.xyz);

                half3 worldVertexPos = mul(unity_ObjectToWorld, v.vertex).xyz;
			    half3 worldViewDir = worldVertexPos - _WorldSpaceCameraPos;

			    //To convert from world space to tangent space we need the following
			    //https://docs.unity3d.com/Manual/SL-VertexFragmentShaderExamples.html
			    half3 worldNormal = TransformObjectToWorldNormal(v.normal);
			    half3 worldTangent = TransformObjectToWorldDir(v.tangent.xyz);
			    half3 worldBitangent = cross(worldNormal, worldTangent) * v.tangent.w * unity_WorldTransformParams.w;

			    //Use dot products instead of building the matrix
			    OUT.tangentViewDir = half3(
				    dot(worldViewDir, worldTangent),
				    dot(worldViewDir, worldNormal),
				    dot(worldViewDir, worldBitangent)
				);

                return OUT;
            }

            half2 animateUV(half2 texturePos)
		{
			//texturePos.x += sin(_Time.y*texturePos.x*.75*(1-texturePos.y))*.0008;//_Time.x * 5;
			texturePos *= 40.0;
			return texturePos;
		}


		//Get the height from a uv position
		half getHeight(half2 texturePos)
		{
			
			texturePos = animateUV(texturePos);

			//Multiply with 0.2 to make the landscape flatter
			half4 colorNoise = tex2Dlod(_NoiseTex, half4(texturePos * 0.1, 0.0, 0.0));

			//Calculate the height at this uv coordinate
			//Just use r because r = g = b  because color is grayscale
			//(1-color.r) because black should be low
			//-1 because the ray is going down so the ray's y-coordinate will be negative
			half height = ((colorNoise.r) * - 1.0 * _Height);

			return height;
		}


		//Combine stone and grass depending on grayscale color
		half4 getBlendTexture(half2 texturePos, half height)
		{						
			texturePos = animateUV(texturePos);

			half textureSize = 0.1;

			half4 colorGrass = tex2Dlod(_GrassTex, half4(texturePos * textureSize, 0.0, 0.0));
			half4 colorStone = tex2Dlod(_StoneTex, half4(texturePos * textureSize, 0.0, 0.0));

			half colorGrayscale = 1 - (abs(height) / _Height);
			colorGrayscale = (-abs(height) / _Height);
			
			half4 mixedColor = 0.0;//lerp(colorGrass, colorStone, colorGrayscale);

			if (colorGrayscale < 0.4)
			{
				mixedColor = clamp(.00625/tex2Dlod(_NoiseTex, half4(texturePos * 0.1, 0.0, 0.0)),0.0 , 0.7) * 0.5 * (_WaterColor);
			}

			return mixedColor;
		}


		half2 getWeightedTexPos(half3 rayPos, half3 rayDir, half stepDistance)
		{
			half3 oldPos = rayPos - stepDistance * rayDir;

			half oldHeight = getHeight(oldPos.xz);

			half oldDistToTerrain = abs(oldHeight - oldPos.y);

			half currentHeight = getHeight(rayPos.xz);

			half currentDistToTerrain = rayPos.y - currentHeight;

			half weight = currentDistToTerrain / (currentDistToTerrain - oldDistToTerrain);

			half2 weightedTexPos = oldPos.xz * weight + rayPos.xz * (1.0 - weight);
			
			return currentHeight.xx;//weightedTexPos;
		}


            half4 frag(v2f IN) : SV_Target
            {
                //half4 customColor;

			 //   float3 rayDir = normalize(IN.tangentViewDir);
                //customColor= float4(abs(rayDir),1);

			half3 rayPos = half3(IN.uv_NoiseTex.x, 0.0, IN.uv_NoiseTex.y);

			half3 rayDir = normalize(IN.tangentViewDir);

			half modif = pow(abs(rayDir.y), 0.753);
			
			
			int STEPS = (int)(100.0 * modif);
			half stepDistance = 0.0001;
			//stepDistance *= modif;

			float4 finalColor = _WaterColor*modif * 0.1;
			
			half lastheight = 0.0;
			half2 pt = half2(0.0, 0.0);
			//half shdw = 0.0;

			//for (int i = 10; i < STEPS; i++)
			for (int i = 10; i < STEPS; i++)
			{
			//	stepDistance*=1-rayDir.y;
				//Get the current height at this uv coordinate
				half height = getHeight(rayPos.xz);
				
				//If the ray is below the surface
				if (rayPos.y < height)
				{
					//Get the texture position by interpolation between the position where we hit terrain and the position before
					lastheight = height;
					half2 weightedTex = getWeightedTexPos(rayPos, rayDir, stepDistance);
					
					half height = getHeight(weightedTex);
					pt=weightedTex;
					finalColor = getBlendTexture(weightedTex, height);
					half gh = getHeight(weightedTex);
					break;
				}
				rayPos += stepDistance * rayDir;
			}

                //return float4(abs(rayDir), 1);
				return half4(finalColor.rgb, 1.0);
            }
            ENDHLSL
        }
    }
	 FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
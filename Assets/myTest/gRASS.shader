// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Unlit/gRASS" {
   Properties  	{
		_NoiseTex("Noise texture", 2D) = "white" {}
		_GrassTex("Grass (RGB)", 2D) = "white" {}
		_StoneTex("Stone (RGB)", 2D) = "white" {}
		_GrassColor("Water (RGB)", Color) = (1.0, 1.0, 1.0, 1.0)
		_Height("Height", Range(0.0001, 5.0)) = 1.0
        _InvFade ("Soft Factor", Range(0.01, 3.0)) = 1.0
        _depthPow ("Depth pow", float) = 1.0
        _depthFactor ("Depth Factor", float) = 1.0
	}
	
	SubShader  	{
		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
		LOD 300
		//ZWrite Off
		ZWrite On
        Blend SrcAlpha OneMinusSrcAlpha


		CGPROGRAM
		   //HLSLPROGRAM
		#pragma surface surf Lambert vertex:vert alpha
		#pragma target 3.0

		#include "UnityCG.cginc"

		//Input
		sampler2D _NoiseTex;
		sampler2D _GrassTex;
		sampler2D _StoneTex;
		float4 _GrassColor;
		float _Height;
		sampler2D _CameraDepthTexture;
		float _depthFactor;
		fixed _depthPow;
        float _InvFade;

		struct Input  {
			//What Unity can give you
			float2 uv_NoiseTex;
			float3 viewDir;
			//What you have to calculate yourself
			float3 tangentViewDir;
			float4 screenPos;
			float eyeDepth;
		};

		void vert(inout appdata_full v, out Input o) 		{
			COMPUTE_EYEDEPTH(o.eyeDepth);

			UNITY_INITIALIZE_OUTPUT(Input, o);
			//Transform the view direction from world space to tangent space			
			float3 worldVertexPos = mul(unity_ObjectToWorld, v.vertex).xyz;
			float3 worldViewDir = worldVertexPos - _WorldSpaceCameraPos;
			//To convert from world space to tangent space we need the following
			//https://docs.unity3d.com/Manual/SL-VertexFragmentShaderExamples.html
			float3 worldNormal = UnityObjectToWorldNormal(v.normal);
			float3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
			float3 worldBitangent = cross(worldNormal, worldTangent) * v.tangent.w * unity_WorldTransformParams.w;
			//Use dot products instead of building the matrix
			o.tangentViewDir = float3(
				dot(worldViewDir, worldTangent),
				dot(worldViewDir, worldNormal),
				dot(worldViewDir, worldBitangent)
				);
		}

		//Animate the uv coordinates so the landscape is moving
		float2 animateUV(float2 texturePos) {
			texturePos.x += sin(_Time.y*texturePos.x * 0.75 * (1.0 - texturePos.y)) * 0.0008;//_Time.x * 5;
			texturePos *= 400.0;
			return texturePos;
		}

		//Get the height from a uv position
		float getHeight(float2 texturePos) {
			texturePos = animateUV(texturePos);
			float direction = -1.0;
			float landscapeFlatter = 0.1;
			//Multiply with 0.2 to make the landscape flatter
			float4 colorNoise = tex2Dlod(_NoiseTex, float4(texturePos * landscapeFlatter, 0.0, 0.0));
			//Calculate the height at this uv coordinate
			//Just use r because r = g = b  because color is grayscale
			//(1-color.r) because black should be low
			//-1 because the ray is going down so the ray's y-coordinate will be negative
			//float height = ((colorNoise.r) * -1 * _Height);
			float height = ((colorNoise.r) * direction * _Height);
			return height;
		}

		//Combine stone and grass depending on grayscale color
		float4 getBlendTexture(float2 texturePos, float height) {						
			float maxColorGray = 0.4;
			float offset = 0.1;
			float maxMixedColor = 0.7;
			float stepMixedColor = 0.5;
			texturePos = animateUV(texturePos);
			//To make it look nice by making the texture a little bigger
			float textureSize = 0.1;
			float4 colorGrass = tex2Dlod(_GrassTex, float4(texturePos * textureSize, 0.0, 0.0));
			float4 colorStone = tex2Dlod(_StoneTex, float4(texturePos * textureSize, 0.0, 0.0));
			//Height is negative so convert it to positive, also invert it so mountains are high and not the grass
			//Divide with _Height because this height is actual height and we need it in 0 -> 1 range
			float colorGrayscale = 1 - (abs(height) / _Height);
			colorGrayscale = (-abs(height) / _Height);
			//Combine grass and stone depending on height
			float4 mixedColor = lerp(colorGrass, colorStone, colorGrayscale);

			/////
			//Grass
			//if (colorGrayscale < maxColorGray)
			//{
				//mixedColor = clamp(.00625/tex2Dlod(_NoiseTex, float4(texturePos * .1, 0, 0))-.01,0,.8) * (_GrassColor);
				mixedColor = clamp(.00625/tex2Dlod(_NoiseTex, float4(texturePos * offset, 0.0, 0.0)), 0.0, maxMixedColor) * stepMixedColor * (_GrassColor);
			//}

			return mixedColor * (colorGrayscale < maxColorGray);
		}


		//Get the texture position by interpolation between the position where we hit terrain and the position before
		float2 getWeightedTexPos(float3 rayPos, float3 rayDir, float stepDistance) 		{
			//Move one step back to the position before we hit terrain
			float3 oldPos = rayPos - stepDistance * rayDir;
			float oldHeight = getHeight(oldPos.xz);
			//Always positive
			float oldDistToTerrain = abs(oldHeight - oldPos.y);
			float currentHeight = getHeight(rayPos.xz);
			//Always negative
			float currentDistToTerrain = rayPos.y - currentHeight;
			float weight = currentDistToTerrain / (currentDistToTerrain - oldDistToTerrain);
			//Calculate a weighted texture coordinate
			//If height is -2 and oldHeight is 2, then weightedTex is 0.5, which is good because we should use 
			//the exact middle between the coordinates
			float2 weightedTexPos = oldPos.xz * weight + rayPos.xz * (1.0 - weight);
			//weightedTexPos = float2(sin(weightedTexPos.x*3.14),cos(weightedTexPos.y*3.14));
			return weightedTexPos;
		}
		
/*		float3 ColorBelowWater (float4 screenPos) {
			float2 uv = screenPos.xy / screenPos.w;
	float backgroundDepth =
		LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv));
	float surfaceDepth = UNITY_Z_0_FAR_FROM_CLIPSPACE(screenPos.z);
	float depthDifference = backgroundDepth - surfaceDepth;
	
	return depthDifference / 20;
		}*/
		void surf (Input IN, inout SurfaceOutput o) {
			//Where is the ray starting? y is up and we always start at the surface
			float3 rayPos = float3(IN.uv_NoiseTex.x, 0, IN.uv_NoiseTex.y);
			//What's the direction of the ray?
			float3 rayDir = normalize(IN.tangentViewDir);
			//Find where the ray is intersecting with the terrain with a raymarch algorithm
			int STEPS = (int)clamp(100 * (IN.viewDir.g + 0.5), 10, 60);
			float stepDistance = 0.0001;
			//The default color used if the ray doesnt hit anything
			float4 finalColor = _GrassColor*IN.viewDir.g * 0.1;
			float lastheight = 0.0;
			float2 pt = float2(0.0, 0.0);
			float shdw = 0.0;


			for (int i = 0; i < STEPS; i++) {
				//Get the current height at this uv coordinate
				float height = getHeight(rayPos.xz);

				//If the ray is below the surface
				if (rayPos.y < height) {
					//Get the texture position by interpolation between the position where we hit terrain and the position before
					float2 weightedTex = getWeightedTexPos(rayPos, rayDir, stepDistance);
					//float height = getHeight(weightedTex);
					height = getHeight(weightedTex);
					lastheight = height;
					pt = weightedTex;
					finalColor = getBlendTexture(weightedTex, height);
					float gh = getHeight(weightedTex - _WorldSpaceLightPos0.xz * 0.1);
				//if(gh > height)
				//{
					shdw = (gh - height) * 10.0 * (gh > height);
				//}/**/
						break;
					//We have hit the terrain so we dont need to loop anymore	
				}
				//_WorldSpaceLightPos0.xyz
				//Move along the ray
				rayPos += stepDistance * rayDir;
			}


			 //  int i = 0;
			 //  float lastHeight = 0.0;
			 //  float2 lastPt = float2(0.0, 0.0);;
			 //  float4 lastfinalColor = _GrassColor*IN.viewDir.g * 0.1;
			 //  float3 lastRayPos = float3(IN.uv_NoiseTex.x, 0, IN.uv_NoiseTex.y);

				////for (int i = 0; i < STEPS; i++) {
				//while(i < STEPS) {

				////Get the current height at this uv coordinate
				//float heightRayPos = getHeight(rayPos.xz);
				////If the ray is below the surface

				////if (rayPos.y < heightRayPos) {

				//bool isHeightRayPos = rayPos.y < heightRayPos;

				//	//Get the texture position by interpolation between the position where we hit terrain and the position before
				//	float2 weightedTex = getWeightedTexPos(rayPos, rayDir, stepDistance);
					
				//	float height = getHeight(weightedTex);

				//	lastheight = height * isHeightRayPos + lastHeight * !isHeightRayPos;

				//	pt = weightedTex * isHeightRayPos + lastPt * !isHeightRayPos;

				//	finalColor = getBlendTexture(weightedTex, height) * isHeightRayPos + lastfinalColor * !isHeightRayPos;

				//	float gh = getHeight(weightedTex - _WorldSpaceLightPos0.xz * 0.1);
				////if(gh > height)
				////{
				//	shdw = (gh - height) * 10.0 * (gh > height);
				////}/**/
				//        lastHeight = lastheight;
				//		lastPt = pt;
				//		lastfinalColor = finalColor;
				//		//break;
				//	//We have hit the terrain so we dont need to loop anymore	

				////}
				//i++;
				//i = i * !isHeightRayPos * STEPS;
				////_WorldSpaceLightPos0.xyz
				////Move along the ray
				//}
				//rayPos += stepDistance * rayDir;
				//lastRayPos = rayPos;

			//}




			//if (lastheight != 0) {



			 //   float4 lastFinalColor = finalColor;
			 //   bool isLastHeight = (lastheight != 0);
				//float microstep = .00001;
				//float3 p1 = float3(pt.x,lastheight,pt.y);
				//float3 p2 = float3(pt.x-microstep,getHeight(float2(pt.x-microstep,pt.y)),pt.y);
				//float3 p3 = float3(pt.x,getHeight(float2(pt.x,pt.y-microstep)),pt.y-microstep);
				//float3 A = p2-p1;
				//float3 B = p3-p1;
				//float3 normal = float3(A.y*B.z-A.z*B.y,A.z*B.x-A.x*B.z,A.x*B.y-A.y*B.x);
				//normal = normalize(normal);
				//float a = length(_WorldSpaceLightPos0.xyz-normal) * 0.5;
				//a *= a;
				//finalColor = float4(finalColor.rgb*(1.0 - a), 1.0) * isLastHeight + lastFinalColor * !isLastHeight;


			//}/**/
			
			//float z = clipPos.z / clipPos.w;
			//Output
			/*float rawZ = SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(IN.screenPos));
			float sceneZ = LinearEyeDepth(rawZ);//length(finalColor);
			float partZ = IN.screenPos.z;
			float depth = sceneZ-partZ;
            float fade = 1.0;
            if ( rawZ > 0.0 ) {// Make sure the depth texture exists
                fade = saturate((abs(pow(depth,_depthPow)))/_depthFactor);
			}*/
			//finalColor-=shdw;
			o.Albedo = finalColor.rgb;//ColorBelowWater(IN.screenPos);//finalColor.rgb;
			o.Alpha = 1.0; //.9;
		}
		ENDCG
		  //ENDHLSL
	}
	FallBack "Diffuse"
}



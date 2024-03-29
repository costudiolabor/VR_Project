Shader "Unlit/DEPTH"
{
    Properties
    {
        [HDR] _Color("Color", Color) = (1, 1, 1, 1)
        _DepthFactor("Depth Factor", float) = 1.0
        _DepthPow("Depth Pow", float) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "IgnoreProjector"="True" "Queue" = "Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        LOD 100

        Pass
        {
            Tags { "LightMode" = "ForwardBase" }

            //CGPROGRAM
            HLSLPROGRAM 
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight

            //#include "UnityCG.cginc"
            #include "HLSLSupport.cginc"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 screenPos : TEXCOORD1;
            };

            float4 _Color;
            
            UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);
            sampler2D tempTexture;
            float _DepthFactor;
            float _DepthPow;


            v2f vert (appdata v)
            {
                v2f o;
                //o.vertex = UnityObjectToClipPos(v.vertex);
                o.vertex = TransformObjectToHClip(v.vertex);

                 // compute depth
                o.screenPos = ComputeScreenPos(o.vertex);

                LinearDepthToEyeDepth(o.screenPos.z);

                //COMPUTE_EYEDEPTH(o.screenPos.z);
                

                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                float4 col = _Color;
                 float zBuf;
                 //zBuf = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, _CameraDepthTexture, i.screenPos);
                 //float sceneZ = LinearEyeDepth(zBuf, _ZBufferParams);
                  //float sceneZ = LinearEyeDepth (SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.screenPos)));
                 // float depth = sceneZ - i.screenPos.z;

                  // fade with depth
                 // float depthFading = saturate((abs(pow(depth, _DepthPow))) / _DepthFactor);
                  //col *= depthFading;

                return col;
            }
            //ENDCG
		  ENDHLSL

        }
    }
}

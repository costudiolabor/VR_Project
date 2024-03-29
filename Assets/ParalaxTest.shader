Shader "MyShader/URP_Parallax"
{
    Properties
    {
        [Header(Base Color)]
        [MainTexture]_BaseMap("_BaseMap (Albedo)", 2D) = "white" {}
        [HDR][MainColor]_BaseColor("_BaseColor", Color) = (1,1,1,1)
        [Header(Bump Map)]
        [MainTexture]_NormalMap("_NormalMap", 2D) = "white" {}
        [MainTexture]_HeightMap("_HeightMap", 2D) = "white" {}
        _Height("_Height",float) = 1
    }
    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType"="Opaque"
            "UniversalMaterialType" = "Lit"
            "Queue"="Geometry"
        }

        Pass
        {
            Name "ForwardLit"
            Tags
            {
                "LightMode"="UniversalForward"
            }


            HLSLPROGRAM

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float3 positionOS   : POSITION;
                half3 normalOS      : NORMAL;
                half4 tangentOS     : TANGENT;
                float2 uv           : TEXCOORD0;
            };

            struct Varyings
            {
                float2 uv                       : TEXCOORD0;
                float4 positionWSAndFogFactor   : TEXCOORD1; // xyz: positionWS, w: vertex fog factor
                half3 normalWS                  : TEXCOORD2;
                float4 positionCS               : SV_POSITION;
                float3 lightTS                  : TEXCOORD3; // light Direction in tangent space
                float3 viewTS                   : TEXCOORD4; // camera direction in tangent space
            };


            sampler2D _BaseMap;
            sampler2D _NormalMap;
            sampler2D _HeightMap;


            CBUFFER_START(UnityPerMaterial)
            float4 _BaseMap_ST;
            float4 _NormalMap_ST;
            float4 _HeightMap_ST;
            float _Height;
            CBUFFER_END


            /*  MEMO
            from Core.hlsl
            struct VertexPositionInputs
            {
                float3 positionWS; // World space position
                float3 positionVS; // View space position
                float4 positionCS; // Homogeneous clip space position
                float4 positionNDC;// Homogeneous normalized device coordinates
            };
            from Core.hlsl
            struct VertexNormalInputs
            {
                real3 tangentWS;
                real3 bitangentWS;
                float3 normalWS;
            };
            from Lighting.hlsl
            struct Light
            {
                half3   direction;
                half3   color;
                half    distanceAttenuation;
                half    shadowAttenuation;
            };
            */

            Varyings vert (Attributes input)
            {
                Varyings output;

                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS);
                VertexNormalInputs vertexNormalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);

                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                output.positionCS = TransformWorldToHClip(vertexInput.positionWS);
                //output.normalWS = vertexNormalInput.normalWS;

                // Get Main Light
                Light mainLight = GetMainLight();

                // world to tangent
                float3x3 tangentMat = float3x3(vertexNormalInput.tangentWS, vertexNormalInput.bitangentWS, vertexNormalInput.normalWS);
                output.lightTS = mul(tangentMat, mainLight.direction);;


                // view direction
                float3 camera = _WorldSpaceCameraPos;
                output.viewTS = mul(tangentMat, normalize(camera - vertexInput.positionWS));

                return output;
            }

            half4 frag (Varyings input) : SV_Target
            {
                float height = tex2D(_HeightMap, input.uv) * _Height * 0.01;

                // creepy animation
                //height *=  sin(_Time.y * 2) + 1;

                float2 uvh = input.uv + input.viewTS.xy * height;
                half4 col = tex2D(_BaseMap, uvh);
                float3 normal = UnpackNormal(tex2D(_NormalMap, uvh));
                float diff = saturate(dot(input.lightTS, normal));

                col *= diff;
                return col;
            }
            ENDHLSL
        }
    }
}














//Shader "Unlit/ParalaxTest"
//{
//    Properties
//    {
//        _MainTex ("Texture", 2D) = "white" {}
//    }
//    SubShader
//    {
//        Tags { "RenderType"="Opaque" }
//        LOD 100

//        Pass
//        {
//            CGPROGRAM
//            #pragma vertex vert
//            #pragma fragment frag
//            // make fog work
//            #pragma multi_compile_fog

//            #include "UnityCG.cginc"

//            struct appdata
//            {
//                float4 vertex : POSITION;
//                float2 uv : TEXCOORD0;
//            };

//            struct v2f
//            {
//                float2 uv : TEXCOORD0;
//                UNITY_FOG_COORDS(1)
//                float4 vertex : SV_POSITION;
//            };

//            sampler2D _MainTex;
//            float4 _MainTex_ST;

//            v2f vert (appdata v)
//            {
//                v2f o;
//                o.vertex = UnityObjectToClipPos(v.vertex);
//                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
//                UNITY_TRANSFER_FOG(o,o.vertex);
//                return o;
//            }

//            fixed4 frag (v2f i) : SV_Target
//            {
//                // sample the texture
//                fixed4 col = tex2D(_MainTex, i.uv);
//                // apply fog
//                UNITY_APPLY_FOG(i.fogCoord, col);
//                return col;
//            }
//            ENDCG
//        }
//    }
//}

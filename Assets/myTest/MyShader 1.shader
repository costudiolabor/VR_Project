Shader "Unlit/MyShader" {
    Properties     {
        _MainTex ("_MainTex", 2D) = "white" {}
        _MainColor("_MainColor (RGB)", Color) = (1.0, 1.0, 1.0, 1.0)
        _Height("Height", Range(0.0001,5.0)) = 1.0
    }

    SubShader {
        Tags {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType"="Opaque"
        }

        Pass {
            HLSLPROGRAM
            //#pragma enable_d3d11_debug_symbols
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct appdata {
                float4 positionOS : POSITION;
                float4 vertex : POSITION;
                float3 normal : TEXCOORD1;
                float4 tangent: TANGENT;
                float2 uv_MainTex:TEXCOORD3;
            };

            struct v2f {
                float4 positionHCS : SV_POSITION;
                float3 tangentViewDir: TEXCOORD1;
                float4 tangent: TANGENT;
                float2 uv_MainTex:TEXCOORD3;
            };

            sampler2D _MainTex;
            float4 _MainColor;
            float4 _MainTex_ST;
            float _Height;

            v2f vert(appdata v) {
                v2f OUT;
                OUT.positionHCS = TransformObjectToHClip(v.vertex.xyz);
                OUT.uv_MainTex = TRANSFORM_TEX(v.uv_MainTex, _MainTex);
                OUT.tangent = _MainTex_ST;

                //Transform the view direction from world space to tangent space
                //Преобразуйте направление обзора из мирового пространства в касательное пространство
                
                float3 worldVertexPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                float3 worldViewDir = worldVertexPos - _WorldSpaceCameraPos;

                //To convert from world space to tangent space we need the following
                //Для преобразования из мирового пространства в касательное пространство нам понадобится следующее
                
                //https://docs.unity3d.com/Manual/SL-VertexFragmentShaderExamples.html
                float3 worldNormal = TransformObjectToWorldNormal(v.normal);
                float3 worldTangent = TransformObjectToWorldDir(v.tangent.xyz);
                float3 worldBitangent = cross(worldNormal, worldTangent) * v.tangent.w * unity_WorldTransformParams.w;

                //Use dot products instead of building the matrix
                //Используйте точечные произведения вместо построения матрицы
                
                OUT.tangentViewDir = half3(
                    dot(worldViewDir, worldTangent),
                    dot(worldViewDir, worldNormal),
                    dot(worldViewDir, worldBitangent)
                );
                return OUT;
            }


            float2 animateUV(float2 texturePos)             {
                //texturePos.x += sin(_Time.y * texturePos.x * 0.75 * (1.0 - texturePos.y)) * 0.0008; //_Time.x * 5
                texturePos.x += sin(_Time.y * texturePos.x * 0.75 * (1.0 - texturePos.y)) * 0.0008; //_Time.x * 5
                texturePos *= 400.0;
                return texturePos;
            }

            float getHeight(float2 texturePos) {
                texturePos = animateUV(texturePos);
                //Multiply with 0.2 to make the landscape flatter
                //Умножьте на 0,2, чтобы сделать ландшафт более плоским
                
                float4 colorNoise = tex2Dlod(_MainTex, float4(texturePos * 0.1, 0.0, 0.0));
                //Calculate the height at this uv coordinate
                //Just use r because r = g = b  because color is grayscale
                //(1-color.r) because black should be low
                //-1 because the ray is going down so the ray's y-coordinate will be negative
                //Вычислите высоту по этой координате uv
                //Просто используйте r, потому что r = g = b, потому что цвет в оттенках серого
                //(1-color.r), потому что черный должен быть низким
                //-1, потому что луч идет вниз, поэтому координата y луча будет отрицательной
                
                float height = ((colorNoise.r) * -1.0 * _Height);
                return height;
            }

            //Combine stone and grass depending on grayscale color
            //Комбинируйте камень и траву в зависимости от оттенков серого
            float4 getBlendTexture(float2 texturePos, float height) {
                float maxColorGray = 0.4;
                float offset = 0.1;
                float maxMixedColor = 0.7;
                float stepMixedColor = 0.5;
                texturePos = animateUV(texturePos);
                
                //To make it look nice by making the texture a little bigger
                //Чтобы это выглядело красиво, сделайте текстуру немного крупнее
                float textureSize = 0.1;
                
                //float4 colorGrass = tex2Dlod(_GrassTex, float4(texturePos * textureSize, 0.0, 0.0));
                //float4 colorStone = tex2Dlod(_StoneTex, float4(texturePos * textureSize, 0.0, 0.0));
                
                //Height is negative so convert it to positive, also invert it so mountains are high and not the grass
                //Divide with _Height because this height is actual height and we need it in 0 -> 1 range
                // Высота отрицательна, поэтому преобразуйте ее в положительную, также инвертируйте, чтобы горы были высокими, а не трава
                // Разделите на _Height, потому что эта высота является фактической высотой, и нам нужно, чтобы она была в диапазоне 0 -> 1
                float colorGrayscale = 1 - (abs(height) / _Height);
                colorGrayscale = (-abs(height) / _Height);
                
                //Combine grass and stone depending on height
                //Комбинируйте траву и камень в зависимости от высоты
                //float4 mixedColor = lerp(colorGrass, colorStone, colorGrayscale);

                //Grass
                float4 mixedColor = clamp(.00625 / tex2Dlod(_MainTex, float4(texturePos * offset, 0.0, 0.0)), 0.0,
                    maxMixedColor) * stepMixedColor * (_MainColor);
                
                return mixedColor * (colorGrayscale < maxColorGray);
            }

            float2 getWeightedTexPos(float3 rayPos, float3 rayDir, half stepDistance) {
                float3 oldPos = rayPos - stepDistance * rayDir;
                float oldHeight = getHeight(oldPos.xz);
                float oldDistToTerrain = abs(oldHeight - oldPos.y);
                float currentHeight = getHeight(rayPos.xz);
                float currentDistToTerrain = rayPos.y - currentHeight;
                float weight = currentDistToTerrain / (currentDistToTerrain - oldDistToTerrain);
                float2 weightedTexPos = oldPos.xz * weight + rayPos.xz * (1.0 - weight);
                return weightedTexPos;
            }
            
            float4 frag(v2f IN) : SV_Target {
                float3 rayPos = float3(IN.uv_MainTex.x, 0.0, IN.uv_MainTex.y);
                float3 rayDir = normalize(IN.tangentViewDir);
                
                float4 debugValue = float4(0.5, 0.5, 0.5, 1.0);
                
                //float modif = pow(abs(rayDir.y), 0.753);
                float modif = pow(abs(rayDir.y), 0.1);

                int STEPS = (int)(100.0 * modif);
                float stepDistance = 0.0001;
                float4 finalColor = _MainColor * modif * 0.1;
                    float lasth=0;
                
                for (int i = 1; i < STEPS; i++) {
                    
                    //Get the current height at this uv coordinate
                    //Получить текущую высоту в этой uv координате
                    float height = getHeight(rayPos.xz);
                    //If the ray is below the surface
                    //Если луч находится ниже поверхности
                    if (rayPos.y < height) {
                        //Get the texture position by interpolation between the position where we hit terrain and the position before
                        //Получаем положение текстуры путем интерполяции между положением, в котором мы попали на рельеф, и положением до этого
                        float2 weightedTex = getWeightedTexPos(rayPos, rayDir, stepDistance);
                        float height = getHeight(weightedTex);
                        lasth = height;
                        finalColor = getBlendTexture(weightedTex, height);
                        break;
                    }
                    
                    rayPos += stepDistance * rayDir;
                }

                //float dist = length(_WorldSpaceViewDir(v.vertex));
                //finalColor.xyz = abs(lasth.xxx)*100;
                
                //return float4(abs(rayDir), 1);
                return float4(finalColor.rgb, 1.0);
                //return float4(, 1.0);
            }

            //half4 frag (v2f IN) : SV_Target {
            //    half4  col = tex2D(_MainTex, IN.uv) * _MainColor;
            //    return col;
            //}
            ENDHLSL
        }
    }
}
















//Shader "VertexInputSimple" {
//    SubShader {
//        Pass {
//            CGPROGRAM
//            #pragma vertex vert
//            #pragma fragment frag
//            #include "UnityCG.cginc"

//            struct v2f {
//                float4 pos : SV_POSITION;
//                fixed4 color : COLOR;
//            };

//            v2f vert (appdata_base v)
//            {
//                v2f o;
//                o.pos = UnityObjectToClipPos(v.vertex);
//                o.color.xyz = v.normal * 0.5 + 0.5;
//                o.color.w = 1.0;
//                return o;
//            }

//            fixed4 frag (v2f i) : SV_Target { return i.color; }
//            ENDCG
//        }
//    } 
//}
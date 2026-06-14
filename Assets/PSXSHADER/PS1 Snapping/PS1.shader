Shader "Unlit/PSX_Composite"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _ColorDepth ("Color Depth (32 for PSX)", Float) = 32.0
        _DitherStrength ("Dither Strength", Range(0, 1)) = 0.1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 screenPos : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _ColorDepth;
            float _DitherStrength;

            // Матрица Байера 4x4 для дизеринга
            static const float4x4 bayerMatrix = float4x4(
                0,  8,  2, 10,
                12, 4, 14,  6,
                3, 11,  1,  9,
                15, 7, 13,  5
            ) / 16.0;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.screenPos = ComputeScreenPos(o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // 1. Берем исходный цвет с Рендер Текстуры
                fixed4 col = tex2D(_MainTex, i.uv);

                // 2. Вычисляем координаты пикселя для наложения шума (Dither)
                float2 pixelPos = i.screenPos.xy / i.screenPos.w * _ScreenParams.xy;
                int x = (int)fmod(pixelPos.x, 4);
                int y = (int)fmod(pixelPos.y, 4);
                
                // Получаем значение из матрицы
                float ditherValue = bayerMatrix[x][y];

                // 3. Добавляем дизеринг перед урезанием цветов
                // Это позволяет "смешать" границы цветов
                col.rgb += (ditherValue - 0.5) * _DitherStrength;

                // 4. Постеризация (урезание цветов)
                // PSX использовала 5 бит на канал (32 градации)
                col.rgb = floor(col.rgb * _ColorDepth) / _ColorDepth;

                return col;
            }
            ENDCG
        }
    }
}
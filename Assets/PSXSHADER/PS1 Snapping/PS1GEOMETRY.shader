Shader "Unlit/PSX_GeometryLit"
{
    Properties
    {
        [MainTexture] _BaseMap("Texture", 2D) = "white" {}
        [MainColor] _BaseColor("Color", Color) = (1, 1, 1, 1)
        
        // Настройка силы "похеривания". Чем меньше число, тем жестче сетка.
        // Оптимально для 320x240: около 100-200.
        _SnapGridSize("Snap Grid Size", Float) = 160.0 
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
        LOD 100

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // Нужен для работы с URP
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl" 

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float2 uv           : TEXCOORD0;
                float4 color        : COLOR; // Поддержка Vertex Color (важно для PSX!)
            };

            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
                float2 uv           : TEXCOORD0;
                float4 color        : COLOR;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                half4 _BaseColor;
                float _SnapGridSize;
            CBUFFER_END

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            Varyings vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                
                // --- МАГИЯ НАЧИНАЕТСЯ ЗДЕСЬ ---
                
                // 1. Получаем позицию в Clip Space (пространство экрана до деления на перспективу)
                float4 clipPos = vertexInput.positionCS;

                // 2. "Херим" координаты.
                // Мы берем XY координаты, умножаем на размер сетки, округляем (floor) и делим обратно.
                // Это заставляет вершины прилипать к узлам воображаемой сетки.
                
                // Защита от деления на ноль, если вдруг _SnapGridSize будет 0
                float snapRes = max(_SnapGridSize, 0.001); 

                // Важно: округляем clipPos.xy / clipPos.w, чтобы сетка зависела от перспективы
                float2 snappedPos = clipPos.xy / clipPos.w; 
                snappedPos = floor(snappedPos * snapRes) / snapRes;
                clipPos.xy = snappedPos * clipPos.w;

                output.positionCS = clipPos;

                // --- МАГИЯ ЗАКАНЧИВАЕТСЯ ---

                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                // Передаем Vertex Color умноженный на основной цвет материала
                output.color = input.color * _BaseColor; 
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // Просто берем текстуру и умножаем на вертекс колор
                half4 texColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv);
                return texColor * input.color;
            }
            ENDHLSL
        }
    }
}
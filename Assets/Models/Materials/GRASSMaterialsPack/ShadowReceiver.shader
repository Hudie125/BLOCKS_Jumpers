Shader "Custom/ShadowReceiver"
{
    Properties
    {
        _Color ("Shadow Color", Color) = (0,0,0,0.5)
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100

        Pass
        {
            Name "FORWARD"
            Tags { "LightMode" = "ForwardBase" }
            
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdbase
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                LIGHTING_COORDS(0,1)
            };

            fixed4 _Color;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                // Заменяем проблемную строку на стандартный макрос переноса теней
                TRANSFER_SHADOW(o);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Вычисляем затенение (1.0 - светло, 0.0 - тень)
                UNITY_LIGHT_ATTENUATION(attenuation, i, i.pos);
                
                // Рисуем цвет только там, где есть тень
                return fixed4(_Color.rgb, (1.0 - attenuation) * _Color.a);
            }
            ENDCG
        }
    }
    Fallback "Transparent/VertexLit"
}
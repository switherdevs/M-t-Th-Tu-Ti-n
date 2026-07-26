Shader "Custom/LocalFadeSpriteShader"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _PlayerMin ("Player Bounds Min", Vector) = (0,0,0,0)
        _PlayerMax ("Player Bounds Max", Vector) = (0,0,0,0)
        _MinAlpha ("Min Alpha", Float) = 0.3
        _Smoothness ("Smoothness", Float) = 0.3
    }

    SubShader
    {
        Tags 
        { 
            "Queue"="Transparent" 
            "IgnoreProjector"="True" 
            "RenderType"="Transparent" 
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
            };

            fixed4 _Color;
            sampler2D _MainTex;
            float4 _PlayerMin;
            float4 _PlayerMax;
            float _MinAlpha;
            float _Smoothness;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.worldPos = mul(unity_ObjectToWorld, IN.vertex).xyz;
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                fixed4 c = tex2D(_MainTex, IN.texcoord) * IN.color;

                // Kiểm tra xem Pixel hiện tại của vật thể có nằm trong Bounding Box của Collider Player không
                if (_PlayerMax.x > _PlayerMin.x)
                {
                    // Tính khoảng cách từ Pixel tới ranh giới Collider
                    float dx = max(_PlayerMin.x - IN.worldPos.x, IN.worldPos.x - _PlayerMax.x);
                    float dy = max(_PlayerMin.y - IN.worldPos.y, IN.worldPos.y - _PlayerMax.y);
                    float dist = max(dx, dy);

                    // Nếu Pixel nằm bên trong hoặc sát viền Collider, thực hiện làm mờ
                    if (dist < _Smoothness)
                    {
                        float fadeFactor = saturate((dist + _Smoothness) / (_Smoothness * 2.0));
                        float targetAlpha = lerp(_MinAlpha, 1.0, fadeFactor);
                        c.a *= targetAlpha;
                    }
                }

                c.rgb *= c.a;
                return c;
            }
            ENDCG
        }
    }
}
Shader "Unlit/S_MapMask"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Coordinates("Coordinate", Vector) = (0,0,0,0)
        _Color("Draw Color", Color) = (1,0,0,0)
        _Strength("Strength", Range(0,1)) = 1
        _Size("Size", Range(1,500)) = 0
        _EdgeSharpness("Edge Sharpness", Range(1,500)) = 5
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
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Coordinates, _Color;
            half _Size, _Strength, _EdgeSharpness;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 prevMask = tex2D(_MainTex, i.uv);

                float dist = distance(i.uv, _Coordinates.xy);

                float draw = pow(saturate(1 - (dist * _EdgeSharpness)), 500 / _Size);
                draw = smoothstep(0.0, 1.0 / _EdgeSharpness, draw);

                fixed4 drawcol = _Color * (draw * _Strength);

                return max(prevMask, drawcol);
            }
            ENDCG
        }
    }
}

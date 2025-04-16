Shader "Unlit/S_Laplacian"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _MapThreshold ("Edge Threshold", Range(0, 1)) = 0.1
    }
    
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

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
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }
            
            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            float _MapThreshold;
            
            // Luminance calculation macro
            #define LUM(c) ((c).r * 0.3 + (c).g * 0.59 + (c).b * 0.11)
            
            fixed4 frag (v2f i) : SV_Target
            {
                // Sample the texture in a 3x3 pattern (but only the cross pixels)
                float3 C = tex2D(_MainTex, i.uv).rgb; // Center
                float3 N = tex2D(_MainTex, i.uv + fixed2(0, _MainTex_TexelSize.y)).rgb; // North
                float3 S = tex2D(_MainTex, i.uv - fixed2(0, _MainTex_TexelSize.y)).rgb; // South
                float3 W = tex2D(_MainTex, i.uv + fixed2(_MainTex_TexelSize.x, 0)).rgb; // West
                float3 E = tex2D(_MainTex, i.uv - fixed2(_MainTex_TexelSize.x, 0)).rgb; // East
                
                // Calculate luminance for each sample
                float C_lum = LUM(C);
                float N_lum = LUM(N);
                float S_lum = LUM(S);
                float W_lum = LUM(W);
                float E_lum = LUM(E);
                
                // Laplacian edge detection
                float L_lum = saturate(N_lum + S_lum + W_lum + E_lum - 4 * C_lum);
                
                // Apply threshold to create binary edge map
                L_lum = step(_MapThreshold, L_lum);
                
                return float4(L_lum, L_lum, L_lum, 1);
            }
            ENDCG
        }
    }
}

Shader "Unlit/S_Hole"
{
    SubShader
    {
        Tags {"Queue" = "AlphaTest+1"}

        ColorMask 0
        ZWrite On

        Pass {}
    }
}

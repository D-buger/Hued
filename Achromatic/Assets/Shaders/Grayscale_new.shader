Shader"Unlit/Grayscale"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("ActivationColor", Color) = (0, 0, 0, 1)
        _Smoothless ("Smoothless", Range(0, 1)) = 0.5
        _Sensitivity ("Sensitivity", Range(0, 1)) = 0.8
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert

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
            
sampler2D _MainTex;
float4 _MainTex_ST;
float4 _Color;
half _Smoothless;
half _Sensitivity;
            
v2f vert(appdata v)
{
    v2f o;
    o.vertex = UnityObjectToClipPos(v.vertex);
    o.uv = TRANSFORM_TEX(v.uv, _MainTex);
    return o;
}
            
fixed4 frag(v2f i) : SV_Target
{
                            // sample the texture
    fixed4 col = tex2D(_MainTex, i.uv);
                
    float gray = col.r * 0.299 + col.g * 0.587 + col.b * 0.114;
    float4 grayscale = float4(gray, gray, gray, 1);
                
    if (_Color.r > 0 || _Color.g > 0 || _Color.b > 0)
    {
        if (col.b <= col.r && col.g <= col.r && _Color.r > 0)
        {
            grayscale.r = col.r;
            grayscale.g = col.g;
            grayscale.b = col.b;
        }
        if (col.b <= col.g && col.r <= col.g && _Color.g > 0)
        {
            grayscale.r = col.r;
            grayscale.g = col.g;
            grayscale.b = col.b;
        }
        if (col.r <= col.b && col.g <= col.b && _Color.b > 0)
        {
            grayscale.r = col.r;
            grayscale.g = col.g;
            grayscale.b = col.b;
        }
    }
                
    return grayscale;
}
            ENDCG
        }
    }
}

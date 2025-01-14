Shader "Custom/CRTShader"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _Color("Main Color", Color) = (0.0, 0.0, 1.0, 1)
        _ScanlineIntensity("Scanline Intensity", Range(0, 1)) = 0.5
        
    }
        SubShader
        {
            Tags { "RenderType" = "Opaque" }
            Pass
            {
                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #include "UnityCG.cginc"
                

                struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
                struct v2f { float2 uv : TEXCOORD0; float4 pos : SV_POSITION; };

                sampler2D _MainTex;
                float _ScanlineIntensity;
                float4 _Color;

                v2f vert(appdata v) {
                    v2f o;
                    o.pos = UnityObjectToClipPos(v.vertex);
                    o.uv = v.uv;
                    return o;
                }

                fixed4 frag(v2f i) : SV_Target {
                    float2 uv = i.uv;
                    float scanline = sin(uv.y * 800.0) * _ScanlineIntensity;
                    fixed4 col = tex2D(_MainTex, uv);

                    col.rgb = _Color.rgb;
                    col.rgb += scanline;

                    return col;
                }
                ENDCG
            }
        }
}
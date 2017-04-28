// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Tango/PointCloud (Occlusion)" {
	Properties{
		point_size("Point Size", Float) = 10.0
	}
    SubShader {
        Tags { "Queue" = "Background-1" }
        Pass {
            ColorMask 0
        
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
               float4 vertex : POSITION;
            };
       
            struct v2f
            {
               float4 vertex : SV_POSITION;
               float size : PSIZE;
            };
           
			float point_size;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.size = 10;
                return o;
            }
           
            fixed4 frag (v2f i) : SV_Target
            {
                return fixed4(1, 1, 1, 1);
            }
            ENDCG
        }
    }
}

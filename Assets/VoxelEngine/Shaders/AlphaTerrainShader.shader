Shader "VoxelEngine/AlphaTerrainShader" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}
	SubShader {
		Tags { "RenderType"="Transparent" "Queue" = "Transparent+100" }
		LOD 200
		Cull Off
		Zwrite On
		ZTest On
		Blend SrcAlpha OneMinusSrcAlpha // Alpha blending
		
		CGPROGRAM
		#pragma surface surf Lambert alpha
		
		sampler2D _MainTex;

		struct Input {
			float2 uv_MainTex;
		};

		void surf (Input IN, inout SurfaceOutput o) {
			half4 c = tex2Dlod (_MainTex, float4(IN.uv_MainTex,0,0));
			o.Albedo = c.rgb;
			o.Alpha = c.a;
		}
		ENDCG
	} 
	FallBack "Diffuse"
}

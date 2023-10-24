// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "DBK/Rebar"
{
	Properties
	{
		_MainTex("MainTex", 2D) = "white" {}
		_Smoothness("Smoothness", 2D) = "white" {}
		_Normals("Normals", 2D) = "bump" {}
		_Cutoff( "Mask Clip Value", Float ) = 0.3
		_NormalsScale("Normals Scale", Range( 0 , 2)) = 4.080567
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "TransparentCutout"  "Queue" = "AlphaTest+0" }
		Cull Back
		CGPROGRAM
		#include "UnityStandardUtils.cginc"
		#pragma target 3.0
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform float _NormalsScale;
		uniform sampler2D _Normals;
		uniform float4 _Normals_ST;
		uniform sampler2D _MainTex;
		uniform float4 _MainTex_ST;
		uniform sampler2D _Smoothness;
		uniform float4 _Smoothness_ST;
		uniform float _Cutoff = 0.3;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_Normals = i.uv_texcoord * _Normals_ST.xy + _Normals_ST.zw;
			o.Normal = UnpackScaleNormal( tex2D( _Normals, uv_Normals ), _NormalsScale );
			float2 uv_MainTex = i.uv_texcoord * _MainTex_ST.xy + _MainTex_ST.zw;
			float4 tex2DNode1 = tex2D( _MainTex, uv_MainTex );
			o.Albedo = tex2DNode1.rgb;
			float2 uv_Smoothness = i.uv_texcoord * _Smoothness_ST.xy + _Smoothness_ST.zw;
			o.Smoothness = tex2D( _Smoothness, uv_Smoothness ).r;
			o.Alpha = 1;
			clip( tex2DNode1.a - _Cutoff );
		}

		ENDCG
	}
	Fallback "Diffuse"
}
/*ASEBEGIN
Version=18100
1927;84;1666;984;1223.616;173.267;1;True;False
Node;AmplifyShaderEditor.RangedFloatNode;10;-1044.75,302.4063;Inherit;False;Property;_NormalsScale;Normals Scale;4;0;Create;True;0;0;False;0;False;4.080567;1.807;0;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;21;-359.113,687.5356;Float;False;Constant;_Float6;Float 5;7;0;Create;True;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;18;-668.7016,225.8074;Inherit;True;Property;_Normals;Normals;2;0;Create;True;0;0;False;0;False;-1;e6aeb2ad4d01a6a43b80b8183f22fe23;025f906ac54c33c41aa39ba2f05df651;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;16;-753.2176,486.1059;Inherit;True;Property;_Smoothness;Smoothness;1;0;Create;True;0;0;False;0;False;-1;c95b89c88fbf3e0429daf4630eb88437;eb1c6adb4e77a5e4086c5ce509ba7784;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StaticSwitch;20;-264.113,521.5356;Float;False;Property;_Keyword0;Keyword 0;5;0;Fetch;True;0;0;False;0;False;0;0;0;True;UNITY_PASS_SHADOWCASTER;Toggle;2;Key0;Key1;Fetch;False;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;1;-647.7939,-92;Inherit;True;Property;_MainTex;MainTex;0;0;Create;True;0;0;False;0;False;-1;97eb01fbde9f0234ba9a2ca6218b1642;0ff8d15b6c99feb4eaa8345891568060;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;-44.49131,-3;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;DBK/Rebar;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Masked;0.3;True;True;0;False;TransparentCutout;;AlphaTest;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;3;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;18;5;10;0
WireConnection;20;1;1;4
WireConnection;20;0;21;0
WireConnection;0;0;1;0
WireConnection;0;1;18;0
WireConnection;0;4;16;0
WireConnection;0;10;1;4
ASEEND*/
//CHKSM=5C67E67F69C90177EB3F8FE40548821C3295CC36
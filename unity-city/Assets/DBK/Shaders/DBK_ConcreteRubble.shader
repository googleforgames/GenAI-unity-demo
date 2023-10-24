// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "DBK/ConcreteRubble"
{
	Properties
	{
		_MainTex1("Color Theme", 2D) = "white" {}
		_ColorMap("Color Map", 2D) = "white" {}
		_ConcreteBrightness("Concrete Brightness", Range( 0 , 0.1)) = 0
		[PerRendererData]_Color("Color", Int) = 0
		_PaintRange("Paint Range", Range( 0 , 1)) = 0
		_PaintSmooth("Paint Smooth", Range( 0 , 0.1)) = 0
		_MainNM("Main NM", 2D) = "bump" {}
		_DamageNM("Damage NM", 2D) = "bump" {}
		_DamageNMTiling("Damage NM Tiling", Range( 1 , 8)) = 1
		_DamageScale("Damage Scale", Range( 0 , 1)) = 0
		_DetailNM("Detail NM", 2D) = "bump" {}
		_DetailNMTiling("Detail NM Tiling", Range( 1 , 8)) = 0
		_DetailScale("Detail Scale", Range( 1 , 4)) = 0
		_DirtContrast("Dirt Contrast", Range( 0 , 2)) = 0
		_DirtRange("Dirt Range", Range( 0 , 1)) = 0
		[MaxGay]_DirtSmooth("Dirt Smooth", Range( 0 , 1)) = 0
		_EdgesAdd("Edges Add", Range( 0 , 0.25)) = 0
		_MaskB("Mask B", 2D) = "white" {}
		_MainTex("Mask A", 2D) = "white" {}
		_DirtInsideMultiplier("Dirt Inside Multiplier", Range( 0 , 1)) = 0
		_AODirt("AO Dirt", Range( 0 , 1)) = 0
		_MainSmoothness("Main Smoothness", Range( 0 , 1)) = 0
		_PaintSmoothness("Paint Smoothness", Range( 0 , 1)) = 0
		_DirtSmoothness("Dirt Smoothness", Range( 0 , 3)) = 0
		[Toggle(_USECUSTOMCOLOR_ON)] _UseCustomColor("Use Custom Color", Float) = 0
		_CustomColor("Custom Color", Int) = 0
		_Cutoff( "Mask Clip Value", Float ) = 0.5
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "TransparentCutout"  "Queue" = "AlphaTest+0" }
		Cull Back
		CGPROGRAM
		#include "UnityStandardUtils.cginc"
		#pragma target 4.6
		#pragma shader_feature _USECUSTOMCOLOR_ON
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows 
		struct Input
		{
			float2 uv_texcoord;
			float4 vertexColor : COLOR;
		};

		uniform sampler2D _MainNM;
		uniform float4 _MainNM_ST;
		uniform float _DetailScale;
		uniform sampler2D _DetailNM;
		uniform float _DetailNMTiling;
		uniform float _DamageScale;
		uniform sampler2D _DamageNM;
		uniform float _DamageNMTiling;
		uniform sampler2D _MainTex;
		uniform float4 _MainTex_ST;
		uniform sampler2D _ColorMap;
		uniform float4 _ColorMap_ST;
		uniform sampler2D _MaskB;
		uniform float _ConcreteBrightness;
		uniform float _AODirt;
		uniform sampler2D _MainTex1;
		uniform int _Color;
		uniform int _CustomColor;
		uniform float _PaintRange;
		uniform float _PaintSmooth;
		uniform float _DirtInsideMultiplier;
		uniform float _DirtRange;
		uniform float _DirtSmooth;
		uniform float _DirtContrast;
		uniform float _EdgesAdd;
		uniform float _MainSmoothness;
		uniform float _DirtSmoothness;
		uniform float _PaintSmoothness;
		uniform float _Cutoff = 0.5;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_MainNM = i.uv_texcoord * _MainNM_ST.xy + _MainNM_ST.zw;
			float3 tex2DNode2 = UnpackNormal( tex2D( _MainNM, uv_MainNM ) );
			float2 temp_cast_0 = (_DetailNMTiling).xx;
			float2 uv_TexCoord18 = i.uv_texcoord * temp_cast_0;
			float2 temp_cast_1 = (_DamageNMTiling).xx;
			float2 uv_TexCoord11 = i.uv_texcoord * temp_cast_1;
			float2 uv_MainTex = i.uv_texcoord * _MainTex_ST.xy + _MainTex_ST.zw;
			float4 tex2DNode75 = tex2D( _MainTex, uv_MainTex );
			float MaskABlue76 = tex2DNode75.b;
			float3 lerpResult10 = lerp( BlendNormals( tex2DNode2 , UnpackScaleNormal( tex2D( _DetailNM, uv_TexCoord18 ), _DetailScale ) ) , BlendNormals( tex2DNode2 , UnpackScaleNormal( tex2D( _DamageNM, uv_TexCoord11 ), _DamageScale ) ) , MaskABlue76);
			float3 Normals71 = lerpResult10;
			o.Normal = Normals71;
			float2 uv_ColorMap = i.uv_texcoord * _ColorMap_ST.xy + _ColorMap_ST.zw;
			float4 tex2DNode250 = tex2D( _ColorMap, uv_ColorMap );
			float2 ConcreteUVTiling73 = uv_TexCoord18;
			float4 tex2DNode85 = tex2D( _MaskB, ConcreteUVTiling73 );
			float Mask_B_Alpha89 = tex2DNode85.a;
			float MaskARed84 = tex2DNode75.r;
			float3 desaturateInitialColor247 = ( ( ( tex2DNode250 * Mask_B_Alpha89 ) + ( ( 1.0 - MaskABlue76 ) * _ConcreteBrightness ) ) * ( 1.0 - ( MaskARed84 * _AODirt ) ) ).rgb;
			float desaturateDot247 = dot( desaturateInitialColor247, float3( 0.299, 0.587, 0.114 ));
			float3 desaturateVar247 = lerp( desaturateInitialColor247, desaturateDot247.xxx, ( 1.0 - MaskABlue76 ) );
			half2 _ColorsNumber = half2(0,-0.1);
			#ifdef _USECUSTOMCOLOR_ON
				float staticSwitch94 = (float)_CustomColor;
			#else
				float staticSwitch94 = (float)_Color;
			#endif
			float2 temp_output_96_0 = ( half2( 0.015625,0 ) * staticSwitch94 );
			float2 uv_TexCoord98 = i.uv_texcoord * _ColorsNumber + temp_output_96_0;
			float4 Color1UV3105 = tex2D( _MainTex1, uv_TexCoord98 );
			float Mask_B_Blue88 = tex2DNode85.b;
			float Mask_B_Red86 = tex2DNode85.r;
			float3 temp_cast_6 = (( 1.0 - Mask_B_Red86 )).xxx;
			float Mask_B_Green87 = tex2DNode85.g;
			float VertexGreen112 = i.vertexColor.g;
			float3 temp_cast_7 = (( Mask_B_Green87 * VertexGreen112 )).xxx;
			float PaintSelection123 = ( saturate( ( 1.0 - ( ( distance( temp_cast_6 , temp_cast_7 ) - _PaintRange ) / max( _PaintSmooth , 1E-05 ) ) ) ) * ( 1.0 - MaskABlue76 ) );
			float4 lerpResult37 = lerp( float4( desaturateVar247 , 0.0 ) , ( Color1UV3105 + ( Mask_B_Blue88 * Color1UV3105 ) ) , PaintSelection123);
			float2 appendResult103 = (float2(temp_output_96_0.x , 0.1));
			float2 uv_TexCoord107 = i.uv_texcoord * _ColorsNumber + appendResult103;
			float4 DirtColorUV3109 = tex2D( _MainTex1, uv_TexCoord107 );
			float VertexRed125 = i.vertexColor.r;
			float3 temp_cast_9 = (( VertexRed125 * Mask_B_Alpha89 )).xxx;
			float3 temp_cast_10 = (( ( 1.0 - Mask_B_Red86 ) + ( ( 1.0 - MaskARed84 ) * _DirtInsideMultiplier ) )).xxx;
			float clampResult63 = clamp( ( saturate( ( 1.0 - ( ( distance( temp_cast_9 , temp_cast_10 ) - _DirtRange ) / max( _DirtSmooth , 1E-05 ) ) ) ) * _DirtContrast ) , 0.0 , 1.0 );
			float DirtSelection136 = clampResult63;
			float4 lerpResult194 = lerp( lerpResult37 , ( DirtColorUV3109 * Mask_B_Alpha89 ) , DirtSelection136);
			float ColorAlpha259 = tex2DNode250.a;
			float4 Albedo223 = ( lerpResult194 + ( _EdgesAdd * ColorAlpha259 ) );
			o.Albedo = Albedo223.rgb;
			float Smoothnes244 = ( ( Mask_B_Alpha89 * _MainSmoothness * ( 1.0 - MaskARed84 ) * ( 1.0 - ( DirtSelection136 * _DirtSmoothness ) ) ) + ( _PaintSmoothness * PaintSelection123 ) );
			o.Smoothness = Smoothnes244;
			o.Alpha = 1;
			float MaskAAlpha78 = tex2DNode75.a;
			clip( MaskAAlpha78 - _Cutoff );
		}

		ENDCG
	}
	Fallback "Diffuse"
}
/*ASEBEGIN
Version=18100
1920;77;1680;998;5141.893;554.4559;4.160641;True;False
Node;AmplifyShaderEditor.CommentaryNode;81;-2971.166,171.5238;Inherit;False;1955.045;975.9982;Comment;15;12;19;18;11;3;2;15;77;17;9;10;14;20;73;71;Normals;0.2130651,0.4259926,0.7924528,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;19;-2921.166,834.166;Inherit;False;Property;_DetailNMTiling;Detail NM Tiling;11;0;Create;True;0;0;False;0;False;0;6;1;8;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;18;-2607.141,786.6826;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;82;-3000.09,-400.1125;Inherit;False;708.3281;446.6394;Comment;5;83;76;78;75;84;Mask A;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;90;-2196.428,-396.2478;Inherit;False;895.2126;451.0433;Comment;6;118;89;87;86;88;85;Mask B;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;73;-2318.597,790.5878;Inherit;False;ConcreteUVTiling;-1;True;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;75;-2950.09,-350.1125;Inherit;True;Property;_MainTex;Mask A;18;0;Create;False;0;0;False;0;False;-1;fb4470513e42fce4988b8a33454a2d23;3f36e0caf78aeca4f81e435486cde529;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;118;-2186.229,-306.5392;Inherit;False;73;ConcreteUVTiling;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;85;-1919.399,-331.5393;Inherit;True;Property;_MaskB;Mask B;17;0;Create;True;0;0;False;0;False;-1;1787681c2c1799842ba3c182bb57a76f;5b71a3cf759201643b7d66aa9084dbd4;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;127;-1238.045,-387.5768;Inherit;False;485.4663;265.6519;Comment;3;32;112;125;Vertex Color;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;84;-2587.405,-304.6636;Inherit;False;MaskARed;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;137;-1459.2,1268.584;Inherit;False;1732.343;824.353;Comment;17;136;45;46;63;41;40;131;39;129;43;134;126;130;135;132;42;198;Dirt Selection;1,1,1,1;0;0
Node;AmplifyShaderEditor.VertexColorNode;32;-1188.045,-337.5768;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;132;-1431.301,1428.827;Inherit;False;84;MaskARed;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;91;-2955.796,-1373.783;Inherit;False;2202.638;833.9692;Comment;18;109;108;107;106;105;104;103;102;101;100;99;98;97;96;95;94;93;92;Color Selection;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;86;-1549.717,-333.7606;Inherit;False;Mask_B_Red;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.IntNode;93;-2897.202,-1102.97;Inherit;False;Property;_Color;Color;3;1;[PerRendererData];Create;False;0;0;False;0;False;0;0;0;1;INT;0
Node;AmplifyShaderEditor.GetLocalVarNode;129;-1333.761,1318.584;Inherit;False;86;Mask_B_Red;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;198;-1238.084,1433.548;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.IntNode;92;-2914.08,-949.0204;Inherit;False;Property;_CustomColor;Custom Color;25;0;Create;True;0;0;False;0;False;0;1;0;1;INT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;125;-986.3386,-323.9253;Inherit;False;VertexRed;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;135;-1361.873,1557.477;Inherit;False;Property;_DirtInsideMultiplier;Dirt Inside Multiplier;19;0;Create;True;0;0;False;0;False;0;0.1622185;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;89;-1543.469,-44.07379;Inherit;False;Mask_B_Alpha;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;94;-2694.08,-984.0204;Inherit;False;Property;_UseCustomColor;Use Custom Color;24;0;Create;True;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;False;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;43;-1080.908,1338.673;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;126;-1332.139,1677.84;Inherit;False;125;VertexRed;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;134;-1046.873,1499.477;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;130;-1339.726,1760.416;Inherit;False;89;Mask_B_Alpha;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;95;-2695.445,-1277.643;Half;False;Constant;_NumberOfColors;NumberOfColors;19;0;Create;True;0;0;False;1;;False;0.015625,0;0.125,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.RangedFloatNode;40;-1103.042,1879.537;Inherit;False;Property;_DirtRange;Dirt Range;14;0;Create;True;0;0;False;0;False;0;0.6583209;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;131;-862.2997,1459.827;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;87;-1554.47,-246.0881;Inherit;False;Mask_B_Green;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;76;-2590.18,-139.9208;Inherit;False;MaskABlue;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;225;-687.4859,-1373.42;Inherit;False;3738.026;1218.467;;34;223;194;201;37;195;140;200;202;247;143;186;249;146;153;189;141;145;248;151;187;165;67;172;155;150;167;168;138;166;164;250;256;259;260;Albedo;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;42;-1107.306,1692.095;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;97;-2330.451,-1345.763;Half;False;Constant;_ColorsNumber;ColorsNumber;19;1;[HideInInspector];Create;True;0;0;False;0;False;0,-0.1;0.125,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.RegisterLocalVarNode;112;-987.7551,-241.925;Inherit;False;VertexGreen;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;96;-2333.148,-1148.45;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.CommentaryNode;124;-2956.061,1244.983;Inherit;False;1404.351;589.2565;Comment;12;29;26;113;111;114;117;31;30;119;120;35;123;Paint Selection;0.764151,0.3913952,0.1117391,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;41;-1108.646,1950.517;Inherit;False;Property;_DirtSmooth;Dirt Smooth;15;0;Create;True;0;0;False;1;MaxGay;False;0;0.7691464;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;98;-1893.557,-1300.662;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;0.1,0.1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;164;-459.8553,-723.0621;Inherit;False;76;MaskABlue;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;99;-1892.992,-1152.964;Inherit;True;Property;_MainTex1;Color Theme;0;0;Create;False;0;0;False;0;False;bf84d265e341525459ab1fd8a003c547;bf84d265e341525459ab1fd8a003c547;False;white;Auto;Texture2D;-1;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.GetLocalVarNode;111;-2901.394,1293.983;Inherit;False;87;Mask_B_Green;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;114;-2894.061,1504.102;Inherit;False;86;Mask_B_Red;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;45;-643.4355,1902.083;Inherit;False;Property;_DirtContrast;Dirt Contrast;13;0;Create;True;0;0;False;0;False;0;0.9294118;0;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;39;-675.8323,1669.753;Inherit;False;Color Mask;-1;;41;eec747d987850564c95bde0e5a6d1867;0;4;1;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT;0.17;False;5;FLOAT;0.37;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;113;-2906.061,1394.102;Inherit;False;112;VertexGreen;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;172;542.4216,-1233.566;Inherit;False;Property;_AODirt;AO Dirt;20;0;Create;True;0;0;False;0;False;0;0.2194365;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;117;-2645.65,1489.56;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;102;-1468.444,-1300.557;Inherit;True;Property;_TextureSample2;Texture Sample 2;30;0;Create;True;0;0;False;0;False;-1;None;0b2ec52f502fe5943bc55689099e11b5;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;26;-2623.886,1324.629;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;166;-225.2991,-739.5607;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;46;-377.6087,1683.003;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;31;-2741.605,1744.239;Inherit;False;Property;_PaintSmooth;Paint Smooth;5;0;Create;True;0;0;False;0;False;0;0.06668227;0;0.1;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;119;-2376.108,1580.97;Inherit;False;76;MaskABlue;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;155;602.4171,-1323.42;Inherit;False;84;MaskARed;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;250;-539.0715,-1231.969;Inherit;True;Property;_ColorMap;Color Map;1;0;Create;True;0;0;False;0;False;-1;89f871b5c2ed6704e9b783d5c11d09a8;89f871b5c2ed6704e9b783d5c11d09a8;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;103;-2165.615,-673.9423;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0.1;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;30;-2741.436,1647.875;Inherit;False;Property;_PaintRange;Paint Range;4;0;Create;True;0;0;False;0;False;0;0.64755;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;138;-525.9857,-914.3888;Inherit;False;89;Mask_B_Alpha;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;168;-455.5076,-600.5148;Inherit;False;Property;_ConcreteBrightness;Concrete Brightness;2;0;Create;True;0;0;False;0;False;0;0.06670231;0;0.1;0;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;29;-2354.354,1324.576;Inherit;False;Color Mask;-1;;42;eec747d987850564c95bde0e5a6d1867;0;4;1;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT;0.17;False;5;FLOAT;0.37;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;187;827.6987,-1257.466;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;256;-127.1885,-1057.743;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ClampOpNode;63;-185.0249,1667.224;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;167;2.822084,-658.1818;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;107;-1892.969,-712.7419;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;0.1,0.1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.OneMinusNode;120;-2154.885,1587.907;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;105;-1021.317,-1305.432;Inherit;False;Color1UV3;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;88;-1539.47,-141.0881;Inherit;False;Mask_B_Blue;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;108;-1449.86,-788.6917;Inherit;True;Property;_TextureSample5;Texture Sample 5;32;0;Create;True;0;0;False;0;False;-1;None;0b2ec52f502fe5943bc55689099e11b5;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;136;19.59619,1677.929;Inherit;False;DirtSelection;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;165;75.25674,-964.192;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.CommentaryNode;246;-841.0201,211.8367;Inherit;False;1663.547;695.5518;Smoothness;14;235;233;242;229;240;239;226;243;241;238;244;232;234;230;;0.764151,0.2342916,0.6549128,1;0;0
Node;AmplifyShaderEditor.GetLocalVarNode;248;1018.451,-699.2063;Inherit;False;76;MaskABlue;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;189;1035.147,-1249.39;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;145;753.8755,-290.1053;Inherit;False;88;Mask_B_Blue;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;35;-2001.987,1327.448;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;141;744.8387,-503.6768;Inherit;False;105;Color1UV3;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;232;-791.0201,722.3155;Inherit;False;Property;_DirtSmoothness;Dirt Smoothness;23;0;Create;True;0;0;False;0;False;0;1.076944;0;3;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;146;1009.349,-353.2183;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;235;-784.8109,579.2812;Inherit;False;136;DirtSelection;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;123;-1792.709,1326.897;Inherit;False;PaintSelection;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;109;-997.4891,-783.3391;Inherit;False;DirtColorUV3;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.OneMinusNode;249;1200.63,-763.2423;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;186;1092.119,-952.0336;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;12;-2913.929,505.2257;Inherit;False;Property;_DamageNMTiling;Damage NM Tiling;8;0;Create;True;0;0;False;0;False;1;8;1;8;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;11;-2578.93,489.2256;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DesaturateOpNode;247;1332.145,-882.5805;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;242;-771.4767,446.3885;Inherit;False;84;MaskARed;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;143;1127.735,-495.6462;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;140;1444.812,-273.1688;Inherit;False;123;PaintSelection;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;20;-2586.196,994.4211;Inherit;False;Property;_DetailScale;Detail Scale;12;0;Create;True;0;0;False;0;False;0;2.605882;1;4;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;202;1953.582,-496.9103;Inherit;False;89;Mask_B_Alpha;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;259;-112.9238,-1141.089;Inherit;False;ColorAlpha;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;233;-491.0207,626.3157;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;14;-2634.162,666.2617;Inherit;False;Property;_DamageScale;Damage Scale;9;0;Create;True;0;0;False;0;False;0;0.7267291;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;200;1984.912,-621.4187;Inherit;False;109;DirtColorUV3;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;201;2200.582,-596.9105;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;2;-2307.802,221.5238;Inherit;True;Property;_MainNM;Main NM;6;0;Create;True;0;0;False;0;False;-1;5f34cf4c7404a0c44b51bf8e72241005;e7e06dc793e3ae7449663a387c4f4933;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;15;-2211.133,917.5221;Inherit;True;Property;_DetailNM;Detail NM;10;0;Create;True;0;0;False;0;False;-1;5151c7b9c95404c4c9c83ccd6634c674;e2f2024fc17d02a4cae64316bfc62502;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;195;2216.892,-375.7718;Inherit;False;136;DirtSelection;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;260;2294.958,-882.5239;Inherit;False;259;ColorAlpha;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;67;2214.181,-1130.429;Inherit;False;Property;_EdgesAdd;Edges Add;16;0;Create;True;0;0;False;0;False;0;0.04346062;0;0.25;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;37;1688.14,-742.4882;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;229;-763.5272,351.0518;Inherit;False;Property;_MainSmoothness;Main Smoothness;21;0;Create;True;0;0;False;0;False;0;0.224124;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;240;-113.4765,613.3879;Inherit;False;Property;_PaintSmoothness;Paint Smoothness;22;0;Create;True;0;0;False;0;False;0;0.1882353;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;3;-2307.802,486.9239;Inherit;True;Property;_DamageNM;Damage NM;7;0;Create;True;0;0;False;0;False;-1;02045fcd06eb2d74397ad0dcb7450563;4ea5083d09e1aa747b5ad9a32ad2cb74;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.OneMinusNode;234;-366.0204,581.3159;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;226;-699.5082,261.8367;Inherit;False;89;Mask_B_Alpha;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;239;-13.47653,792.3879;Inherit;False;123;PaintSelection;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;243;-519.4769,455.3885;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;241;176.5234,622.3879;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.BlendNormalsNode;9;-1907.829,472.7256;Inherit;False;0;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.BlendNormalsNode;17;-1886.484,634.688;Inherit;False;0;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;230;-267.527,333.0518;Inherit;False;4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;151;2513.542,-1048.65;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;77;-1713.651,860.559;Inherit;False;76;MaskABlue;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;194;2358.105,-742.2111;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;238;265.4937,384.9278;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;153;2654.03,-754.9596;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;10;-1494.529,550.2257;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;71;-1259.121,508.2696;Inherit;False;Normals;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;223;2807.54,-764.3882;Inherit;False;Albedo;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;244;579.5267,370.1684;Inherit;False;Smoothnes;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;78;-2597.761,-45.47308;Inherit;False;MaskAAlpha;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;104;-1450.597,-1018.89;Inherit;True;Property;_Texture4;Texture4;30;0;Create;True;0;0;False;0;False;-1;None;0b2ec52f502fe5943bc55689099e11b5;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;224;4369.458,593.1423;Inherit;False;223;Albedo;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;106;-1000.334,-1008.855;Inherit;False;Color2UV3;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.DynamicAppendNode;100;-2167.153,-868.8768;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0.5;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;83;-2583.365,-227.6636;Inherit;False;MaskAGreen;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;79;4354.006,903.7347;Inherit;False;78;MaskAAlpha;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;101;-1899.987,-887.2889;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;0.1,0.1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;72;4364.979,679.228;Inherit;False;71;Normals;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;245;4361.854,760.3544;Inherit;False;244;Smoothnes;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;150;2235.151,-1018.693;Inherit;False;83;MaskAGreen;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;4664.406,624.5295;Float;False;True;-1;6;ASEMaterialInspector;0;0;Standard;DBK/ConcreteRubble;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Masked;0.5;True;True;0;False;TransparentCutout;;AlphaTest;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;26;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;18;0;19;0
WireConnection;73;0;18;0
WireConnection;85;1;118;0
WireConnection;84;0;75;1
WireConnection;86;0;85;1
WireConnection;198;0;132;0
WireConnection;125;0;32;1
WireConnection;89;0;85;4
WireConnection;94;1;93;0
WireConnection;94;0;92;0
WireConnection;43;0;129;0
WireConnection;134;0;198;0
WireConnection;134;1;135;0
WireConnection;131;0;43;0
WireConnection;131;1;134;0
WireConnection;87;0;85;2
WireConnection;76;0;75;3
WireConnection;42;0;126;0
WireConnection;42;1;130;0
WireConnection;112;0;32;2
WireConnection;96;0;95;0
WireConnection;96;1;94;0
WireConnection;98;0;97;0
WireConnection;98;1;96;0
WireConnection;39;1;131;0
WireConnection;39;3;42;0
WireConnection;39;4;40;0
WireConnection;39;5;41;0
WireConnection;117;0;114;0
WireConnection;102;0;99;0
WireConnection;102;1;98;0
WireConnection;26;0;111;0
WireConnection;26;1;113;0
WireConnection;166;0;164;0
WireConnection;46;0;39;0
WireConnection;46;1;45;0
WireConnection;103;0;96;0
WireConnection;29;1;26;0
WireConnection;29;3;117;0
WireConnection;29;4;30;0
WireConnection;29;5;31;0
WireConnection;187;0;155;0
WireConnection;187;1;172;0
WireConnection;256;0;250;0
WireConnection;256;1;138;0
WireConnection;63;0;46;0
WireConnection;167;0;166;0
WireConnection;167;1;168;0
WireConnection;107;0;97;0
WireConnection;107;1;103;0
WireConnection;120;0;119;0
WireConnection;105;0;102;0
WireConnection;88;0;85;3
WireConnection;108;0;99;0
WireConnection;108;1;107;0
WireConnection;136;0;63;0
WireConnection;165;0;256;0
WireConnection;165;1;167;0
WireConnection;189;0;187;0
WireConnection;35;0;29;0
WireConnection;35;1;120;0
WireConnection;146;0;145;0
WireConnection;146;1;141;0
WireConnection;123;0;35;0
WireConnection;109;0;108;0
WireConnection;249;0;248;0
WireConnection;186;0;165;0
WireConnection;186;1;189;0
WireConnection;11;0;12;0
WireConnection;247;0;186;0
WireConnection;247;1;249;0
WireConnection;143;0;141;0
WireConnection;143;1;146;0
WireConnection;259;0;250;4
WireConnection;233;0;235;0
WireConnection;233;1;232;0
WireConnection;201;0;200;0
WireConnection;201;1;202;0
WireConnection;15;1;18;0
WireConnection;15;5;20;0
WireConnection;37;0;247;0
WireConnection;37;1;143;0
WireConnection;37;2;140;0
WireConnection;3;1;11;0
WireConnection;3;5;14;0
WireConnection;234;0;233;0
WireConnection;243;0;242;0
WireConnection;241;0;240;0
WireConnection;241;1;239;0
WireConnection;9;0;2;0
WireConnection;9;1;3;0
WireConnection;17;0;2;0
WireConnection;17;1;15;0
WireConnection;230;0;226;0
WireConnection;230;1;229;0
WireConnection;230;2;243;0
WireConnection;230;3;234;0
WireConnection;151;0;67;0
WireConnection;151;1;260;0
WireConnection;194;0;37;0
WireConnection;194;1;201;0
WireConnection;194;2;195;0
WireConnection;238;0;230;0
WireConnection;238;1;241;0
WireConnection;153;0;194;0
WireConnection;153;1;151;0
WireConnection;10;0;17;0
WireConnection;10;1;9;0
WireConnection;10;2;77;0
WireConnection;71;0;10;0
WireConnection;223;0;153;0
WireConnection;244;0;238;0
WireConnection;78;0;75;4
WireConnection;104;0;99;0
WireConnection;104;1;101;0
WireConnection;106;0;104;0
WireConnection;100;0;96;0
WireConnection;83;0;75;2
WireConnection;101;0;97;0
WireConnection;101;1;100;0
WireConnection;0;0;224;0
WireConnection;0;1;72;0
WireConnection;0;4;245;0
WireConnection;0;10;79;0
ASEEND*/
//CHKSM=B3CFAF9EB65F9946433DAC52BE16C8050C0FD642
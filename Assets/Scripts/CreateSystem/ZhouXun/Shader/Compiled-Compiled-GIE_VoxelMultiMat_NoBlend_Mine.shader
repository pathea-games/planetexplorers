Shader "GIEVoxel/Compiled_Multi_Material" 
{
	Properties 
	{
		_DiffuseTex ("Diffuse Texture", 2D) = "white" {} 
	    _NormalMap ("Normal Map", 2D) = "bump" {} 
		_NormSpecPowerTex ("Normal Specular Power Texture", 2D) = "grey" {}
		_SpecColorTex ("Specular Color Texture", 2D) = "black" {}
		_DiamondSpecTex ("Diamond Specular Texture", 2D) = "white" {}
	
		_NormalPower ("Normal Scale", Float) = 0.05
		_SpecPower ("Specular Scale", Float) = 1
		_TileSize("Tile Size", Float) = 0.125
		_Repetition("_Repetition Size", Float)= 0.08
		_Indent("_Indent", Float)= 4
		
		_DiamondSpecParam1( "Diamond Specular Parameter 1", Float ) = 2
		_DiamondSpecParam2( "Diamond Specular Parameter 2", Float ) = 0.9
		_DiamondSpecParam3( "Diamond Specular Parameter 3", Float ) = 0.4
    }

    SubShader 
    {
		Tags { "RenderType" = "Opaque" }
		
			
	Pass {
		Name "FORWARD"
		Tags { "LightMode" = "ForwardBase" }
Program "vp" {
// Vertex combos: 6
//   d3d9 - ALU: 96 to 156, TEX: 2 to 2
SubProgram "opengl " {
Keywords { "DIRECTIONAL" "LIGHTMAP_OFF" "DIRLIGHTMAP_OFF" "SHADOWS_OFF" }
"!!GLSL
#ifdef VERTEX
#define DIRECTIONAL 1
#define LIGHTMAP_OFF 1
#define DIRLIGHTMAP_OFF 1
#define SHADOWS_OFF 1
#define SHADER_API_OPENGL 1
#define SHADER_API_DESKTOP 1
vec4 xll_tex2Dlod(sampler2D s, vec4 coord) {
   return texture2DLod( s, coord.xy, coord.w);
}
mat3 xll_constructMat3( mat4 m) {
  return mat3( vec3( m[0]), vec3( m[1]), vec3( m[2]));
}
struct v2f_vertex_lit {
    vec2 uv;
    vec4 diff;
    vec4 spec;
};
struct v2f_img {
    vec4 pos;
    vec2 uv;
};
struct appdata_img {
    vec4 vertex;
    vec2 texcoord;
};
struct SurfaceOutput {
    vec3 Albedo;
    vec3 Normal;
    vec3 Emission;
    float Specular;
    float Gloss;
    float Alpha;
};
struct CustomSurfaceOutput {
    vec3 Albedo;
    vec3 Normal;
    vec3 Normal_;
    vec3 Emission;
    float Specular;
    float Alpha;
};
struct appdata_full {
    vec4 vertex;
    vec4 tangent;
    vec3 normal;
    vec4 texcoord;
    vec4 texcoord1;
    vec4 color;
};
struct Input {
    vec3 localPos;
    vec3 localNormal;
    vec3 worldNormal;
    vec4 weights;
    vec4 blendMask;
    vec4 color;
};
struct v2f_surf {
    vec4 pos;
    vec3 worldNormal;
    vec4 color;
    vec3 cust_localPos;
    vec3 cust_localNormal;
    vec4 cust_weights;
    vec4 cust_blendMask;
    vec3 normal;
    vec3 vlight;
    vec3 viewDir;
};
uniform sampler2D _NormSpecPowerTex;
uniform float _NormalPower;
uniform mat4 _Object2World;
uniform vec3 _WorldSpaceCameraPos;

uniform vec4 unity_SHAb;
uniform vec4 unity_SHAg;
uniform vec4 unity_SHAr;
uniform vec4 unity_SHBb;
uniform vec4 unity_SHBg;
uniform vec4 unity_SHBr;
uniform vec4 unity_SHC;
uniform vec4 unity_Scale;
void vert( inout appdata_full v, out Input o );
vec3 WorldSpaceViewDir( in vec4 v );
vec3 ShadeSH9( in vec4 normal );
v2f_surf vert_surf( in appdata_full v );
void vert( inout appdata_full v, out Input o ) {
    int mat0;
    int mat12;
    vec3 matIdx;
    int idx;
    vec2 normSpec;
    mat0 = int( (v.texcoord1.x  / 4.00000) );
    mat12 = int( v.texcoord1.y  );
    matIdx = vec3( float( mat0 ), float( (mat12 / 256) ), float( (mat12 % 256) ));
    o.weights.xyz  = matIdx;
    idx = int( (((v.texcoord1.y  - float( mat12 )) * 10.0000) + 0.499900) );
    o.blendMask.xyz  = vec3( float( (idx == 0) ), float( (idx == 1) ), float( (idx == 2) ));
    normSpec = xll_tex2Dlod( _NormSpecPowerTex, vec4( (dot( o.blendMask, vec4( matIdx, 0.000000)) / 255.000), 0.100000, 0.000000, 0.000000)).xy ;
    o.weights.w  = (((normSpec.x  * 16.0000) - 8.00000) * _NormalPower);
    o.blendMask.w  = (normSpec.y  * 4.00000);
    v.normal = vec3( v.texcoord.x , v.texcoord.y , ((v.texcoord1.x  - float( (4 * mat0) )) - 2.00000));
    o.localPos = vec3( (v.vertex * 0.0500000));
    o.localNormal = v.normal;
}
vec3 WorldSpaceViewDir( in vec4 v ) {
    return (_WorldSpaceCameraPos.xyz  - ( _Object2World * v ).xyz );
}
vec3 ShadeSH9( in vec4 normal ) {
    vec3 x1;
    vec4 vB;
    vec3 x2;
    float vC;
    vec3 x3;
    x1.x  = dot( unity_SHAr, normal);
    x1.y  = dot( unity_SHAg, normal);
    x1.z  = dot( unity_SHAb, normal);
    vB = (normal.xyzz  * normal.yzzx );
    x2.x  = dot( unity_SHBr, vB);
    x2.y  = dot( unity_SHBg, vB);
    x2.z  = dot( unity_SHBb, vB);
    vC = ((normal.x  * normal.x ) - (normal.y  * normal.y ));
    x3 = (unity_SHC.xyz  * vC);
    return ((x1 + x2) + x3);
}
v2f_surf vert_surf( in appdata_full v ) {
    Input customInputData;
    v2f_surf o;
    vec3 worldN;
    vec3 viewDirForLight;
    vec3 shlight;
    vert( v, customInputData);
    o.cust_localPos = customInputData.localPos;
    o.cust_localNormal = customInputData.localNormal;
    o.cust_weights = customInputData.weights;
    o.cust_blendMask = customInputData.blendMask;
    o.pos = ( gl_ModelViewProjectionMatrix * v.vertex );
    o.worldNormal = ( xll_constructMat3( _Object2World) * v.normal );
    o.color = v.color;
    worldN = ( xll_constructMat3( _Object2World) * (v.normal * unity_Scale.w ) );
    o.normal = worldN;
    viewDirForLight = WorldSpaceViewDir( v.vertex);
    o.viewDir = viewDirForLight;
    shlight = ShadeSH9( vec4( worldN, 1.00000));
    o.vlight = shlight;
    return o;
}
attribute vec4 TANGENT;
varying vec3 xlv_TEXCOORD0;
varying vec4 xlv_COLOR0;
varying vec3 xlv_TEXCOORD1;
varying vec3 xlv_TEXCOORD2;
varying vec4 xlv_TEXCOORD3;
varying vec4 xlv_TEXCOORD4;
varying vec3 xlv_TEXCOORD5;
varying vec3 xlv_TEXCOORD6;
varying vec3 xlv_TEXCOORD7;
void main() {
    v2f_surf xl_retval;
    appdata_full xlt_v;
    xlt_v.vertex = vec4( gl_Vertex);
    xlt_v.tangent = vec4( TANGENT);
    xlt_v.normal = vec3( gl_Normal);
    xlt_v.texcoord = vec4( gl_MultiTexCoord0);
    xlt_v.texcoord1 = vec4( gl_MultiTexCoord1);
    xlt_v.color = vec4( gl_Color);
    xl_retval = vert_surf( xlt_v);
    gl_Position = vec4( xl_retval.pos);
    xlv_TEXCOORD0 = vec3( xl_retval.worldNormal);
    xlv_COLOR0 = vec4( xl_retval.color);
    xlv_TEXCOORD1 = vec3( xl_retval.cust_localPos);
    xlv_TEXCOORD2 = vec3( xl_retval.cust_localNormal);
    xlv_TEXCOORD3 = vec4( xl_retval.cust_weights);
    xlv_TEXCOORD4 = vec4( xl_retval.cust_blendMask);
    xlv_TEXCOORD5 = vec3( xl_retval.normal);
    xlv_TEXCOORD6 = vec3( xl_retval.vlight);
    xlv_TEXCOORD7 = vec3( xl_retval.viewDir);
}
/* NOTE: GLSL optimization failed
0:95(75): error: operator '%' is reserved in (null)
0:95(56): error: cannot construct `float' from a non-numeric data type
0:95(11): error: cannot construct `vec3' from a non-numeric data type
*/

#endif
#ifdef FRAGMENT
#extension GL_ARB_shader_texture_lod : enable
varying vec3 xlv_TEXCOORD7;
varying vec3 xlv_TEXCOORD6;
varying vec4 xlv_TEXCOORD4;
varying vec4 xlv_TEXCOORD3;
varying vec3 xlv_TEXCOORD2;
varying vec3 xlv_TEXCOORD1;
varying vec4 xlv_COLOR0;
varying vec3 xlv_TEXCOORD0;
vec4 xlat_mutable__SpecColor;
uniform vec4 _SpecColor;
vec4 xlat_mutable__LightColor0;
uniform vec4 _LightColor0;
uniform vec4 _WorldSpaceLightPos0;
uniform float _TileSize;
uniform float _SpecPower;
uniform sampler2D _SpecColorTex;
uniform float _Repetition;
uniform sampler2D _NormalMap;
uniform float _Indent;
uniform sampler2D _DiffuseTex;
uniform sampler2D _DiamondSpecTex;
uniform float _DiamondSpecParam3;
uniform float _DiamondSpecParam2;
uniform float _DiamondSpecParam1;
void main ()
{
  xlat_mutable__SpecColor = _SpecColor;
  xlat_mutable__LightColor0 = _LightColor0;
  vec4 c;
  vec3 tmpvar_1;
  tmpvar_1 = max (((normalize (abs (xlv_TEXCOORD2)) - 0.2) * 7.0), vec3(0.0, 0.0, 0.0));
  vec3 tmpvar_2;
  tmpvar_2 = (tmpvar_1 / vec3(((tmpvar_1.x + tmpvar_1.y) + tmpvar_1.z)));
  float tmpvar_3;
  tmpvar_3 = (float(int(floor ((xlv_TEXCOORD3.x + 0.5)))) * _TileSize);
  float tmpvar_4;
  tmpvar_4 = (floor (tmpvar_3) * _TileSize);
  vec3 tmpvar_5;
  tmpvar_5.x = fract (tmpvar_3);
  tmpvar_5.y = fract (tmpvar_4);
  tmpvar_5.z = floor (tmpvar_4);
  float tmpvar_6;
  tmpvar_6 = (float(int(floor ((xlv_TEXCOORD3.y + 0.5)))) * _TileSize);
  float tmpvar_7;
  tmpvar_7 = (floor (tmpvar_6) * _TileSize);
  vec3 tmpvar_8;
  tmpvar_8.x = fract (tmpvar_6);
  tmpvar_8.y = fract (tmpvar_7);
  tmpvar_8.z = floor (tmpvar_7);
  float tmpvar_9;
  tmpvar_9 = (float(int(floor ((xlv_TEXCOORD3.z + 0.5)))) * _TileSize);
  float tmpvar_10;
  tmpvar_10 = (floor (tmpvar_9) * _TileSize);
  vec3 tmpvar_11;
  tmpvar_11.x = fract (tmpvar_9);
  tmpvar_11.y = fract (tmpvar_10);
  tmpvar_11.z = floor (tmpvar_10);
  vec2 tmpvar_12;
  tmpvar_12 = (tmpvar_5.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.xy / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_13;
  tmpvar_13.zw = vec2(0.0, 0.0);
  tmpvar_13.x = tmpvar_12.x;
  tmpvar_13.y = tmpvar_12.y;
  vec2 tmpvar_14;
  tmpvar_14 = (tmpvar_5.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.yz / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_15;
  tmpvar_15.zw = vec2(0.0, 0.0);
  tmpvar_15.x = tmpvar_14.x;
  tmpvar_15.y = tmpvar_14.y;
  vec2 tmpvar_16;
  tmpvar_16 = (tmpvar_5.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.zx / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_17;
  tmpvar_17.zw = vec2(0.0, 0.0);
  tmpvar_17.x = tmpvar_16.x;
  tmpvar_17.y = tmpvar_16.y;
  vec2 tmpvar_18;
  tmpvar_18 = (tmpvar_8.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.xy / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_19;
  tmpvar_19.zw = vec2(0.0, 0.0);
  tmpvar_19.x = tmpvar_18.x;
  tmpvar_19.y = tmpvar_18.y;
  vec2 tmpvar_20;
  tmpvar_20 = (tmpvar_8.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.yz / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_21;
  tmpvar_21.zw = vec2(0.0, 0.0);
  tmpvar_21.x = tmpvar_20.x;
  tmpvar_21.y = tmpvar_20.y;
  vec2 tmpvar_22;
  tmpvar_22 = (tmpvar_8.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.zx / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_23;
  tmpvar_23.zw = vec2(0.0, 0.0);
  tmpvar_23.x = tmpvar_22.x;
  tmpvar_23.y = tmpvar_22.y;
  vec2 tmpvar_24;
  tmpvar_24 = (tmpvar_11.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.xy / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_25;
  tmpvar_25.zw = vec2(0.0, 0.0);
  tmpvar_25.x = tmpvar_24.x;
  tmpvar_25.y = tmpvar_24.y;
  vec2 tmpvar_26;
  tmpvar_26 = (tmpvar_11.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.yz / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_27;
  tmpvar_27.zw = vec2(0.0, 0.0);
  tmpvar_27.x = tmpvar_26.x;
  tmpvar_27.y = tmpvar_26.y;
  vec2 tmpvar_28;
  tmpvar_28 = (tmpvar_11.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.zx / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_29;
  tmpvar_29.zw = vec2(0.0, 0.0);
  tmpvar_29.x = tmpvar_28.x;
  tmpvar_29.y = tmpvar_28.y;
  vec4 tmpvar_30;
  vec2 tmpvar_31;
  tmpvar_31 = (tmpvar_5.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.yz / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_32;
  tmpvar_32.zw = vec2(0.0, 0.0);
  tmpvar_32.x = tmpvar_31.x;
  tmpvar_32.y = tmpvar_31.y;
  tmpvar_30 = (texture2DLod (_NormalMap, tmpvar_32.xy, 0.0) - 0.5);
  vec4 tmpvar_33;
  vec2 tmpvar_34;
  tmpvar_34 = (tmpvar_5.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.zx / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_35;
  tmpvar_35.zw = vec2(0.0, 0.0);
  tmpvar_35.x = tmpvar_34.x;
  tmpvar_35.y = tmpvar_34.y;
  tmpvar_33 = (texture2DLod (_NormalMap, tmpvar_35.xy, 0.0) - 0.5);
  vec4 tmpvar_36;
  vec2 tmpvar_37;
  tmpvar_37 = (tmpvar_5.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.xy / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_38;
  tmpvar_38.zw = vec2(0.0, 0.0);
  tmpvar_38.x = tmpvar_37.x;
  tmpvar_38.y = tmpvar_37.y;
  tmpvar_36 = (texture2DLod (_NormalMap, tmpvar_38.xy, 0.0) - 0.5);
  vec3 tmpvar_39;
  tmpvar_39.x = 0.0;
  tmpvar_39.y = tmpvar_30.x;
  tmpvar_39.z = tmpvar_30.y;
  vec3 tmpvar_40;
  tmpvar_40.y = 0.0;
  tmpvar_40.x = tmpvar_33.y;
  tmpvar_40.z = tmpvar_33.x;
  vec3 tmpvar_41;
  tmpvar_41.z = 0.0;
  tmpvar_41.x = tmpvar_36.x;
  tmpvar_41.y = tmpvar_36.y;
  vec4 tmpvar_42;
  vec2 tmpvar_43;
  tmpvar_43 = (tmpvar_8.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.yz / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_44;
  tmpvar_44.zw = vec2(0.0, 0.0);
  tmpvar_44.x = tmpvar_43.x;
  tmpvar_44.y = tmpvar_43.y;
  tmpvar_42 = (texture2DLod (_NormalMap, tmpvar_44.xy, 0.0) - 0.5);
  vec4 tmpvar_45;
  vec2 tmpvar_46;
  tmpvar_46 = (tmpvar_8.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.zx / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_47;
  tmpvar_47.zw = vec2(0.0, 0.0);
  tmpvar_47.x = tmpvar_46.x;
  tmpvar_47.y = tmpvar_46.y;
  tmpvar_45 = (texture2DLod (_NormalMap, tmpvar_47.xy, 0.0) - 0.5);
  vec4 tmpvar_48;
  vec2 tmpvar_49;
  tmpvar_49 = (tmpvar_8.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.xy / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_50;
  tmpvar_50.zw = vec2(0.0, 0.0);
  tmpvar_50.x = tmpvar_49.x;
  tmpvar_50.y = tmpvar_49.y;
  tmpvar_48 = (texture2DLod (_NormalMap, tmpvar_50.xy, 0.0) - 0.5);
  vec3 tmpvar_51;
  tmpvar_51.x = 0.0;
  tmpvar_51.y = tmpvar_42.x;
  tmpvar_51.z = tmpvar_42.y;
  vec3 tmpvar_52;
  tmpvar_52.y = 0.0;
  tmpvar_52.x = tmpvar_45.y;
  tmpvar_52.z = tmpvar_45.x;
  vec3 tmpvar_53;
  tmpvar_53.z = 0.0;
  tmpvar_53.x = tmpvar_48.x;
  tmpvar_53.y = tmpvar_48.y;
  vec4 tmpvar_54;
  vec2 tmpvar_55;
  tmpvar_55 = (tmpvar_11.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.yz / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_56;
  tmpvar_56.zw = vec2(0.0, 0.0);
  tmpvar_56.x = tmpvar_55.x;
  tmpvar_56.y = tmpvar_55.y;
  tmpvar_54 = (texture2DLod (_NormalMap, tmpvar_56.xy, 0.0) - 0.5);
  vec4 tmpvar_57;
  vec2 tmpvar_58;
  tmpvar_58 = (tmpvar_11.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.zx / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_59;
  tmpvar_59.zw = vec2(0.0, 0.0);
  tmpvar_59.x = tmpvar_58.x;
  tmpvar_59.y = tmpvar_58.y;
  tmpvar_57 = (texture2DLod (_NormalMap, tmpvar_59.xy, 0.0) - 0.5);
  vec4 tmpvar_60;
  vec2 tmpvar_61;
  tmpvar_61 = (tmpvar_11.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.xy / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_62;
  tmpvar_62.zw = vec2(0.0, 0.0);
  tmpvar_62.x = tmpvar_61.x;
  tmpvar_62.y = tmpvar_61.y;
  tmpvar_60 = (texture2DLod (_NormalMap, tmpvar_62.xy, 0.0) - 0.5);
  vec3 tmpvar_63;
  tmpvar_63.x = 0.0;
  tmpvar_63.y = tmpvar_54.x;
  tmpvar_63.z = tmpvar_54.y;
  vec3 tmpvar_64;
  tmpvar_64.y = 0.0;
  tmpvar_64.x = tmpvar_57.y;
  tmpvar_64.z = tmpvar_57.x;
  vec3 tmpvar_65;
  tmpvar_65.z = 0.0;
  tmpvar_65.x = tmpvar_60.x;
  tmpvar_65.y = tmpvar_60.y;
  vec3 tmpvar_66;
  tmpvar_66 = ((((xlv_TEXCOORD4.x * (((tmpvar_39 * tmpvar_2.xxx) + (tmpvar_40 * tmpvar_2.yyy)) + (tmpvar_41 * tmpvar_2.zzz))) * 2.0) + ((xlv_TEXCOORD4.y * (((tmpvar_51 * tmpvar_2.xxx) + (tmpvar_52 * tmpvar_2.yyy)) + (tmpvar_53 * tmpvar_2.zzz))) * 2.0)) + ((xlv_TEXCOORD4.z * (((tmpvar_63 * tmpvar_2.xxx) + (tmpvar_64 * tmpvar_2.yyy)) + (tmpvar_65 * tmpvar_2.zzz))) * 2.0));
  vec3 tmpvar_67;
  tmpvar_67 = ((((((xlv_TEXCOORD4.x * (((texture2DLod (_DiffuseTex, tmpvar_13.xy, 0.0) * tmpvar_2.z) + (texture2DLod (_DiffuseTex, tmpvar_15.xy, 0.0) * tmpvar_2.x)) + (texture2DLod (_DiffuseTex, tmpvar_17.xy, 0.0) * tmpvar_2.y))) + (xlv_TEXCOORD4.y * (((texture2DLod (_DiffuseTex, tmpvar_19.xy, 0.0) * tmpvar_2.z) + (texture2DLod (_DiffuseTex, tmpvar_21.xy, 0.0) * tmpvar_2.x)) + (texture2DLod (_DiffuseTex, tmpvar_23.xy, 0.0) * tmpvar_2.y)))) + (xlv_TEXCOORD4.z * (((texture2DLod (_DiffuseTex, tmpvar_25.xy, 0.0) * tmpvar_2.z) + (texture2DLod (_DiffuseTex, tmpvar_27.xy, 0.0) * tmpvar_2.x)) + (texture2DLod (_DiffuseTex, tmpvar_29.xy, 0.0) * tmpvar_2.y)))).xyz * xlv_COLOR0.xyz) * (1.0 - xlv_COLOR0.w)) + (xlv_COLOR0.xyz * xlv_COLOR0.w));
  vec4 tmpvar_68;
  tmpvar_68.w = 1.0;
  tmpvar_68.x = tmpvar_66.x;
  tmpvar_68.y = tmpvar_66.y;
  tmpvar_68.z = tmpvar_66.z;
  vec3 tmpvar_69;
  tmpvar_69 = normalize ((xlv_TEXCOORD0 - (normalize (tmpvar_68).xyz * xlv_TEXCOORD3.w)));
  float tmpvar_70;
  tmpvar_70 = (xlv_TEXCOORD4.w * _SpecPower);
  vec4 tmpvar_71;
  tmpvar_71.w = 0.0;
  tmpvar_71.xyz = xlv_TEXCOORD3.xyz;
  vec4 tmpvar_72;
  tmpvar_72.yzw = vec3(0.1, 0.0, 0.0);
  tmpvar_72.x = (dot (xlv_TEXCOORD4, tmpvar_71) / 255.0);
  vec4 tmpvar_73;
  tmpvar_73 = texture2DLod (_SpecColorTex, tmpvar_72.xy, 0.0);
  c = vec4(0.0, 0.0, 0.0, 0.0);
  vec3 tmpvar_74;
  tmpvar_74 = normalize (xlv_TEXCOORD7);
  vec3 normLightDir;
  normLightDir = _WorldSpaceLightPos0.xyz;
  vec4 c_i0;
  float m;
  m = tmpvar_73.x;
  if ((tmpvar_73.y > tmpvar_73.x)) {
    m = tmpvar_73.y;
  };
  if ((tmpvar_73.z > m)) {
    m = tmpvar_73.z;
  };
  if ((m > 0.03)) {
    float tmpvar_75;
    tmpvar_75 = (((((tmpvar_74.x * tmpvar_74.x) * tmpvar_74.y) + (tmpvar_74.z * tmpvar_74.x)) + (tmpvar_74.y * tmpvar_74.z)) * _DiamondSpecParam1);
    vec4 tmpvar_76;
    tmpvar_76.zw = vec2(0.0, 0.0);
    tmpvar_76.x = tmpvar_75;
    tmpvar_76.y = _DiamondSpecParam2;
    xlat_mutable__SpecColor.xyz = texture2DLod (_DiamondSpecTex, tmpvar_76.xy, 0.0).xyz;
    vec4 tmpvar_77;
    tmpvar_77.zw = vec2(0.0, 0.0);
    tmpvar_77.x = tmpvar_75;
    tmpvar_77.y = _DiamondSpecParam3;
    xlat_mutable__LightColor0.xyz = (_LightColor0.xyz * texture2DLod (_DiamondSpecTex, tmpvar_77.xy, 0.0).xyz);
  } else {
    if ((m > 0.0)) {
      xlat_mutable__SpecColor.xyz = (tmpvar_73.xyz * (1.0/(m)));
    } else {
      xlat_mutable__SpecColor.xyz = vec3(1.0, 1.0, 1.0);
    };
  };
  c_i0 = vec4(0.0, 0.0, 0.0, 0.0);
  c_i0.xyz = (((tmpvar_67 * xlat_mutable__LightColor0.xyz) * dot (tmpvar_69, normLightDir)) + ((xlat_mutable__LightColor0.xyz * xlat_mutable__SpecColor.xyz) * (smoothstep (0.0, 1.0, pow (max (0.0, dot (tmpvar_69, normalize ((_WorldSpaceLightPos0.xyz + tmpvar_74)))), (32.0 * tmpvar_70))) * tmpvar_70)));
  c_i0.w = 1.0;
  c = c_i0;
  c.xyz = (c_i0.xyz + (tmpvar_67 * xlv_TEXCOORD6));
  c.xyz = (c.xyz + tmpvar_73.xyz);
  gl_FragData[0] = c;
}


#endif
"
}

SubProgram "d3d9 " {
Keywords { "DIRECTIONAL" "LIGHTMAP_OFF" "DIRLIGHTMAP_OFF" "SHADOWS_OFF" }
Bind "vertex" Vertex
Bind "texcoord" TexCoord0
Bind "texcoord1" TexCoord1
Bind "color" Color
Matrix 0 [glstate_matrix_mvp]
Vector 8 [unity_Scale]
Vector 9 [_WorldSpaceCameraPos]
Matrix 4 [_Object2World]
Vector 10 [unity_SHAr]
Vector 11 [unity_SHAg]
Vector 12 [unity_SHAb]
Vector 13 [unity_SHBr]
Vector 14 [unity_SHBg]
Vector 15 [unity_SHBb]
Vector 16 [unity_SHC]
Float 17 [_NormalPower]
SetTexture 0 [_NormSpecPowerTex] 2D
"vs_3_0
; 121 ALU, 2 TEX
dcl_position o0
dcl_texcoord0 o1
dcl_color0 o2
dcl_texcoord1 o3
dcl_texcoord2 o4
dcl_texcoord3 o5
dcl_texcoord4 o6
dcl_texcoord5 o7
dcl_texcoord6 o8
dcl_texcoord7 o9
def c18, 0.25000000, 0.00000000, 1.00000000, 0.00390625
def c19, 256.00000000, 0.50000000, 10.00000000, 0.49990001
def c20, -0.49990001, 2.00000000, 0.00392157, 4.00000000
def c21, 0.10000000, 0.00000000, 16.00000000, -8.00000000
def c22, -2.00000000, 0.05000000, 0, 0
dcl_position0 v0
dcl_texcoord0 v3
dcl_texcoord1 v4
dcl_color0 v5
dcl_2d s0
abs r0.y, v4
frc r0.z, r0.y
slt r0.x, v4.y, c18.y
max r0.x, -r0, r0
slt r0.x, c18.y, r0
add r0.y, r0, -r0.z
add r0.w, -r0.x, c18.z
mul r0.z, r0.y, r0.w
mad r0.z, r0.x, -r0.y, r0
add r0.x, v4.y, -r0.z
mul r0.x, r0, c19.z
mov r4.xy, v3
mov r2.w, c18.z
slt r0.y, r0.x, c20.x
add r0.w, r0.x, c19
max r0.x, -r0.y, r0.y
abs r0.y, r0.w
slt r0.x, c18.y, r0
frc r0.w, r0.y
add r0.y, r0, -r0.w
add r1.x, -r0, c18.z
mul r0.w, r0.y, r1.x
mad r0.w, r0.x, -r0.y, r0
sge r0.y, c18, r0.w
sge r0.x, r0.w, c18.y
mul r0.x, r0, r0.y
sge r1.x, c18.z, r0.w
sge r0.y, r0.w, c18.z
mul r0.y, r0, r1.x
mul r1.x, r0.z, c18.w
abs r1.y, r1.x
frc r1.z, r1.y
sge r0.z, r0.w, c20.y
sge r0.w, c20.y, r0
mul r0.z, r0, r0.w
slt r0.w, r1.x, c18.y
max r0.w, -r0, r0
add r1.y, r1, -r1.z
frc r1.x, r1
mul r1.z, r1.x, c19.x
slt r0.w, c18.y, r0
abs r1.w, r1.z
add r1.w, r1, c19.y
frc r2.x, r1.w
add r1.x, -r0.w, c18.z
mul r1.x, r1.y, r1
mad r1.y, r0.w, -r1, r1.x
slt r1.x, r1.z, c18.y
add r2.y, r1.w, -r2.x
max r1.z, -r1.x, r1.x
mul r0.w, v4.x, c18.x
slt r2.x, c18.y, r1.z
slt r1.x, r0.w, c18.y
abs r1.z, r0.w
max r1.x, -r1, r1
slt r0.w, c18.y, r1.x
frc r1.x, r1.z
add r1.w, -r0, c18.z
add r1.x, r1.z, -r1
mul r1.z, r1.x, r1.w
mad r1.x, r0.w, -r1, r1.z
add r1.w, -r2.x, c18.z
mul r1.z, r2.y, r1.w
mad r0.w, -r1.x, c20, v4.x
add r4.z, r0.w, c22.x
mul r3.xyz, r4, c8.w
mad r1.z, r2.x, -r2.y, r1
dp3 r4.w, r3, c5
dp3 r5.w, r3, c6
dp3 r2.x, r3, c4
dp3 r0.w, r0, r1
mov r2.y, r4.w
mov r2.z, r5.w
mul r3, r2.xyzz, r2.yzzx
dp4 r5.z, r2, c12
dp4 r5.y, r2, c11
dp4 r5.x, r2, c10
dp4 r2.w, r3, c15
dp4 r2.y, r3, c13
dp4 r2.z, r3, c14
mul r3.x, r0.w, c20.z
mul r0.w, r4, r4
mov r3.yz, c21.xxyw
add r2.yzw, r5.xxyz, r2
mad r0.w, r2.x, r2.x, -r0
mul r5.xyz, r0.w, c16
texldl r3.xy, r3.xyzz, s0
mad r0.w, r3.x, c21.z, c21
mul r1.w, r0, c17.x
mul r0.w, r3.y, c20
mov o6, r0
dp4 r0.z, v0, c6
dp4 r0.x, v0, c4
dp4 r0.y, v0, c5
add o8.xyz, r2.yzww, r5
mov o5, r1
mov o2, v5
dp3 o1.z, r4, c6
dp3 o1.y, r4, c5
dp3 o1.x, r4, c4
mul o3.xyz, v0, c22.y
mov o4.xyz, r4
mov o7.z, r5.w
mov o7.y, r4.w
mov o7.x, r2
add o9.xyz, -r0, c9
dp4 o0.w, v0, c3
dp4 o0.z, v0, c2
dp4 o0.y, v0, c1
dp4 o0.x, v0, c0
"
}

SubProgram "opengl " {
Keywords { "DIRECTIONAL" "LIGHTMAP_ON" "DIRLIGHTMAP_OFF" "SHADOWS_OFF" }
"!!GLSL
#ifdef VERTEX
#define DIRECTIONAL 1
#define LIGHTMAP_ON 1
#define DIRLIGHTMAP_OFF 1
#define SHADOWS_OFF 1
#define SHADER_API_OPENGL 1
#define SHADER_API_DESKTOP 1
vec4 xll_tex2Dlod(sampler2D s, vec4 coord) {
   return texture2DLod( s, coord.xy, coord.w);
}
mat3 xll_constructMat3( mat4 m) {
  return mat3( vec3( m[0]), vec3( m[1]), vec3( m[2]));
}
struct v2f_vertex_lit {
    vec2 uv;
    vec4 diff;
    vec4 spec;
};
struct v2f_img {
    vec4 pos;
    vec2 uv;
};
struct appdata_img {
    vec4 vertex;
    vec2 texcoord;
};
struct SurfaceOutput {
    vec3 Albedo;
    vec3 Normal;
    vec3 Emission;
    float Specular;
    float Gloss;
    float Alpha;
};
struct CustomSurfaceOutput {
    vec3 Albedo;
    vec3 Normal;
    vec3 Normal_;
    vec3 Emission;
    float Specular;
    float Alpha;
};
struct appdata_full {
    vec4 vertex;
    vec4 tangent;
    vec3 normal;
    vec4 texcoord;
    vec4 texcoord1;
    vec4 color;
};
struct Input {
    vec3 localPos;
    vec3 localNormal;
    vec3 worldNormal;
    vec4 weights;
    vec4 blendMask;
    vec4 color;
};
struct v2f_surf {
    vec4 pos;
    vec3 worldNormal;
    vec4 color;
    vec3 cust_localPos;
    vec3 cust_localNormal;
    vec4 cust_weights;
    vec4 cust_blendMask;
    vec2 lmap;
};
uniform sampler2D _NormSpecPowerTex;
uniform float _NormalPower;
uniform mat4 _Object2World;

uniform vec4 unity_LightmapST;
uniform vec4 unity_Scale;
void vert( inout appdata_full v, out Input o );
v2f_surf vert_surf( in appdata_full v );
void vert( inout appdata_full v, out Input o ) {
    int mat0;
    int mat12;
    vec3 matIdx;
    int idx;
    vec2 normSpec;
    mat0 = int( (v.texcoord1.x  / 4.00000) );
    mat12 = int( v.texcoord1.y  );
    matIdx = vec3( float( mat0 ), float( (mat12 / 256) ), float( (mat12 % 256) ));
    o.weights.xyz  = matIdx;
    idx = int( (((v.texcoord1.y  - float( mat12 )) * 10.0000) + 0.499900) );
    o.blendMask.xyz  = vec3( float( (idx == 0) ), float( (idx == 1) ), float( (idx == 2) ));
    normSpec = xll_tex2Dlod( _NormSpecPowerTex, vec4( (dot( o.blendMask, vec4( matIdx, 0.000000)) / 255.000), 0.100000, 0.000000, 0.000000)).xy ;
    o.weights.w  = (((normSpec.x  * 16.0000) - 8.00000) * _NormalPower);
    o.blendMask.w  = (normSpec.y  * 4.00000);
    v.normal = vec3( v.texcoord.x , v.texcoord.y , ((v.texcoord1.x  - float( (4 * mat0) )) - 2.00000));
    o.localPos = vec3( (v.vertex * 0.0500000));
    o.localNormal = v.normal;
}
v2f_surf vert_surf( in appdata_full v ) {
    Input customInputData;
    v2f_surf o;
    vec3 worldN;
    vert( v, customInputData);
    o.cust_localPos = customInputData.localPos;
    o.cust_localNormal = customInputData.localNormal;
    o.cust_weights = customInputData.weights;
    o.cust_blendMask = customInputData.blendMask;
    o.pos = ( gl_ModelViewProjectionMatrix * v.vertex );
    o.worldNormal = ( xll_constructMat3( _Object2World) * v.normal );
    o.color = v.color;
    o.lmap.xy  = ((v.texcoord1.xy  * unity_LightmapST.xy ) + unity_LightmapST.zw );
    worldN = ( xll_constructMat3( _Object2World) * (v.normal * unity_Scale.w ) );
    return o;
}
attribute vec4 TANGENT;
varying vec3 xlv_TEXCOORD0;
varying vec4 xlv_COLOR0;
varying vec3 xlv_TEXCOORD1;
varying vec3 xlv_TEXCOORD2;
varying vec4 xlv_TEXCOORD3;
varying vec4 xlv_TEXCOORD4;
varying vec2 xlv_TEXCOORD5;
void main() {
    v2f_surf xl_retval;
    appdata_full xlt_v;
    xlt_v.vertex = vec4( gl_Vertex);
    xlt_v.tangent = vec4( TANGENT);
    xlt_v.normal = vec3( gl_Normal);
    xlt_v.texcoord = vec4( gl_MultiTexCoord0);
    xlt_v.texcoord1 = vec4( gl_MultiTexCoord1);
    xlt_v.color = vec4( gl_Color);
    xl_retval = vert_surf( xlt_v);
    gl_Position = vec4( xl_retval.pos);
    xlv_TEXCOORD0 = vec3( xl_retval.worldNormal);
    xlv_COLOR0 = vec4( xl_retval.color);
    xlv_TEXCOORD1 = vec3( xl_retval.cust_localPos);
    xlv_TEXCOORD2 = vec3( xl_retval.cust_localNormal);
    xlv_TEXCOORD3 = vec4( xl_retval.cust_weights);
    xlv_TEXCOORD4 = vec4( xl_retval.cust_blendMask);
    xlv_TEXCOORD5 = vec2( xl_retval.lmap);
}
/* NOTE: GLSL optimization failed
0:84(75): error: operator '%' is reserved in (null)
0:84(56): error: cannot construct `float' from a non-numeric data type
0:84(11): error: cannot construct `vec3' from a non-numeric data type
*/

#endif
#ifdef FRAGMENT
#extension GL_ARB_shader_texture_lod : enable
varying vec2 xlv_TEXCOORD5;
varying vec4 xlv_TEXCOORD4;
varying vec4 xlv_TEXCOORD3;
varying vec3 xlv_TEXCOORD2;
varying vec3 xlv_TEXCOORD1;
varying vec4 xlv_COLOR0;
uniform sampler2D unity_Lightmap;
uniform float _TileSize;
uniform sampler2D _SpecColorTex;
uniform float _Repetition;
uniform float _Indent;
uniform sampler2D _DiffuseTex;
void main ()
{
  vec4 c;
  vec3 tmpvar_1;
  tmpvar_1 = max (((normalize (abs (xlv_TEXCOORD2)) - 0.2) * 7.0), vec3(0.0, 0.0, 0.0));
  vec3 tmpvar_2;
  tmpvar_2 = (tmpvar_1 / vec3(((tmpvar_1.x + tmpvar_1.y) + tmpvar_1.z)));
  float tmpvar_3;
  tmpvar_3 = (float(int(floor ((xlv_TEXCOORD3.x + 0.5)))) * _TileSize);
  float tmpvar_4;
  tmpvar_4 = (floor (tmpvar_3) * _TileSize);
  vec3 tmpvar_5;
  tmpvar_5.x = fract (tmpvar_3);
  tmpvar_5.y = fract (tmpvar_4);
  tmpvar_5.z = floor (tmpvar_4);
  float tmpvar_6;
  tmpvar_6 = (float(int(floor ((xlv_TEXCOORD3.y + 0.5)))) * _TileSize);
  float tmpvar_7;
  tmpvar_7 = (floor (tmpvar_6) * _TileSize);
  vec3 tmpvar_8;
  tmpvar_8.x = fract (tmpvar_6);
  tmpvar_8.y = fract (tmpvar_7);
  tmpvar_8.z = floor (tmpvar_7);
  float tmpvar_9;
  tmpvar_9 = (float(int(floor ((xlv_TEXCOORD3.z + 0.5)))) * _TileSize);
  float tmpvar_10;
  tmpvar_10 = (floor (tmpvar_9) * _TileSize);
  vec3 tmpvar_11;
  tmpvar_11.x = fract (tmpvar_9);
  tmpvar_11.y = fract (tmpvar_10);
  tmpvar_11.z = floor (tmpvar_10);
  vec2 tmpvar_12;
  tmpvar_12 = (tmpvar_5.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.xy / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_13;
  tmpvar_13.zw = vec2(0.0, 0.0);
  tmpvar_13.x = tmpvar_12.x;
  tmpvar_13.y = tmpvar_12.y;
  vec2 tmpvar_14;
  tmpvar_14 = (tmpvar_5.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.yz / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_15;
  tmpvar_15.zw = vec2(0.0, 0.0);
  tmpvar_15.x = tmpvar_14.x;
  tmpvar_15.y = tmpvar_14.y;
  vec2 tmpvar_16;
  tmpvar_16 = (tmpvar_5.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.zx / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_17;
  tmpvar_17.zw = vec2(0.0, 0.0);
  tmpvar_17.x = tmpvar_16.x;
  tmpvar_17.y = tmpvar_16.y;
  vec2 tmpvar_18;
  tmpvar_18 = (tmpvar_8.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.xy / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_19;
  tmpvar_19.zw = vec2(0.0, 0.0);
  tmpvar_19.x = tmpvar_18.x;
  tmpvar_19.y = tmpvar_18.y;
  vec2 tmpvar_20;
  tmpvar_20 = (tmpvar_8.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.yz / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_21;
  tmpvar_21.zw = vec2(0.0, 0.0);
  tmpvar_21.x = tmpvar_20.x;
  tmpvar_21.y = tmpvar_20.y;
  vec2 tmpvar_22;
  tmpvar_22 = (tmpvar_8.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.zx / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_23;
  tmpvar_23.zw = vec2(0.0, 0.0);
  tmpvar_23.x = tmpvar_22.x;
  tmpvar_23.y = tmpvar_22.y;
  vec2 tmpvar_24;
  tmpvar_24 = (tmpvar_11.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.xy / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_25;
  tmpvar_25.zw = vec2(0.0, 0.0);
  tmpvar_25.x = tmpvar_24.x;
  tmpvar_25.y = tmpvar_24.y;
  vec2 tmpvar_26;
  tmpvar_26 = (tmpvar_11.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.yz / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_27;
  tmpvar_27.zw = vec2(0.0, 0.0);
  tmpvar_27.x = tmpvar_26.x;
  tmpvar_27.y = tmpvar_26.y;
  vec2 tmpvar_28;
  tmpvar_28 = (tmpvar_11.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.zx / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_29;
  tmpvar_29.zw = vec2(0.0, 0.0);
  tmpvar_29.x = tmpvar_28.x;
  tmpvar_29.y = tmpvar_28.y;
  vec4 tmpvar_30;
  tmpvar_30.w = 0.0;
  tmpvar_30.xyz = xlv_TEXCOORD3.xyz;
  vec4 tmpvar_31;
  tmpvar_31.yzw = vec3(0.1, 0.0, 0.0);
  tmpvar_31.x = (dot (xlv_TEXCOORD4, tmpvar_30) / 255.0);
  c = vec4(0.0, 0.0, 0.0, 0.0);
  vec4 tmpvar_32;
  tmpvar_32 = texture2D (unity_Lightmap, xlv_TEXCOORD5);
  c.xyz = (((((((xlv_TEXCOORD4.x * (((texture2DLod (_DiffuseTex, tmpvar_13.xy, 0.0) * tmpvar_2.z) + (texture2DLod (_DiffuseTex, tmpvar_15.xy, 0.0) * tmpvar_2.x)) + (texture2DLod (_DiffuseTex, tmpvar_17.xy, 0.0) * tmpvar_2.y))) + (xlv_TEXCOORD4.y * (((texture2DLod (_DiffuseTex, tmpvar_19.xy, 0.0) * tmpvar_2.z) + (texture2DLod (_DiffuseTex, tmpvar_21.xy, 0.0) * tmpvar_2.x)) + (texture2DLod (_DiffuseTex, tmpvar_23.xy, 0.0) * tmpvar_2.y)))) + (xlv_TEXCOORD4.z * (((texture2DLod (_DiffuseTex, tmpvar_25.xy, 0.0) * tmpvar_2.z) + (texture2DLod (_DiffuseTex, tmpvar_27.xy, 0.0) * tmpvar_2.x)) + (texture2DLod (_DiffuseTex, tmpvar_29.xy, 0.0) * tmpvar_2.y)))).xyz * xlv_COLOR0.xyz) * (1.0 - xlv_COLOR0.w)) + (xlv_COLOR0.xyz * xlv_COLOR0.w)) * ((8.0 * tmpvar_32.w) * tmpvar_32.xyz));
  c.w = 0.0;
  c.xyz = (c.xyz + texture2DLod (_SpecColorTex, tmpvar_31.xy, 0.0).xyz);
  gl_FragData[0] = c;
}


#endif
"
}

SubProgram "d3d9 " {
Keywords { "DIRECTIONAL" "LIGHTMAP_ON" "DIRLIGHTMAP_OFF" "SHADOWS_OFF" }
Bind "vertex" Vertex
Bind "texcoord" TexCoord0
Bind "texcoord1" TexCoord1
Bind "color" Color
Matrix 0 [glstate_matrix_mvp]
Matrix 4 [_Object2World]
Float 8 [_NormalPower]
Vector 9 [unity_LightmapST]
SetTexture 0 [_NormSpecPowerTex] 2D
"vs_3_0
; 96 ALU, 2 TEX
dcl_position o0
dcl_texcoord0 o1
dcl_color0 o2
dcl_texcoord1 o3
dcl_texcoord2 o4
dcl_texcoord3 o5
dcl_texcoord4 o6
dcl_texcoord5 o7
def c10, 0.25000000, 0.00000000, 1.00000000, 0.00390625
def c11, 256.00000000, 0.50000000, 10.00000000, 0.49990001
def c12, -0.49990001, 2.00000000, 0.00392157, 4.00000000
def c13, 0.10000000, 0.00000000, 16.00000000, -8.00000000
def c14, -2.00000000, 0.05000000, 0, 0
dcl_position0 v0
dcl_texcoord0 v3
dcl_texcoord1 v4
dcl_color0 v5
dcl_2d s0
abs r0.y, v4
frc r0.z, r0.y
slt r0.x, v4.y, c10.y
max r0.x, -r0, r0
slt r0.x, c10.y, r0
add r0.y, r0, -r0.z
add r0.w, -r0.x, c10.z
mul r0.z, r0.y, r0.w
mad r0.z, r0.x, -r0.y, r0
add r0.x, v4.y, -r0.z
mul r0.x, r0, c11.z
mul r1.y, r0.z, c10.w
slt r0.y, r0.x, c12.x
add r0.w, r0.x, c11
max r0.x, -r0.y, r0.y
abs r0.y, r0.w
slt r0.x, c10.y, r0
frc r0.w, r0.y
add r0.y, r0, -r0.w
add r1.x, -r0, c10.z
mul r0.w, r0.y, r1.x
mad r0.w, r0.x, -r0.y, r0
sge r0.y, c10, r0.w
sge r0.x, r0.w, c10.y
mul r0.x, r0, r0.y
sge r1.x, c10.z, r0.w
sge r0.y, r0.w, c10.z
mul r0.y, r0, r1.x
sge r1.x, r0.w, c12.y
sge r0.w, c12.y, r0
mul r0.z, r1.x, r0.w
abs r1.x, r1.y
frc r1.z, r1.x
add r1.x, r1, -r1.z
slt r0.w, r1.y, c10.y
frc r1.z, r1.y
mul r1.z, r1, c11.x
max r0.w, -r0, r0
slt r0.w, c10.y, r0
add r1.y, -r0.w, c10.z
mul r1.y, r1.x, r1
mad r1.y, r0.w, -r1.x, r1
abs r1.w, r1.z
add r1.x, r1.w, c11.y
mul r1.w, v4.x, c10.x
slt r0.w, r1.z, c10.y
frc r1.z, r1.x
slt r2.x, r1.w, c10.y
max r0.w, -r0, r0
add r1.x, r1, -r1.z
slt r0.w, c10.y, r0
add r1.z, -r0.w, c10
mul r1.z, r1.x, r1
abs r2.y, r1.w
max r2.x, -r2, r2
slt r1.w, c10.y, r2.x
frc r2.x, r2.y
add r2.x, r2.y, -r2
add r2.z, -r1.w, c10
mad r1.z, r0.w, -r1.x, r1
mul r2.y, r2.x, r2.z
mad r1.x, r1.w, -r2, r2.y
dp3 r0.w, r0, r1
mul r2.x, r0.w, c12.z
mov r2.yz, c13.xxyw
texldl r2.xy, r2.xyzz, s0
mad r0.w, r2.x, c13.z, c13
mul r1.w, r0, c8.x
mul r0.w, r2.y, c12
mov o6, r0
mad r0.x, -r1, c12.w, v4
add r0.z, r0.x, c14.x
mov r0.xy, v3
mov o5, r1
mov o2, v5
dp3 o1.z, r0, c6
dp3 o1.y, r0, c5
dp3 o1.x, r0, c4
mul o3.xyz, v0, c14.y
mov o4.xyz, r0
mad o7.xy, v4, c9, c9.zwzw
dp4 o0.w, v0, c3
dp4 o0.z, v0, c2
dp4 o0.y, v0, c1
dp4 o0.x, v0, c0
"
}

SubProgram "opengl " {
Keywords { "DIRECTIONAL" "LIGHTMAP_OFF" "DIRLIGHTMAP_OFF" "SHADOWS_SCREEN" }
"!!GLSL
#ifdef VERTEX
#define DIRECTIONAL 1
#define LIGHTMAP_OFF 1
#define DIRLIGHTMAP_OFF 1
#define SHADOWS_SCREEN 1
#define SHADER_API_OPENGL 1
#define SHADER_API_DESKTOP 1
vec4 xll_tex2Dlod(sampler2D s, vec4 coord) {
   return texture2DLod( s, coord.xy, coord.w);
}
mat3 xll_constructMat3( mat4 m) {
  return mat3( vec3( m[0]), vec3( m[1]), vec3( m[2]));
}
struct v2f_vertex_lit {
    vec2 uv;
    vec4 diff;
    vec4 spec;
};
struct v2f_img {
    vec4 pos;
    vec2 uv;
};
struct appdata_img {
    vec4 vertex;
    vec2 texcoord;
};
struct SurfaceOutput {
    vec3 Albedo;
    vec3 Normal;
    vec3 Emission;
    float Specular;
    float Gloss;
    float Alpha;
};
struct CustomSurfaceOutput {
    vec3 Albedo;
    vec3 Normal;
    vec3 Normal_;
    vec3 Emission;
    float Specular;
    float Alpha;
};
struct appdata_full {
    vec4 vertex;
    vec4 tangent;
    vec3 normal;
    vec4 texcoord;
    vec4 texcoord1;
    vec4 color;
};
struct Input {
    vec3 localPos;
    vec3 localNormal;
    vec3 worldNormal;
    vec4 weights;
    vec4 blendMask;
    vec4 color;
};
struct v2f_surf {
    vec4 pos;
    vec3 worldNormal;
    vec4 color;
    vec3 cust_localPos;
    vec3 cust_localNormal;
    vec4 cust_weights;
    vec4 cust_blendMask;
    vec3 normal;
    vec3 vlight;
    vec3 viewDir;
    vec4 _ShadowCoord;
};
uniform sampler2D _NormSpecPowerTex;
uniform float _NormalPower;
uniform mat4 _Object2World;
uniform vec4 _ProjectionParams;
uniform vec3 _WorldSpaceCameraPos;

uniform vec4 unity_SHAb;
uniform vec4 unity_SHAg;
uniform vec4 unity_SHAr;
uniform vec4 unity_SHBb;
uniform vec4 unity_SHBg;
uniform vec4 unity_SHBr;
uniform vec4 unity_SHC;
uniform vec4 unity_Scale;
void vert( inout appdata_full v, out Input o );
vec3 WorldSpaceViewDir( in vec4 v );
vec3 ShadeSH9( in vec4 normal );
vec4 ComputeScreenPos( in vec4 pos );
v2f_surf vert_surf( in appdata_full v );
void vert( inout appdata_full v, out Input o ) {
    int mat0;
    int mat12;
    vec3 matIdx;
    int idx;
    vec2 normSpec;
    mat0 = int( (v.texcoord1.x  / 4.00000) );
    mat12 = int( v.texcoord1.y  );
    matIdx = vec3( float( mat0 ), float( (mat12 / 256) ), float( (mat12 % 256) ));
    o.weights.xyz  = matIdx;
    idx = int( (((v.texcoord1.y  - float( mat12 )) * 10.0000) + 0.499900) );
    o.blendMask.xyz  = vec3( float( (idx == 0) ), float( (idx == 1) ), float( (idx == 2) ));
    normSpec = xll_tex2Dlod( _NormSpecPowerTex, vec4( (dot( o.blendMask, vec4( matIdx, 0.000000)) / 255.000), 0.100000, 0.000000, 0.000000)).xy ;
    o.weights.w  = (((normSpec.x  * 16.0000) - 8.00000) * _NormalPower);
    o.blendMask.w  = (normSpec.y  * 4.00000);
    v.normal = vec3( v.texcoord.x , v.texcoord.y , ((v.texcoord1.x  - float( (4 * mat0) )) - 2.00000));
    o.localPos = vec3( (v.vertex * 0.0500000));
    o.localNormal = v.normal;
}
vec3 WorldSpaceViewDir( in vec4 v ) {
    return (_WorldSpaceCameraPos.xyz  - ( _Object2World * v ).xyz );
}
vec3 ShadeSH9( in vec4 normal ) {
    vec3 x1;
    vec4 vB;
    vec3 x2;
    float vC;
    vec3 x3;
    x1.x  = dot( unity_SHAr, normal);
    x1.y  = dot( unity_SHAg, normal);
    x1.z  = dot( unity_SHAb, normal);
    vB = (normal.xyzz  * normal.yzzx );
    x2.x  = dot( unity_SHBr, vB);
    x2.y  = dot( unity_SHBg, vB);
    x2.z  = dot( unity_SHBb, vB);
    vC = ((normal.x  * normal.x ) - (normal.y  * normal.y ));
    x3 = (unity_SHC.xyz  * vC);
    return ((x1 + x2) + x3);
}
vec4 ComputeScreenPos( in vec4 pos ) {
    vec4 o;
    o = (pos * 0.500000);
    o.xy  = (vec2( o.x , (o.y  * _ProjectionParams.x )) + o.w );
    o.zw  = pos.zw ;
    return o;
}
v2f_surf vert_surf( in appdata_full v ) {
    Input customInputData;
    v2f_surf o;
    vec3 worldN;
    vec3 viewDirForLight;
    vec3 shlight;
    vert( v, customInputData);
    o.cust_localPos = customInputData.localPos;
    o.cust_localNormal = customInputData.localNormal;
    o.cust_weights = customInputData.weights;
    o.cust_blendMask = customInputData.blendMask;
    o.pos = ( gl_ModelViewProjectionMatrix * v.vertex );
    o.worldNormal = ( xll_constructMat3( _Object2World) * v.normal );
    o.color = v.color;
    worldN = ( xll_constructMat3( _Object2World) * (v.normal * unity_Scale.w ) );
    o.normal = worldN;
    viewDirForLight = WorldSpaceViewDir( v.vertex);
    o.viewDir = viewDirForLight;
    shlight = ShadeSH9( vec4( worldN, 1.00000));
    o.vlight = shlight;
    o._ShadowCoord = ComputeScreenPos( o.pos);
    return o;
}
attribute vec4 TANGENT;
varying vec3 xlv_TEXCOORD0;
varying vec4 xlv_COLOR0;
varying vec3 xlv_TEXCOORD1;
varying vec3 xlv_TEXCOORD2;
varying vec4 xlv_TEXCOORD3;
varying vec4 xlv_TEXCOORD4;
varying vec3 xlv_TEXCOORD5;
varying vec3 xlv_TEXCOORD6;
varying vec3 xlv_TEXCOORD7;
varying vec4 xlv_TEXCOORD8;
void main() {
    v2f_surf xl_retval;
    appdata_full xlt_v;
    xlt_v.vertex = vec4( gl_Vertex);
    xlt_v.tangent = vec4( TANGENT);
    xlt_v.normal = vec3( gl_Normal);
    xlt_v.texcoord = vec4( gl_MultiTexCoord0);
    xlt_v.texcoord1 = vec4( gl_MultiTexCoord1);
    xlt_v.color = vec4( gl_Color);
    xl_retval = vert_surf( xlt_v);
    gl_Position = vec4( xl_retval.pos);
    xlv_TEXCOORD0 = vec3( xl_retval.worldNormal);
    xlv_COLOR0 = vec4( xl_retval.color);
    xlv_TEXCOORD1 = vec3( xl_retval.cust_localPos);
    xlv_TEXCOORD2 = vec3( xl_retval.cust_localNormal);
    xlv_TEXCOORD3 = vec4( xl_retval.cust_weights);
    xlv_TEXCOORD4 = vec4( xl_retval.cust_blendMask);
    xlv_TEXCOORD5 = vec3( xl_retval.normal);
    xlv_TEXCOORD6 = vec3( xl_retval.vlight);
    xlv_TEXCOORD7 = vec3( xl_retval.viewDir);
    xlv_TEXCOORD8 = vec4( xl_retval._ShadowCoord);
}
/* NOTE: GLSL optimization failed
0:98(75): error: operator '%' is reserved in (null)
0:98(56): error: cannot construct `float' from a non-numeric data type
0:98(11): error: cannot construct `vec3' from a non-numeric data type
*/

#endif
#ifdef FRAGMENT
#extension GL_ARB_shader_texture_lod : enable
varying vec4 xlv_TEXCOORD8;
varying vec3 xlv_TEXCOORD7;
varying vec3 xlv_TEXCOORD6;
varying vec4 xlv_TEXCOORD4;
varying vec4 xlv_TEXCOORD3;
varying vec3 xlv_TEXCOORD2;
varying vec3 xlv_TEXCOORD1;
varying vec4 xlv_COLOR0;
varying vec3 xlv_TEXCOORD0;
vec4 xlat_mutable__SpecColor;
uniform vec4 _SpecColor;
vec4 xlat_mutable__LightColor0;
uniform vec4 _LightColor0;
uniform vec4 _WorldSpaceLightPos0;
uniform float _TileSize;
uniform float _SpecPower;
uniform sampler2D _SpecColorTex;
uniform sampler2D _ShadowMapTexture;
uniform float _Repetition;
uniform sampler2D _NormalMap;
uniform float _Indent;
uniform sampler2D _DiffuseTex;
uniform sampler2D _DiamondSpecTex;
uniform float _DiamondSpecParam3;
uniform float _DiamondSpecParam2;
uniform float _DiamondSpecParam1;
void main ()
{
  xlat_mutable__SpecColor = _SpecColor;
  xlat_mutable__LightColor0 = _LightColor0;
  vec4 c;
  vec3 tmpvar_1;
  tmpvar_1 = max (((normalize (abs (xlv_TEXCOORD2)) - 0.2) * 7.0), vec3(0.0, 0.0, 0.0));
  vec3 tmpvar_2;
  tmpvar_2 = (tmpvar_1 / vec3(((tmpvar_1.x + tmpvar_1.y) + tmpvar_1.z)));
  float tmpvar_3;
  tmpvar_3 = (float(int(floor ((xlv_TEXCOORD3.x + 0.5)))) * _TileSize);
  float tmpvar_4;
  tmpvar_4 = (floor (tmpvar_3) * _TileSize);
  vec3 tmpvar_5;
  tmpvar_5.x = fract (tmpvar_3);
  tmpvar_5.y = fract (tmpvar_4);
  tmpvar_5.z = floor (tmpvar_4);
  float tmpvar_6;
  tmpvar_6 = (float(int(floor ((xlv_TEXCOORD3.y + 0.5)))) * _TileSize);
  float tmpvar_7;
  tmpvar_7 = (floor (tmpvar_6) * _TileSize);
  vec3 tmpvar_8;
  tmpvar_8.x = fract (tmpvar_6);
  tmpvar_8.y = fract (tmpvar_7);
  tmpvar_8.z = floor (tmpvar_7);
  float tmpvar_9;
  tmpvar_9 = (float(int(floor ((xlv_TEXCOORD3.z + 0.5)))) * _TileSize);
  float tmpvar_10;
  tmpvar_10 = (floor (tmpvar_9) * _TileSize);
  vec3 tmpvar_11;
  tmpvar_11.x = fract (tmpvar_9);
  tmpvar_11.y = fract (tmpvar_10);
  tmpvar_11.z = floor (tmpvar_10);
  vec2 tmpvar_12;
  tmpvar_12 = (tmpvar_5.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.xy / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_13;
  tmpvar_13.zw = vec2(0.0, 0.0);
  tmpvar_13.x = tmpvar_12.x;
  tmpvar_13.y = tmpvar_12.y;
  vec2 tmpvar_14;
  tmpvar_14 = (tmpvar_5.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.yz / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_15;
  tmpvar_15.zw = vec2(0.0, 0.0);
  tmpvar_15.x = tmpvar_14.x;
  tmpvar_15.y = tmpvar_14.y;
  vec2 tmpvar_16;
  tmpvar_16 = (tmpvar_5.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.zx / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_17;
  tmpvar_17.zw = vec2(0.0, 0.0);
  tmpvar_17.x = tmpvar_16.x;
  tmpvar_17.y = tmpvar_16.y;
  vec2 tmpvar_18;
  tmpvar_18 = (tmpvar_8.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.xy / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_19;
  tmpvar_19.zw = vec2(0.0, 0.0);
  tmpvar_19.x = tmpvar_18.x;
  tmpvar_19.y = tmpvar_18.y;
  vec2 tmpvar_20;
  tmpvar_20 = (tmpvar_8.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.yz / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_21;
  tmpvar_21.zw = vec2(0.0, 0.0);
  tmpvar_21.x = tmpvar_20.x;
  tmpvar_21.y = tmpvar_20.y;
  vec2 tmpvar_22;
  tmpvar_22 = (tmpvar_8.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.zx / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_23;
  tmpvar_23.zw = vec2(0.0, 0.0);
  tmpvar_23.x = tmpvar_22.x;
  tmpvar_23.y = tmpvar_22.y;
  vec2 tmpvar_24;
  tmpvar_24 = (tmpvar_11.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.xy / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_25;
  tmpvar_25.zw = vec2(0.0, 0.0);
  tmpvar_25.x = tmpvar_24.x;
  tmpvar_25.y = tmpvar_24.y;
  vec2 tmpvar_26;
  tmpvar_26 = (tmpvar_11.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.yz / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_27;
  tmpvar_27.zw = vec2(0.0, 0.0);
  tmpvar_27.x = tmpvar_26.x;
  tmpvar_27.y = tmpvar_26.y;
  vec2 tmpvar_28;
  tmpvar_28 = (tmpvar_11.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.zx / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_29;
  tmpvar_29.zw = vec2(0.0, 0.0);
  tmpvar_29.x = tmpvar_28.x;
  tmpvar_29.y = tmpvar_28.y;
  vec4 tmpvar_30;
  vec2 tmpvar_31;
  tmpvar_31 = (tmpvar_5.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.yz / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_32;
  tmpvar_32.zw = vec2(0.0, 0.0);
  tmpvar_32.x = tmpvar_31.x;
  tmpvar_32.y = tmpvar_31.y;
  tmpvar_30 = (texture2DLod (_NormalMap, tmpvar_32.xy, 0.0) - 0.5);
  vec4 tmpvar_33;
  vec2 tmpvar_34;
  tmpvar_34 = (tmpvar_5.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.zx / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_35;
  tmpvar_35.zw = vec2(0.0, 0.0);
  tmpvar_35.x = tmpvar_34.x;
  tmpvar_35.y = tmpvar_34.y;
  tmpvar_33 = (texture2DLod (_NormalMap, tmpvar_35.xy, 0.0) - 0.5);
  vec4 tmpvar_36;
  vec2 tmpvar_37;
  tmpvar_37 = (tmpvar_5.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.xy / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_38;
  tmpvar_38.zw = vec2(0.0, 0.0);
  tmpvar_38.x = tmpvar_37.x;
  tmpvar_38.y = tmpvar_37.y;
  tmpvar_36 = (texture2DLod (_NormalMap, tmpvar_38.xy, 0.0) - 0.5);
  vec3 tmpvar_39;
  tmpvar_39.x = 0.0;
  tmpvar_39.y = tmpvar_30.x;
  tmpvar_39.z = tmpvar_30.y;
  vec3 tmpvar_40;
  tmpvar_40.y = 0.0;
  tmpvar_40.x = tmpvar_33.y;
  tmpvar_40.z = tmpvar_33.x;
  vec3 tmpvar_41;
  tmpvar_41.z = 0.0;
  tmpvar_41.x = tmpvar_36.x;
  tmpvar_41.y = tmpvar_36.y;
  vec4 tmpvar_42;
  vec2 tmpvar_43;
  tmpvar_43 = (tmpvar_8.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.yz / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_44;
  tmpvar_44.zw = vec2(0.0, 0.0);
  tmpvar_44.x = tmpvar_43.x;
  tmpvar_44.y = tmpvar_43.y;
  tmpvar_42 = (texture2DLod (_NormalMap, tmpvar_44.xy, 0.0) - 0.5);
  vec4 tmpvar_45;
  vec2 tmpvar_46;
  tmpvar_46 = (tmpvar_8.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.zx / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_47;
  tmpvar_47.zw = vec2(0.0, 0.0);
  tmpvar_47.x = tmpvar_46.x;
  tmpvar_47.y = tmpvar_46.y;
  tmpvar_45 = (texture2DLod (_NormalMap, tmpvar_47.xy, 0.0) - 0.5);
  vec4 tmpvar_48;
  vec2 tmpvar_49;
  tmpvar_49 = (tmpvar_8.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.xy / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_50;
  tmpvar_50.zw = vec2(0.0, 0.0);
  tmpvar_50.x = tmpvar_49.x;
  tmpvar_50.y = tmpvar_49.y;
  tmpvar_48 = (texture2DLod (_NormalMap, tmpvar_50.xy, 0.0) - 0.5);
  vec3 tmpvar_51;
  tmpvar_51.x = 0.0;
  tmpvar_51.y = tmpvar_42.x;
  tmpvar_51.z = tmpvar_42.y;
  vec3 tmpvar_52;
  tmpvar_52.y = 0.0;
  tmpvar_52.x = tmpvar_45.y;
  tmpvar_52.z = tmpvar_45.x;
  vec3 tmpvar_53;
  tmpvar_53.z = 0.0;
  tmpvar_53.x = tmpvar_48.x;
  tmpvar_53.y = tmpvar_48.y;
  vec4 tmpvar_54;
  vec2 tmpvar_55;
  tmpvar_55 = (tmpvar_11.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.yz / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_56;
  tmpvar_56.zw = vec2(0.0, 0.0);
  tmpvar_56.x = tmpvar_55.x;
  tmpvar_56.y = tmpvar_55.y;
  tmpvar_54 = (texture2DLod (_NormalMap, tmpvar_56.xy, 0.0) - 0.5);
  vec4 tmpvar_57;
  vec2 tmpvar_58;
  tmpvar_58 = (tmpvar_11.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.zx / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_59;
  tmpvar_59.zw = vec2(0.0, 0.0);
  tmpvar_59.x = tmpvar_58.x;
  tmpvar_59.y = tmpvar_58.y;
  tmpvar_57 = (texture2DLod (_NormalMap, tmpvar_59.xy, 0.0) - 0.5);
  vec4 tmpvar_60;
  vec2 tmpvar_61;
  tmpvar_61 = (tmpvar_11.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.xy / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_62;
  tmpvar_62.zw = vec2(0.0, 0.0);
  tmpvar_62.x = tmpvar_61.x;
  tmpvar_62.y = tmpvar_61.y;
  tmpvar_60 = (texture2DLod (_NormalMap, tmpvar_62.xy, 0.0) - 0.5);
  vec3 tmpvar_63;
  tmpvar_63.x = 0.0;
  tmpvar_63.y = tmpvar_54.x;
  tmpvar_63.z = tmpvar_54.y;
  vec3 tmpvar_64;
  tmpvar_64.y = 0.0;
  tmpvar_64.x = tmpvar_57.y;
  tmpvar_64.z = tmpvar_57.x;
  vec3 tmpvar_65;
  tmpvar_65.z = 0.0;
  tmpvar_65.x = tmpvar_60.x;
  tmpvar_65.y = tmpvar_60.y;
  vec3 tmpvar_66;
  tmpvar_66 = ((((xlv_TEXCOORD4.x * (((tmpvar_39 * tmpvar_2.xxx) + (tmpvar_40 * tmpvar_2.yyy)) + (tmpvar_41 * tmpvar_2.zzz))) * 2.0) + ((xlv_TEXCOORD4.y * (((tmpvar_51 * tmpvar_2.xxx) + (tmpvar_52 * tmpvar_2.yyy)) + (tmpvar_53 * tmpvar_2.zzz))) * 2.0)) + ((xlv_TEXCOORD4.z * (((tmpvar_63 * tmpvar_2.xxx) + (tmpvar_64 * tmpvar_2.yyy)) + (tmpvar_65 * tmpvar_2.zzz))) * 2.0));
  vec3 tmpvar_67;
  tmpvar_67 = ((((((xlv_TEXCOORD4.x * (((texture2DLod (_DiffuseTex, tmpvar_13.xy, 0.0) * tmpvar_2.z) + (texture2DLod (_DiffuseTex, tmpvar_15.xy, 0.0) * tmpvar_2.x)) + (texture2DLod (_DiffuseTex, tmpvar_17.xy, 0.0) * tmpvar_2.y))) + (xlv_TEXCOORD4.y * (((texture2DLod (_DiffuseTex, tmpvar_19.xy, 0.0) * tmpvar_2.z) + (texture2DLod (_DiffuseTex, tmpvar_21.xy, 0.0) * tmpvar_2.x)) + (texture2DLod (_DiffuseTex, tmpvar_23.xy, 0.0) * tmpvar_2.y)))) + (xlv_TEXCOORD4.z * (((texture2DLod (_DiffuseTex, tmpvar_25.xy, 0.0) * tmpvar_2.z) + (texture2DLod (_DiffuseTex, tmpvar_27.xy, 0.0) * tmpvar_2.x)) + (texture2DLod (_DiffuseTex, tmpvar_29.xy, 0.0) * tmpvar_2.y)))).xyz * xlv_COLOR0.xyz) * (1.0 - xlv_COLOR0.w)) + (xlv_COLOR0.xyz * xlv_COLOR0.w));
  vec4 tmpvar_68;
  tmpvar_68.w = 1.0;
  tmpvar_68.x = tmpvar_66.x;
  tmpvar_68.y = tmpvar_66.y;
  tmpvar_68.z = tmpvar_66.z;
  vec3 tmpvar_69;
  tmpvar_69 = normalize ((xlv_TEXCOORD0 - (normalize (tmpvar_68).xyz * xlv_TEXCOORD3.w)));
  float tmpvar_70;
  tmpvar_70 = (xlv_TEXCOORD4.w * _SpecPower);
  vec4 tmpvar_71;
  tmpvar_71.w = 0.0;
  tmpvar_71.xyz = xlv_TEXCOORD3.xyz;
  vec4 tmpvar_72;
  tmpvar_72.yzw = vec3(0.1, 0.0, 0.0);
  tmpvar_72.x = (dot (xlv_TEXCOORD4, tmpvar_71) / 255.0);
  vec4 tmpvar_73;
  tmpvar_73 = texture2DLod (_SpecColorTex, tmpvar_72.xy, 0.0);
  vec4 tmpvar_74;
  tmpvar_74 = texture2DProj (_ShadowMapTexture, xlv_TEXCOORD8);
  c = vec4(0.0, 0.0, 0.0, 0.0);
  vec3 tmpvar_75;
  tmpvar_75 = normalize (xlv_TEXCOORD7);
  vec3 normLightDir;
  normLightDir = _WorldSpaceLightPos0.xyz;
  vec4 c_i0;
  float m;
  m = tmpvar_73.x;
  if ((tmpvar_73.y > tmpvar_73.x)) {
    m = tmpvar_73.y;
  };
  if ((tmpvar_73.z > m)) {
    m = tmpvar_73.z;
  };
  if ((m > 0.03)) {
    float tmpvar_76;
    tmpvar_76 = (((((tmpvar_75.x * tmpvar_75.x) * tmpvar_75.y) + (tmpvar_75.z * tmpvar_75.x)) + (tmpvar_75.y * tmpvar_75.z)) * _DiamondSpecParam1);
    vec4 tmpvar_77;
    tmpvar_77.zw = vec2(0.0, 0.0);
    tmpvar_77.x = tmpvar_76;
    tmpvar_77.y = _DiamondSpecParam2;
    xlat_mutable__SpecColor.xyz = texture2DLod (_DiamondSpecTex, tmpvar_77.xy, 0.0).xyz;
    vec4 tmpvar_78;
    tmpvar_78.zw = vec2(0.0, 0.0);
    tmpvar_78.x = tmpvar_76;
    tmpvar_78.y = _DiamondSpecParam3;
    xlat_mutable__LightColor0.xyz = (_LightColor0.xyz * texture2DLod (_DiamondSpecTex, tmpvar_78.xy, 0.0).xyz);
  } else {
    if ((m > 0.0)) {
      xlat_mutable__SpecColor.xyz = (tmpvar_73.xyz * (1.0/(m)));
    } else {
      xlat_mutable__SpecColor.xyz = vec3(1.0, 1.0, 1.0);
    };
  };
  c_i0 = vec4(0.0, 0.0, 0.0, 0.0);
  c_i0.xyz = ((((tmpvar_67 * xlat_mutable__LightColor0.xyz) * dot (tmpvar_69, normLightDir)) + ((xlat_mutable__LightColor0.xyz * xlat_mutable__SpecColor.xyz) * (smoothstep (0.0, 1.0, pow (max (0.0, dot (tmpvar_69, normalize ((_WorldSpaceLightPos0.xyz + tmpvar_75)))), (32.0 * tmpvar_70))) * tmpvar_70))) * tmpvar_74.x);
  c_i0.w = 1.0;
  c = c_i0;
  c.xyz = (c_i0.xyz + (tmpvar_67 * xlv_TEXCOORD6));
  c.xyz = (c.xyz + tmpvar_73.xyz);
  gl_FragData[0] = c;
}


#endif
"
}

SubProgram "d3d9 " {
Keywords { "DIRECTIONAL" "LIGHTMAP_OFF" "DIRLIGHTMAP_OFF" "SHADOWS_SCREEN" }
Bind "vertex" Vertex
Bind "texcoord" TexCoord0
Bind "texcoord1" TexCoord1
Bind "color" Color
Matrix 0 [glstate_matrix_mvp]
Vector 8 [_ProjectionParams]
Vector 9 [_ScreenParams]
Vector 10 [unity_Scale]
Vector 11 [_WorldSpaceCameraPos]
Matrix 4 [_Object2World]
Vector 12 [unity_SHAr]
Vector 13 [unity_SHAg]
Vector 14 [unity_SHAb]
Vector 15 [unity_SHBr]
Vector 16 [unity_SHBg]
Vector 17 [unity_SHBb]
Vector 18 [unity_SHC]
Float 19 [_NormalPower]
SetTexture 0 [_NormSpecPowerTex] 2D
"vs_3_0
; 127 ALU, 2 TEX
dcl_position o0
dcl_texcoord0 o1
dcl_color0 o2
dcl_texcoord1 o3
dcl_texcoord2 o4
dcl_texcoord3 o5
dcl_texcoord4 o6
dcl_texcoord5 o7
dcl_texcoord6 o8
dcl_texcoord7 o9
dcl_texcoord8 o10
def c20, 0.25000000, 0.00000000, 1.00000000, 0.00390625
def c21, 256.00000000, 0.50000000, 10.00000000, 0.49990001
def c22, -0.49990001, 2.00000000, 0.00392157, 4.00000000
def c23, 0.10000000, 0.00000000, 16.00000000, -8.00000000
def c24, -2.00000000, 0.05000000, 0, 0
dcl_position0 v0
dcl_texcoord0 v3
dcl_texcoord1 v4
dcl_color0 v5
dcl_2d s0
abs r0.y, v4
frc r0.z, r0.y
slt r0.x, v4.y, c20.y
max r0.x, -r0, r0
slt r0.x, c20.y, r0
add r0.y, r0, -r0.z
add r0.w, -r0.x, c20.z
mul r0.z, r0.y, r0.w
mad r0.x, r0, -r0.y, r0.z
add r0.y, v4, -r0.x
mul r0.y, r0, c21.z
mov r4.xy, v3
slt r0.z, r0.y, c22.x
add r0.w, r0.y, c21
max r0.y, -r0.z, r0.z
abs r0.z, r0.w
frc r0.w, r0.z
slt r0.y, c20, r0
add r0.z, r0, -r0.w
add r1.x, -r0.y, c20.z
mul r0.w, r0.z, r1.x
mad r0.y, r0, -r0.z, r0.w
sge r0.w, c20.y, r0.y
sge r0.z, r0.y, c20.y
mul r3.x, r0.z, r0.w
sge r0.w, c20.z, r0.y
sge r0.z, r0.y, c20
mul r3.y, r0.z, r0.w
mul r0.z, r0.x, c20.w
abs r0.w, r0.z
sge r0.x, r0.y, c22.y
sge r0.y, c22, r0
mul r3.z, r0.x, r0.y
slt r0.x, r0.z, c20.y
frc r1.x, r0.w
max r0.x, -r0, r0
add r0.y, r0.w, -r1.x
frc r0.z, r0
mul r0.w, r0.z, c21.x
slt r0.x, c20.y, r0
abs r1.x, r0.w
add r1.x, r1, c21.y
add r0.z, -r0.x, c20
mul r0.z, r0.y, r0
mad r2.y, r0.x, -r0, r0.z
slt r0.y, r0.w, c20
frc r1.y, r1.x
add r1.y, r1.x, -r1
max r0.z, -r0.y, r0.y
mul r0.x, v4, c20
slt r1.x, c20.y, r0.z
slt r0.y, r0.x, c20
abs r0.z, r0.x
max r0.y, -r0, r0
slt r0.x, c20.y, r0.y
frc r0.y, r0.z
add r0.y, r0.z, -r0
add r0.w, -r0.x, c20.z
mul r0.z, r0.y, r0.w
mad r2.x, r0, -r0.y, r0.z
add r0.w, -r1.x, c20.z
mul r0.y, r1, r0.w
mad r2.z, r1.x, -r1.y, r0.y
mad r0.x, -r2, c22.w, v4
add r4.z, r0.x, c24.x
mul r1.xyz, r4, c10.w
dp3 r4.w, r1, c5
dp3 r5.w, r1, c6
dp3 r0.x, r1, c4
mov r0.y, r4.w
mov r0.z, r5.w
mov r0.w, c20.z
mul r1, r0.xyzz, r0.yzzx
dp4 r5.z, r0, c14
dp4 r5.y, r0, c13
dp4 r5.x, r0, c12
dp4 r0.w, r1, c17
dp4 r0.y, r1, c15
dp4 r0.z, r1, c16
dp3 r2.w, r3, r2
add r5.xyz, r5, r0.yzww
mul r0.y, r4.w, r4.w
mad r0.y, r0.x, r0.x, -r0
mul r0.yzw, r0.y, c18.xxyz
add o8.xyz, r5, r0.yzww
mov o7.x, r0
dp4 r1.w, v0, c3
mul r1.x, r2.w, c22.z
mov r1.yz, c23.xxyw
texldl r1.xy, r1.xyzz, s0
mul r3.w, r1.y, c22
mad r1.x, r1, c23.z, c23.w
mul r2.w, r1.x, c19.x
dp4 r1.z, v0, c2
dp4 r1.x, v0, c0
dp4 r1.y, v0, c1
mov o5, r2
mul r2.xyz, r1.xyww, c21.y
mov r0.z, r2.x
mul r0.w, r2.y, c8.x
mad o10.xy, r2.z, c9.zwzw, r0.zwzw
dp4 r0.z, v0, c6
dp4 r0.x, v0, c4
dp4 r0.y, v0, c5
mov o6, r3
mov o0, r1
mov o2, v5
mov o10.zw, r1
dp3 o1.z, r4, c6
dp3 o1.y, r4, c5
dp3 o1.x, r4, c4
mul o3.xyz, v0, c24.y
mov o4.xyz, r4
mov o7.z, r5.w
mov o7.y, r4.w
add o9.xyz, -r0, c11
"
}

SubProgram "opengl " {
Keywords { "DIRECTIONAL" "LIGHTMAP_ON" "DIRLIGHTMAP_OFF" "SHADOWS_SCREEN" }
"!!GLSL
#ifdef VERTEX
#define DIRECTIONAL 1
#define LIGHTMAP_ON 1
#define DIRLIGHTMAP_OFF 1
#define SHADOWS_SCREEN 1
#define SHADER_API_OPENGL 1
#define SHADER_API_DESKTOP 1
vec4 xll_tex2Dlod(sampler2D s, vec4 coord) {
   return texture2DLod( s, coord.xy, coord.w);
}
mat3 xll_constructMat3( mat4 m) {
  return mat3( vec3( m[0]), vec3( m[1]), vec3( m[2]));
}
struct v2f_vertex_lit {
    vec2 uv;
    vec4 diff;
    vec4 spec;
};
struct v2f_img {
    vec4 pos;
    vec2 uv;
};
struct appdata_img {
    vec4 vertex;
    vec2 texcoord;
};
struct SurfaceOutput {
    vec3 Albedo;
    vec3 Normal;
    vec3 Emission;
    float Specular;
    float Gloss;
    float Alpha;
};
struct CustomSurfaceOutput {
    vec3 Albedo;
    vec3 Normal;
    vec3 Normal_;
    vec3 Emission;
    float Specular;
    float Alpha;
};
struct appdata_full {
    vec4 vertex;
    vec4 tangent;
    vec3 normal;
    vec4 texcoord;
    vec4 texcoord1;
    vec4 color;
};
struct Input {
    vec3 localPos;
    vec3 localNormal;
    vec3 worldNormal;
    vec4 weights;
    vec4 blendMask;
    vec4 color;
};
struct v2f_surf {
    vec4 pos;
    vec3 worldNormal;
    vec4 color;
    vec3 cust_localPos;
    vec3 cust_localNormal;
    vec4 cust_weights;
    vec4 cust_blendMask;
    vec2 lmap;
    vec4 _ShadowCoord;
};
uniform sampler2D _NormSpecPowerTex;
uniform float _NormalPower;
uniform mat4 _Object2World;
uniform vec4 _ProjectionParams;

uniform vec4 unity_LightmapST;
uniform vec4 unity_Scale;
void vert( inout appdata_full v, out Input o );
vec4 ComputeScreenPos( in vec4 pos );
v2f_surf vert_surf( in appdata_full v );
void vert( inout appdata_full v, out Input o ) {
    int mat0;
    int mat12;
    vec3 matIdx;
    int idx;
    vec2 normSpec;
    mat0 = int( (v.texcoord1.x  / 4.00000) );
    mat12 = int( v.texcoord1.y  );
    matIdx = vec3( float( mat0 ), float( (mat12 / 256) ), float( (mat12 % 256) ));
    o.weights.xyz  = matIdx;
    idx = int( (((v.texcoord1.y  - float( mat12 )) * 10.0000) + 0.499900) );
    o.blendMask.xyz  = vec3( float( (idx == 0) ), float( (idx == 1) ), float( (idx == 2) ));
    normSpec = xll_tex2Dlod( _NormSpecPowerTex, vec4( (dot( o.blendMask, vec4( matIdx, 0.000000)) / 255.000), 0.100000, 0.000000, 0.000000)).xy ;
    o.weights.w  = (((normSpec.x  * 16.0000) - 8.00000) * _NormalPower);
    o.blendMask.w  = (normSpec.y  * 4.00000);
    v.normal = vec3( v.texcoord.x , v.texcoord.y , ((v.texcoord1.x  - float( (4 * mat0) )) - 2.00000));
    o.localPos = vec3( (v.vertex * 0.0500000));
    o.localNormal = v.normal;
}
vec4 ComputeScreenPos( in vec4 pos ) {
    vec4 o;
    o = (pos * 0.500000);
    o.xy  = (vec2( o.x , (o.y  * _ProjectionParams.x )) + o.w );
    o.zw  = pos.zw ;
    return o;
}
v2f_surf vert_surf( in appdata_full v ) {
    Input customInputData;
    v2f_surf o;
    vec3 worldN;
    vert( v, customInputData);
    o.cust_localPos = customInputData.localPos;
    o.cust_localNormal = customInputData.localNormal;
    o.cust_weights = customInputData.weights;
    o.cust_blendMask = customInputData.blendMask;
    o.pos = ( gl_ModelViewProjectionMatrix * v.vertex );
    o.worldNormal = ( xll_constructMat3( _Object2World) * v.normal );
    o.color = v.color;
    o.lmap.xy  = ((v.texcoord1.xy  * unity_LightmapST.xy ) + unity_LightmapST.zw );
    worldN = ( xll_constructMat3( _Object2World) * (v.normal * unity_Scale.w ) );
    o._ShadowCoord = ComputeScreenPos( o.pos);
    return o;
}
attribute vec4 TANGENT;
varying vec3 xlv_TEXCOORD0;
varying vec4 xlv_COLOR0;
varying vec3 xlv_TEXCOORD1;
varying vec3 xlv_TEXCOORD2;
varying vec4 xlv_TEXCOORD3;
varying vec4 xlv_TEXCOORD4;
varying vec2 xlv_TEXCOORD5;
varying vec4 xlv_TEXCOORD6;
void main() {
    v2f_surf xl_retval;
    appdata_full xlt_v;
    xlt_v.vertex = vec4( gl_Vertex);
    xlt_v.tangent = vec4( TANGENT);
    xlt_v.normal = vec3( gl_Normal);
    xlt_v.texcoord = vec4( gl_MultiTexCoord0);
    xlt_v.texcoord1 = vec4( gl_MultiTexCoord1);
    xlt_v.color = vec4( gl_Color);
    xl_retval = vert_surf( xlt_v);
    gl_Position = vec4( xl_retval.pos);
    xlv_TEXCOORD0 = vec3( xl_retval.worldNormal);
    xlv_COLOR0 = vec4( xl_retval.color);
    xlv_TEXCOORD1 = vec3( xl_retval.cust_localPos);
    xlv_TEXCOORD2 = vec3( xl_retval.cust_localNormal);
    xlv_TEXCOORD3 = vec4( xl_retval.cust_weights);
    xlv_TEXCOORD4 = vec4( xl_retval.cust_blendMask);
    xlv_TEXCOORD5 = vec2( xl_retval.lmap);
    xlv_TEXCOORD6 = vec4( xl_retval._ShadowCoord);
}
/* NOTE: GLSL optimization failed
0:87(75): error: operator '%' is reserved in (null)
0:87(56): error: cannot construct `float' from a non-numeric data type
0:87(11): error: cannot construct `vec3' from a non-numeric data type
*/

#endif
#ifdef FRAGMENT
#extension GL_ARB_shader_texture_lod : enable
varying vec4 xlv_TEXCOORD6;
varying vec2 xlv_TEXCOORD5;
varying vec4 xlv_TEXCOORD4;
varying vec4 xlv_TEXCOORD3;
varying vec3 xlv_TEXCOORD2;
varying vec3 xlv_TEXCOORD1;
varying vec4 xlv_COLOR0;
uniform sampler2D unity_Lightmap;
uniform float _TileSize;
uniform sampler2D _SpecColorTex;
uniform sampler2D _ShadowMapTexture;
uniform float _Repetition;
uniform float _Indent;
uniform sampler2D _DiffuseTex;
void main ()
{
  vec4 c;
  vec3 tmpvar_1;
  tmpvar_1 = max (((normalize (abs (xlv_TEXCOORD2)) - 0.2) * 7.0), vec3(0.0, 0.0, 0.0));
  vec3 tmpvar_2;
  tmpvar_2 = (tmpvar_1 / vec3(((tmpvar_1.x + tmpvar_1.y) + tmpvar_1.z)));
  float tmpvar_3;
  tmpvar_3 = (float(int(floor ((xlv_TEXCOORD3.x + 0.5)))) * _TileSize);
  float tmpvar_4;
  tmpvar_4 = (floor (tmpvar_3) * _TileSize);
  vec3 tmpvar_5;
  tmpvar_5.x = fract (tmpvar_3);
  tmpvar_5.y = fract (tmpvar_4);
  tmpvar_5.z = floor (tmpvar_4);
  float tmpvar_6;
  tmpvar_6 = (float(int(floor ((xlv_TEXCOORD3.y + 0.5)))) * _TileSize);
  float tmpvar_7;
  tmpvar_7 = (floor (tmpvar_6) * _TileSize);
  vec3 tmpvar_8;
  tmpvar_8.x = fract (tmpvar_6);
  tmpvar_8.y = fract (tmpvar_7);
  tmpvar_8.z = floor (tmpvar_7);
  float tmpvar_9;
  tmpvar_9 = (float(int(floor ((xlv_TEXCOORD3.z + 0.5)))) * _TileSize);
  float tmpvar_10;
  tmpvar_10 = (floor (tmpvar_9) * _TileSize);
  vec3 tmpvar_11;
  tmpvar_11.x = fract (tmpvar_9);
  tmpvar_11.y = fract (tmpvar_10);
  tmpvar_11.z = floor (tmpvar_10);
  vec2 tmpvar_12;
  tmpvar_12 = (tmpvar_5.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.xy / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_13;
  tmpvar_13.zw = vec2(0.0, 0.0);
  tmpvar_13.x = tmpvar_12.x;
  tmpvar_13.y = tmpvar_12.y;
  vec2 tmpvar_14;
  tmpvar_14 = (tmpvar_5.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.yz / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_15;
  tmpvar_15.zw = vec2(0.0, 0.0);
  tmpvar_15.x = tmpvar_14.x;
  tmpvar_15.y = tmpvar_14.y;
  vec2 tmpvar_16;
  tmpvar_16 = (tmpvar_5.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.zx / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_17;
  tmpvar_17.zw = vec2(0.0, 0.0);
  tmpvar_17.x = tmpvar_16.x;
  tmpvar_17.y = tmpvar_16.y;
  vec2 tmpvar_18;
  tmpvar_18 = (tmpvar_8.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.xy / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_19;
  tmpvar_19.zw = vec2(0.0, 0.0);
  tmpvar_19.x = tmpvar_18.x;
  tmpvar_19.y = tmpvar_18.y;
  vec2 tmpvar_20;
  tmpvar_20 = (tmpvar_8.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.yz / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_21;
  tmpvar_21.zw = vec2(0.0, 0.0);
  tmpvar_21.x = tmpvar_20.x;
  tmpvar_21.y = tmpvar_20.y;
  vec2 tmpvar_22;
  tmpvar_22 = (tmpvar_8.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.zx / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_23;
  tmpvar_23.zw = vec2(0.0, 0.0);
  tmpvar_23.x = tmpvar_22.x;
  tmpvar_23.y = tmpvar_22.y;
  vec2 tmpvar_24;
  tmpvar_24 = (tmpvar_11.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.xy / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_25;
  tmpvar_25.zw = vec2(0.0, 0.0);
  tmpvar_25.x = tmpvar_24.x;
  tmpvar_25.y = tmpvar_24.y;
  vec2 tmpvar_26;
  tmpvar_26 = (tmpvar_11.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.yz / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_27;
  tmpvar_27.zw = vec2(0.0, 0.0);
  tmpvar_27.x = tmpvar_26.x;
  tmpvar_27.y = tmpvar_26.y;
  vec2 tmpvar_28;
  tmpvar_28 = (tmpvar_11.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.zx / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_29;
  tmpvar_29.zw = vec2(0.0, 0.0);
  tmpvar_29.x = tmpvar_28.x;
  tmpvar_29.y = tmpvar_28.y;
  vec4 tmpvar_30;
  tmpvar_30.w = 0.0;
  tmpvar_30.xyz = xlv_TEXCOORD3.xyz;
  vec4 tmpvar_31;
  tmpvar_31.yzw = vec3(0.1, 0.0, 0.0);
  tmpvar_31.x = (dot (xlv_TEXCOORD4, tmpvar_30) / 255.0);
  vec4 tmpvar_32;
  tmpvar_32 = texture2DProj (_ShadowMapTexture, xlv_TEXCOORD6);
  c = vec4(0.0, 0.0, 0.0, 0.0);
  vec4 tmpvar_33;
  tmpvar_33 = texture2D (unity_Lightmap, xlv_TEXCOORD5);
  vec3 tmpvar_34;
  tmpvar_34 = ((8.0 * tmpvar_33.w) * tmpvar_33.xyz);
  c.xyz = (((((((xlv_TEXCOORD4.x * (((texture2DLod (_DiffuseTex, tmpvar_13.xy, 0.0) * tmpvar_2.z) + (texture2DLod (_DiffuseTex, tmpvar_15.xy, 0.0) * tmpvar_2.x)) + (texture2DLod (_DiffuseTex, tmpvar_17.xy, 0.0) * tmpvar_2.y))) + (xlv_TEXCOORD4.y * (((texture2DLod (_DiffuseTex, tmpvar_19.xy, 0.0) * tmpvar_2.z) + (texture2DLod (_DiffuseTex, tmpvar_21.xy, 0.0) * tmpvar_2.x)) + (texture2DLod (_DiffuseTex, tmpvar_23.xy, 0.0) * tmpvar_2.y)))) + (xlv_TEXCOORD4.z * (((texture2DLod (_DiffuseTex, tmpvar_25.xy, 0.0) * tmpvar_2.z) + (texture2DLod (_DiffuseTex, tmpvar_27.xy, 0.0) * tmpvar_2.x)) + (texture2DLod (_DiffuseTex, tmpvar_29.xy, 0.0) * tmpvar_2.y)))).xyz * xlv_COLOR0.xyz) * (1.0 - xlv_COLOR0.w)) + (xlv_COLOR0.xyz * xlv_COLOR0.w)) * max (min (tmpvar_34, ((tmpvar_32.x * 2.0) * tmpvar_33.xyz)), (tmpvar_34 * tmpvar_32.x)));
  c.w = 0.0;
  c.xyz = (c.xyz + texture2DLod (_SpecColorTex, tmpvar_31.xy, 0.0).xyz);
  gl_FragData[0] = c;
}


#endif
"
}

SubProgram "d3d9 " {
Keywords { "DIRECTIONAL" "LIGHTMAP_ON" "DIRLIGHTMAP_OFF" "SHADOWS_SCREEN" }
Bind "vertex" Vertex
Bind "texcoord" TexCoord0
Bind "texcoord1" TexCoord1
Bind "color" Color
Matrix 0 [glstate_matrix_mvp]
Vector 8 [_ProjectionParams]
Vector 9 [_ScreenParams]
Matrix 4 [_Object2World]
Float 10 [_NormalPower]
Vector 11 [unity_LightmapST]
SetTexture 0 [_NormSpecPowerTex] 2D
"vs_3_0
; 102 ALU, 2 TEX
dcl_position o0
dcl_texcoord0 o1
dcl_color0 o2
dcl_texcoord1 o3
dcl_texcoord2 o4
dcl_texcoord3 o5
dcl_texcoord4 o6
dcl_texcoord5 o7
dcl_texcoord6 o8
def c12, 0.25000000, 0.00000000, 1.00000000, 0.00390625
def c13, 256.00000000, 0.50000000, 10.00000000, 0.49990001
def c14, -0.49990001, 2.00000000, 0.00392157, 4.00000000
def c15, 0.10000000, 0.00000000, 16.00000000, -8.00000000
def c16, -2.00000000, 0.05000000, 0, 0
dcl_position0 v0
dcl_texcoord0 v3
dcl_texcoord1 v4
dcl_color0 v5
dcl_2d s0
abs r0.y, v4
frc r0.z, r0.y
slt r0.x, v4.y, c12.y
max r0.x, -r0, r0
slt r0.x, c12.y, r0
add r0.y, r0, -r0.z
add r0.w, -r0.x, c12.z
mul r0.z, r0.y, r0.w
mad r0.z, r0.x, -r0.y, r0
add r0.x, v4.y, -r0.z
mul r0.x, r0, c13.z
mul r1.y, r0.z, c12.w
slt r0.y, r0.x, c14.x
add r0.w, r0.x, c13
max r0.x, -r0.y, r0.y
abs r0.y, r0.w
slt r0.x, c12.y, r0
frc r0.w, r0.y
add r0.y, r0, -r0.w
add r1.x, -r0, c12.z
mul r0.w, r0.y, r1.x
mad r0.w, r0.x, -r0.y, r0
sge r0.y, c12, r0.w
sge r0.x, r0.w, c12.y
mul r0.x, r0, r0.y
sge r1.x, c12.z, r0.w
sge r0.y, r0.w, c12.z
mul r0.y, r0, r1.x
sge r1.x, r0.w, c14.y
sge r0.w, c14.y, r0
mul r0.z, r1.x, r0.w
abs r1.x, r1.y
frc r1.z, r1.x
add r1.x, r1, -r1.z
slt r0.w, r1.y, c12.y
frc r1.z, r1.y
mul r1.z, r1, c13.x
max r0.w, -r0, r0
slt r0.w, c12.y, r0
add r1.y, -r0.w, c12.z
mul r1.y, r1.x, r1
mad r2.x, r0.w, -r1, r1.y
abs r1.w, r1.z
slt r0.w, r1.z, c12.y
add r1.x, r1.w, c13.y
mul r1.z, v4.x, c12.x
frc r1.y, r1.x
slt r1.w, r1.z, c12.y
max r0.w, -r0, r0
add r1.x, r1, -r1.y
slt r0.w, c12.y, r0
add r1.y, -r0.w, c12.z
abs r2.y, r1.z
max r1.w, -r1, r1
slt r1.z, c12.y, r1.w
frc r1.w, r2.y
add r1.w, r2.y, -r1
mul r1.y, r1.x, r1
add r2.z, -r1, c12
mul r2.z, r1.w, r2
mad r2.w, r1.z, -r1, r2.z
mad r2.y, r0.w, -r1.x, r1
dp3 r0.w, r0, r2.wxyw
dp4 r1.w, v0, c3
mov r1.yz, c15.xxyw
mul r1.x, r0.w, c14.z
texldl r1.xy, r1.xyzz, s0
mad r0.w, r1.x, c15.z, c15
mul r2.z, r0.w, c10.x
mul r0.w, r1.y, c14
dp4 r1.z, v0, c2
mov o6, r0
dp4 r1.x, v0, c0
dp4 r1.y, v0, c1
mov o5, r2.wxyz
mul r2.xyz, r1.xyww, c13.y
mov r0.x, r2
mul r0.y, r2, c8.x
mad o8.xy, r2.z, c9.zwzw, r0
mad r0.x, -r2.w, c14.w, v4
add r0.z, r0.x, c16.x
mov r0.xy, v3
mov o0, r1
mov o2, v5
mov o8.zw, r1
dp3 o1.z, r0, c6
dp3 o1.y, r0, c5
dp3 o1.x, r0, c4
mul o3.xyz, v0, c16.y
mov o4.xyz, r0
mad o7.xy, v4, c11, c11.zwzw
"
}

SubProgram "opengl " {
Keywords { "DIRECTIONAL" "LIGHTMAP_OFF" "DIRLIGHTMAP_OFF" "SHADOWS_OFF" "VERTEXLIGHT_ON" }
"!!GLSL
#ifdef VERTEX
#define DIRECTIONAL 1
#define LIGHTMAP_OFF 1
#define DIRLIGHTMAP_OFF 1
#define SHADOWS_OFF 1
#define VERTEXLIGHT_ON 1
#define SHADER_API_OPENGL 1
#define SHADER_API_DESKTOP 1
vec4 xll_tex2Dlod(sampler2D s, vec4 coord) {
   return texture2DLod( s, coord.xy, coord.w);
}
mat3 xll_constructMat3( mat4 m) {
  return mat3( vec3( m[0]), vec3( m[1]), vec3( m[2]));
}
struct v2f_vertex_lit {
    vec2 uv;
    vec4 diff;
    vec4 spec;
};
struct v2f_img {
    vec4 pos;
    vec2 uv;
};
struct appdata_img {
    vec4 vertex;
    vec2 texcoord;
};
struct SurfaceOutput {
    vec3 Albedo;
    vec3 Normal;
    vec3 Emission;
    float Specular;
    float Gloss;
    float Alpha;
};
struct CustomSurfaceOutput {
    vec3 Albedo;
    vec3 Normal;
    vec3 Normal_;
    vec3 Emission;
    float Specular;
    float Alpha;
};
struct appdata_full {
    vec4 vertex;
    vec4 tangent;
    vec3 normal;
    vec4 texcoord;
    vec4 texcoord1;
    vec4 color;
};
struct Input {
    vec3 localPos;
    vec3 localNormal;
    vec3 worldNormal;
    vec4 weights;
    vec4 blendMask;
    vec4 color;
};
struct v2f_surf {
    vec4 pos;
    vec3 worldNormal;
    vec4 color;
    vec3 cust_localPos;
    vec3 cust_localNormal;
    vec4 cust_weights;
    vec4 cust_blendMask;
    vec3 normal;
    vec3 vlight;
    vec3 viewDir;
};
uniform sampler2D _NormSpecPowerTex;
uniform float _NormalPower;
uniform mat4 _Object2World;
uniform vec3 _WorldSpaceCameraPos;

uniform vec4 unity_4LightAtten0;
uniform vec4 unity_4LightPosX0;
uniform vec4 unity_4LightPosY0;
uniform vec4 unity_4LightPosZ0;
uniform vec4 unity_LightColor[4];
uniform vec4 unity_SHAb;
uniform vec4 unity_SHAg;
uniform vec4 unity_SHAr;
uniform vec4 unity_SHBb;
uniform vec4 unity_SHBg;
uniform vec4 unity_SHBr;
uniform vec4 unity_SHC;
uniform vec4 unity_Scale;
void vert( inout appdata_full v, out Input o );
vec3 WorldSpaceViewDir( in vec4 v );
vec3 ShadeSH9( in vec4 normal );
vec3 Shade4PointLights( in vec4 lightPosX, in vec4 lightPosY, in vec4 lightPosZ, in vec3 lightColor0, in vec3 lightColor1, in vec3 lightColor2, in vec3 lightColor3, in vec4 lightAttenSq, in vec3 pos, in vec3 normal );
v2f_surf vert_surf( in appdata_full v );
void vert( inout appdata_full v, out Input o ) {
    int mat0;
    int mat12;
    vec3 matIdx;
    int idx;
    vec2 normSpec;
    mat0 = int( (v.texcoord1.x  / 4.00000) );
    mat12 = int( v.texcoord1.y  );
    matIdx = vec3( float( mat0 ), float( (mat12 / 256) ), float( (mat12 % 256) ));
    o.weights.xyz  = matIdx;
    idx = int( (((v.texcoord1.y  - float( mat12 )) * 10.0000) + 0.499900) );
    o.blendMask.xyz  = vec3( float( (idx == 0) ), float( (idx == 1) ), float( (idx == 2) ));
    normSpec = xll_tex2Dlod( _NormSpecPowerTex, vec4( (dot( o.blendMask, vec4( matIdx, 0.000000)) / 255.000), 0.100000, 0.000000, 0.000000)).xy ;
    o.weights.w  = (((normSpec.x  * 16.0000) - 8.00000) * _NormalPower);
    o.blendMask.w  = (normSpec.y  * 4.00000);
    v.normal = vec3( v.texcoord.x , v.texcoord.y , ((v.texcoord1.x  - float( (4 * mat0) )) - 2.00000));
    o.localPos = vec3( (v.vertex * 0.0500000));
    o.localNormal = v.normal;
}
vec3 WorldSpaceViewDir( in vec4 v ) {
    return (_WorldSpaceCameraPos.xyz  - ( _Object2World * v ).xyz );
}
vec3 ShadeSH9( in vec4 normal ) {
    vec3 x1;
    vec4 vB;
    vec3 x2;
    float vC;
    vec3 x3;
    x1.x  = dot( unity_SHAr, normal);
    x1.y  = dot( unity_SHAg, normal);
    x1.z  = dot( unity_SHAb, normal);
    vB = (normal.xyzz  * normal.yzzx );
    x2.x  = dot( unity_SHBr, vB);
    x2.y  = dot( unity_SHBg, vB);
    x2.z  = dot( unity_SHBb, vB);
    vC = ((normal.x  * normal.x ) - (normal.y  * normal.y ));
    x3 = (unity_SHC.xyz  * vC);
    return ((x1 + x2) + x3);
}
vec3 Shade4PointLights( in vec4 lightPosX, in vec4 lightPosY, in vec4 lightPosZ, in vec3 lightColor0, in vec3 lightColor1, in vec3 lightColor2, in vec3 lightColor3, in vec4 lightAttenSq, in vec3 pos, in vec3 normal ) {
    vec4 toLightX;
    vec4 toLightY;
    vec4 toLightZ;
    vec4 lengthSq;
    vec4 ndotl;
    vec4 corr;
    vec4 atten;
    vec4 diff;
    vec3 col;
    toLightX = (lightPosX - pos.x );
    toLightY = (lightPosY - pos.y );
    toLightZ = (lightPosZ - pos.z );
    lengthSq = vec4( 0.000000);
    lengthSq += (toLightX * toLightX);
    lengthSq += (toLightY * toLightY);
    lengthSq += (toLightZ * toLightZ);
    ndotl = vec4( 0.000000);
    ndotl += (toLightX * normal.x );
    ndotl += (toLightY * normal.y );
    ndotl += (toLightZ * normal.z );
    corr = inversesqrt( lengthSq );
    ndotl = max( vec4( 0.000000, 0.000000, 0.000000, 0.000000), (ndotl * corr));
    atten = (1.00000 / (1.00000 + (lengthSq * lightAttenSq)));
    diff = (ndotl * atten);
    col = vec3( 0.000000);
    col += (lightColor0 * diff.x );
    col += (lightColor1 * diff.y );
    col += (lightColor2 * diff.z );
    col += (lightColor3 * diff.w );
    return col;
}
v2f_surf vert_surf( in appdata_full v ) {
    Input customInputData;
    v2f_surf o;
    vec3 worldN;
    vec3 viewDirForLight;
    vec3 shlight;
    vec3 worldPos;
    vert( v, customInputData);
    o.cust_localPos = customInputData.localPos;
    o.cust_localNormal = customInputData.localNormal;
    o.cust_weights = customInputData.weights;
    o.cust_blendMask = customInputData.blendMask;
    o.pos = ( gl_ModelViewProjectionMatrix * v.vertex );
    o.worldNormal = ( xll_constructMat3( _Object2World) * v.normal );
    o.color = v.color;
    worldN = ( xll_constructMat3( _Object2World) * (v.normal * unity_Scale.w ) );
    o.normal = worldN;
    viewDirForLight = WorldSpaceViewDir( v.vertex);
    o.viewDir = viewDirForLight;
    shlight = ShadeSH9( vec4( worldN, 1.00000));
    o.vlight = shlight;
    worldPos = ( _Object2World * v.vertex ).xyz ;
    o.vlight += Shade4PointLights( unity_4LightPosX0, unity_4LightPosY0, unity_4LightPosZ0, unity_LightColor[ 0 ].xyz , unity_LightColor[ 1 ].xyz , unity_LightColor[ 2 ].xyz , unity_LightColor[ 3 ].xyz , unity_4LightAtten0, worldPos, worldN);
    return o;
}
attribute vec4 TANGENT;
varying vec3 xlv_TEXCOORD0;
varying vec4 xlv_COLOR0;
varying vec3 xlv_TEXCOORD1;
varying vec3 xlv_TEXCOORD2;
varying vec4 xlv_TEXCOORD3;
varying vec4 xlv_TEXCOORD4;
varying vec3 xlv_TEXCOORD5;
varying vec3 xlv_TEXCOORD6;
varying vec3 xlv_TEXCOORD7;
void main() {
    v2f_surf xl_retval;
    appdata_full xlt_v;
    xlt_v.vertex = vec4( gl_Vertex);
    xlt_v.tangent = vec4( TANGENT);
    xlt_v.normal = vec3( gl_Normal);
    xlt_v.texcoord = vec4( gl_MultiTexCoord0);
    xlt_v.texcoord1 = vec4( gl_MultiTexCoord1);
    xlt_v.color = vec4( gl_Color);
    xl_retval = vert_surf( xlt_v);
    gl_Position = vec4( xl_retval.pos);
    xlv_TEXCOORD0 = vec3( xl_retval.worldNormal);
    xlv_COLOR0 = vec4( xl_retval.color);
    xlv_TEXCOORD1 = vec3( xl_retval.cust_localPos);
    xlv_TEXCOORD2 = vec3( xl_retval.cust_localNormal);
    xlv_TEXCOORD3 = vec4( xl_retval.cust_weights);
    xlv_TEXCOORD4 = vec4( xl_retval.cust_blendMask);
    xlv_TEXCOORD5 = vec3( xl_retval.normal);
    xlv_TEXCOORD6 = vec3( xl_retval.vlight);
    xlv_TEXCOORD7 = vec3( xl_retval.viewDir);
}
/* NOTE: GLSL optimization failed
0:102(75): error: operator '%' is reserved in (null)
0:102(56): error: cannot construct `float' from a non-numeric data type
0:102(11): error: cannot construct `vec3' from a non-numeric data type
*/

#endif
#ifdef FRAGMENT
#extension GL_ARB_shader_texture_lod : enable
varying vec3 xlv_TEXCOORD7;
varying vec3 xlv_TEXCOORD6;
varying vec4 xlv_TEXCOORD4;
varying vec4 xlv_TEXCOORD3;
varying vec3 xlv_TEXCOORD2;
varying vec3 xlv_TEXCOORD1;
varying vec4 xlv_COLOR0;
varying vec3 xlv_TEXCOORD0;
vec4 xlat_mutable__SpecColor;
uniform vec4 _SpecColor;
vec4 xlat_mutable__LightColor0;
uniform vec4 _LightColor0;
uniform vec4 _WorldSpaceLightPos0;
uniform float _TileSize;
uniform float _SpecPower;
uniform sampler2D _SpecColorTex;
uniform float _Repetition;
uniform sampler2D _NormalMap;
uniform float _Indent;
uniform sampler2D _DiffuseTex;
uniform sampler2D _DiamondSpecTex;
uniform float _DiamondSpecParam3;
uniform float _DiamondSpecParam2;
uniform float _DiamondSpecParam1;
void main ()
{
  xlat_mutable__SpecColor = _SpecColor;
  xlat_mutable__LightColor0 = _LightColor0;
  vec4 c;
  vec3 tmpvar_1;
  tmpvar_1 = max (((normalize (abs (xlv_TEXCOORD2)) - 0.2) * 7.0), vec3(0.0, 0.0, 0.0));
  vec3 tmpvar_2;
  tmpvar_2 = (tmpvar_1 / vec3(((tmpvar_1.x + tmpvar_1.y) + tmpvar_1.z)));
  float tmpvar_3;
  tmpvar_3 = (float(int(floor ((xlv_TEXCOORD3.x + 0.5)))) * _TileSize);
  float tmpvar_4;
  tmpvar_4 = (floor (tmpvar_3) * _TileSize);
  vec3 tmpvar_5;
  tmpvar_5.x = fract (tmpvar_3);
  tmpvar_5.y = fract (tmpvar_4);
  tmpvar_5.z = floor (tmpvar_4);
  float tmpvar_6;
  tmpvar_6 = (float(int(floor ((xlv_TEXCOORD3.y + 0.5)))) * _TileSize);
  float tmpvar_7;
  tmpvar_7 = (floor (tmpvar_6) * _TileSize);
  vec3 tmpvar_8;
  tmpvar_8.x = fract (tmpvar_6);
  tmpvar_8.y = fract (tmpvar_7);
  tmpvar_8.z = floor (tmpvar_7);
  float tmpvar_9;
  tmpvar_9 = (float(int(floor ((xlv_TEXCOORD3.z + 0.5)))) * _TileSize);
  float tmpvar_10;
  tmpvar_10 = (floor (tmpvar_9) * _TileSize);
  vec3 tmpvar_11;
  tmpvar_11.x = fract (tmpvar_9);
  tmpvar_11.y = fract (tmpvar_10);
  tmpvar_11.z = floor (tmpvar_10);
  vec2 tmpvar_12;
  tmpvar_12 = (tmpvar_5.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.xy / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_13;
  tmpvar_13.zw = vec2(0.0, 0.0);
  tmpvar_13.x = tmpvar_12.x;
  tmpvar_13.y = tmpvar_12.y;
  vec2 tmpvar_14;
  tmpvar_14 = (tmpvar_5.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.yz / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_15;
  tmpvar_15.zw = vec2(0.0, 0.0);
  tmpvar_15.x = tmpvar_14.x;
  tmpvar_15.y = tmpvar_14.y;
  vec2 tmpvar_16;
  tmpvar_16 = (tmpvar_5.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.zx / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_17;
  tmpvar_17.zw = vec2(0.0, 0.0);
  tmpvar_17.x = tmpvar_16.x;
  tmpvar_17.y = tmpvar_16.y;
  vec2 tmpvar_18;
  tmpvar_18 = (tmpvar_8.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.xy / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_19;
  tmpvar_19.zw = vec2(0.0, 0.0);
  tmpvar_19.x = tmpvar_18.x;
  tmpvar_19.y = tmpvar_18.y;
  vec2 tmpvar_20;
  tmpvar_20 = (tmpvar_8.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.yz / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_21;
  tmpvar_21.zw = vec2(0.0, 0.0);
  tmpvar_21.x = tmpvar_20.x;
  tmpvar_21.y = tmpvar_20.y;
  vec2 tmpvar_22;
  tmpvar_22 = (tmpvar_8.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.zx / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_23;
  tmpvar_23.zw = vec2(0.0, 0.0);
  tmpvar_23.x = tmpvar_22.x;
  tmpvar_23.y = tmpvar_22.y;
  vec2 tmpvar_24;
  tmpvar_24 = (tmpvar_11.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.xy / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_25;
  tmpvar_25.zw = vec2(0.0, 0.0);
  tmpvar_25.x = tmpvar_24.x;
  tmpvar_25.y = tmpvar_24.y;
  vec2 tmpvar_26;
  tmpvar_26 = (tmpvar_11.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.yz / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_27;
  tmpvar_27.zw = vec2(0.0, 0.0);
  tmpvar_27.x = tmpvar_26.x;
  tmpvar_27.y = tmpvar_26.y;
  vec2 tmpvar_28;
  tmpvar_28 = (tmpvar_11.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.zx / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_29;
  tmpvar_29.zw = vec2(0.0, 0.0);
  tmpvar_29.x = tmpvar_28.x;
  tmpvar_29.y = tmpvar_28.y;
  vec4 tmpvar_30;
  vec2 tmpvar_31;
  tmpvar_31 = (tmpvar_5.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.yz / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_32;
  tmpvar_32.zw = vec2(0.0, 0.0);
  tmpvar_32.x = tmpvar_31.x;
  tmpvar_32.y = tmpvar_31.y;
  tmpvar_30 = (texture2DLod (_NormalMap, tmpvar_32.xy, 0.0) - 0.5);
  vec4 tmpvar_33;
  vec2 tmpvar_34;
  tmpvar_34 = (tmpvar_5.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.zx / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_35;
  tmpvar_35.zw = vec2(0.0, 0.0);
  tmpvar_35.x = tmpvar_34.x;
  tmpvar_35.y = tmpvar_34.y;
  tmpvar_33 = (texture2DLod (_NormalMap, tmpvar_35.xy, 0.0) - 0.5);
  vec4 tmpvar_36;
  vec2 tmpvar_37;
  tmpvar_37 = (tmpvar_5.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.xy / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_38;
  tmpvar_38.zw = vec2(0.0, 0.0);
  tmpvar_38.x = tmpvar_37.x;
  tmpvar_38.y = tmpvar_37.y;
  tmpvar_36 = (texture2DLod (_NormalMap, tmpvar_38.xy, 0.0) - 0.5);
  vec3 tmpvar_39;
  tmpvar_39.x = 0.0;
  tmpvar_39.y = tmpvar_30.x;
  tmpvar_39.z = tmpvar_30.y;
  vec3 tmpvar_40;
  tmpvar_40.y = 0.0;
  tmpvar_40.x = tmpvar_33.y;
  tmpvar_40.z = tmpvar_33.x;
  vec3 tmpvar_41;
  tmpvar_41.z = 0.0;
  tmpvar_41.x = tmpvar_36.x;
  tmpvar_41.y = tmpvar_36.y;
  vec4 tmpvar_42;
  vec2 tmpvar_43;
  tmpvar_43 = (tmpvar_8.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.yz / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_44;
  tmpvar_44.zw = vec2(0.0, 0.0);
  tmpvar_44.x = tmpvar_43.x;
  tmpvar_44.y = tmpvar_43.y;
  tmpvar_42 = (texture2DLod (_NormalMap, tmpvar_44.xy, 0.0) - 0.5);
  vec4 tmpvar_45;
  vec2 tmpvar_46;
  tmpvar_46 = (tmpvar_8.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.zx / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_47;
  tmpvar_47.zw = vec2(0.0, 0.0);
  tmpvar_47.x = tmpvar_46.x;
  tmpvar_47.y = tmpvar_46.y;
  tmpvar_45 = (texture2DLod (_NormalMap, tmpvar_47.xy, 0.0) - 0.5);
  vec4 tmpvar_48;
  vec2 tmpvar_49;
  tmpvar_49 = (tmpvar_8.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.xy / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_50;
  tmpvar_50.zw = vec2(0.0, 0.0);
  tmpvar_50.x = tmpvar_49.x;
  tmpvar_50.y = tmpvar_49.y;
  tmpvar_48 = (texture2DLod (_NormalMap, tmpvar_50.xy, 0.0) - 0.5);
  vec3 tmpvar_51;
  tmpvar_51.x = 0.0;
  tmpvar_51.y = tmpvar_42.x;
  tmpvar_51.z = tmpvar_42.y;
  vec3 tmpvar_52;
  tmpvar_52.y = 0.0;
  tmpvar_52.x = tmpvar_45.y;
  tmpvar_52.z = tmpvar_45.x;
  vec3 tmpvar_53;
  tmpvar_53.z = 0.0;
  tmpvar_53.x = tmpvar_48.x;
  tmpvar_53.y = tmpvar_48.y;
  vec4 tmpvar_54;
  vec2 tmpvar_55;
  tmpvar_55 = (tmpvar_11.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.yz / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_56;
  tmpvar_56.zw = vec2(0.0, 0.0);
  tmpvar_56.x = tmpvar_55.x;
  tmpvar_56.y = tmpvar_55.y;
  tmpvar_54 = (texture2DLod (_NormalMap, tmpvar_56.xy, 0.0) - 0.5);
  vec4 tmpvar_57;
  vec2 tmpvar_58;
  tmpvar_58 = (tmpvar_11.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.zx / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_59;
  tmpvar_59.zw = vec2(0.0, 0.0);
  tmpvar_59.x = tmpvar_58.x;
  tmpvar_59.y = tmpvar_58.y;
  tmpvar_57 = (texture2DLod (_NormalMap, tmpvar_59.xy, 0.0) - 0.5);
  vec4 tmpvar_60;
  vec2 tmpvar_61;
  tmpvar_61 = (tmpvar_11.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.xy / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_62;
  tmpvar_62.zw = vec2(0.0, 0.0);
  tmpvar_62.x = tmpvar_61.x;
  tmpvar_62.y = tmpvar_61.y;
  tmpvar_60 = (texture2DLod (_NormalMap, tmpvar_62.xy, 0.0) - 0.5);
  vec3 tmpvar_63;
  tmpvar_63.x = 0.0;
  tmpvar_63.y = tmpvar_54.x;
  tmpvar_63.z = tmpvar_54.y;
  vec3 tmpvar_64;
  tmpvar_64.y = 0.0;
  tmpvar_64.x = tmpvar_57.y;
  tmpvar_64.z = tmpvar_57.x;
  vec3 tmpvar_65;
  tmpvar_65.z = 0.0;
  tmpvar_65.x = tmpvar_60.x;
  tmpvar_65.y = tmpvar_60.y;
  vec3 tmpvar_66;
  tmpvar_66 = ((((xlv_TEXCOORD4.x * (((tmpvar_39 * tmpvar_2.xxx) + (tmpvar_40 * tmpvar_2.yyy)) + (tmpvar_41 * tmpvar_2.zzz))) * 2.0) + ((xlv_TEXCOORD4.y * (((tmpvar_51 * tmpvar_2.xxx) + (tmpvar_52 * tmpvar_2.yyy)) + (tmpvar_53 * tmpvar_2.zzz))) * 2.0)) + ((xlv_TEXCOORD4.z * (((tmpvar_63 * tmpvar_2.xxx) + (tmpvar_64 * tmpvar_2.yyy)) + (tmpvar_65 * tmpvar_2.zzz))) * 2.0));
  vec3 tmpvar_67;
  tmpvar_67 = ((((((xlv_TEXCOORD4.x * (((texture2DLod (_DiffuseTex, tmpvar_13.xy, 0.0) * tmpvar_2.z) + (texture2DLod (_DiffuseTex, tmpvar_15.xy, 0.0) * tmpvar_2.x)) + (texture2DLod (_DiffuseTex, tmpvar_17.xy, 0.0) * tmpvar_2.y))) + (xlv_TEXCOORD4.y * (((texture2DLod (_DiffuseTex, tmpvar_19.xy, 0.0) * tmpvar_2.z) + (texture2DLod (_DiffuseTex, tmpvar_21.xy, 0.0) * tmpvar_2.x)) + (texture2DLod (_DiffuseTex, tmpvar_23.xy, 0.0) * tmpvar_2.y)))) + (xlv_TEXCOORD4.z * (((texture2DLod (_DiffuseTex, tmpvar_25.xy, 0.0) * tmpvar_2.z) + (texture2DLod (_DiffuseTex, tmpvar_27.xy, 0.0) * tmpvar_2.x)) + (texture2DLod (_DiffuseTex, tmpvar_29.xy, 0.0) * tmpvar_2.y)))).xyz * xlv_COLOR0.xyz) * (1.0 - xlv_COLOR0.w)) + (xlv_COLOR0.xyz * xlv_COLOR0.w));
  vec4 tmpvar_68;
  tmpvar_68.w = 1.0;
  tmpvar_68.x = tmpvar_66.x;
  tmpvar_68.y = tmpvar_66.y;
  tmpvar_68.z = tmpvar_66.z;
  vec3 tmpvar_69;
  tmpvar_69 = normalize ((xlv_TEXCOORD0 - (normalize (tmpvar_68).xyz * xlv_TEXCOORD3.w)));
  float tmpvar_70;
  tmpvar_70 = (xlv_TEXCOORD4.w * _SpecPower);
  vec4 tmpvar_71;
  tmpvar_71.w = 0.0;
  tmpvar_71.xyz = xlv_TEXCOORD3.xyz;
  vec4 tmpvar_72;
  tmpvar_72.yzw = vec3(0.1, 0.0, 0.0);
  tmpvar_72.x = (dot (xlv_TEXCOORD4, tmpvar_71) / 255.0);
  vec4 tmpvar_73;
  tmpvar_73 = texture2DLod (_SpecColorTex, tmpvar_72.xy, 0.0);
  c = vec4(0.0, 0.0, 0.0, 0.0);
  vec3 tmpvar_74;
  tmpvar_74 = normalize (xlv_TEXCOORD7);
  vec3 normLightDir;
  normLightDir = _WorldSpaceLightPos0.xyz;
  vec4 c_i0;
  float m;
  m = tmpvar_73.x;
  if ((tmpvar_73.y > tmpvar_73.x)) {
    m = tmpvar_73.y;
  };
  if ((tmpvar_73.z > m)) {
    m = tmpvar_73.z;
  };
  if ((m > 0.03)) {
    float tmpvar_75;
    tmpvar_75 = (((((tmpvar_74.x * tmpvar_74.x) * tmpvar_74.y) + (tmpvar_74.z * tmpvar_74.x)) + (tmpvar_74.y * tmpvar_74.z)) * _DiamondSpecParam1);
    vec4 tmpvar_76;
    tmpvar_76.zw = vec2(0.0, 0.0);
    tmpvar_76.x = tmpvar_75;
    tmpvar_76.y = _DiamondSpecParam2;
    xlat_mutable__SpecColor.xyz = texture2DLod (_DiamondSpecTex, tmpvar_76.xy, 0.0).xyz;
    vec4 tmpvar_77;
    tmpvar_77.zw = vec2(0.0, 0.0);
    tmpvar_77.x = tmpvar_75;
    tmpvar_77.y = _DiamondSpecParam3;
    xlat_mutable__LightColor0.xyz = (_LightColor0.xyz * texture2DLod (_DiamondSpecTex, tmpvar_77.xy, 0.0).xyz);
  } else {
    if ((m > 0.0)) {
      xlat_mutable__SpecColor.xyz = (tmpvar_73.xyz * (1.0/(m)));
    } else {
      xlat_mutable__SpecColor.xyz = vec3(1.0, 1.0, 1.0);
    };
  };
  c_i0 = vec4(0.0, 0.0, 0.0, 0.0);
  c_i0.xyz = (((tmpvar_67 * xlat_mutable__LightColor0.xyz) * dot (tmpvar_69, normLightDir)) + ((xlat_mutable__LightColor0.xyz * xlat_mutable__SpecColor.xyz) * (smoothstep (0.0, 1.0, pow (max (0.0, dot (tmpvar_69, normalize ((_WorldSpaceLightPos0.xyz + tmpvar_74)))), (32.0 * tmpvar_70))) * tmpvar_70)));
  c_i0.w = 1.0;
  c = c_i0;
  c.xyz = (c_i0.xyz + (tmpvar_67 * xlv_TEXCOORD6));
  c.xyz = (c.xyz + tmpvar_73.xyz);
  gl_FragData[0] = c;
}


#endif
"
}

SubProgram "d3d9 " {
Keywords { "DIRECTIONAL" "LIGHTMAP_OFF" "DIRLIGHTMAP_OFF" "SHADOWS_OFF" "VERTEXLIGHT_ON" }
Bind "vertex" Vertex
Bind "texcoord" TexCoord0
Bind "texcoord1" TexCoord1
Bind "color" Color
Matrix 0 [glstate_matrix_mvp]
Vector 8 [unity_Scale]
Vector 9 [_WorldSpaceCameraPos]
Matrix 4 [_Object2World]
Vector 10 [unity_4LightPosX0]
Vector 11 [unity_4LightPosY0]
Vector 12 [unity_4LightPosZ0]
Vector 13 [unity_4LightAtten0]
Vector 14 [unity_LightColor0]
Vector 15 [unity_LightColor1]
Vector 16 [unity_LightColor2]
Vector 17 [unity_LightColor3]
Vector 18 [unity_SHAr]
Vector 19 [unity_SHAg]
Vector 20 [unity_SHAb]
Vector 21 [unity_SHBr]
Vector 22 [unity_SHBg]
Vector 23 [unity_SHBb]
Vector 24 [unity_SHC]
Float 25 [_NormalPower]
SetTexture 0 [_NormSpecPowerTex] 2D
"vs_3_0
; 150 ALU, 2 TEX
dcl_position o0
dcl_texcoord0 o1
dcl_color0 o2
dcl_texcoord1 o3
dcl_texcoord2 o4
dcl_texcoord3 o5
dcl_texcoord4 o6
dcl_texcoord5 o7
dcl_texcoord6 o8
dcl_texcoord7 o9
def c26, 0.25000000, 0.00000000, 1.00000000, 4.00000000
def c27, -2.00000000, 0.05000000, 10.00000000, -0.49990001
def c28, 0.49990001, 2.00000000, 0.00390625, 256.00000000
def c29, 0.50000000, 0.00392157, 0.10000000, 0.00000000
def c30, 16.00000000, -8.00000000, 0, 0
dcl_position0 v0
dcl_texcoord0 v3
dcl_texcoord1 v4
dcl_color0 v5
dcl_2d s0
mul r0.x, v4, c26
slt r0.y, r0.x, c26
dp4 r5.w, v0, c4
mov r5.xy, v3
abs r0.z, r0.x
max r0.y, -r0, r0
slt r0.x, c26.y, r0.y
frc r0.y, r0.z
add r0.y, r0.z, -r0
add r0.w, -r0.x, c26.z
mul r0.z, r0.y, r0.w
mad r4.x, r0, -r0.y, r0.z
mad r0.x, -r4, c26.w, v4
add r5.z, r0.x, c27.x
mul r6.xyz, r5, c8.w
dp3 r6.w, r6, c5
dp3 r3.x, r6, c4
dp4 r7.zw, v0, c5
add r0, -r7.z, c11
mul r1, r6.w, r0
add r2, -r5.w, c10
mul r0, r0, r0
mov r3.y, r6.w
mov r3.w, c26.z
dp4 r7.xy, v0, c6
mad r1, r3.x, r2, r1
mad r0, r2, r2, r0
add r2, -r7.x, c12
dp3 r7.x, r6, c6
mad r0, r2, r2, r0
mad r1, r7.x, r2, r1
mul r2, r0, c13
mov r3.z, r7.x
dp3 o1.z, r5, c6
dp3 o1.y, r5, c5
dp3 o1.x, r5, c4
mov o4.xyz, r5
rsq r0.x, r0.x
rsq r0.y, r0.y
rsq r0.w, r0.w
rsq r0.z, r0.z
mul r0, r1, r0
add r1, r2, c26.z
abs r2.y, v4
frc r2.z, r2.y
slt r2.x, v4.y, c26.y
max r2.x, -r2, r2
slt r2.x, c26.y, r2
add r2.y, r2, -r2.z
add r2.w, -r2.x, c26.z
mul r2.z, r2.y, r2.w
rcp r1.x, r1.x
rcp r1.y, r1.y
rcp r1.z, r1.z
rcp r1.w, r1.w
max r0, r0, c26.y
mul r0, r0, r1
mul r1.xyz, r0.y, c15
mad r1.w, r2.x, -r2.y, r2.z
add r0.y, v4, -r1.w
mad r1.xyz, r0.x, c14, r1
mul r2.x, r0.y, c27.z
mad r0.xyz, r0.z, c16, r1
slt r1.x, r2, c27.w
add r1.y, r2.x, c28.x
abs r1.y, r1
frc r1.z, r1.y
max r1.x, -r1, r1
slt r1.x, c26.y, r1
add r1.y, r1, -r1.z
add r2.x, -r1, c26.z
mul r1.z, r1.y, r2.x
mad r2.w, r1.x, -r1.y, r1.z
mad r2.xyz, r0.w, c17, r0
mul r0, r3.xyzz, r3.yzzx
dp4 r6.z, r0, c23
dp4 r6.x, r0, c21
dp4 r6.y, r0, c22
mul r0.w, r1, c28.z
frc r0.z, r0.w
slt r1.w, r0, c26.y
dp4 r1.z, r3, c20
dp4 r1.y, r3, c19
dp4 r1.x, r3, c18
add r3.yzw, r1.xxyz, r6.xxyz
sge r0.y, c26, r2.w
sge r0.x, r2.w, c26.y
mul r1.x, r0, r0.y
sge r0.y, c26.z, r2.w
sge r0.x, r2.w, c26.z
mul r1.y, r0.x, r0
sge r0.x, r2.w, c28.y
sge r0.y, c28, r2.w
mul r1.z, r0.x, r0.y
mul r0.z, r0, c28.w
abs r0.y, r0.z
slt r0.x, r0.z, c26.y
add r0.y, r0, c29.x
frc r0.z, r0.y
max r0.x, -r0, r0
add r0.y, r0, -r0.z
slt r0.x, c26.y, r0
add r0.z, -r0.x, c26
mul r0.z, r0.y, r0
mad r4.z, r0.x, -r0.y, r0
abs r2.w, r0
max r1.w, -r1, r1
slt r0.w, c26.y, r1
frc r1.w, r2
add r1.w, r2, -r1
add r4.y, -r0.w, c26.z
mul r2.w, r1, r4.y
mad r4.y, r0.w, -r1.w, r2.w
mul r0.y, r6.w, r6.w
mad r0.w, r3.x, r3.x, -r0.y
dp3 r0.x, r1, r4
mul r6.xyz, r0.w, c24
add r6.xyz, r3.yzww, r6
mul r0.x, r0, c29.y
mov r0.yz, c29.xzww
texldl r0.xy, r0.xyzz, s0
mad r0.x, r0, c30, c30.y
mul r4.w, r0.x, c25.x
mul r1.w, r0.y, c26
mov r5.x, r7.w
mov r5.y, r7
add o8.xyz, r6, r2
mov o5, r4
mov o6, r1
mov o2, v5
mul o3.xyz, v0, c27.y
mov o7.z, r7.x
mov o7.y, r6.w
mov o7.x, r3
add o9.xyz, -r5.wxyw, c9
dp4 o0.w, v0, c3
dp4 o0.z, v0, c2
dp4 o0.y, v0, c1
dp4 o0.x, v0, c0
"
}

SubProgram "opengl " {
Keywords { "DIRECTIONAL" "LIGHTMAP_OFF" "DIRLIGHTMAP_OFF" "SHADOWS_SCREEN" "VERTEXLIGHT_ON" }
"!!GLSL
#ifdef VERTEX
#define DIRECTIONAL 1
#define LIGHTMAP_OFF 1
#define DIRLIGHTMAP_OFF 1
#define SHADOWS_SCREEN 1
#define VERTEXLIGHT_ON 1
#define SHADER_API_OPENGL 1
#define SHADER_API_DESKTOP 1
vec4 xll_tex2Dlod(sampler2D s, vec4 coord) {
   return texture2DLod( s, coord.xy, coord.w);
}
mat3 xll_constructMat3( mat4 m) {
  return mat3( vec3( m[0]), vec3( m[1]), vec3( m[2]));
}
struct v2f_vertex_lit {
    vec2 uv;
    vec4 diff;
    vec4 spec;
};
struct v2f_img {
    vec4 pos;
    vec2 uv;
};
struct appdata_img {
    vec4 vertex;
    vec2 texcoord;
};
struct SurfaceOutput {
    vec3 Albedo;
    vec3 Normal;
    vec3 Emission;
    float Specular;
    float Gloss;
    float Alpha;
};
struct CustomSurfaceOutput {
    vec3 Albedo;
    vec3 Normal;
    vec3 Normal_;
    vec3 Emission;
    float Specular;
    float Alpha;
};
struct appdata_full {
    vec4 vertex;
    vec4 tangent;
    vec3 normal;
    vec4 texcoord;
    vec4 texcoord1;
    vec4 color;
};
struct Input {
    vec3 localPos;
    vec3 localNormal;
    vec3 worldNormal;
    vec4 weights;
    vec4 blendMask;
    vec4 color;
};
struct v2f_surf {
    vec4 pos;
    vec3 worldNormal;
    vec4 color;
    vec3 cust_localPos;
    vec3 cust_localNormal;
    vec4 cust_weights;
    vec4 cust_blendMask;
    vec3 normal;
    vec3 vlight;
    vec3 viewDir;
    vec4 _ShadowCoord;
};
uniform sampler2D _NormSpecPowerTex;
uniform float _NormalPower;
uniform mat4 _Object2World;
uniform vec4 _ProjectionParams;
uniform vec3 _WorldSpaceCameraPos;

uniform vec4 unity_4LightAtten0;
uniform vec4 unity_4LightPosX0;
uniform vec4 unity_4LightPosY0;
uniform vec4 unity_4LightPosZ0;
uniform vec4 unity_LightColor[4];
uniform vec4 unity_SHAb;
uniform vec4 unity_SHAg;
uniform vec4 unity_SHAr;
uniform vec4 unity_SHBb;
uniform vec4 unity_SHBg;
uniform vec4 unity_SHBr;
uniform vec4 unity_SHC;
uniform vec4 unity_Scale;
void vert( inout appdata_full v, out Input o );
vec3 WorldSpaceViewDir( in vec4 v );
vec3 ShadeSH9( in vec4 normal );
vec3 Shade4PointLights( in vec4 lightPosX, in vec4 lightPosY, in vec4 lightPosZ, in vec3 lightColor0, in vec3 lightColor1, in vec3 lightColor2, in vec3 lightColor3, in vec4 lightAttenSq, in vec3 pos, in vec3 normal );
vec4 ComputeScreenPos( in vec4 pos );
v2f_surf vert_surf( in appdata_full v );
void vert( inout appdata_full v, out Input o ) {
    int mat0;
    int mat12;
    vec3 matIdx;
    int idx;
    vec2 normSpec;
    mat0 = int( (v.texcoord1.x  / 4.00000) );
    mat12 = int( v.texcoord1.y  );
    matIdx = vec3( float( mat0 ), float( (mat12 / 256) ), float( (mat12 % 256) ));
    o.weights.xyz  = matIdx;
    idx = int( (((v.texcoord1.y  - float( mat12 )) * 10.0000) + 0.499900) );
    o.blendMask.xyz  = vec3( float( (idx == 0) ), float( (idx == 1) ), float( (idx == 2) ));
    normSpec = xll_tex2Dlod( _NormSpecPowerTex, vec4( (dot( o.blendMask, vec4( matIdx, 0.000000)) / 255.000), 0.100000, 0.000000, 0.000000)).xy ;
    o.weights.w  = (((normSpec.x  * 16.0000) - 8.00000) * _NormalPower);
    o.blendMask.w  = (normSpec.y  * 4.00000);
    v.normal = vec3( v.texcoord.x , v.texcoord.y , ((v.texcoord1.x  - float( (4 * mat0) )) - 2.00000));
    o.localPos = vec3( (v.vertex * 0.0500000));
    o.localNormal = v.normal;
}
vec3 WorldSpaceViewDir( in vec4 v ) {
    return (_WorldSpaceCameraPos.xyz  - ( _Object2World * v ).xyz );
}
vec3 ShadeSH9( in vec4 normal ) {
    vec3 x1;
    vec4 vB;
    vec3 x2;
    float vC;
    vec3 x3;
    x1.x  = dot( unity_SHAr, normal);
    x1.y  = dot( unity_SHAg, normal);
    x1.z  = dot( unity_SHAb, normal);
    vB = (normal.xyzz  * normal.yzzx );
    x2.x  = dot( unity_SHBr, vB);
    x2.y  = dot( unity_SHBg, vB);
    x2.z  = dot( unity_SHBb, vB);
    vC = ((normal.x  * normal.x ) - (normal.y  * normal.y ));
    x3 = (unity_SHC.xyz  * vC);
    return ((x1 + x2) + x3);
}
vec3 Shade4PointLights( in vec4 lightPosX, in vec4 lightPosY, in vec4 lightPosZ, in vec3 lightColor0, in vec3 lightColor1, in vec3 lightColor2, in vec3 lightColor3, in vec4 lightAttenSq, in vec3 pos, in vec3 normal ) {
    vec4 toLightX;
    vec4 toLightY;
    vec4 toLightZ;
    vec4 lengthSq;
    vec4 ndotl;
    vec4 corr;
    vec4 atten;
    vec4 diff;
    vec3 col;
    toLightX = (lightPosX - pos.x );
    toLightY = (lightPosY - pos.y );
    toLightZ = (lightPosZ - pos.z );
    lengthSq = vec4( 0.000000);
    lengthSq += (toLightX * toLightX);
    lengthSq += (toLightY * toLightY);
    lengthSq += (toLightZ * toLightZ);
    ndotl = vec4( 0.000000);
    ndotl += (toLightX * normal.x );
    ndotl += (toLightY * normal.y );
    ndotl += (toLightZ * normal.z );
    corr = inversesqrt( lengthSq );
    ndotl = max( vec4( 0.000000, 0.000000, 0.000000, 0.000000), (ndotl * corr));
    atten = (1.00000 / (1.00000 + (lengthSq * lightAttenSq)));
    diff = (ndotl * atten);
    col = vec3( 0.000000);
    col += (lightColor0 * diff.x );
    col += (lightColor1 * diff.y );
    col += (lightColor2 * diff.z );
    col += (lightColor3 * diff.w );
    return col;
}
vec4 ComputeScreenPos( in vec4 pos ) {
    vec4 o;
    o = (pos * 0.500000);
    o.xy  = (vec2( o.x , (o.y  * _ProjectionParams.x )) + o.w );
    o.zw  = pos.zw ;
    return o;
}
v2f_surf vert_surf( in appdata_full v ) {
    Input customInputData;
    v2f_surf o;
    vec3 worldN;
    vec3 viewDirForLight;
    vec3 shlight;
    vec3 worldPos;
    vert( v, customInputData);
    o.cust_localPos = customInputData.localPos;
    o.cust_localNormal = customInputData.localNormal;
    o.cust_weights = customInputData.weights;
    o.cust_blendMask = customInputData.blendMask;
    o.pos = ( gl_ModelViewProjectionMatrix * v.vertex );
    o.worldNormal = ( xll_constructMat3( _Object2World) * v.normal );
    o.color = v.color;
    worldN = ( xll_constructMat3( _Object2World) * (v.normal * unity_Scale.w ) );
    o.normal = worldN;
    viewDirForLight = WorldSpaceViewDir( v.vertex);
    o.viewDir = viewDirForLight;
    shlight = ShadeSH9( vec4( worldN, 1.00000));
    o.vlight = shlight;
    worldPos = ( _Object2World * v.vertex ).xyz ;
    o.vlight += Shade4PointLights( unity_4LightPosX0, unity_4LightPosY0, unity_4LightPosZ0, unity_LightColor[ 0 ].xyz , unity_LightColor[ 1 ].xyz , unity_LightColor[ 2 ].xyz , unity_LightColor[ 3 ].xyz , unity_4LightAtten0, worldPos, worldN);
    o._ShadowCoord = ComputeScreenPos( o.pos);
    return o;
}
attribute vec4 TANGENT;
varying vec3 xlv_TEXCOORD0;
varying vec4 xlv_COLOR0;
varying vec3 xlv_TEXCOORD1;
varying vec3 xlv_TEXCOORD2;
varying vec4 xlv_TEXCOORD3;
varying vec4 xlv_TEXCOORD4;
varying vec3 xlv_TEXCOORD5;
varying vec3 xlv_TEXCOORD6;
varying vec3 xlv_TEXCOORD7;
varying vec4 xlv_TEXCOORD8;
void main() {
    v2f_surf xl_retval;
    appdata_full xlt_v;
    xlt_v.vertex = vec4( gl_Vertex);
    xlt_v.tangent = vec4( TANGENT);
    xlt_v.normal = vec3( gl_Normal);
    xlt_v.texcoord = vec4( gl_MultiTexCoord0);
    xlt_v.texcoord1 = vec4( gl_MultiTexCoord1);
    xlt_v.color = vec4( gl_Color);
    xl_retval = vert_surf( xlt_v);
    gl_Position = vec4( xl_retval.pos);
    xlv_TEXCOORD0 = vec3( xl_retval.worldNormal);
    xlv_COLOR0 = vec4( xl_retval.color);
    xlv_TEXCOORD1 = vec3( xl_retval.cust_localPos);
    xlv_TEXCOORD2 = vec3( xl_retval.cust_localNormal);
    xlv_TEXCOORD3 = vec4( xl_retval.cust_weights);
    xlv_TEXCOORD4 = vec4( xl_retval.cust_blendMask);
    xlv_TEXCOORD5 = vec3( xl_retval.normal);
    xlv_TEXCOORD6 = vec3( xl_retval.vlight);
    xlv_TEXCOORD7 = vec3( xl_retval.viewDir);
    xlv_TEXCOORD8 = vec4( xl_retval._ShadowCoord);
}
/* NOTE: GLSL optimization failed
0:105(75): error: operator '%' is reserved in (null)
0:105(56): error: cannot construct `float' from a non-numeric data type
0:105(11): error: cannot construct `vec3' from a non-numeric data type
*/

#endif
#ifdef FRAGMENT
#extension GL_ARB_shader_texture_lod : enable
varying vec4 xlv_TEXCOORD8;
varying vec3 xlv_TEXCOORD7;
varying vec3 xlv_TEXCOORD6;
varying vec4 xlv_TEXCOORD4;
varying vec4 xlv_TEXCOORD3;
varying vec3 xlv_TEXCOORD2;
varying vec3 xlv_TEXCOORD1;
varying vec4 xlv_COLOR0;
varying vec3 xlv_TEXCOORD0;
vec4 xlat_mutable__SpecColor;
uniform vec4 _SpecColor;
vec4 xlat_mutable__LightColor0;
uniform vec4 _LightColor0;
uniform vec4 _WorldSpaceLightPos0;
uniform float _TileSize;
uniform float _SpecPower;
uniform sampler2D _SpecColorTex;
uniform sampler2D _ShadowMapTexture;
uniform float _Repetition;
uniform sampler2D _NormalMap;
uniform float _Indent;
uniform sampler2D _DiffuseTex;
uniform sampler2D _DiamondSpecTex;
uniform float _DiamondSpecParam3;
uniform float _DiamondSpecParam2;
uniform float _DiamondSpecParam1;
void main ()
{
  xlat_mutable__SpecColor = _SpecColor;
  xlat_mutable__LightColor0 = _LightColor0;
  vec4 c;
  vec3 tmpvar_1;
  tmpvar_1 = max (((normalize (abs (xlv_TEXCOORD2)) - 0.2) * 7.0), vec3(0.0, 0.0, 0.0));
  vec3 tmpvar_2;
  tmpvar_2 = (tmpvar_1 / vec3(((tmpvar_1.x + tmpvar_1.y) + tmpvar_1.z)));
  float tmpvar_3;
  tmpvar_3 = (float(int(floor ((xlv_TEXCOORD3.x + 0.5)))) * _TileSize);
  float tmpvar_4;
  tmpvar_4 = (floor (tmpvar_3) * _TileSize);
  vec3 tmpvar_5;
  tmpvar_5.x = fract (tmpvar_3);
  tmpvar_5.y = fract (tmpvar_4);
  tmpvar_5.z = floor (tmpvar_4);
  float tmpvar_6;
  tmpvar_6 = (float(int(floor ((xlv_TEXCOORD3.y + 0.5)))) * _TileSize);
  float tmpvar_7;
  tmpvar_7 = (floor (tmpvar_6) * _TileSize);
  vec3 tmpvar_8;
  tmpvar_8.x = fract (tmpvar_6);
  tmpvar_8.y = fract (tmpvar_7);
  tmpvar_8.z = floor (tmpvar_7);
  float tmpvar_9;
  tmpvar_9 = (float(int(floor ((xlv_TEXCOORD3.z + 0.5)))) * _TileSize);
  float tmpvar_10;
  tmpvar_10 = (floor (tmpvar_9) * _TileSize);
  vec3 tmpvar_11;
  tmpvar_11.x = fract (tmpvar_9);
  tmpvar_11.y = fract (tmpvar_10);
  tmpvar_11.z = floor (tmpvar_10);
  vec2 tmpvar_12;
  tmpvar_12 = (tmpvar_5.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.xy / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_13;
  tmpvar_13.zw = vec2(0.0, 0.0);
  tmpvar_13.x = tmpvar_12.x;
  tmpvar_13.y = tmpvar_12.y;
  vec2 tmpvar_14;
  tmpvar_14 = (tmpvar_5.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.yz / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_15;
  tmpvar_15.zw = vec2(0.0, 0.0);
  tmpvar_15.x = tmpvar_14.x;
  tmpvar_15.y = tmpvar_14.y;
  vec2 tmpvar_16;
  tmpvar_16 = (tmpvar_5.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.zx / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_17;
  tmpvar_17.zw = vec2(0.0, 0.0);
  tmpvar_17.x = tmpvar_16.x;
  tmpvar_17.y = tmpvar_16.y;
  vec2 tmpvar_18;
  tmpvar_18 = (tmpvar_8.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.xy / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_19;
  tmpvar_19.zw = vec2(0.0, 0.0);
  tmpvar_19.x = tmpvar_18.x;
  tmpvar_19.y = tmpvar_18.y;
  vec2 tmpvar_20;
  tmpvar_20 = (tmpvar_8.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.yz / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_21;
  tmpvar_21.zw = vec2(0.0, 0.0);
  tmpvar_21.x = tmpvar_20.x;
  tmpvar_21.y = tmpvar_20.y;
  vec2 tmpvar_22;
  tmpvar_22 = (tmpvar_8.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.zx / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_23;
  tmpvar_23.zw = vec2(0.0, 0.0);
  tmpvar_23.x = tmpvar_22.x;
  tmpvar_23.y = tmpvar_22.y;
  vec2 tmpvar_24;
  tmpvar_24 = (tmpvar_11.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.xy / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_25;
  tmpvar_25.zw = vec2(0.0, 0.0);
  tmpvar_25.x = tmpvar_24.x;
  tmpvar_25.y = tmpvar_24.y;
  vec2 tmpvar_26;
  tmpvar_26 = (tmpvar_11.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.yz / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_27;
  tmpvar_27.zw = vec2(0.0, 0.0);
  tmpvar_27.x = tmpvar_26.x;
  tmpvar_27.y = tmpvar_26.y;
  vec2 tmpvar_28;
  tmpvar_28 = (tmpvar_11.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.zx / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_29;
  tmpvar_29.zw = vec2(0.0, 0.0);
  tmpvar_29.x = tmpvar_28.x;
  tmpvar_29.y = tmpvar_28.y;
  vec4 tmpvar_30;
  vec2 tmpvar_31;
  tmpvar_31 = (tmpvar_5.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.yz / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_32;
  tmpvar_32.zw = vec2(0.0, 0.0);
  tmpvar_32.x = tmpvar_31.x;
  tmpvar_32.y = tmpvar_31.y;
  tmpvar_30 = (texture2DLod (_NormalMap, tmpvar_32.xy, 0.0) - 0.5);
  vec4 tmpvar_33;
  vec2 tmpvar_34;
  tmpvar_34 = (tmpvar_5.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.zx / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_35;
  tmpvar_35.zw = vec2(0.0, 0.0);
  tmpvar_35.x = tmpvar_34.x;
  tmpvar_35.y = tmpvar_34.y;
  tmpvar_33 = (texture2DLod (_NormalMap, tmpvar_35.xy, 0.0) - 0.5);
  vec4 tmpvar_36;
  vec2 tmpvar_37;
  tmpvar_37 = (tmpvar_5.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.xy / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_38;
  tmpvar_38.zw = vec2(0.0, 0.0);
  tmpvar_38.x = tmpvar_37.x;
  tmpvar_38.y = tmpvar_37.y;
  tmpvar_36 = (texture2DLod (_NormalMap, tmpvar_38.xy, 0.0) - 0.5);
  vec3 tmpvar_39;
  tmpvar_39.x = 0.0;
  tmpvar_39.y = tmpvar_30.x;
  tmpvar_39.z = tmpvar_30.y;
  vec3 tmpvar_40;
  tmpvar_40.y = 0.0;
  tmpvar_40.x = tmpvar_33.y;
  tmpvar_40.z = tmpvar_33.x;
  vec3 tmpvar_41;
  tmpvar_41.z = 0.0;
  tmpvar_41.x = tmpvar_36.x;
  tmpvar_41.y = tmpvar_36.y;
  vec4 tmpvar_42;
  vec2 tmpvar_43;
  tmpvar_43 = (tmpvar_8.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.yz / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_44;
  tmpvar_44.zw = vec2(0.0, 0.0);
  tmpvar_44.x = tmpvar_43.x;
  tmpvar_44.y = tmpvar_43.y;
  tmpvar_42 = (texture2DLod (_NormalMap, tmpvar_44.xy, 0.0) - 0.5);
  vec4 tmpvar_45;
  vec2 tmpvar_46;
  tmpvar_46 = (tmpvar_8.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.zx / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_47;
  tmpvar_47.zw = vec2(0.0, 0.0);
  tmpvar_47.x = tmpvar_46.x;
  tmpvar_47.y = tmpvar_46.y;
  tmpvar_45 = (texture2DLod (_NormalMap, tmpvar_47.xy, 0.0) - 0.5);
  vec4 tmpvar_48;
  vec2 tmpvar_49;
  tmpvar_49 = (tmpvar_8.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.xy / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_50;
  tmpvar_50.zw = vec2(0.0, 0.0);
  tmpvar_50.x = tmpvar_49.x;
  tmpvar_50.y = tmpvar_49.y;
  tmpvar_48 = (texture2DLod (_NormalMap, tmpvar_50.xy, 0.0) - 0.5);
  vec3 tmpvar_51;
  tmpvar_51.x = 0.0;
  tmpvar_51.y = tmpvar_42.x;
  tmpvar_51.z = tmpvar_42.y;
  vec3 tmpvar_52;
  tmpvar_52.y = 0.0;
  tmpvar_52.x = tmpvar_45.y;
  tmpvar_52.z = tmpvar_45.x;
  vec3 tmpvar_53;
  tmpvar_53.z = 0.0;
  tmpvar_53.x = tmpvar_48.x;
  tmpvar_53.y = tmpvar_48.y;
  vec4 tmpvar_54;
  vec2 tmpvar_55;
  tmpvar_55 = (tmpvar_11.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.yz / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_56;
  tmpvar_56.zw = vec2(0.0, 0.0);
  tmpvar_56.x = tmpvar_55.x;
  tmpvar_56.y = tmpvar_55.y;
  tmpvar_54 = (texture2DLod (_NormalMap, tmpvar_56.xy, 0.0) - 0.5);
  vec4 tmpvar_57;
  vec2 tmpvar_58;
  tmpvar_58 = (tmpvar_11.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.zx / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_59;
  tmpvar_59.zw = vec2(0.0, 0.0);
  tmpvar_59.x = tmpvar_58.x;
  tmpvar_59.y = tmpvar_58.y;
  tmpvar_57 = (texture2DLod (_NormalMap, tmpvar_59.xy, 0.0) - 0.5);
  vec4 tmpvar_60;
  vec2 tmpvar_61;
  tmpvar_61 = (tmpvar_11.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.xy / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_62;
  tmpvar_62.zw = vec2(0.0, 0.0);
  tmpvar_62.x = tmpvar_61.x;
  tmpvar_62.y = tmpvar_61.y;
  tmpvar_60 = (texture2DLod (_NormalMap, tmpvar_62.xy, 0.0) - 0.5);
  vec3 tmpvar_63;
  tmpvar_63.x = 0.0;
  tmpvar_63.y = tmpvar_54.x;
  tmpvar_63.z = tmpvar_54.y;
  vec3 tmpvar_64;
  tmpvar_64.y = 0.0;
  tmpvar_64.x = tmpvar_57.y;
  tmpvar_64.z = tmpvar_57.x;
  vec3 tmpvar_65;
  tmpvar_65.z = 0.0;
  tmpvar_65.x = tmpvar_60.x;
  tmpvar_65.y = tmpvar_60.y;
  vec3 tmpvar_66;
  tmpvar_66 = ((((xlv_TEXCOORD4.x * (((tmpvar_39 * tmpvar_2.xxx) + (tmpvar_40 * tmpvar_2.yyy)) + (tmpvar_41 * tmpvar_2.zzz))) * 2.0) + ((xlv_TEXCOORD4.y * (((tmpvar_51 * tmpvar_2.xxx) + (tmpvar_52 * tmpvar_2.yyy)) + (tmpvar_53 * tmpvar_2.zzz))) * 2.0)) + ((xlv_TEXCOORD4.z * (((tmpvar_63 * tmpvar_2.xxx) + (tmpvar_64 * tmpvar_2.yyy)) + (tmpvar_65 * tmpvar_2.zzz))) * 2.0));
  vec3 tmpvar_67;
  tmpvar_67 = ((((((xlv_TEXCOORD4.x * (((texture2DLod (_DiffuseTex, tmpvar_13.xy, 0.0) * tmpvar_2.z) + (texture2DLod (_DiffuseTex, tmpvar_15.xy, 0.0) * tmpvar_2.x)) + (texture2DLod (_DiffuseTex, tmpvar_17.xy, 0.0) * tmpvar_2.y))) + (xlv_TEXCOORD4.y * (((texture2DLod (_DiffuseTex, tmpvar_19.xy, 0.0) * tmpvar_2.z) + (texture2DLod (_DiffuseTex, tmpvar_21.xy, 0.0) * tmpvar_2.x)) + (texture2DLod (_DiffuseTex, tmpvar_23.xy, 0.0) * tmpvar_2.y)))) + (xlv_TEXCOORD4.z * (((texture2DLod (_DiffuseTex, tmpvar_25.xy, 0.0) * tmpvar_2.z) + (texture2DLod (_DiffuseTex, tmpvar_27.xy, 0.0) * tmpvar_2.x)) + (texture2DLod (_DiffuseTex, tmpvar_29.xy, 0.0) * tmpvar_2.y)))).xyz * xlv_COLOR0.xyz) * (1.0 - xlv_COLOR0.w)) + (xlv_COLOR0.xyz * xlv_COLOR0.w));
  vec4 tmpvar_68;
  tmpvar_68.w = 1.0;
  tmpvar_68.x = tmpvar_66.x;
  tmpvar_68.y = tmpvar_66.y;
  tmpvar_68.z = tmpvar_66.z;
  vec3 tmpvar_69;
  tmpvar_69 = normalize ((xlv_TEXCOORD0 - (normalize (tmpvar_68).xyz * xlv_TEXCOORD3.w)));
  float tmpvar_70;
  tmpvar_70 = (xlv_TEXCOORD4.w * _SpecPower);
  vec4 tmpvar_71;
  tmpvar_71.w = 0.0;
  tmpvar_71.xyz = xlv_TEXCOORD3.xyz;
  vec4 tmpvar_72;
  tmpvar_72.yzw = vec3(0.1, 0.0, 0.0);
  tmpvar_72.x = (dot (xlv_TEXCOORD4, tmpvar_71) / 255.0);
  vec4 tmpvar_73;
  tmpvar_73 = texture2DLod (_SpecColorTex, tmpvar_72.xy, 0.0);
  vec4 tmpvar_74;
  tmpvar_74 = texture2DProj (_ShadowMapTexture, xlv_TEXCOORD8);
  c = vec4(0.0, 0.0, 0.0, 0.0);
  vec3 tmpvar_75;
  tmpvar_75 = normalize (xlv_TEXCOORD7);
  vec3 normLightDir;
  normLightDir = _WorldSpaceLightPos0.xyz;
  vec4 c_i0;
  float m;
  m = tmpvar_73.x;
  if ((tmpvar_73.y > tmpvar_73.x)) {
    m = tmpvar_73.y;
  };
  if ((tmpvar_73.z > m)) {
    m = tmpvar_73.z;
  };
  if ((m > 0.03)) {
    float tmpvar_76;
    tmpvar_76 = (((((tmpvar_75.x * tmpvar_75.x) * tmpvar_75.y) + (tmpvar_75.z * tmpvar_75.x)) + (tmpvar_75.y * tmpvar_75.z)) * _DiamondSpecParam1);
    vec4 tmpvar_77;
    tmpvar_77.zw = vec2(0.0, 0.0);
    tmpvar_77.x = tmpvar_76;
    tmpvar_77.y = _DiamondSpecParam2;
    xlat_mutable__SpecColor.xyz = texture2DLod (_DiamondSpecTex, tmpvar_77.xy, 0.0).xyz;
    vec4 tmpvar_78;
    tmpvar_78.zw = vec2(0.0, 0.0);
    tmpvar_78.x = tmpvar_76;
    tmpvar_78.y = _DiamondSpecParam3;
    xlat_mutable__LightColor0.xyz = (_LightColor0.xyz * texture2DLod (_DiamondSpecTex, tmpvar_78.xy, 0.0).xyz);
  } else {
    if ((m > 0.0)) {
      xlat_mutable__SpecColor.xyz = (tmpvar_73.xyz * (1.0/(m)));
    } else {
      xlat_mutable__SpecColor.xyz = vec3(1.0, 1.0, 1.0);
    };
  };
  c_i0 = vec4(0.0, 0.0, 0.0, 0.0);
  c_i0.xyz = ((((tmpvar_67 * xlat_mutable__LightColor0.xyz) * dot (tmpvar_69, normLightDir)) + ((xlat_mutable__LightColor0.xyz * xlat_mutable__SpecColor.xyz) * (smoothstep (0.0, 1.0, pow (max (0.0, dot (tmpvar_69, normalize ((_WorldSpaceLightPos0.xyz + tmpvar_75)))), (32.0 * tmpvar_70))) * tmpvar_70))) * tmpvar_74.x);
  c_i0.w = 1.0;
  c = c_i0;
  c.xyz = (c_i0.xyz + (tmpvar_67 * xlv_TEXCOORD6));
  c.xyz = (c.xyz + tmpvar_73.xyz);
  gl_FragData[0] = c;
}


#endif
"
}

SubProgram "d3d9 " {
Keywords { "DIRECTIONAL" "LIGHTMAP_OFF" "DIRLIGHTMAP_OFF" "SHADOWS_SCREEN" "VERTEXLIGHT_ON" }
Bind "vertex" Vertex
Bind "texcoord" TexCoord0
Bind "texcoord1" TexCoord1
Bind "color" Color
Matrix 0 [glstate_matrix_mvp]
Vector 8 [_ProjectionParams]
Vector 9 [_ScreenParams]
Vector 10 [unity_Scale]
Vector 11 [_WorldSpaceCameraPos]
Matrix 4 [_Object2World]
Vector 12 [unity_4LightPosX0]
Vector 13 [unity_4LightPosY0]
Vector 14 [unity_4LightPosZ0]
Vector 15 [unity_4LightAtten0]
Vector 16 [unity_LightColor0]
Vector 17 [unity_LightColor1]
Vector 18 [unity_LightColor2]
Vector 19 [unity_LightColor3]
Vector 20 [unity_SHAr]
Vector 21 [unity_SHAg]
Vector 22 [unity_SHAb]
Vector 23 [unity_SHBr]
Vector 24 [unity_SHBg]
Vector 25 [unity_SHBb]
Vector 26 [unity_SHC]
Float 27 [_NormalPower]
SetTexture 0 [_NormSpecPowerTex] 2D
"vs_3_0
; 156 ALU, 2 TEX
dcl_position o0
dcl_texcoord0 o1
dcl_color0 o2
dcl_texcoord1 o3
dcl_texcoord2 o4
dcl_texcoord3 o5
dcl_texcoord4 o6
dcl_texcoord5 o7
dcl_texcoord6 o8
dcl_texcoord7 o9
dcl_texcoord8 o10
def c28, 0.25000000, 0.00000000, 1.00000000, 4.00000000
def c29, -2.00000000, 0.05000000, 10.00000000, -0.49990001
def c30, 0.49990001, 2.00000000, 0.00390625, 256.00000000
def c31, 0.50000000, 0.00392157, 0.10000000, 0.00000000
def c32, 16.00000000, -8.00000000, 0, 0
dcl_position0 v0
dcl_texcoord0 v3
dcl_texcoord1 v4
dcl_color0 v5
dcl_2d s0
mul r0.x, v4, c28
slt r0.y, r0.x, c28
dp4 r5.w, v0, c4
mov r5.xy, v3
abs r0.z, r0.x
max r0.y, -r0, r0
slt r0.x, c28.y, r0.y
frc r0.y, r0.z
add r0.y, r0.z, -r0
add r0.w, -r0.x, c28.z
mul r0.z, r0.y, r0.w
mad r3.x, r0, -r0.y, r0.z
mad r0.x, -r3, c28.w, v4
add r5.z, r0.x, c29.x
mul r6.xyz, r5, c10.w
dp3 r6.w, r6, c5
dp3 r4.x, r6, c4
dp4 r7.zw, v0, c5
add r0, -r7.z, c13
mul r1, r6.w, r0
add r2, -r5.w, c12
mul r0, r0, r0
mov r4.y, r6.w
mov r4.w, c28.z
dp4 r7.xy, v0, c6
mad r1, r4.x, r2, r1
mad r0, r2, r2, r0
add r2, -r7.x, c14
dp3 r7.x, r6, c6
mov r4.z, r7.x
mad r0, r2, r2, r0
mad r1, r7.x, r2, r1
mul r2, r0, c15
dp4 r6.z, r4, c22
dp4 r6.y, r4, c21
dp4 r6.x, r4, c20
dp3 o1.z, r5, c6
dp3 o1.y, r5, c5
dp3 o1.x, r5, c4
mov o4.xyz, r5
rsq r0.x, r0.x
rsq r0.y, r0.y
rsq r0.w, r0.w
rsq r0.z, r0.z
mul r0, r1, r0
add r1, r2, c28.z
abs r2.y, v4
frc r2.z, r2.y
slt r2.x, v4.y, c28.y
max r2.x, -r2, r2
slt r2.x, c28.y, r2
add r2.y, r2, -r2.z
add r2.w, -r2.x, c28.z
mul r2.z, r2.y, r2.w
mad r2.w, r2.x, -r2.y, r2.z
rcp r1.x, r1.x
rcp r1.y, r1.y
rcp r1.z, r1.z
rcp r1.w, r1.w
max r0, r0, c28.y
mul r0, r0, r1
mul r1.xyz, r0.y, c17
add r1.w, v4.y, -r2
mad r1.xyz, r0.x, c16, r1
mul r0.y, r1.w, c29.z
slt r0.x, r0.y, c29.w
max r0.x, -r0, r0
slt r1.w, c28.y, r0.x
add r0.y, r0, c30.x
abs r0.y, r0
frc r0.x, r0.y
add r2.x, r0.y, -r0
add r2.y, -r1.w, c28.z
mad r0.xyz, r0.z, c18, r1
mul r2.y, r2.x, r2
mad r1.z, r1.w, -r2.x, r2.y
mad r2.xyz, r0.w, c19, r0
sge r0.y, c28, r1.z
sge r0.x, r1.z, c28.y
mul r1.x, r0, r0.y
sge r0.y, c28.z, r1.z
sge r0.x, r1.z, c28.z
mul r1.y, r0.x, r0
mul r0.x, r2.w, c30.z
frc r0.w, r0.x
sge r0.y, r1.z, c30
sge r0.z, c30.y, r1
mul r1.z, r0.y, r0
mul r0.w, r0, c30
abs r0.z, r0.w
slt r0.y, r0.w, c28
add r0.z, r0, c31.x
frc r0.w, r0.z
add r0.z, r0, -r0.w
slt r0.w, r0.x, c28.y
max r0.y, -r0, r0
slt r0.y, c28, r0
add r1.w, -r0.y, c28.z
mul r3.y, r0.z, r1.w
abs r1.w, r0.x
max r0.w, -r0, r0
slt r0.x, c28.y, r0.w
frc r0.w, r1
add r0.w, r1, -r0
add r2.w, -r0.x, c28.z
mad r3.z, r0.y, -r0, r3.y
mul r1.w, r0, r2
mad r3.y, r0.x, -r0.w, r1.w
mul r0, r4.xyzz, r4.yzzx
dp3 r1.w, r1, r3
dp4 r4.w, r0, c25
dp4 r4.y, r0, c23
dp4 r4.z, r0, c24
mul r0.w, r6, r6
add r4.yzw, r6.xxyz, r4
mad r0.w, r4.x, r4.x, -r0
mul r6.xyz, r0.w, c26
add r6.xyz, r4.yzww, r6
dp4 r0.w, v0, c3
mul r0.x, r1.w, c31.y
mov r0.yz, c31.xzww
texldl r0.xy, r0.xyzz, s0
mul r1.w, r0.y, c28
mad r0.x, r0, c32, c32.y
mul r3.w, r0.x, c27.x
dp4 r0.z, v0, c2
dp4 r0.x, v0, c0
dp4 r0.y, v0, c1
add o8.xyz, r6, r2
mul r2.xyz, r0.xyww, c31.x
mov o6, r1
mov r1.x, r2
mul r1.y, r2, c8.x
mov r5.x, r7.w
mov r5.y, r7
mov o5, r3
mad o10.xy, r2.z, c9.zwzw, r1
mov o0, r0
mov o2, v5
mov o10.zw, r0
mul o3.xyz, v0, c29.y
mov o7.z, r7.x
mov o7.y, r6.w
mov o7.x, r4
add o9.xyz, -r5.wxyw, c11
"
}

}
Program "fp" {
// Fragment combos: 4
//   d3d9 - ALU: 106 to 190, TEX: 21 to 43, FLOW: 2 to 2
SubProgram "opengl " {
Keywords { "DIRECTIONAL" "LIGHTMAP_OFF" "DIRLIGHTMAP_OFF" "SHADOWS_OFF" }
"!!GLSL"
}

SubProgram "d3d9 " {
Keywords { "DIRECTIONAL" "LIGHTMAP_OFF" "DIRLIGHTMAP_OFF" "SHADOWS_OFF" }
Vector 0 [_WorldSpaceLightPos0]
Vector 1 [_LightColor0]
Float 2 [_SpecPower]
Float 3 [_TileSize]
Float 4 [_Repetition]
Float 5 [_Indent]
Float 6 [_DiamondSpecParam1]
Float 7 [_DiamondSpecParam2]
Float 8 [_DiamondSpecParam3]
SetTexture 0 [_DiffuseTex] 2D
SetTexture 1 [_NormalMap] 2D
SetTexture 2 [_SpecColorTex] 2D
SetTexture 3 [_DiamondSpecTex] 2D
"ps_3_0
; 188 ALU, 42 TEX, 2 FLOW
dcl_2d s0
dcl_2d s1
dcl_2d s2
dcl_2d s3
def c9, 0.00195313, 0.00390625, 1.00000000, 0.50000000
def c10, 0.00000000, -0.20000000, 7.00000000, -0.50000000
def c11, 2.00000000, 0.00392157, 0.10000000, 0.00000000
def c12, 0.02999878, 0.00000000, 1.00000000, 32.00000000
def c13, 2.00000000, 3.00000000, 0, 0
dcl_texcoord0 v0.xyz
dcl_color0 v1
dcl_texcoord1 v2.xyz
dcl_texcoord2 v3.xyz
dcl_texcoord3 v4
dcl_texcoord4 v5
dcl_texcoord6 v6.xyz
dcl_texcoord7 v7.xyz
add r0.w, v4.x, c9
frc r1.x, r0.w
add r1.x, r0.w, -r1
abs r1.y, r1.x
abs r0.xyz, v3
dp3 r0.w, r0, r0
rsq r0.w, r0.w
mad r0.xyz, r0.w, r0, c10.y
frc r1.z, r1.y
add r0.w, r1.y, -r1.z
cmp r0.w, r1.x, r0, -r0
mul r1.x, r0.w, c3
frc r2.x, r1
add r1.x, r1, -r2
mul r1.x, r1, c3
mul r0.xyz, r0, c10.z
max r0.xyz, r0, c10.x
add r0.w, r0.x, r0.y
add r0.w, r0.z, r0
rcp r0.w, r0.w
mul r10.xyz, r0, r0.w
mov r0.x, c5
frc r2.y, r1.x
rcp r1.w, c4.x
mad r0.w, -r0.x, c9.y, c9.z
mul r1.xy, v2.zxzw, r1.w
frc r0.xy, r1
mul r1.xy, r0, r0.w
mov r0.x, c5
mad r11.xy, c9.x, r0.x, r1
mul r1.xy, v2.yzzw, r1.w
frc r2.zw, r1.xyxy
mul r2.zw, r0.w, r2
mad r6.xy, r11, c3.x, r2
mov r6.z, c10.x
texldl r0.xy, r6.xyzz, s1
add r0.xz, r0.yyxw, c10.w
mov r0.y, c10.x
mul r1.xyz, r10.y, r0
mov r0.x, c5
mad r8.zw, c9.x, r0.x, r2
add r0.x, v4.y, c9.w
frc r0.y, r0.x
add r2.z, r0.x, -r0.y
abs r2.w, r2.z
mad r4.xy, r8.zwzw, c3.x, r2
mov r4.z, c10.x
texldl r0.xy, r4.xyzz, s1
add r0.yz, r0.xxyw, c10.w
mov r0.x, c10
mad r1.xyz, r10.x, r0, r1
frc r3.x, r2.w
add r0.z, r2.w, -r3.x
cmp r0.z, r2, r0, -r0
mul r0.xy, v2, r1.w
mul r1.w, r0.z, c3.x
frc r0.xy, r0
mul r0.zw, r0.w, r0.xyxy
mov r0.x, c5
mad r13.xy, c9.x, r0.x, r0.zwzw
frc r8.x, r1.w
add r0.x, r1.w, -r8
mul r0.z, r0.x, c3.x
frc r8.y, r0.z
mad r5.xy, r13, c3.x, r2
mov r5.z, c10.x
texldl r0.xy, r5.xyzz, s1
mad r3.xy, r11, c3.x, r8
mov r3.z, c10.x
texldl r2.xy, r3.xyzz, s1
add r0.w, v4.z, c9
mov r2.z, c10.x
texldl r3.xyz, r3.xyzz, s0
add r0.xy, r0, c10.w
mov r0.z, c10.x
mad r0.xyz, r10.z, r0, r1
add r1.xz, r2.yyxw, c10.w
mov r1.y, c10.x
mul r7.xyz, r10.y, r1
frc r1.x, r0.w
add r0.w, r0, -r1.x
mad r2.xy, r8.zwzw, c3.x, r8
texldl r1.xy, r2.xyzz, s1
abs r1.w, r0
add r1.yz, r1.xxyw, c10.w
mov r1.x, c10
mad r9.xyz, r10.x, r1, r7
frc r2.w, r1
add r1.z, r1.w, -r2.w
cmp r0.w, r0, r1.z, -r1.z
texldl r2.xyz, r2.xyzz, s0
mad r1.xy, r13, c3.x, r8
mov r1.z, c10.x
texldl r7.xy, r1.xyzz, s1
mul r0.w, r0, c3.x
frc r8.x, r0.w
add r0.w, r0, -r8.x
mul r0.w, r0, c3.x
frc r8.y, r0.w
mul r2.xyz, r10.x, r2
texldl r1.xyz, r1.xyzz, s0
mad r1.xyz, r10.z, r1, r2
texldl r2.xyz, r4.xyzz, s0
mad r1.xyz, r10.y, r3, r1
texldl r3.xyz, r5.xyzz, s0
mul r2.xyz, r10.x, r2
mad r2.xyz, r10.z, r3, r2
texldl r3.xyz, r6.xyzz, s0
mad r2.xyz, r10.y, r3, r2
mul r1.xyz, v5.y, r1
mad r1.xyz, v5.x, r2, r1
mov r4.xyz, v4
add r7.xy, r7, c10.w
mov r7.z, c10.x
mad r7.xyz, r10.z, r7, r9
mul r7.xyz, v5.y, r7
mad r12.xyz, v5.x, r0, r7
mad r7.xy, r8.zwzw, c3.x, r8
mad r9.xy, r11, c3.x, r8
mov r9.z, c10.x
texldl r11.xy, r9.xyzz, s1
mov r7.z, c10.x
texldl r3.xyz, r7.xyzz, s0
add r0.xz, r11.yyxw, c10.w
mov r0.y, c10.x
mul r11.xyz, r10.y, r0
texldl r0.xy, r7.xyzz, s1
add r0.yz, r0.xxyw, c10.w
mov r0.x, c10
mad r11.xyz, r10.x, r0, r11
mad r8.xy, r13, c3.x, r8
mov r8.z, c10.x
mul r2.xyz, r10.x, r3
texldl r13.xy, r8.xyzz, s1
texldl r3.xyz, r8.xyzz, s0
mad r3.xyz, r10.z, r3, r2
texldl r2.xyz, r9.xyzz, s0
mad r3.xyz, r2, r10.y, r3
mad r1.xyz, v5.z, r3, r1
mul r1.xyz, r1, v1
add r0.xy, r13, c10.w
mov r0.z, c10.x
mad r0.xyz, r10.z, r0, r11
mad r0.xyz, v5.z, r0, r12
mul r0.xyz, r0, c11.x
mov r0.w, c9.z
dp4 r0.w, r0, r0
rsq r0.w, r0.w
mul r0.xyz, r0.w, r0
mul r0.xyz, v4.w, -r0
add r0.xyz, r0, v0
dp3 r0.w, r0, r0
rsq r0.w, r0.w
mul r0.xyz, r0.w, r0
dp3 r0.w, v5, r4
mul r2.x, r0.w, c11.y
mov r2.yz, c11.xzww
texldl r2.xyz, r2.xyzz, s2
add_pp r0.w, r2.y, -r2.x
cmp_pp r1.w, -r0, r2.x, r2.y
add r0.w, -v1, c9.z
mul r3.xyz, v1, v1.w
mad r5.xyz, r1, r0.w, r3
add_pp r1.x, r2.z, -r1.w
cmp_pp r1.w, -r1.x, r1, r2.z
dp3_pp r0.w, v7, v7
rsq_pp r0.w, r0.w
mul_pp r1.xyz, r0.w, v7
mov_pp r4.xyz, c1
mul r0.w, v5, c2.x
if_gt r1.w, c12.x
mov r3.z, c10.x
mov r3.y, c8.x
mul_pp r2.w, r1.z, r1.x
mul_pp r1.w, r1.x, r1.x
mad_pp r1.w, r1.y, r1, r2
mad_pp r1.w, r1.y, r1.z, r1
mul r3.x, r1.w, c6
texldl r4.xyz, r3.xyzz, s3
mov r3.z, c10.x
mov r3.y, c7.x
texldl r3.xyz, r3.xyzz, s3
mul_pp r4.xyz, r4, c1
else
rcp_pp r2.w, r1.w
cmp_pp r1.w, -r1, c12.y, c12.z
mul_pp r3.xyz, r2, r2.w
abs_pp r1.w, r1
cmp_pp r3.xyz, -r1.w, c9.z, r3
endif
add_pp r1.xyz, r1, c0
dp3_pp r1.w, r1, r1
rsq_pp r1.w, r1.w
mul_pp r1.xyz, r1.w, r1
dp3_pp r1.x, r0, r1
mul_pp r3.w, r0, c12
max_pp r2.w, r1.x, c10.x
pow_pp_sat r1, r2.w, r3.w
mad_pp r1.y, -r1.x, c13.x, c13
mul_pp r1.x, r1, r1
mul_pp r1.x, r1, r1.y
mul_pp r0.w, r1.x, r0
mul_pp r1.xyz, r4, r3
mul_pp r1.xyz, r1, r0.w
mul_pp r3.xyz, r5, r4
dp3_pp r0.x, r0, c0
mad_pp r0.xyz, r3, r0.x, r1
mad_pp r0.xyz, r5, v6, r0
add_pp oC0.xyz, r0, r2
mov_pp oC0.w, c9.z
"
}

SubProgram "opengl " {
Keywords { "DIRECTIONAL" "LIGHTMAP_ON" "DIRLIGHTMAP_OFF" "SHADOWS_OFF" }
"!!GLSL"
}

SubProgram "d3d9 " {
Keywords { "DIRECTIONAL" "LIGHTMAP_ON" "DIRLIGHTMAP_OFF" "SHADOWS_OFF" }
Float 0 [_TileSize]
Float 1 [_Repetition]
Float 2 [_Indent]
SetTexture 0 [_DiffuseTex] 2D
SetTexture 2 [_SpecColorTex] 2D
SetTexture 3 [unity_Lightmap] 2D
"ps_3_0
; 106 ALU, 21 TEX
dcl_2d s0
dcl_2d s2
dcl_2d s3
def c3, -0.20000000, 7.00000000, 0.00000000, 0.50000000
def c4, 0.00390625, 1.00000000, 0.00195313, 0.00392157
def c5, 0.10000000, 0.00000000, 8.00000000, 0
dcl_color0 v1
dcl_texcoord1 v2.xyz
dcl_texcoord2 v3.xyz
dcl_texcoord3 v4.xyz
dcl_texcoord4 v5.xyz
dcl_texcoord5 v6.xy
add r1.x, v4, c3.w
frc r1.y, r1.x
add r1.w, r1.x, -r1.y
abs r0.xyz, v3
dp3 r0.w, r0, r0
rsq r0.w, r0.w
mad r0.xyz, r0.w, r0, c3.x
abs r1.x, r1.w
frc r0.w, r1.x
mul r0.xyz, r0, c3.y
add r0.w, r1.x, -r0
max r1.xyz, r0, c3.z
cmp r0.y, r1.w, r0.w, -r0.w
mov r0.w, c2.x
add r0.x, r1, r1.y
mul r0.z, r0.y, c0.x
add r0.y, r1.z, r0.x
frc r0.x, r0.z
rcp r0.y, r0.y
mul r1.xyz, r1, r0.y
add r0.z, r0, -r0.x
mul r0.y, r0.z, c0.x
rcp r0.z, c1.x
mul r3.xy, v2, r0.z
mul r2.xy, v2.yzzw, r0.z
frc r4.xy, r3
frc r0.y, r0
mad r0.w, -r0, c4.x, c4.y
frc r2.xy, r2
mul r2.xy, r0.w, r2
mov r1.w, c2.x
mad r5.zw, c4.z, r1.w, r2.xyxy
mad r2.xy, r5.zwzw, c0.x, r0
mov r2.z, c3
texldl r2.xyz, r2.xyzz, s0
mul r3.xyz, r1.x, r2
mul r2.xy, r4, r0.w
mov r1.w, c2.x
mad r5.xy, c4.z, r1.w, r2
add r2.z, v4.y, c3.w
frc r1.w, r2.z
add r1.w, r2.z, -r1
abs r2.w, r1
frc r3.w, r2
mad r2.xy, r5, c0.x, r0
mov r2.z, c3
texldl r2.xyz, r2.xyzz, s0
mad r2.xyz, r1.z, r2, r3
mul r3.xy, v2.zxzw, r0.z
add r2.w, r2, -r3
cmp r0.z, r1.w, r2.w, -r2.w
mul r2.w, r0.z, c0.x
frc r3.xy, r3
frc r4.x, r2.w
mul r0.zw, r3.xyxy, r0.w
mov r1.w, c2.x
mad r4.zw, c4.z, r1.w, r0
add r0.w, r2, -r4.x
mul r0.w, r0, c0.x
frc r4.y, r0.w
add r0.w, v4.z, c3
frc r1.w, r0
add r0.w, r0, -r1
abs r1.w, r0
frc r2.w, r1
add r1.w, r1, -r2
cmp r0.w, r0, r1, -r1
mul r0.w, r0, c0.x
mad r0.xy, r4.zwzw, c0.x, r0
mov r0.z, c3
texldl r0.xyz, r0.xyzz, s0
mad r2.xyz, r1.y, r0, r2
mad r0.xy, r5.zwzw, c0.x, r4
mov r0.z, c3
texldl r0.xyz, r0.xyzz, s0
mul r3.xyz, r1.x, r0
mad r0.xy, r5, c0.x, r4
mov r0.z, c3
texldl r0.xyz, r0.xyzz, s0
mad r0.xyz, r1.z, r0, r3
mad r3.xy, r4.zwzw, c0.x, r4
frc r4.x, r0.w
mov r3.z, c3
texldl r3.xyz, r3.xyzz, s0
mad r0.xyz, r1.y, r3, r0
add r0.w, r0, -r4.x
mul r0.w, r0, c0.x
frc r4.y, r0.w
mul r0.xyz, v5.y, r0
mad r0.xyz, v5.x, r2, r0
add r0.w, -v1, c4.y
mov r3.z, c3
mad r3.xy, r5.zwzw, c0.x, r4
texldl r3.xyz, r3.xyzz, s0
mul r2.xyz, r1.x, r3
mov r3.z, c3
mad r3.xy, r5, c0.x, r4
texldl r3.xyz, r3.xyzz, s0
mad r2.xyz, r1.z, r3, r2
mov r3.z, c3
mad r3.xy, r4.zwzw, c0.x, r4
texldl r3.xyz, r3.xyzz, s0
mad r1.xyz, r3, r1.y, r2
mad r0.xyz, v5.z, r1, r0
mul r0.xyz, r0, v1
mul r1.xyz, v1, v1.w
mad r2.xyz, r0, r0.w, r1
texld r0, v6, s3
mul_pp r0.xyz, r0.w, r0
mov r1.xyz, v4
dp3 r0.w, v5, r1
mov r1.yz, c5.xxyw
mul r1.x, r0.w, c4.w
texldl r1.xyz, r1.xyzz, s2
mul_pp r0.xyz, r0, r2
mad_pp oC0.xyz, r0, c5.z, r1
mov_pp oC0.w, c3.z
"
}

SubProgram "opengl " {
Keywords { "DIRECTIONAL" "LIGHTMAP_OFF" "DIRLIGHTMAP_OFF" "SHADOWS_SCREEN" }
"!!GLSL"
}

SubProgram "d3d9 " {
Keywords { "DIRECTIONAL" "LIGHTMAP_OFF" "DIRLIGHTMAP_OFF" "SHADOWS_SCREEN" }
Vector 0 [_WorldSpaceLightPos0]
Vector 1 [_LightColor0]
Float 2 [_SpecPower]
Float 3 [_TileSize]
Float 4 [_Repetition]
Float 5 [_Indent]
Float 6 [_DiamondSpecParam1]
Float 7 [_DiamondSpecParam2]
Float 8 [_DiamondSpecParam3]
SetTexture 0 [_DiffuseTex] 2D
SetTexture 1 [_NormalMap] 2D
SetTexture 2 [_SpecColorTex] 2D
SetTexture 3 [_ShadowMapTexture] 2D
SetTexture 4 [_DiamondSpecTex] 2D
"ps_3_0
; 190 ALU, 43 TEX, 2 FLOW
dcl_2d s0
dcl_2d s1
dcl_2d s2
dcl_2d s3
dcl_2d s4
def c9, 0.00195313, 0.00390625, 1.00000000, 0.50000000
def c10, 0.00000000, -0.20000000, 7.00000000, -0.50000000
def c11, 2.00000000, 0.00392157, 0.10000000, 0.00000000
def c12, 0.02999878, 0.00000000, 1.00000000, 32.00000000
def c13, 2.00000000, 3.00000000, 0, 0
dcl_texcoord0 v0.xyz
dcl_color0 v1
dcl_texcoord1 v2.xyz
dcl_texcoord2 v3.xyz
dcl_texcoord3 v4
dcl_texcoord4 v5
dcl_texcoord6 v6.xyz
dcl_texcoord7 v7.xyz
dcl_texcoord8 v8
add r0.w, v4.x, c9
frc r1.x, r0.w
add r1.x, r0.w, -r1
abs r1.y, r1.x
abs r0.xyz, v3
dp3 r0.w, r0, r0
rsq r0.w, r0.w
mad r0.xyz, r0.w, r0, c10.y
frc r1.z, r1.y
add r0.w, r1.y, -r1.z
cmp r0.w, r1.x, r0, -r0
mul r1.x, r0.w, c3
frc r2.x, r1
add r1.x, r1, -r2
mul r1.x, r1, c3
mul r0.xyz, r0, c10.z
max r0.xyz, r0, c10.x
add r0.w, r0.x, r0.y
add r0.w, r0.z, r0
rcp r0.w, r0.w
mul r10.xyz, r0, r0.w
mov r0.x, c5
frc r2.y, r1.x
rcp r1.w, c4.x
mad r0.w, -r0.x, c9.y, c9.z
mul r1.xy, v2.zxzw, r1.w
frc r0.xy, r1
mul r1.xy, r0, r0.w
mov r0.x, c5
mad r11.xy, c9.x, r0.x, r1
mul r1.xy, v2.yzzw, r1.w
frc r2.zw, r1.xyxy
mul r2.zw, r0.w, r2
mad r6.xy, r11, c3.x, r2
mov r6.z, c10.x
texldl r0.xy, r6.xyzz, s1
add r0.xz, r0.yyxw, c10.w
mov r0.y, c10.x
mul r1.xyz, r10.y, r0
mov r0.x, c5
mad r8.zw, c9.x, r0.x, r2
add r0.x, v4.y, c9.w
frc r0.y, r0.x
add r2.z, r0.x, -r0.y
abs r2.w, r2.z
mad r4.xy, r8.zwzw, c3.x, r2
mov r4.z, c10.x
texldl r0.xy, r4.xyzz, s1
add r0.yz, r0.xxyw, c10.w
mov r0.x, c10
mad r1.xyz, r10.x, r0, r1
frc r3.x, r2.w
add r0.z, r2.w, -r3.x
cmp r0.z, r2, r0, -r0
mul r0.xy, v2, r1.w
mul r1.w, r0.z, c3.x
frc r0.xy, r0
mul r0.zw, r0.w, r0.xyxy
mov r0.x, c5
mad r13.xy, c9.x, r0.x, r0.zwzw
frc r8.x, r1.w
add r0.x, r1.w, -r8
mul r0.z, r0.x, c3.x
frc r8.y, r0.z
mad r5.xy, r13, c3.x, r2
mov r5.z, c10.x
texldl r0.xy, r5.xyzz, s1
mad r3.xy, r11, c3.x, r8
mov r3.z, c10.x
texldl r2.xy, r3.xyzz, s1
add r0.w, v4.z, c9
mov r2.z, c10.x
texldl r3.xyz, r3.xyzz, s0
add r0.xy, r0, c10.w
mov r0.z, c10.x
mad r0.xyz, r10.z, r0, r1
add r1.xz, r2.yyxw, c10.w
mov r1.y, c10.x
mul r7.xyz, r10.y, r1
frc r1.x, r0.w
add r0.w, r0, -r1.x
mad r2.xy, r8.zwzw, c3.x, r8
texldl r1.xy, r2.xyzz, s1
abs r1.w, r0
add r1.yz, r1.xxyw, c10.w
mov r1.x, c10
mad r9.xyz, r10.x, r1, r7
frc r2.w, r1
add r1.z, r1.w, -r2.w
cmp r0.w, r0, r1.z, -r1.z
texldl r2.xyz, r2.xyzz, s0
mad r1.xy, r13, c3.x, r8
mov r1.z, c10.x
texldl r7.xy, r1.xyzz, s1
mul r0.w, r0, c3.x
frc r8.x, r0.w
add r0.w, r0, -r8.x
mul r0.w, r0, c3.x
frc r8.y, r0.w
mul r2.xyz, r10.x, r2
texldl r1.xyz, r1.xyzz, s0
mad r1.xyz, r10.z, r1, r2
texldl r2.xyz, r4.xyzz, s0
mad r1.xyz, r10.y, r3, r1
texldl r3.xyz, r5.xyzz, s0
mul r2.xyz, r10.x, r2
mad r2.xyz, r10.z, r3, r2
texldl r3.xyz, r6.xyzz, s0
mad r2.xyz, r10.y, r3, r2
mul r1.xyz, v5.y, r1
mad r1.xyz, v5.x, r2, r1
mov r4.xyz, v4
add r7.xy, r7, c10.w
mov r7.z, c10.x
mad r7.xyz, r10.z, r7, r9
mul r7.xyz, v5.y, r7
mad r12.xyz, v5.x, r0, r7
mad r7.xy, r8.zwzw, c3.x, r8
mad r9.xy, r11, c3.x, r8
mov r9.z, c10.x
texldl r11.xy, r9.xyzz, s1
mov r7.z, c10.x
texldl r3.xyz, r7.xyzz, s0
add r0.xz, r11.yyxw, c10.w
mov r0.y, c10.x
mul r11.xyz, r10.y, r0
texldl r0.xy, r7.xyzz, s1
add r0.yz, r0.xxyw, c10.w
mov r0.x, c10
mad r11.xyz, r10.x, r0, r11
mad r8.xy, r13, c3.x, r8
mov r8.z, c10.x
mul r2.xyz, r10.x, r3
texldl r13.xy, r8.xyzz, s1
texldl r3.xyz, r8.xyzz, s0
mad r3.xyz, r10.z, r3, r2
texldl r2.xyz, r9.xyzz, s0
mad r3.xyz, r2, r10.y, r3
mad r1.xyz, v5.z, r3, r1
mul r1.xyz, r1, v1
mul r3.xyz, v1, v1.w
add r0.xy, r13, c10.w
mov r0.z, c10.x
mad r0.xyz, r10.z, r0, r11
mad r0.xyz, v5.z, r0, r12
mul r0.xyz, r0, c11.x
mov r0.w, c9.z
dp4 r0.w, r0, r0
rsq r0.w, r0.w
mul r0.xyz, r0.w, r0
mul r0.xyz, v4.w, -r0
add r0.xyz, r0, v0
dp3 r0.w, r0, r0
rsq r0.w, r0.w
mul r0.xyz, r0.w, r0
dp3 r0.w, v5, r4
mul r2.x, r0.w, c11.y
mov r2.yz, c11.xzww
texldl r2.xyz, r2.xyzz, s2
add_pp r0.w, r2.y, -r2.x
cmp_pp r1.w, -r0, r2.x, r2.y
add r0.w, -v1, c9.z
mad r5.xyz, r1, r0.w, r3
add_pp r1.x, r2.z, -r1.w
cmp_pp r1.w, -r1.x, r1, r2.z
dp3_pp r0.w, v7, v7
rsq_pp r0.w, r0.w
mul_pp r1.xyz, r0.w, v7
texldp r3.x, v8, s3
mov_pp r4.xyz, c1
mul r2.w, v5, c2.x
mov_pp r0.w, r3.x
if_gt r1.w, c12.x
mul_pp r3.x, r1.z, r1
mul_pp r1.w, r1.x, r1.x
mad_pp r1.w, r1.y, r1, r3.x
mad_pp r1.w, r1.y, r1.z, r1
mul r3.x, r1.w, c6
mov r3.z, c10.x
mov r3.y, c8.x
texldl r4.xyz, r3.xyzz, s4
mov r3.z, c10.x
mov r3.y, c7.x
texldl r3.xyz, r3.xyzz, s4
mul_pp r4.xyz, r4, c1
else
rcp_pp r3.x, r1.w
cmp_pp r1.w, -r1, c12.y, c12.z
mul_pp r3.xyz, r2, r3.x
abs_pp r1.w, r1
cmp_pp r3.xyz, -r1.w, c9.z, r3
endif
add_pp r1.xyz, r1, c0
dp3_pp r1.w, r1, r1
rsq_pp r1.w, r1.w
mul_pp r1.xyz, r1.w, r1
dp3_pp r1.x, r0, r1
mul_pp r4.w, r2, c12
max_pp r3.w, r1.x, c10.x
pow_pp_sat r1, r3.w, r4.w
mad_pp r1.y, -r1.x, c13.x, c13
mul_pp r1.x, r1, r1
mul_pp r1.x, r1, r1.y
mul_pp r1.w, r1.x, r2
mul_pp r1.xyz, r4, r3
mul_pp r3.xyz, r5, r4
mul_pp r1.xyz, r1, r1.w
dp3_pp r0.x, r0, c0
mul_pp r4.xyz, r5, v6
mad_pp r0.xyz, r3, r0.x, r1
mad_pp r0.xyz, r0, r0.w, r4
add_pp oC0.xyz, r0, r2
mov_pp oC0.w, c9.z
"
}

SubProgram "opengl " {
Keywords { "DIRECTIONAL" "LIGHTMAP_ON" "DIRLIGHTMAP_OFF" "SHADOWS_SCREEN" }
"!!GLSL"
}

SubProgram "d3d9 " {
Keywords { "DIRECTIONAL" "LIGHTMAP_ON" "DIRLIGHTMAP_OFF" "SHADOWS_SCREEN" }
Float 0 [_TileSize]
Float 1 [_Repetition]
Float 2 [_Indent]
SetTexture 0 [_DiffuseTex] 2D
SetTexture 2 [_SpecColorTex] 2D
SetTexture 3 [_ShadowMapTexture] 2D
SetTexture 4 [unity_Lightmap] 2D
"ps_3_0
; 111 ALU, 22 TEX
dcl_2d s0
dcl_2d s2
dcl_2d s3
dcl_2d s4
def c3, -0.20000000, 7.00000000, 0.00000000, 0.50000000
def c4, 0.00390625, 1.00000000, 0.00195313, 0.00392157
def c5, 0.10000000, 0.00000000, 8.00000000, 2.00000000
dcl_color0 v1
dcl_texcoord1 v2.xyz
dcl_texcoord2 v3.xyz
dcl_texcoord3 v4.xyz
dcl_texcoord4 v5.xyz
dcl_texcoord5 v6.xy
dcl_texcoord6 v7
add r1.x, v4, c3.w
frc r1.y, r1.x
add r1.w, r1.x, -r1.y
abs r0.xyz, v3
dp3 r0.w, r0, r0
rsq r0.w, r0.w
mad r0.xyz, r0.w, r0, c3.x
abs r1.x, r1.w
frc r0.w, r1.x
mul r0.xyz, r0, c3.y
add r0.w, r1.x, -r0
max r1.xyz, r0, c3.z
cmp r0.y, r1.w, r0.w, -r0.w
mov r0.w, c2.x
add r0.x, r1, r1.y
mul r0.z, r0.y, c0.x
add r0.y, r1.z, r0.x
frc r0.x, r0.z
rcp r0.y, r0.y
mul r1.xyz, r1, r0.y
add r0.z, r0, -r0.x
mul r0.y, r0.z, c0.x
rcp r0.z, c1.x
mul r3.xy, v2, r0.z
mul r2.xy, v2.yzzw, r0.z
frc r4.xy, r3
frc r0.y, r0
mad r0.w, -r0, c4.x, c4.y
frc r2.xy, r2
mul r2.xy, r0.w, r2
mov r1.w, c2.x
mad r5.zw, c4.z, r1.w, r2.xyxy
mad r2.xy, r5.zwzw, c0.x, r0
mov r2.z, c3
texldl r2.xyz, r2.xyzz, s0
mul r3.xyz, r1.x, r2
mul r2.xy, r4, r0.w
mov r1.w, c2.x
mad r5.xy, c4.z, r1.w, r2
add r2.z, v4.y, c3.w
frc r1.w, r2.z
add r1.w, r2.z, -r1
abs r2.w, r1
frc r3.w, r2
mad r2.xy, r5, c0.x, r0
mov r2.z, c3
texldl r2.xyz, r2.xyzz, s0
mad r2.xyz, r1.z, r2, r3
mul r3.xy, v2.zxzw, r0.z
add r2.w, r2, -r3
cmp r0.z, r1.w, r2.w, -r2.w
mul r2.w, r0.z, c0.x
frc r3.xy, r3
frc r4.x, r2.w
mul r0.zw, r3.xyxy, r0.w
mov r1.w, c2.x
mad r4.zw, c4.z, r1.w, r0
add r0.w, r2, -r4.x
mul r0.w, r0, c0.x
frc r4.y, r0.w
add r0.w, v4.z, c3
frc r1.w, r0
add r0.w, r0, -r1
abs r1.w, r0
frc r2.w, r1
add r1.w, r1, -r2
cmp r0.w, r0, r1, -r1
mul r0.w, r0, c0.x
mad r0.xy, r4.zwzw, c0.x, r0
mov r0.z, c3
texldl r0.xyz, r0.xyzz, s0
mad r2.xyz, r1.y, r0, r2
mad r0.xy, r5.zwzw, c0.x, r4
mov r0.z, c3
texldl r0.xyz, r0.xyzz, s0
mul r3.xyz, r1.x, r0
mad r0.xy, r5, c0.x, r4
mov r0.z, c3
texldl r0.xyz, r0.xyzz, s0
mad r0.xyz, r1.z, r0, r3
mad r3.xy, r4.zwzw, c0.x, r4
frc r4.x, r0.w
add r0.w, r0, -r4.x
mul r0.w, r0, c0.x
frc r4.y, r0.w
mov r3.z, c3
texldl r3.xyz, r3.xyzz, s0
mad r0.xyz, r1.y, r3, r0
mul r0.xyz, v5.y, r0
mad r3.xy, r5.zwzw, c0.x, r4
mov r3.z, c3
texldl r3.xyz, r3.xyzz, s0
mad r0.xyz, v5.x, r2, r0
mul r2.xyz, r1.x, r3
mad r3.xy, r5, c0.x, r4
mov r3.z, c3
texldl r3.xyz, r3.xyzz, s0
mad r2.xyz, r1.z, r3, r2
mad r3.xy, r4.zwzw, c0.x, r4
mov r3.z, c3
texldl r3.xyz, r3.xyzz, s0
mad r1.xyz, r3, r1.y, r2
mad r0.xyz, v5.z, r1, r0
mul r0.xyz, r0, v1
add r0.w, -v1, c4.y
mul r1.xyz, v1, v1.w
mad r1.xyz, r0, r0.w, r1
texldp r4.x, v7, s3
texld r0, v6, s4
mul_pp r2.xyz, r0, r4.x
mul_pp r0.xyz, r0.w, r0
mul_pp r3.xyz, r0, c5.z
mov r0.xyz, v4
dp3 r0.x, v5, r0
mul_pp r2.xyz, r2, c5.w
min_pp r2.xyz, r3, r2
mul_pp r3.xyz, r3, r4.x
mov r0.yz, c5.xxyw
mul r0.x, r0, c4.w
texldl r0.xyz, r0.xyzz, s2
max_pp r2.xyz, r2, r3
mad_pp oC0.xyz, r1, r2, r0
mov_pp oC0.w, c3.z
"
}

}
	}
	Pass {
		Name "FORWARD"
		Tags { "LightMode" = "ForwardAdd" }
		ZWrite Off Blend One One Fog { Color (0,0,0,0) }
Program "vp" {
// Vertex combos: 5
//   d3d9 - ALU: 104 to 109, TEX: 2 to 2
SubProgram "opengl " {
Keywords { "POINT" }
"!!GLSL
#ifdef VERTEX
#define POINT 1
#define SHADER_API_OPENGL 1
#define SHADER_API_DESKTOP 1
vec4 xll_tex2Dlod(sampler2D s, vec4 coord) {
   return texture2DLod( s, coord.xy, coord.w);
}
mat3 xll_constructMat3( mat4 m) {
  return mat3( vec3( m[0]), vec3( m[1]), vec3( m[2]));
}
struct v2f_vertex_lit {
    vec2 uv;
    vec4 diff;
    vec4 spec;
};
struct v2f_img {
    vec4 pos;
    vec2 uv;
};
struct appdata_img {
    vec4 vertex;
    vec2 texcoord;
};
struct SurfaceOutput {
    vec3 Albedo;
    vec3 Normal;
    vec3 Emission;
    float Specular;
    float Gloss;
    float Alpha;
};
struct CustomSurfaceOutput {
    vec3 Albedo;
    vec3 Normal;
    vec3 Normal_;
    vec3 Emission;
    float Specular;
    float Alpha;
};
struct appdata_full {
    vec4 vertex;
    vec4 tangent;
    vec3 normal;
    vec4 texcoord;
    vec4 texcoord1;
    vec4 color;
};
struct Input {
    vec3 localPos;
    vec3 localNormal;
    vec3 worldNormal;
    vec4 weights;
    vec4 blendMask;
    vec4 color;
};
struct v2f_surf {
    vec4 pos;
    vec3 worldNormal;
    vec4 color;
    vec3 cust_localPos;
    vec3 cust_localNormal;
    vec4 cust_weights;
    vec4 cust_blendMask;
    vec3 normal;
    vec3 lightDir;
    vec3 viewDir;
    vec3 _LightCoord;
};
uniform mat4 _LightMatrix0;
uniform sampler2D _NormSpecPowerTex;
uniform float _NormalPower;
uniform mat4 _Object2World;
uniform vec3 _WorldSpaceCameraPos;
uniform vec4 _WorldSpaceLightPos0;

uniform vec4 unity_Scale;
void vert( inout appdata_full v, out Input o );
vec3 WorldSpaceViewDir( in vec4 v );
vec3 WorldSpaceLightDir( in vec4 v );
v2f_surf vert_surf( in appdata_full v );
void vert( inout appdata_full v, out Input o ) {
    int mat0;
    int mat12;
    vec3 matIdx;
    int idx;
    vec2 normSpec;
    mat0 = int( (v.texcoord1.x  / 4.00000) );
    mat12 = int( v.texcoord1.y  );
    matIdx = vec3( float( mat0 ), float( (mat12 / 256) ), float( (mat12 % 256) ));
    o.weights.xyz  = matIdx;
    idx = int( (((v.texcoord1.y  - float( mat12 )) * 10.0000) + 0.499900) );
    o.blendMask.xyz  = vec3( float( (idx == 0) ), float( (idx == 1) ), float( (idx == 2) ));
    normSpec = xll_tex2Dlod( _NormSpecPowerTex, vec4( (dot( o.blendMask, vec4( matIdx, 0.000000)) / 255.000), 0.100000, 0.000000, 0.000000)).xy ;
    o.weights.w  = (((normSpec.x  * 16.0000) - 8.00000) * _NormalPower);
    o.blendMask.w  = (normSpec.y  * 4.00000);
    v.normal = vec3( v.texcoord.x , v.texcoord.y , ((v.texcoord1.x  - float( (4 * mat0) )) - 2.00000));
    o.localPos = vec3( (v.vertex * 0.0500000));
    o.localNormal = v.normal;
}
vec3 WorldSpaceViewDir( in vec4 v ) {
    return (_WorldSpaceCameraPos.xyz  - ( _Object2World * v ).xyz );
}
vec3 WorldSpaceLightDir( in vec4 v ) {
    vec3 worldPos;
    worldPos = ( _Object2World * v ).xyz ;
    return (_WorldSpaceLightPos0.xyz  - worldPos);
}
v2f_surf vert_surf( in appdata_full v ) {
    Input customInputData;
    v2f_surf o;
    vec3 lightDir;
    vec3 viewDirForLight;
    vert( v, customInputData);
    o.cust_localPos = customInputData.localPos;
    o.cust_localNormal = customInputData.localNormal;
    o.cust_weights = customInputData.weights;
    o.cust_blendMask = customInputData.blendMask;
    o.pos = ( gl_ModelViewProjectionMatrix * v.vertex );
    o.worldNormal = ( xll_constructMat3( _Object2World) * v.normal );
    o.color = v.color;
    o.normal = ( xll_constructMat3( _Object2World) * (v.normal * unity_Scale.w ) );
    lightDir = WorldSpaceLightDir( v.vertex);
    o.lightDir = lightDir;
    viewDirForLight = WorldSpaceViewDir( v.vertex);
    o.viewDir = viewDirForLight;
    o._LightCoord = ( _LightMatrix0 * ( _Object2World * v.vertex ) ).xyz ;
    return o;
}
attribute vec4 TANGENT;
varying vec3 xlv_TEXCOORD0;
varying vec4 xlv_COLOR0;
varying vec3 xlv_TEXCOORD1;
varying vec3 xlv_TEXCOORD2;
varying vec4 xlv_TEXCOORD3;
varying vec4 xlv_TEXCOORD4;
varying vec3 xlv_TEXCOORD5;
varying vec3 xlv_TEXCOORD6;
varying vec3 xlv_TEXCOORD7;
varying vec3 xlv_TEXCOORD8;
void main() {
    v2f_surf xl_retval;
    appdata_full xlt_v;
    xlt_v.vertex = vec4( gl_Vertex);
    xlt_v.tangent = vec4( TANGENT);
    xlt_v.normal = vec3( gl_Normal);
    xlt_v.texcoord = vec4( gl_MultiTexCoord0);
    xlt_v.texcoord1 = vec4( gl_MultiTexCoord1);
    xlt_v.color = vec4( gl_Color);
    xl_retval = vert_surf( xlt_v);
    gl_Position = vec4( xl_retval.pos);
    xlv_TEXCOORD0 = vec3( xl_retval.worldNormal);
    xlv_COLOR0 = vec4( xl_retval.color);
    xlv_TEXCOORD1 = vec3( xl_retval.cust_localPos);
    xlv_TEXCOORD2 = vec3( xl_retval.cust_localNormal);
    xlv_TEXCOORD3 = vec4( xl_retval.cust_weights);
    xlv_TEXCOORD4 = vec4( xl_retval.cust_blendMask);
    xlv_TEXCOORD5 = vec3( xl_retval.normal);
    xlv_TEXCOORD6 = vec3( xl_retval.lightDir);
    xlv_TEXCOORD7 = vec3( xl_retval.viewDir);
    xlv_TEXCOORD8 = vec3( xl_retval._LightCoord);
}
/* NOTE: GLSL optimization failed
0:88(75): error: operator '%' is reserved in (null)
0:88(56): error: cannot construct `float' from a non-numeric data type
0:88(11): error: cannot construct `vec3' from a non-numeric data type
*/

#endif
#ifdef FRAGMENT
#extension GL_ARB_shader_texture_lod : enable
varying vec3 xlv_TEXCOORD8;
varying vec3 xlv_TEXCOORD7;
varying vec3 xlv_TEXCOORD6;
varying vec4 xlv_TEXCOORD4;
varying vec4 xlv_TEXCOORD3;
varying vec3 xlv_TEXCOORD2;
varying vec3 xlv_TEXCOORD1;
varying vec4 xlv_COLOR0;
varying vec3 xlv_TEXCOORD0;
vec4 xlat_mutable__SpecColor;
uniform vec4 _SpecColor;
vec4 xlat_mutable__LightColor0;
uniform vec4 _LightColor0;
uniform float _TileSize;
uniform float _SpecPower;
uniform sampler2D _SpecColorTex;
uniform float _Repetition;
uniform sampler2D _NormalMap;
uniform sampler2D _LightTexture0;
uniform float _Indent;
uniform sampler2D _DiffuseTex;
uniform sampler2D _DiamondSpecTex;
uniform float _DiamondSpecParam3;
uniform float _DiamondSpecParam2;
uniform float _DiamondSpecParam1;
void main ()
{
  xlat_mutable__SpecColor = _SpecColor;
  xlat_mutable__LightColor0 = _LightColor0;
  vec4 c;
  vec3 tmpvar_1;
  tmpvar_1 = max (((normalize (abs (xlv_TEXCOORD2)) - 0.2) * 7.0), vec3(0.0, 0.0, 0.0));
  vec3 tmpvar_2;
  tmpvar_2 = (tmpvar_1 / vec3(((tmpvar_1.x + tmpvar_1.y) + tmpvar_1.z)));
  float tmpvar_3;
  tmpvar_3 = (float(int(floor ((xlv_TEXCOORD3.x + 0.5)))) * _TileSize);
  float tmpvar_4;
  tmpvar_4 = (floor (tmpvar_3) * _TileSize);
  vec3 tmpvar_5;
  tmpvar_5.x = fract (tmpvar_3);
  tmpvar_5.y = fract (tmpvar_4);
  tmpvar_5.z = floor (tmpvar_4);
  float tmpvar_6;
  tmpvar_6 = (float(int(floor ((xlv_TEXCOORD3.y + 0.5)))) * _TileSize);
  float tmpvar_7;
  tmpvar_7 = (floor (tmpvar_6) * _TileSize);
  vec3 tmpvar_8;
  tmpvar_8.x = fract (tmpvar_6);
  tmpvar_8.y = fract (tmpvar_7);
  tmpvar_8.z = floor (tmpvar_7);
  float tmpvar_9;
  tmpvar_9 = (float(int(floor ((xlv_TEXCOORD3.z + 0.5)))) * _TileSize);
  float tmpvar_10;
  tmpvar_10 = (floor (tmpvar_9) * _TileSize);
  vec3 tmpvar_11;
  tmpvar_11.x = fract (tmpvar_9);
  tmpvar_11.y = fract (tmpvar_10);
  tmpvar_11.z = floor (tmpvar_10);
  vec2 tmpvar_12;
  tmpvar_12 = (tmpvar_5.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.xy / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_13;
  tmpvar_13.zw = vec2(0.0, 0.0);
  tmpvar_13.x = tmpvar_12.x;
  tmpvar_13.y = tmpvar_12.y;
  vec2 tmpvar_14;
  tmpvar_14 = (tmpvar_5.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.yz / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_15;
  tmpvar_15.zw = vec2(0.0, 0.0);
  tmpvar_15.x = tmpvar_14.x;
  tmpvar_15.y = tmpvar_14.y;
  vec2 tmpvar_16;
  tmpvar_16 = (tmpvar_5.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.zx / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_17;
  tmpvar_17.zw = vec2(0.0, 0.0);
  tmpvar_17.x = tmpvar_16.x;
  tmpvar_17.y = tmpvar_16.y;
  vec2 tmpvar_18;
  tmpvar_18 = (tmpvar_8.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.xy / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_19;
  tmpvar_19.zw = vec2(0.0, 0.0);
  tmpvar_19.x = tmpvar_18.x;
  tmpvar_19.y = tmpvar_18.y;
  vec2 tmpvar_20;
  tmpvar_20 = (tmpvar_8.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.yz / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_21;
  tmpvar_21.zw = vec2(0.0, 0.0);
  tmpvar_21.x = tmpvar_20.x;
  tmpvar_21.y = tmpvar_20.y;
  vec2 tmpvar_22;
  tmpvar_22 = (tmpvar_8.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.zx / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_23;
  tmpvar_23.zw = vec2(0.0, 0.0);
  tmpvar_23.x = tmpvar_22.x;
  tmpvar_23.y = tmpvar_22.y;
  vec2 tmpvar_24;
  tmpvar_24 = (tmpvar_11.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.xy / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_25;
  tmpvar_25.zw = vec2(0.0, 0.0);
  tmpvar_25.x = tmpvar_24.x;
  tmpvar_25.y = tmpvar_24.y;
  vec2 tmpvar_26;
  tmpvar_26 = (tmpvar_11.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.yz / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_27;
  tmpvar_27.zw = vec2(0.0, 0.0);
  tmpvar_27.x = tmpvar_26.x;
  tmpvar_27.y = tmpvar_26.y;
  vec2 tmpvar_28;
  tmpvar_28 = (tmpvar_11.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.zx / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_29;
  tmpvar_29.zw = vec2(0.0, 0.0);
  tmpvar_29.x = tmpvar_28.x;
  tmpvar_29.y = tmpvar_28.y;
  vec4 tmpvar_30;
  vec2 tmpvar_31;
  tmpvar_31 = (tmpvar_5.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.yz / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_32;
  tmpvar_32.zw = vec2(0.0, 0.0);
  tmpvar_32.x = tmpvar_31.x;
  tmpvar_32.y = tmpvar_31.y;
  tmpvar_30 = (texture2DLod (_NormalMap, tmpvar_32.xy, 0.0) - 0.5);
  vec4 tmpvar_33;
  vec2 tmpvar_34;
  tmpvar_34 = (tmpvar_5.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.zx / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_35;
  tmpvar_35.zw = vec2(0.0, 0.0);
  tmpvar_35.x = tmpvar_34.x;
  tmpvar_35.y = tmpvar_34.y;
  tmpvar_33 = (texture2DLod (_NormalMap, tmpvar_35.xy, 0.0) - 0.5);
  vec4 tmpvar_36;
  vec2 tmpvar_37;
  tmpvar_37 = (tmpvar_5.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.xy / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_38;
  tmpvar_38.zw = vec2(0.0, 0.0);
  tmpvar_38.x = tmpvar_37.x;
  tmpvar_38.y = tmpvar_37.y;
  tmpvar_36 = (texture2DLod (_NormalMap, tmpvar_38.xy, 0.0) - 0.5);
  vec3 tmpvar_39;
  tmpvar_39.x = 0.0;
  tmpvar_39.y = tmpvar_30.x;
  tmpvar_39.z = tmpvar_30.y;
  vec3 tmpvar_40;
  tmpvar_40.y = 0.0;
  tmpvar_40.x = tmpvar_33.y;
  tmpvar_40.z = tmpvar_33.x;
  vec3 tmpvar_41;
  tmpvar_41.z = 0.0;
  tmpvar_41.x = tmpvar_36.x;
  tmpvar_41.y = tmpvar_36.y;
  vec4 tmpvar_42;
  vec2 tmpvar_43;
  tmpvar_43 = (tmpvar_8.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.yz / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_44;
  tmpvar_44.zw = vec2(0.0, 0.0);
  tmpvar_44.x = tmpvar_43.x;
  tmpvar_44.y = tmpvar_43.y;
  tmpvar_42 = (texture2DLod (_NormalMap, tmpvar_44.xy, 0.0) - 0.5);
  vec4 tmpvar_45;
  vec2 tmpvar_46;
  tmpvar_46 = (tmpvar_8.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.zx / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_47;
  tmpvar_47.zw = vec2(0.0, 0.0);
  tmpvar_47.x = tmpvar_46.x;
  tmpvar_47.y = tmpvar_46.y;
  tmpvar_45 = (texture2DLod (_NormalMap, tmpvar_47.xy, 0.0) - 0.5);
  vec4 tmpvar_48;
  vec2 tmpvar_49;
  tmpvar_49 = (tmpvar_8.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.xy / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_50;
  tmpvar_50.zw = vec2(0.0, 0.0);
  tmpvar_50.x = tmpvar_49.x;
  tmpvar_50.y = tmpvar_49.y;
  tmpvar_48 = (texture2DLod (_NormalMap, tmpvar_50.xy, 0.0) - 0.5);
  vec3 tmpvar_51;
  tmpvar_51.x = 0.0;
  tmpvar_51.y = tmpvar_42.x;
  tmpvar_51.z = tmpvar_42.y;
  vec3 tmpvar_52;
  tmpvar_52.y = 0.0;
  tmpvar_52.x = tmpvar_45.y;
  tmpvar_52.z = tmpvar_45.x;
  vec3 tmpvar_53;
  tmpvar_53.z = 0.0;
  tmpvar_53.x = tmpvar_48.x;
  tmpvar_53.y = tmpvar_48.y;
  vec4 tmpvar_54;
  vec2 tmpvar_55;
  tmpvar_55 = (tmpvar_11.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.yz / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_56;
  tmpvar_56.zw = vec2(0.0, 0.0);
  tmpvar_56.x = tmpvar_55.x;
  tmpvar_56.y = tmpvar_55.y;
  tmpvar_54 = (texture2DLod (_NormalMap, tmpvar_56.xy, 0.0) - 0.5);
  vec4 tmpvar_57;
  vec2 tmpvar_58;
  tmpvar_58 = (tmpvar_11.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.zx / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_59;
  tmpvar_59.zw = vec2(0.0, 0.0);
  tmpvar_59.x = tmpvar_58.x;
  tmpvar_59.y = tmpvar_58.y;
  tmpvar_57 = (texture2DLod (_NormalMap, tmpvar_59.xy, 0.0) - 0.5);
  vec4 tmpvar_60;
  vec2 tmpvar_61;
  tmpvar_61 = (tmpvar_11.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.xy / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_62;
  tmpvar_62.zw = vec2(0.0, 0.0);
  tmpvar_62.x = tmpvar_61.x;
  tmpvar_62.y = tmpvar_61.y;
  tmpvar_60 = (texture2DLod (_NormalMap, tmpvar_62.xy, 0.0) - 0.5);
  vec3 tmpvar_63;
  tmpvar_63.x = 0.0;
  tmpvar_63.y = tmpvar_54.x;
  tmpvar_63.z = tmpvar_54.y;
  vec3 tmpvar_64;
  tmpvar_64.y = 0.0;
  tmpvar_64.x = tmpvar_57.y;
  tmpvar_64.z = tmpvar_57.x;
  vec3 tmpvar_65;
  tmpvar_65.z = 0.0;
  tmpvar_65.x = tmpvar_60.x;
  tmpvar_65.y = tmpvar_60.y;
  vec3 tmpvar_66;
  tmpvar_66 = ((((xlv_TEXCOORD4.x * (((tmpvar_39 * tmpvar_2.xxx) + (tmpvar_40 * tmpvar_2.yyy)) + (tmpvar_41 * tmpvar_2.zzz))) * 2.0) + ((xlv_TEXCOORD4.y * (((tmpvar_51 * tmpvar_2.xxx) + (tmpvar_52 * tmpvar_2.yyy)) + (tmpvar_53 * tmpvar_2.zzz))) * 2.0)) + ((xlv_TEXCOORD4.z * (((tmpvar_63 * tmpvar_2.xxx) + (tmpvar_64 * tmpvar_2.yyy)) + (tmpvar_65 * tmpvar_2.zzz))) * 2.0));
  vec3 tmpvar_67;
  tmpvar_67 = ((((((xlv_TEXCOORD4.x * (((texture2DLod (_DiffuseTex, tmpvar_13.xy, 0.0) * tmpvar_2.z) + (texture2DLod (_DiffuseTex, tmpvar_15.xy, 0.0) * tmpvar_2.x)) + (texture2DLod (_DiffuseTex, tmpvar_17.xy, 0.0) * tmpvar_2.y))) + (xlv_TEXCOORD4.y * (((texture2DLod (_DiffuseTex, tmpvar_19.xy, 0.0) * tmpvar_2.z) + (texture2DLod (_DiffuseTex, tmpvar_21.xy, 0.0) * tmpvar_2.x)) + (texture2DLod (_DiffuseTex, tmpvar_23.xy, 0.0) * tmpvar_2.y)))) + (xlv_TEXCOORD4.z * (((texture2DLod (_DiffuseTex, tmpvar_25.xy, 0.0) * tmpvar_2.z) + (texture2DLod (_DiffuseTex, tmpvar_27.xy, 0.0) * tmpvar_2.x)) + (texture2DLod (_DiffuseTex, tmpvar_29.xy, 0.0) * tmpvar_2.y)))).xyz * xlv_COLOR0.xyz) * (1.0 - xlv_COLOR0.w)) + (xlv_COLOR0.xyz * xlv_COLOR0.w));
  vec4 tmpvar_68;
  tmpvar_68.w = 1.0;
  tmpvar_68.x = tmpvar_66.x;
  tmpvar_68.y = tmpvar_66.y;
  tmpvar_68.z = tmpvar_66.z;
  vec3 tmpvar_69;
  tmpvar_69 = normalize ((xlv_TEXCOORD0 - (normalize (tmpvar_68).xyz * xlv_TEXCOORD3.w)));
  float tmpvar_70;
  tmpvar_70 = (xlv_TEXCOORD4.w * _SpecPower);
  vec4 tmpvar_71;
  tmpvar_71.w = 0.0;
  tmpvar_71.xyz = xlv_TEXCOORD3.xyz;
  vec4 tmpvar_72;
  tmpvar_72.yzw = vec3(0.1, 0.0, 0.0);
  tmpvar_72.x = (dot (xlv_TEXCOORD4, tmpvar_71) / 255.0);
  vec4 tmpvar_73;
  tmpvar_73 = texture2DLod (_SpecColorTex, tmpvar_72.xy, 0.0);
  vec3 tmpvar_74;
  tmpvar_74 = normalize (xlv_TEXCOORD6);
  vec3 tmpvar_75;
  tmpvar_75 = normalize (xlv_TEXCOORD7);
  float atten;
  atten = texture2D (_LightTexture0, vec2(dot (xlv_TEXCOORD8, xlv_TEXCOORD8))).w;
  vec4 c_i0;
  float m;
  m = tmpvar_73.x;
  if ((tmpvar_73.y > tmpvar_73.x)) {
    m = tmpvar_73.y;
  };
  if ((tmpvar_73.z > m)) {
    m = tmpvar_73.z;
  };
  if ((m > 0.03)) {
    float tmpvar_76;
    tmpvar_76 = (((((tmpvar_75.x * tmpvar_75.x) * tmpvar_75.y) + (tmpvar_75.z * tmpvar_75.x)) + (tmpvar_75.y * tmpvar_75.z)) * _DiamondSpecParam1);
    vec4 tmpvar_77;
    tmpvar_77.zw = vec2(0.0, 0.0);
    tmpvar_77.x = tmpvar_76;
    tmpvar_77.y = _DiamondSpecParam2;
    xlat_mutable__SpecColor.xyz = texture2DLod (_DiamondSpecTex, tmpvar_77.xy, 0.0).xyz;
    vec4 tmpvar_78;
    tmpvar_78.zw = vec2(0.0, 0.0);
    tmpvar_78.x = tmpvar_76;
    tmpvar_78.y = _DiamondSpecParam3;
    xlat_mutable__LightColor0.xyz = (_LightColor0.xyz * texture2DLod (_DiamondSpecTex, tmpvar_78.xy, 0.0).xyz);
  } else {
    if ((m > 0.0)) {
      xlat_mutable__SpecColor.xyz = (tmpvar_73.xyz * (1.0/(m)));
    } else {
      xlat_mutable__SpecColor.xyz = vec3(1.0, 1.0, 1.0);
    };
  };
  c_i0 = vec4(0.0, 0.0, 0.0, 0.0);
  c_i0.xyz = ((((tmpvar_67 * xlat_mutable__LightColor0.xyz) * dot (tmpvar_69, tmpvar_74)) + ((xlat_mutable__LightColor0.xyz * xlat_mutable__SpecColor.xyz) * (smoothstep (0.0, 1.0, pow (max (0.0, dot (tmpvar_69, normalize ((tmpvar_74 + tmpvar_75)))), (32.0 * tmpvar_70))) * tmpvar_70))) * atten);
  c_i0.w = 1.0;
  c = c_i0;
  c.w = 0.0;
  gl_FragData[0] = c;
}


#endif
"
}

SubProgram "d3d9 " {
Keywords { "POINT" }
Bind "vertex" Vertex
Bind "texcoord" TexCoord0
Bind "texcoord1" TexCoord1
Bind "color" Color
Matrix 0 [glstate_matrix_mvp]
Vector 12 [unity_Scale]
Vector 13 [_WorldSpaceCameraPos]
Vector 14 [_WorldSpaceLightPos0]
Matrix 4 [_Object2World]
Matrix 8 [_LightMatrix0]
Float 15 [_NormalPower]
SetTexture 0 [_NormSpecPowerTex] 2D
"vs_3_0
; 108 ALU, 2 TEX
dcl_position o0
dcl_texcoord0 o1
dcl_color0 o2
dcl_texcoord1 o3
dcl_texcoord2 o4
dcl_texcoord3 o5
dcl_texcoord4 o6
dcl_texcoord5 o7
dcl_texcoord6 o8
dcl_texcoord7 o9
dcl_texcoord8 o10
def c16, 0.25000000, 0.00000000, 1.00000000, 0.00390625
def c17, 256.00000000, 0.50000000, 10.00000000, 0.49990001
def c18, -0.49990001, 2.00000000, 0.00392157, 4.00000000
def c19, 0.10000000, 0.00000000, 16.00000000, -8.00000000
def c20, -2.00000000, 0.05000000, 0, 0
dcl_position0 v0
dcl_texcoord0 v3
dcl_texcoord1 v4
dcl_color0 v5
dcl_2d s0
abs r0.y, v4
frc r0.z, r0.y
slt r0.x, v4.y, c16.y
max r0.x, -r0, r0
slt r0.x, c16.y, r0
add r0.y, r0, -r0.z
add r0.w, -r0.x, c16.z
mul r0.z, r0.y, r0.w
mad r0.z, r0.x, -r0.y, r0
add r0.x, v4.y, -r0.z
mul r0.x, r0, c17.z
mul r1.y, r0.z, c16.w
slt r0.y, r0.x, c18.x
add r0.w, r0.x, c17
max r0.x, -r0.y, r0.y
abs r0.y, r0.w
slt r0.x, c16.y, r0
frc r0.w, r0.y
add r0.y, r0, -r0.w
add r1.x, -r0, c16.z
mul r0.w, r0.y, r1.x
mad r0.w, r0.x, -r0.y, r0
sge r0.y, c16, r0.w
sge r0.x, r0.w, c16.y
mul r0.x, r0, r0.y
sge r1.x, c16.z, r0.w
sge r0.y, r0.w, c16.z
mul r0.y, r0, r1.x
sge r1.x, r0.w, c18.y
sge r0.w, c18.y, r0
mul r0.z, r1.x, r0.w
abs r1.x, r1.y
frc r1.z, r1.x
add r1.x, r1, -r1.z
slt r0.w, r1.y, c16.y
frc r1.z, r1.y
mul r1.z, r1, c17.x
max r0.w, -r0, r0
slt r0.w, c16.y, r0
add r1.y, -r0.w, c16.z
mul r1.y, r1.x, r1
mad r1.y, r0.w, -r1.x, r1
abs r1.w, r1.z
add r1.x, r1.w, c17.y
mul r1.w, v4.x, c16.x
slt r0.w, r1.z, c16.y
frc r1.z, r1.x
slt r2.x, r1.w, c16.y
max r0.w, -r0, r0
add r1.x, r1, -r1.z
slt r0.w, c16.y, r0
add r1.z, -r0.w, c16
mul r1.z, r1.x, r1
mad r1.z, r0.w, -r1.x, r1
abs r2.y, r1.w
max r2.x, -r2, r2
slt r1.w, c16.y, r2.x
frc r2.x, r2.y
add r2.x, r2.y, -r2
add r2.z, -r1.w, c16
mul r2.y, r2.x, r2.z
mad r1.x, r1.w, -r2, r2.y
dp3 r0.w, r0, r1
mov r2.yz, c19.xxyw
mul r2.x, r0.w, c18.z
texldl r2.xy, r2.xyzz, s0
mad r0.w, r2.x, c19.z, c19
mul r1.w, r0, c15.x
mul r0.w, r2.y, c18
mov o6, r0
mov o5, r1
mad r1.x, -r1, c18.w, v4
add r1.z, r1.x, c20.x
mov r1.xy, v3
mul r2.xyz, r1, c12.w
dp4 r0.z, v0, c6
dp4 r0.x, v0, c4
dp4 r0.y, v0, c5
dp4 r0.w, v0, c7
mov o2, v5
dp4 o10.z, r0, c10
dp4 o10.y, r0, c9
dp4 o10.x, r0, c8
dp3 o1.z, r1, c6
dp3 o1.y, r1, c5
dp3 o1.x, r1, c4
mul o3.xyz, v0, c20.y
mov o4.xyz, r1
dp3 o7.z, r2, c6
dp3 o7.y, r2, c5
dp3 o7.x, r2, c4
add o8.xyz, -r0, c14
add o9.xyz, -r0, c13
dp4 o0.w, v0, c3
dp4 o0.z, v0, c2
dp4 o0.y, v0, c1
dp4 o0.x, v0, c0
"
}

SubProgram "opengl " {
Keywords { "DIRECTIONAL" }
"!!GLSL
#ifdef VERTEX
#define DIRECTIONAL 1
#define SHADER_API_OPENGL 1
#define SHADER_API_DESKTOP 1
vec4 xll_tex2Dlod(sampler2D s, vec4 coord) {
   return texture2DLod( s, coord.xy, coord.w);
}
mat3 xll_constructMat3( mat4 m) {
  return mat3( vec3( m[0]), vec3( m[1]), vec3( m[2]));
}
struct v2f_vertex_lit {
    vec2 uv;
    vec4 diff;
    vec4 spec;
};
struct v2f_img {
    vec4 pos;
    vec2 uv;
};
struct appdata_img {
    vec4 vertex;
    vec2 texcoord;
};
struct SurfaceOutput {
    vec3 Albedo;
    vec3 Normal;
    vec3 Emission;
    float Specular;
    float Gloss;
    float Alpha;
};
struct CustomSurfaceOutput {
    vec3 Albedo;
    vec3 Normal;
    vec3 Normal_;
    vec3 Emission;
    float Specular;
    float Alpha;
};
struct appdata_full {
    vec4 vertex;
    vec4 tangent;
    vec3 normal;
    vec4 texcoord;
    vec4 texcoord1;
    vec4 color;
};
struct Input {
    vec3 localPos;
    vec3 localNormal;
    vec3 worldNormal;
    vec4 weights;
    vec4 blendMask;
    vec4 color;
};
struct v2f_surf {
    vec4 pos;
    vec3 worldNormal;
    vec4 color;
    vec3 cust_localPos;
    vec3 cust_localNormal;
    vec4 cust_weights;
    vec4 cust_blendMask;
    vec3 normal;
    vec3 lightDir;
    vec3 viewDir;
};
uniform sampler2D _NormSpecPowerTex;
uniform float _NormalPower;
uniform mat4 _Object2World;
uniform vec3 _WorldSpaceCameraPos;
uniform vec4 _WorldSpaceLightPos0;

uniform vec4 unity_Scale;
void vert( inout appdata_full v, out Input o );
vec3 WorldSpaceViewDir( in vec4 v );
vec3 WorldSpaceLightDir( in vec4 v );
v2f_surf vert_surf( in appdata_full v );
void vert( inout appdata_full v, out Input o ) {
    int mat0;
    int mat12;
    vec3 matIdx;
    int idx;
    vec2 normSpec;
    mat0 = int( (v.texcoord1.x  / 4.00000) );
    mat12 = int( v.texcoord1.y  );
    matIdx = vec3( float( mat0 ), float( (mat12 / 256) ), float( (mat12 % 256) ));
    o.weights.xyz  = matIdx;
    idx = int( (((v.texcoord1.y  - float( mat12 )) * 10.0000) + 0.499900) );
    o.blendMask.xyz  = vec3( float( (idx == 0) ), float( (idx == 1) ), float( (idx == 2) ));
    normSpec = xll_tex2Dlod( _NormSpecPowerTex, vec4( (dot( o.blendMask, vec4( matIdx, 0.000000)) / 255.000), 0.100000, 0.000000, 0.000000)).xy ;
    o.weights.w  = (((normSpec.x  * 16.0000) - 8.00000) * _NormalPower);
    o.blendMask.w  = (normSpec.y  * 4.00000);
    v.normal = vec3( v.texcoord.x , v.texcoord.y , ((v.texcoord1.x  - float( (4 * mat0) )) - 2.00000));
    o.localPos = vec3( (v.vertex * 0.0500000));
    o.localNormal = v.normal;
}
vec3 WorldSpaceViewDir( in vec4 v ) {
    return (_WorldSpaceCameraPos.xyz  - ( _Object2World * v ).xyz );
}
vec3 WorldSpaceLightDir( in vec4 v ) {
    vec3 worldPos;
    worldPos = ( _Object2World * v ).xyz ;
    return _WorldSpaceLightPos0.xyz ;
}
v2f_surf vert_surf( in appdata_full v ) {
    Input customInputData;
    v2f_surf o;
    vec3 lightDir;
    vec3 viewDirForLight;
    vert( v, customInputData);
    o.cust_localPos = customInputData.localPos;
    o.cust_localNormal = customInputData.localNormal;
    o.cust_weights = customInputData.weights;
    o.cust_blendMask = customInputData.blendMask;
    o.pos = ( gl_ModelViewProjectionMatrix * v.vertex );
    o.worldNormal = ( xll_constructMat3( _Object2World) * v.normal );
    o.color = v.color;
    o.normal = ( xll_constructMat3( _Object2World) * (v.normal * unity_Scale.w ) );
    lightDir = WorldSpaceLightDir( v.vertex);
    o.lightDir = lightDir;
    viewDirForLight = WorldSpaceViewDir( v.vertex);
    o.viewDir = viewDirForLight;
    return o;
}
attribute vec4 TANGENT;
varying vec3 xlv_TEXCOORD0;
varying vec4 xlv_COLOR0;
varying vec3 xlv_TEXCOORD1;
varying vec3 xlv_TEXCOORD2;
varying vec4 xlv_TEXCOORD3;
varying vec4 xlv_TEXCOORD4;
varying vec3 xlv_TEXCOORD5;
varying vec3 xlv_TEXCOORD6;
varying vec3 xlv_TEXCOORD7;
void main() {
    v2f_surf xl_retval;
    appdata_full xlt_v;
    xlt_v.vertex = vec4( gl_Vertex);
    xlt_v.tangent = vec4( TANGENT);
    xlt_v.normal = vec3( gl_Normal);
    xlt_v.texcoord = vec4( gl_MultiTexCoord0);
    xlt_v.texcoord1 = vec4( gl_MultiTexCoord1);
    xlt_v.color = vec4( gl_Color);
    xl_retval = vert_surf( xlt_v);
    gl_Position = vec4( xl_retval.pos);
    xlv_TEXCOORD0 = vec3( xl_retval.worldNormal);
    xlv_COLOR0 = vec4( xl_retval.color);
    xlv_TEXCOORD1 = vec3( xl_retval.cust_localPos);
    xlv_TEXCOORD2 = vec3( xl_retval.cust_localNormal);
    xlv_TEXCOORD3 = vec4( xl_retval.cust_weights);
    xlv_TEXCOORD4 = vec4( xl_retval.cust_blendMask);
    xlv_TEXCOORD5 = vec3( xl_retval.normal);
    xlv_TEXCOORD6 = vec3( xl_retval.lightDir);
    xlv_TEXCOORD7 = vec3( xl_retval.viewDir);
}
/* NOTE: GLSL optimization failed
0:86(75): error: operator '%' is reserved in (null)
0:86(56): error: cannot construct `float' from a non-numeric data type
0:86(11): error: cannot construct `vec3' from a non-numeric data type
*/

#endif
#ifdef FRAGMENT
#extension GL_ARB_shader_texture_lod : enable
varying vec3 xlv_TEXCOORD7;
varying vec3 xlv_TEXCOORD6;
varying vec4 xlv_TEXCOORD4;
varying vec4 xlv_TEXCOORD3;
varying vec3 xlv_TEXCOORD2;
varying vec3 xlv_TEXCOORD1;
varying vec4 xlv_COLOR0;
varying vec3 xlv_TEXCOORD0;
vec4 xlat_mutable__SpecColor;
uniform vec4 _SpecColor;
vec4 xlat_mutable__LightColor0;
uniform vec4 _LightColor0;
uniform float _TileSize;
uniform float _SpecPower;
uniform sampler2D _SpecColorTex;
uniform float _Repetition;
uniform sampler2D _NormalMap;
uniform float _Indent;
uniform sampler2D _DiffuseTex;
uniform sampler2D _DiamondSpecTex;
uniform float _DiamondSpecParam3;
uniform float _DiamondSpecParam2;
uniform float _DiamondSpecParam1;
void main ()
{
  xlat_mutable__SpecColor = _SpecColor;
  xlat_mutable__LightColor0 = _LightColor0;
  vec4 c;
  vec3 tmpvar_1;
  tmpvar_1 = max (((normalize (abs (xlv_TEXCOORD2)) - 0.2) * 7.0), vec3(0.0, 0.0, 0.0));
  vec3 tmpvar_2;
  tmpvar_2 = (tmpvar_1 / vec3(((tmpvar_1.x + tmpvar_1.y) + tmpvar_1.z)));
  float tmpvar_3;
  tmpvar_3 = (float(int(floor ((xlv_TEXCOORD3.x + 0.5)))) * _TileSize);
  float tmpvar_4;
  tmpvar_4 = (floor (tmpvar_3) * _TileSize);
  vec3 tmpvar_5;
  tmpvar_5.x = fract (tmpvar_3);
  tmpvar_5.y = fract (tmpvar_4);
  tmpvar_5.z = floor (tmpvar_4);
  float tmpvar_6;
  tmpvar_6 = (float(int(floor ((xlv_TEXCOORD3.y + 0.5)))) * _TileSize);
  float tmpvar_7;
  tmpvar_7 = (floor (tmpvar_6) * _TileSize);
  vec3 tmpvar_8;
  tmpvar_8.x = fract (tmpvar_6);
  tmpvar_8.y = fract (tmpvar_7);
  tmpvar_8.z = floor (tmpvar_7);
  float tmpvar_9;
  tmpvar_9 = (float(int(floor ((xlv_TEXCOORD3.z + 0.5)))) * _TileSize);
  float tmpvar_10;
  tmpvar_10 = (floor (tmpvar_9) * _TileSize);
  vec3 tmpvar_11;
  tmpvar_11.x = fract (tmpvar_9);
  tmpvar_11.y = fract (tmpvar_10);
  tmpvar_11.z = floor (tmpvar_10);
  vec2 tmpvar_12;
  tmpvar_12 = (tmpvar_5.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.xy / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_13;
  tmpvar_13.zw = vec2(0.0, 0.0);
  tmpvar_13.x = tmpvar_12.x;
  tmpvar_13.y = tmpvar_12.y;
  vec2 tmpvar_14;
  tmpvar_14 = (tmpvar_5.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.yz / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_15;
  tmpvar_15.zw = vec2(0.0, 0.0);
  tmpvar_15.x = tmpvar_14.x;
  tmpvar_15.y = tmpvar_14.y;
  vec2 tmpvar_16;
  tmpvar_16 = (tmpvar_5.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.zx / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_17;
  tmpvar_17.zw = vec2(0.0, 0.0);
  tmpvar_17.x = tmpvar_16.x;
  tmpvar_17.y = tmpvar_16.y;
  vec2 tmpvar_18;
  tmpvar_18 = (tmpvar_8.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.xy / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_19;
  tmpvar_19.zw = vec2(0.0, 0.0);
  tmpvar_19.x = tmpvar_18.x;
  tmpvar_19.y = tmpvar_18.y;
  vec2 tmpvar_20;
  tmpvar_20 = (tmpvar_8.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.yz / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_21;
  tmpvar_21.zw = vec2(0.0, 0.0);
  tmpvar_21.x = tmpvar_20.x;
  tmpvar_21.y = tmpvar_20.y;
  vec2 tmpvar_22;
  tmpvar_22 = (tmpvar_8.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.zx / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_23;
  tmpvar_23.zw = vec2(0.0, 0.0);
  tmpvar_23.x = tmpvar_22.x;
  tmpvar_23.y = tmpvar_22.y;
  vec2 tmpvar_24;
  tmpvar_24 = (tmpvar_11.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.xy / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_25;
  tmpvar_25.zw = vec2(0.0, 0.0);
  tmpvar_25.x = tmpvar_24.x;
  tmpvar_25.y = tmpvar_24.y;
  vec2 tmpvar_26;
  tmpvar_26 = (tmpvar_11.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.yz / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_27;
  tmpvar_27.zw = vec2(0.0, 0.0);
  tmpvar_27.x = tmpvar_26.x;
  tmpvar_27.y = tmpvar_26.y;
  vec2 tmpvar_28;
  tmpvar_28 = (tmpvar_11.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.zx / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_29;
  tmpvar_29.zw = vec2(0.0, 0.0);
  tmpvar_29.x = tmpvar_28.x;
  tmpvar_29.y = tmpvar_28.y;
  vec4 tmpvar_30;
  vec2 tmpvar_31;
  tmpvar_31 = (tmpvar_5.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.yz / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_32;
  tmpvar_32.zw = vec2(0.0, 0.0);
  tmpvar_32.x = tmpvar_31.x;
  tmpvar_32.y = tmpvar_31.y;
  tmpvar_30 = (texture2DLod (_NormalMap, tmpvar_32.xy, 0.0) - 0.5);
  vec4 tmpvar_33;
  vec2 tmpvar_34;
  tmpvar_34 = (tmpvar_5.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.zx / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_35;
  tmpvar_35.zw = vec2(0.0, 0.0);
  tmpvar_35.x = tmpvar_34.x;
  tmpvar_35.y = tmpvar_34.y;
  tmpvar_33 = (texture2DLod (_NormalMap, tmpvar_35.xy, 0.0) - 0.5);
  vec4 tmpvar_36;
  vec2 tmpvar_37;
  tmpvar_37 = (tmpvar_5.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.xy / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_38;
  tmpvar_38.zw = vec2(0.0, 0.0);
  tmpvar_38.x = tmpvar_37.x;
  tmpvar_38.y = tmpvar_37.y;
  tmpvar_36 = (texture2DLod (_NormalMap, tmpvar_38.xy, 0.0) - 0.5);
  vec3 tmpvar_39;
  tmpvar_39.x = 0.0;
  tmpvar_39.y = tmpvar_30.x;
  tmpvar_39.z = tmpvar_30.y;
  vec3 tmpvar_40;
  tmpvar_40.y = 0.0;
  tmpvar_40.x = tmpvar_33.y;
  tmpvar_40.z = tmpvar_33.x;
  vec3 tmpvar_41;
  tmpvar_41.z = 0.0;
  tmpvar_41.x = tmpvar_36.x;
  tmpvar_41.y = tmpvar_36.y;
  vec4 tmpvar_42;
  vec2 tmpvar_43;
  tmpvar_43 = (tmpvar_8.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.yz / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_44;
  tmpvar_44.zw = vec2(0.0, 0.0);
  tmpvar_44.x = tmpvar_43.x;
  tmpvar_44.y = tmpvar_43.y;
  tmpvar_42 = (texture2DLod (_NormalMap, tmpvar_44.xy, 0.0) - 0.5);
  vec4 tmpvar_45;
  vec2 tmpvar_46;
  tmpvar_46 = (tmpvar_8.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.zx / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_47;
  tmpvar_47.zw = vec2(0.0, 0.0);
  tmpvar_47.x = tmpvar_46.x;
  tmpvar_47.y = tmpvar_46.y;
  tmpvar_45 = (texture2DLod (_NormalMap, tmpvar_47.xy, 0.0) - 0.5);
  vec4 tmpvar_48;
  vec2 tmpvar_49;
  tmpvar_49 = (tmpvar_8.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.xy / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_50;
  tmpvar_50.zw = vec2(0.0, 0.0);
  tmpvar_50.x = tmpvar_49.x;
  tmpvar_50.y = tmpvar_49.y;
  tmpvar_48 = (texture2DLod (_NormalMap, tmpvar_50.xy, 0.0) - 0.5);
  vec3 tmpvar_51;
  tmpvar_51.x = 0.0;
  tmpvar_51.y = tmpvar_42.x;
  tmpvar_51.z = tmpvar_42.y;
  vec3 tmpvar_52;
  tmpvar_52.y = 0.0;
  tmpvar_52.x = tmpvar_45.y;
  tmpvar_52.z = tmpvar_45.x;
  vec3 tmpvar_53;
  tmpvar_53.z = 0.0;
  tmpvar_53.x = tmpvar_48.x;
  tmpvar_53.y = tmpvar_48.y;
  vec4 tmpvar_54;
  vec2 tmpvar_55;
  tmpvar_55 = (tmpvar_11.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.yz / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_56;
  tmpvar_56.zw = vec2(0.0, 0.0);
  tmpvar_56.x = tmpvar_55.x;
  tmpvar_56.y = tmpvar_55.y;
  tmpvar_54 = (texture2DLod (_NormalMap, tmpvar_56.xy, 0.0) - 0.5);
  vec4 tmpvar_57;
  vec2 tmpvar_58;
  tmpvar_58 = (tmpvar_11.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.zx / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_59;
  tmpvar_59.zw = vec2(0.0, 0.0);
  tmpvar_59.x = tmpvar_58.x;
  tmpvar_59.y = tmpvar_58.y;
  tmpvar_57 = (texture2DLod (_NormalMap, tmpvar_59.xy, 0.0) - 0.5);
  vec4 tmpvar_60;
  vec2 tmpvar_61;
  tmpvar_61 = (tmpvar_11.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.xy / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_62;
  tmpvar_62.zw = vec2(0.0, 0.0);
  tmpvar_62.x = tmpvar_61.x;
  tmpvar_62.y = tmpvar_61.y;
  tmpvar_60 = (texture2DLod (_NormalMap, tmpvar_62.xy, 0.0) - 0.5);
  vec3 tmpvar_63;
  tmpvar_63.x = 0.0;
  tmpvar_63.y = tmpvar_54.x;
  tmpvar_63.z = tmpvar_54.y;
  vec3 tmpvar_64;
  tmpvar_64.y = 0.0;
  tmpvar_64.x = tmpvar_57.y;
  tmpvar_64.z = tmpvar_57.x;
  vec3 tmpvar_65;
  tmpvar_65.z = 0.0;
  tmpvar_65.x = tmpvar_60.x;
  tmpvar_65.y = tmpvar_60.y;
  vec3 tmpvar_66;
  tmpvar_66 = ((((xlv_TEXCOORD4.x * (((tmpvar_39 * tmpvar_2.xxx) + (tmpvar_40 * tmpvar_2.yyy)) + (tmpvar_41 * tmpvar_2.zzz))) * 2.0) + ((xlv_TEXCOORD4.y * (((tmpvar_51 * tmpvar_2.xxx) + (tmpvar_52 * tmpvar_2.yyy)) + (tmpvar_53 * tmpvar_2.zzz))) * 2.0)) + ((xlv_TEXCOORD4.z * (((tmpvar_63 * tmpvar_2.xxx) + (tmpvar_64 * tmpvar_2.yyy)) + (tmpvar_65 * tmpvar_2.zzz))) * 2.0));
  vec3 tmpvar_67;
  tmpvar_67 = ((((((xlv_TEXCOORD4.x * (((texture2DLod (_DiffuseTex, tmpvar_13.xy, 0.0) * tmpvar_2.z) + (texture2DLod (_DiffuseTex, tmpvar_15.xy, 0.0) * tmpvar_2.x)) + (texture2DLod (_DiffuseTex, tmpvar_17.xy, 0.0) * tmpvar_2.y))) + (xlv_TEXCOORD4.y * (((texture2DLod (_DiffuseTex, tmpvar_19.xy, 0.0) * tmpvar_2.z) + (texture2DLod (_DiffuseTex, tmpvar_21.xy, 0.0) * tmpvar_2.x)) + (texture2DLod (_DiffuseTex, tmpvar_23.xy, 0.0) * tmpvar_2.y)))) + (xlv_TEXCOORD4.z * (((texture2DLod (_DiffuseTex, tmpvar_25.xy, 0.0) * tmpvar_2.z) + (texture2DLod (_DiffuseTex, tmpvar_27.xy, 0.0) * tmpvar_2.x)) + (texture2DLod (_DiffuseTex, tmpvar_29.xy, 0.0) * tmpvar_2.y)))).xyz * xlv_COLOR0.xyz) * (1.0 - xlv_COLOR0.w)) + (xlv_COLOR0.xyz * xlv_COLOR0.w));
  vec4 tmpvar_68;
  tmpvar_68.w = 1.0;
  tmpvar_68.x = tmpvar_66.x;
  tmpvar_68.y = tmpvar_66.y;
  tmpvar_68.z = tmpvar_66.z;
  vec3 tmpvar_69;
  tmpvar_69 = normalize ((xlv_TEXCOORD0 - (normalize (tmpvar_68).xyz * xlv_TEXCOORD3.w)));
  float tmpvar_70;
  tmpvar_70 = (xlv_TEXCOORD4.w * _SpecPower);
  vec4 tmpvar_71;
  tmpvar_71.w = 0.0;
  tmpvar_71.xyz = xlv_TEXCOORD3.xyz;
  vec4 tmpvar_72;
  tmpvar_72.yzw = vec3(0.1, 0.0, 0.0);
  tmpvar_72.x = (dot (xlv_TEXCOORD4, tmpvar_71) / 255.0);
  vec4 tmpvar_73;
  tmpvar_73 = texture2DLod (_SpecColorTex, tmpvar_72.xy, 0.0);
  vec3 tmpvar_74;
  tmpvar_74 = normalize (xlv_TEXCOORD7);
  vec4 c_i0;
  float m;
  m = tmpvar_73.x;
  if ((tmpvar_73.y > tmpvar_73.x)) {
    m = tmpvar_73.y;
  };
  if ((tmpvar_73.z > m)) {
    m = tmpvar_73.z;
  };
  if ((m > 0.03)) {
    float tmpvar_75;
    tmpvar_75 = (((((tmpvar_74.x * tmpvar_74.x) * tmpvar_74.y) + (tmpvar_74.z * tmpvar_74.x)) + (tmpvar_74.y * tmpvar_74.z)) * _DiamondSpecParam1);
    vec4 tmpvar_76;
    tmpvar_76.zw = vec2(0.0, 0.0);
    tmpvar_76.x = tmpvar_75;
    tmpvar_76.y = _DiamondSpecParam2;
    xlat_mutable__SpecColor.xyz = texture2DLod (_DiamondSpecTex, tmpvar_76.xy, 0.0).xyz;
    vec4 tmpvar_77;
    tmpvar_77.zw = vec2(0.0, 0.0);
    tmpvar_77.x = tmpvar_75;
    tmpvar_77.y = _DiamondSpecParam3;
    xlat_mutable__LightColor0.xyz = (_LightColor0.xyz * texture2DLod (_DiamondSpecTex, tmpvar_77.xy, 0.0).xyz);
  } else {
    if ((m > 0.0)) {
      xlat_mutable__SpecColor.xyz = (tmpvar_73.xyz * (1.0/(m)));
    } else {
      xlat_mutable__SpecColor.xyz = vec3(1.0, 1.0, 1.0);
    };
  };
  c_i0 = vec4(0.0, 0.0, 0.0, 0.0);
  c_i0.xyz = (((tmpvar_67 * xlat_mutable__LightColor0.xyz) * dot (tmpvar_69, xlv_TEXCOORD6)) + ((xlat_mutable__LightColor0.xyz * xlat_mutable__SpecColor.xyz) * (smoothstep (0.0, 1.0, pow (max (0.0, dot (tmpvar_69, normalize ((xlv_TEXCOORD6 + tmpvar_74)))), (32.0 * tmpvar_70))) * tmpvar_70)));
  c_i0.w = 1.0;
  c = c_i0;
  c.w = 0.0;
  gl_FragData[0] = c;
}


#endif
"
}

SubProgram "d3d9 " {
Keywords { "DIRECTIONAL" }
Bind "vertex" Vertex
Bind "texcoord" TexCoord0
Bind "texcoord1" TexCoord1
Bind "color" Color
Matrix 0 [glstate_matrix_mvp]
Vector 8 [unity_Scale]
Vector 9 [_WorldSpaceCameraPos]
Vector 10 [_WorldSpaceLightPos0]
Matrix 4 [_Object2World]
Float 11 [_NormalPower]
SetTexture 0 [_NormSpecPowerTex] 2D
"vs_3_0
; 104 ALU, 2 TEX
dcl_position o0
dcl_texcoord0 o1
dcl_color0 o2
dcl_texcoord1 o3
dcl_texcoord2 o4
dcl_texcoord3 o5
dcl_texcoord4 o6
dcl_texcoord5 o7
dcl_texcoord6 o8
dcl_texcoord7 o9
def c12, 0.25000000, 0.00000000, 1.00000000, 0.00390625
def c13, 256.00000000, 0.50000000, 10.00000000, 0.49990001
def c14, -0.49990001, 2.00000000, 0.00392157, 4.00000000
def c15, 0.10000000, 0.00000000, 16.00000000, -8.00000000
def c16, -2.00000000, 0.05000000, 0, 0
dcl_position0 v0
dcl_texcoord0 v3
dcl_texcoord1 v4
dcl_color0 v5
dcl_2d s0
abs r0.y, v4
frc r0.z, r0.y
slt r0.x, v4.y, c12.y
max r0.x, -r0, r0
slt r0.x, c12.y, r0
add r0.y, r0, -r0.z
add r0.w, -r0.x, c12.z
mul r0.z, r0.y, r0.w
mad r0.z, r0.x, -r0.y, r0
add r0.x, v4.y, -r0.z
mul r0.x, r0, c13.z
mul r1.y, r0.z, c12.w
slt r0.y, r0.x, c14.x
add r0.w, r0.x, c13
max r0.x, -r0.y, r0.y
abs r0.y, r0.w
slt r0.x, c12.y, r0
frc r0.w, r0.y
add r0.y, r0, -r0.w
add r1.x, -r0, c12.z
mul r0.w, r0.y, r1.x
mad r0.w, r0.x, -r0.y, r0
sge r0.y, c12, r0.w
sge r0.x, r0.w, c12.y
mul r0.x, r0, r0.y
sge r1.x, c12.z, r0.w
sge r0.y, r0.w, c12.z
mul r0.y, r0, r1.x
sge r1.x, r0.w, c14.y
sge r0.w, c14.y, r0
mul r0.z, r1.x, r0.w
abs r1.x, r1.y
frc r1.z, r1.x
add r1.x, r1, -r1.z
slt r0.w, r1.y, c12.y
frc r1.z, r1.y
mul r1.z, r1, c13.x
max r0.w, -r0, r0
slt r0.w, c12.y, r0
add r1.y, -r0.w, c12.z
mul r1.y, r1.x, r1
mad r1.y, r0.w, -r1.x, r1
abs r1.w, r1.z
add r1.x, r1.w, c13.y
mul r1.w, v4.x, c12.x
slt r0.w, r1.z, c12.y
frc r1.z, r1.x
slt r2.x, r1.w, c12.y
max r0.w, -r0, r0
add r1.x, r1, -r1.z
slt r0.w, c12.y, r0
add r1.z, -r0.w, c12
mul r1.z, r1.x, r1
mad r1.z, r0.w, -r1.x, r1
abs r2.y, r1.w
max r2.x, -r2, r2
slt r1.w, c12.y, r2.x
frc r2.x, r2.y
add r2.x, r2.y, -r2
add r2.z, -r1.w, c12
mul r2.y, r2.x, r2.z
mad r1.x, r1.w, -r2, r2.y
dp3 r0.w, r0, r1
mul r2.x, r0.w, c14.z
mov r2.yz, c15.xxyw
texldl r2.xy, r2.xyzz, s0
mad r0.w, r2.x, c15.z, c15
mul r1.w, r0, c11.x
mul r0.w, r2.y, c14
mov o6, r0
mad r0.x, -r1, c14.w, v4
add r0.z, r0.x, c16.x
mov r0.xy, v3
mov o5, r1
mul r1.xyz, r0, c8.w
dp3 o1.z, r0, c6
dp3 o1.y, r0, c5
dp3 o1.x, r0, c4
mov o4.xyz, r0
dp4 r0.z, v0, c6
dp4 r0.x, v0, c4
dp4 r0.y, v0, c5
mov o2, v5
mul o3.xyz, v0, c16.y
dp3 o7.z, r1, c6
dp3 o7.y, r1, c5
dp3 o7.x, r1, c4
mov o8.xyz, c10
add o9.xyz, -r0, c9
dp4 o0.w, v0, c3
dp4 o0.z, v0, c2
dp4 o0.y, v0, c1
dp4 o0.x, v0, c0
"
}

SubProgram "opengl " {
Keywords { "SPOT" }
"!!GLSL
#ifdef VERTEX
#define SPOT 1
#define SHADER_API_OPENGL 1
#define SHADER_API_DESKTOP 1
vec4 xll_tex2Dlod(sampler2D s, vec4 coord) {
   return texture2DLod( s, coord.xy, coord.w);
}
mat3 xll_constructMat3( mat4 m) {
  return mat3( vec3( m[0]), vec3( m[1]), vec3( m[2]));
}
struct v2f_vertex_lit {
    vec2 uv;
    vec4 diff;
    vec4 spec;
};
struct v2f_img {
    vec4 pos;
    vec2 uv;
};
struct appdata_img {
    vec4 vertex;
    vec2 texcoord;
};
struct SurfaceOutput {
    vec3 Albedo;
    vec3 Normal;
    vec3 Emission;
    float Specular;
    float Gloss;
    float Alpha;
};
struct CustomSurfaceOutput {
    vec3 Albedo;
    vec3 Normal;
    vec3 Normal_;
    vec3 Emission;
    float Specular;
    float Alpha;
};
struct appdata_full {
    vec4 vertex;
    vec4 tangent;
    vec3 normal;
    vec4 texcoord;
    vec4 texcoord1;
    vec4 color;
};
struct Input {
    vec3 localPos;
    vec3 localNormal;
    vec3 worldNormal;
    vec4 weights;
    vec4 blendMask;
    vec4 color;
};
struct v2f_surf {
    vec4 pos;
    vec3 worldNormal;
    vec4 color;
    vec3 cust_localPos;
    vec3 cust_localNormal;
    vec4 cust_weights;
    vec4 cust_blendMask;
    vec3 normal;
    vec3 lightDir;
    vec3 viewDir;
    vec4 _LightCoord;
};
uniform mat4 _LightMatrix0;
uniform sampler2D _NormSpecPowerTex;
uniform float _NormalPower;
uniform mat4 _Object2World;
uniform vec3 _WorldSpaceCameraPos;
uniform vec4 _WorldSpaceLightPos0;

uniform vec4 unity_Scale;
void vert( inout appdata_full v, out Input o );
vec3 WorldSpaceViewDir( in vec4 v );
vec3 WorldSpaceLightDir( in vec4 v );
v2f_surf vert_surf( in appdata_full v );
void vert( inout appdata_full v, out Input o ) {
    int mat0;
    int mat12;
    vec3 matIdx;
    int idx;
    vec2 normSpec;
    mat0 = int( (v.texcoord1.x  / 4.00000) );
    mat12 = int( v.texcoord1.y  );
    matIdx = vec3( float( mat0 ), float( (mat12 / 256) ), float( (mat12 % 256) ));
    o.weights.xyz  = matIdx;
    idx = int( (((v.texcoord1.y  - float( mat12 )) * 10.0000) + 0.499900) );
    o.blendMask.xyz  = vec3( float( (idx == 0) ), float( (idx == 1) ), float( (idx == 2) ));
    normSpec = xll_tex2Dlod( _NormSpecPowerTex, vec4( (dot( o.blendMask, vec4( matIdx, 0.000000)) / 255.000), 0.100000, 0.000000, 0.000000)).xy ;
    o.weights.w  = (((normSpec.x  * 16.0000) - 8.00000) * _NormalPower);
    o.blendMask.w  = (normSpec.y  * 4.00000);
    v.normal = vec3( v.texcoord.x , v.texcoord.y , ((v.texcoord1.x  - float( (4 * mat0) )) - 2.00000));
    o.localPos = vec3( (v.vertex * 0.0500000));
    o.localNormal = v.normal;
}
vec3 WorldSpaceViewDir( in vec4 v ) {
    return (_WorldSpaceCameraPos.xyz  - ( _Object2World * v ).xyz );
}
vec3 WorldSpaceLightDir( in vec4 v ) {
    vec3 worldPos;
    worldPos = ( _Object2World * v ).xyz ;
    return (_WorldSpaceLightPos0.xyz  - worldPos);
}
v2f_surf vert_surf( in appdata_full v ) {
    Input customInputData;
    v2f_surf o;
    vec3 lightDir;
    vec3 viewDirForLight;
    vert( v, customInputData);
    o.cust_localPos = customInputData.localPos;
    o.cust_localNormal = customInputData.localNormal;
    o.cust_weights = customInputData.weights;
    o.cust_blendMask = customInputData.blendMask;
    o.pos = ( gl_ModelViewProjectionMatrix * v.vertex );
    o.worldNormal = ( xll_constructMat3( _Object2World) * v.normal );
    o.color = v.color;
    o.normal = ( xll_constructMat3( _Object2World) * (v.normal * unity_Scale.w ) );
    lightDir = WorldSpaceLightDir( v.vertex);
    o.lightDir = lightDir;
    viewDirForLight = WorldSpaceViewDir( v.vertex);
    o.viewDir = viewDirForLight;
    o._LightCoord = ( _LightMatrix0 * ( _Object2World * v.vertex ) );
    return o;
}
attribute vec4 TANGENT;
varying vec3 xlv_TEXCOORD0;
varying vec4 xlv_COLOR0;
varying vec3 xlv_TEXCOORD1;
varying vec3 xlv_TEXCOORD2;
varying vec4 xlv_TEXCOORD3;
varying vec4 xlv_TEXCOORD4;
varying vec3 xlv_TEXCOORD5;
varying vec3 xlv_TEXCOORD6;
varying vec3 xlv_TEXCOORD7;
varying vec4 xlv_TEXCOORD8;
void main() {
    v2f_surf xl_retval;
    appdata_full xlt_v;
    xlt_v.vertex = vec4( gl_Vertex);
    xlt_v.tangent = vec4( TANGENT);
    xlt_v.normal = vec3( gl_Normal);
    xlt_v.texcoord = vec4( gl_MultiTexCoord0);
    xlt_v.texcoord1 = vec4( gl_MultiTexCoord1);
    xlt_v.color = vec4( gl_Color);
    xl_retval = vert_surf( xlt_v);
    gl_Position = vec4( xl_retval.pos);
    xlv_TEXCOORD0 = vec3( xl_retval.worldNormal);
    xlv_COLOR0 = vec4( xl_retval.color);
    xlv_TEXCOORD1 = vec3( xl_retval.cust_localPos);
    xlv_TEXCOORD2 = vec3( xl_retval.cust_localNormal);
    xlv_TEXCOORD3 = vec4( xl_retval.cust_weights);
    xlv_TEXCOORD4 = vec4( xl_retval.cust_blendMask);
    xlv_TEXCOORD5 = vec3( xl_retval.normal);
    xlv_TEXCOORD6 = vec3( xl_retval.lightDir);
    xlv_TEXCOORD7 = vec3( xl_retval.viewDir);
    xlv_TEXCOORD8 = vec4( xl_retval._LightCoord);
}
/* NOTE: GLSL optimization failed
0:88(75): error: operator '%' is reserved in (null)
0:88(56): error: cannot construct `float' from a non-numeric data type
0:88(11): error: cannot construct `vec3' from a non-numeric data type
*/

#endif
#ifdef FRAGMENT
#extension GL_ARB_shader_texture_lod : enable
varying vec4 xlv_TEXCOORD8;
varying vec3 xlv_TEXCOORD7;
varying vec3 xlv_TEXCOORD6;
varying vec4 xlv_TEXCOORD4;
varying vec4 xlv_TEXCOORD3;
varying vec3 xlv_TEXCOORD2;
varying vec3 xlv_TEXCOORD1;
varying vec4 xlv_COLOR0;
varying vec3 xlv_TEXCOORD0;
vec4 xlat_mutable__SpecColor;
uniform vec4 _SpecColor;
vec4 xlat_mutable__LightColor0;
uniform vec4 _LightColor0;
uniform float _TileSize;
uniform float _SpecPower;
uniform sampler2D _SpecColorTex;
uniform float _Repetition;
uniform sampler2D _NormalMap;
uniform sampler2D _LightTextureB0;
uniform sampler2D _LightTexture0;
uniform float _Indent;
uniform sampler2D _DiffuseTex;
uniform sampler2D _DiamondSpecTex;
uniform float _DiamondSpecParam3;
uniform float _DiamondSpecParam2;
uniform float _DiamondSpecParam1;
void main ()
{
  xlat_mutable__SpecColor = _SpecColor;
  xlat_mutable__LightColor0 = _LightColor0;
  vec4 c;
  vec3 tmpvar_1;
  tmpvar_1 = max (((normalize (abs (xlv_TEXCOORD2)) - 0.2) * 7.0), vec3(0.0, 0.0, 0.0));
  vec3 tmpvar_2;
  tmpvar_2 = (tmpvar_1 / vec3(((tmpvar_1.x + tmpvar_1.y) + tmpvar_1.z)));
  float tmpvar_3;
  tmpvar_3 = (float(int(floor ((xlv_TEXCOORD3.x + 0.5)))) * _TileSize);
  float tmpvar_4;
  tmpvar_4 = (floor (tmpvar_3) * _TileSize);
  vec3 tmpvar_5;
  tmpvar_5.x = fract (tmpvar_3);
  tmpvar_5.y = fract (tmpvar_4);
  tmpvar_5.z = floor (tmpvar_4);
  float tmpvar_6;
  tmpvar_6 = (float(int(floor ((xlv_TEXCOORD3.y + 0.5)))) * _TileSize);
  float tmpvar_7;
  tmpvar_7 = (floor (tmpvar_6) * _TileSize);
  vec3 tmpvar_8;
  tmpvar_8.x = fract (tmpvar_6);
  tmpvar_8.y = fract (tmpvar_7);
  tmpvar_8.z = floor (tmpvar_7);
  float tmpvar_9;
  tmpvar_9 = (float(int(floor ((xlv_TEXCOORD3.z + 0.5)))) * _TileSize);
  float tmpvar_10;
  tmpvar_10 = (floor (tmpvar_9) * _TileSize);
  vec3 tmpvar_11;
  tmpvar_11.x = fract (tmpvar_9);
  tmpvar_11.y = fract (tmpvar_10);
  tmpvar_11.z = floor (tmpvar_10);
  vec2 tmpvar_12;
  tmpvar_12 = (tmpvar_5.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.xy / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_13;
  tmpvar_13.zw = vec2(0.0, 0.0);
  tmpvar_13.x = tmpvar_12.x;
  tmpvar_13.y = tmpvar_12.y;
  vec2 tmpvar_14;
  tmpvar_14 = (tmpvar_5.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.yz / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_15;
  tmpvar_15.zw = vec2(0.0, 0.0);
  tmpvar_15.x = tmpvar_14.x;
  tmpvar_15.y = tmpvar_14.y;
  vec2 tmpvar_16;
  tmpvar_16 = (tmpvar_5.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.zx / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_17;
  tmpvar_17.zw = vec2(0.0, 0.0);
  tmpvar_17.x = tmpvar_16.x;
  tmpvar_17.y = tmpvar_16.y;
  vec2 tmpvar_18;
  tmpvar_18 = (tmpvar_8.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.xy / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_19;
  tmpvar_19.zw = vec2(0.0, 0.0);
  tmpvar_19.x = tmpvar_18.x;
  tmpvar_19.y = tmpvar_18.y;
  vec2 tmpvar_20;
  tmpvar_20 = (tmpvar_8.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.yz / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_21;
  tmpvar_21.zw = vec2(0.0, 0.0);
  tmpvar_21.x = tmpvar_20.x;
  tmpvar_21.y = tmpvar_20.y;
  vec2 tmpvar_22;
  tmpvar_22 = (tmpvar_8.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.zx / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_23;
  tmpvar_23.zw = vec2(0.0, 0.0);
  tmpvar_23.x = tmpvar_22.x;
  tmpvar_23.y = tmpvar_22.y;
  vec2 tmpvar_24;
  tmpvar_24 = (tmpvar_11.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.xy / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_25;
  tmpvar_25.zw = vec2(0.0, 0.0);
  tmpvar_25.x = tmpvar_24.x;
  tmpvar_25.y = tmpvar_24.y;
  vec2 tmpvar_26;
  tmpvar_26 = (tmpvar_11.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.yz / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_27;
  tmpvar_27.zw = vec2(0.0, 0.0);
  tmpvar_27.x = tmpvar_26.x;
  tmpvar_27.y = tmpvar_26.y;
  vec2 tmpvar_28;
  tmpvar_28 = (tmpvar_11.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.zx / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_29;
  tmpvar_29.zw = vec2(0.0, 0.0);
  tmpvar_29.x = tmpvar_28.x;
  tmpvar_29.y = tmpvar_28.y;
  vec4 tmpvar_30;
  vec2 tmpvar_31;
  tmpvar_31 = (tmpvar_5.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.yz / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_32;
  tmpvar_32.zw = vec2(0.0, 0.0);
  tmpvar_32.x = tmpvar_31.x;
  tmpvar_32.y = tmpvar_31.y;
  tmpvar_30 = (texture2DLod (_NormalMap, tmpvar_32.xy, 0.0) - 0.5);
  vec4 tmpvar_33;
  vec2 tmpvar_34;
  tmpvar_34 = (tmpvar_5.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.zx / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_35;
  tmpvar_35.zw = vec2(0.0, 0.0);
  tmpvar_35.x = tmpvar_34.x;
  tmpvar_35.y = tmpvar_34.y;
  tmpvar_33 = (texture2DLod (_NormalMap, tmpvar_35.xy, 0.0) - 0.5);
  vec4 tmpvar_36;
  vec2 tmpvar_37;
  tmpvar_37 = (tmpvar_5.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.xy / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_38;
  tmpvar_38.zw = vec2(0.0, 0.0);
  tmpvar_38.x = tmpvar_37.x;
  tmpvar_38.y = tmpvar_37.y;
  tmpvar_36 = (texture2DLod (_NormalMap, tmpvar_38.xy, 0.0) - 0.5);
  vec3 tmpvar_39;
  tmpvar_39.x = 0.0;
  tmpvar_39.y = tmpvar_30.x;
  tmpvar_39.z = tmpvar_30.y;
  vec3 tmpvar_40;
  tmpvar_40.y = 0.0;
  tmpvar_40.x = tmpvar_33.y;
  tmpvar_40.z = tmpvar_33.x;
  vec3 tmpvar_41;
  tmpvar_41.z = 0.0;
  tmpvar_41.x = tmpvar_36.x;
  tmpvar_41.y = tmpvar_36.y;
  vec4 tmpvar_42;
  vec2 tmpvar_43;
  tmpvar_43 = (tmpvar_8.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.yz / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_44;
  tmpvar_44.zw = vec2(0.0, 0.0);
  tmpvar_44.x = tmpvar_43.x;
  tmpvar_44.y = tmpvar_43.y;
  tmpvar_42 = (texture2DLod (_NormalMap, tmpvar_44.xy, 0.0) - 0.5);
  vec4 tmpvar_45;
  vec2 tmpvar_46;
  tmpvar_46 = (tmpvar_8.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.zx / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_47;
  tmpvar_47.zw = vec2(0.0, 0.0);
  tmpvar_47.x = tmpvar_46.x;
  tmpvar_47.y = tmpvar_46.y;
  tmpvar_45 = (texture2DLod (_NormalMap, tmpvar_47.xy, 0.0) - 0.5);
  vec4 tmpvar_48;
  vec2 tmpvar_49;
  tmpvar_49 = (tmpvar_8.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.xy / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_50;
  tmpvar_50.zw = vec2(0.0, 0.0);
  tmpvar_50.x = tmpvar_49.x;
  tmpvar_50.y = tmpvar_49.y;
  tmpvar_48 = (texture2DLod (_NormalMap, tmpvar_50.xy, 0.0) - 0.5);
  vec3 tmpvar_51;
  tmpvar_51.x = 0.0;
  tmpvar_51.y = tmpvar_42.x;
  tmpvar_51.z = tmpvar_42.y;
  vec3 tmpvar_52;
  tmpvar_52.y = 0.0;
  tmpvar_52.x = tmpvar_45.y;
  tmpvar_52.z = tmpvar_45.x;
  vec3 tmpvar_53;
  tmpvar_53.z = 0.0;
  tmpvar_53.x = tmpvar_48.x;
  tmpvar_53.y = tmpvar_48.y;
  vec4 tmpvar_54;
  vec2 tmpvar_55;
  tmpvar_55 = (tmpvar_11.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.yz / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_56;
  tmpvar_56.zw = vec2(0.0, 0.0);
  tmpvar_56.x = tmpvar_55.x;
  tmpvar_56.y = tmpvar_55.y;
  tmpvar_54 = (texture2DLod (_NormalMap, tmpvar_56.xy, 0.0) - 0.5);
  vec4 tmpvar_57;
  vec2 tmpvar_58;
  tmpvar_58 = (tmpvar_11.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.zx / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_59;
  tmpvar_59.zw = vec2(0.0, 0.0);
  tmpvar_59.x = tmpvar_58.x;
  tmpvar_59.y = tmpvar_58.y;
  tmpvar_57 = (texture2DLod (_NormalMap, tmpvar_59.xy, 0.0) - 0.5);
  vec4 tmpvar_60;
  vec2 tmpvar_61;
  tmpvar_61 = (tmpvar_11.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.xy / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_62;
  tmpvar_62.zw = vec2(0.0, 0.0);
  tmpvar_62.x = tmpvar_61.x;
  tmpvar_62.y = tmpvar_61.y;
  tmpvar_60 = (texture2DLod (_NormalMap, tmpvar_62.xy, 0.0) - 0.5);
  vec3 tmpvar_63;
  tmpvar_63.x = 0.0;
  tmpvar_63.y = tmpvar_54.x;
  tmpvar_63.z = tmpvar_54.y;
  vec3 tmpvar_64;
  tmpvar_64.y = 0.0;
  tmpvar_64.x = tmpvar_57.y;
  tmpvar_64.z = tmpvar_57.x;
  vec3 tmpvar_65;
  tmpvar_65.z = 0.0;
  tmpvar_65.x = tmpvar_60.x;
  tmpvar_65.y = tmpvar_60.y;
  vec3 tmpvar_66;
  tmpvar_66 = ((((xlv_TEXCOORD4.x * (((tmpvar_39 * tmpvar_2.xxx) + (tmpvar_40 * tmpvar_2.yyy)) + (tmpvar_41 * tmpvar_2.zzz))) * 2.0) + ((xlv_TEXCOORD4.y * (((tmpvar_51 * tmpvar_2.xxx) + (tmpvar_52 * tmpvar_2.yyy)) + (tmpvar_53 * tmpvar_2.zzz))) * 2.0)) + ((xlv_TEXCOORD4.z * (((tmpvar_63 * tmpvar_2.xxx) + (tmpvar_64 * tmpvar_2.yyy)) + (tmpvar_65 * tmpvar_2.zzz))) * 2.0));
  vec3 tmpvar_67;
  tmpvar_67 = ((((((xlv_TEXCOORD4.x * (((texture2DLod (_DiffuseTex, tmpvar_13.xy, 0.0) * tmpvar_2.z) + (texture2DLod (_DiffuseTex, tmpvar_15.xy, 0.0) * tmpvar_2.x)) + (texture2DLod (_DiffuseTex, tmpvar_17.xy, 0.0) * tmpvar_2.y))) + (xlv_TEXCOORD4.y * (((texture2DLod (_DiffuseTex, tmpvar_19.xy, 0.0) * tmpvar_2.z) + (texture2DLod (_DiffuseTex, tmpvar_21.xy, 0.0) * tmpvar_2.x)) + (texture2DLod (_DiffuseTex, tmpvar_23.xy, 0.0) * tmpvar_2.y)))) + (xlv_TEXCOORD4.z * (((texture2DLod (_DiffuseTex, tmpvar_25.xy, 0.0) * tmpvar_2.z) + (texture2DLod (_DiffuseTex, tmpvar_27.xy, 0.0) * tmpvar_2.x)) + (texture2DLod (_DiffuseTex, tmpvar_29.xy, 0.0) * tmpvar_2.y)))).xyz * xlv_COLOR0.xyz) * (1.0 - xlv_COLOR0.w)) + (xlv_COLOR0.xyz * xlv_COLOR0.w));
  vec4 tmpvar_68;
  tmpvar_68.w = 1.0;
  tmpvar_68.x = tmpvar_66.x;
  tmpvar_68.y = tmpvar_66.y;
  tmpvar_68.z = tmpvar_66.z;
  vec3 tmpvar_69;
  tmpvar_69 = normalize ((xlv_TEXCOORD0 - (normalize (tmpvar_68).xyz * xlv_TEXCOORD3.w)));
  float tmpvar_70;
  tmpvar_70 = (xlv_TEXCOORD4.w * _SpecPower);
  vec4 tmpvar_71;
  tmpvar_71.w = 0.0;
  tmpvar_71.xyz = xlv_TEXCOORD3.xyz;
  vec4 tmpvar_72;
  tmpvar_72.yzw = vec3(0.1, 0.0, 0.0);
  tmpvar_72.x = (dot (xlv_TEXCOORD4, tmpvar_71) / 255.0);
  vec4 tmpvar_73;
  tmpvar_73 = texture2DLod (_SpecColorTex, tmpvar_72.xy, 0.0);
  vec3 tmpvar_74;
  tmpvar_74 = normalize (xlv_TEXCOORD6);
  vec3 tmpvar_75;
  tmpvar_75 = normalize (xlv_TEXCOORD7);
  vec3 LightCoord_i0;
  LightCoord_i0 = xlv_TEXCOORD8.xyz;
  float atten;
  atten = ((float((xlv_TEXCOORD8.z > 0.0)) * texture2D (_LightTexture0, ((xlv_TEXCOORD8.xy / xlv_TEXCOORD8.w) + 0.5)).w) * texture2D (_LightTextureB0, vec2(dot (LightCoord_i0, LightCoord_i0))).w);
  vec4 c_i0;
  float m;
  m = tmpvar_73.x;
  if ((tmpvar_73.y > tmpvar_73.x)) {
    m = tmpvar_73.y;
  };
  if ((tmpvar_73.z > m)) {
    m = tmpvar_73.z;
  };
  if ((m > 0.03)) {
    float tmpvar_76;
    tmpvar_76 = (((((tmpvar_75.x * tmpvar_75.x) * tmpvar_75.y) + (tmpvar_75.z * tmpvar_75.x)) + (tmpvar_75.y * tmpvar_75.z)) * _DiamondSpecParam1);
    vec4 tmpvar_77;
    tmpvar_77.zw = vec2(0.0, 0.0);
    tmpvar_77.x = tmpvar_76;
    tmpvar_77.y = _DiamondSpecParam2;
    xlat_mutable__SpecColor.xyz = texture2DLod (_DiamondSpecTex, tmpvar_77.xy, 0.0).xyz;
    vec4 tmpvar_78;
    tmpvar_78.zw = vec2(0.0, 0.0);
    tmpvar_78.x = tmpvar_76;
    tmpvar_78.y = _DiamondSpecParam3;
    xlat_mutable__LightColor0.xyz = (_LightColor0.xyz * texture2DLod (_DiamondSpecTex, tmpvar_78.xy, 0.0).xyz);
  } else {
    if ((m > 0.0)) {
      xlat_mutable__SpecColor.xyz = (tmpvar_73.xyz * (1.0/(m)));
    } else {
      xlat_mutable__SpecColor.xyz = vec3(1.0, 1.0, 1.0);
    };
  };
  c_i0 = vec4(0.0, 0.0, 0.0, 0.0);
  c_i0.xyz = ((((tmpvar_67 * xlat_mutable__LightColor0.xyz) * dot (tmpvar_69, tmpvar_74)) + ((xlat_mutable__LightColor0.xyz * xlat_mutable__SpecColor.xyz) * (smoothstep (0.0, 1.0, pow (max (0.0, dot (tmpvar_69, normalize ((tmpvar_74 + tmpvar_75)))), (32.0 * tmpvar_70))) * tmpvar_70))) * atten);
  c_i0.w = 1.0;
  c = c_i0;
  c.w = 0.0;
  gl_FragData[0] = c;
}


#endif
"
}

SubProgram "d3d9 " {
Keywords { "SPOT" }
Bind "vertex" Vertex
Bind "texcoord" TexCoord0
Bind "texcoord1" TexCoord1
Bind "color" Color
Matrix 0 [glstate_matrix_mvp]
Vector 12 [unity_Scale]
Vector 13 [_WorldSpaceCameraPos]
Vector 14 [_WorldSpaceLightPos0]
Matrix 4 [_Object2World]
Matrix 8 [_LightMatrix0]
Float 15 [_NormalPower]
SetTexture 0 [_NormSpecPowerTex] 2D
"vs_3_0
; 109 ALU, 2 TEX
dcl_position o0
dcl_texcoord0 o1
dcl_color0 o2
dcl_texcoord1 o3
dcl_texcoord2 o4
dcl_texcoord3 o5
dcl_texcoord4 o6
dcl_texcoord5 o7
dcl_texcoord6 o8
dcl_texcoord7 o9
dcl_texcoord8 o10
def c16, 0.25000000, 0.00000000, 1.00000000, 0.00390625
def c17, 256.00000000, 0.50000000, 10.00000000, 0.49990001
def c18, -0.49990001, 2.00000000, 0.00392157, 4.00000000
def c19, 0.10000000, 0.00000000, 16.00000000, -8.00000000
def c20, -2.00000000, 0.05000000, 0, 0
dcl_position0 v0
dcl_texcoord0 v3
dcl_texcoord1 v4
dcl_color0 v5
dcl_2d s0
abs r0.y, v4
frc r0.z, r0.y
slt r0.x, v4.y, c16.y
max r0.x, -r0, r0
slt r0.x, c16.y, r0
add r0.y, r0, -r0.z
add r0.w, -r0.x, c16.z
mul r0.z, r0.y, r0.w
mad r0.z, r0.x, -r0.y, r0
add r0.x, v4.y, -r0.z
mul r0.x, r0, c17.z
mul r1.y, r0.z, c16.w
slt r0.y, r0.x, c18.x
add r0.w, r0.x, c17
max r0.x, -r0.y, r0.y
abs r0.y, r0.w
slt r0.x, c16.y, r0
frc r0.w, r0.y
add r0.y, r0, -r0.w
add r1.x, -r0, c16.z
mul r0.w, r0.y, r1.x
mad r0.w, r0.x, -r0.y, r0
sge r0.y, c16, r0.w
sge r0.x, r0.w, c16.y
mul r0.x, r0, r0.y
sge r1.x, c16.z, r0.w
sge r0.y, r0.w, c16.z
mul r0.y, r0, r1.x
sge r1.x, r0.w, c18.y
sge r0.w, c18.y, r0
mul r0.z, r1.x, r0.w
abs r1.x, r1.y
frc r1.z, r1.x
add r1.x, r1, -r1.z
slt r0.w, r1.y, c16.y
frc r1.z, r1.y
mul r1.z, r1, c17.x
max r0.w, -r0, r0
slt r0.w, c16.y, r0
add r1.y, -r0.w, c16.z
mul r1.y, r1.x, r1
mad r1.y, r0.w, -r1.x, r1
abs r1.w, r1.z
add r1.x, r1.w, c17.y
mul r1.w, v4.x, c16.x
slt r0.w, r1.z, c16.y
frc r1.z, r1.x
slt r2.x, r1.w, c16.y
max r0.w, -r0, r0
add r1.x, r1, -r1.z
slt r0.w, c16.y, r0
add r1.z, -r0.w, c16
mul r1.z, r1.x, r1
mad r1.z, r0.w, -r1.x, r1
abs r2.y, r1.w
max r2.x, -r2, r2
slt r1.w, c16.y, r2.x
frc r2.x, r2.y
add r2.x, r2.y, -r2
add r2.z, -r1.w, c16
mul r2.y, r2.x, r2.z
mad r1.x, r1.w, -r2, r2.y
dp3 r0.w, r0, r1
mov r2.yz, c19.xxyw
mul r2.x, r0.w, c18.z
texldl r2.xy, r2.xyzz, s0
mad r0.w, r2.x, c19.z, c19
mul r1.w, r0, c15.x
mul r0.w, r2.y, c18
mov o6, r0
mov o5, r1
mad r1.x, -r1, c18.w, v4
add r1.z, r1.x, c20.x
mov r1.xy, v3
mul r2.xyz, r1, c12.w
dp4 r0.z, v0, c6
dp4 r0.x, v0, c4
dp4 r0.y, v0, c5
dp4 r0.w, v0, c7
mov o2, v5
dp4 o10.w, r0, c11
dp4 o10.z, r0, c10
dp4 o10.y, r0, c9
dp4 o10.x, r0, c8
dp3 o1.z, r1, c6
dp3 o1.y, r1, c5
dp3 o1.x, r1, c4
mul o3.xyz, v0, c20.y
mov o4.xyz, r1
dp3 o7.z, r2, c6
dp3 o7.y, r2, c5
dp3 o7.x, r2, c4
add o8.xyz, -r0, c14
add o9.xyz, -r0, c13
dp4 o0.w, v0, c3
dp4 o0.z, v0, c2
dp4 o0.y, v0, c1
dp4 o0.x, v0, c0
"
}

SubProgram "opengl " {
Keywords { "POINT_COOKIE" }
"!!GLSL
#ifdef VERTEX
#define POINT_COOKIE 1
#define SHADER_API_OPENGL 1
#define SHADER_API_DESKTOP 1
vec4 xll_tex2Dlod(sampler2D s, vec4 coord) {
   return texture2DLod( s, coord.xy, coord.w);
}
mat3 xll_constructMat3( mat4 m) {
  return mat3( vec3( m[0]), vec3( m[1]), vec3( m[2]));
}
struct v2f_vertex_lit {
    vec2 uv;
    vec4 diff;
    vec4 spec;
};
struct v2f_img {
    vec4 pos;
    vec2 uv;
};
struct appdata_img {
    vec4 vertex;
    vec2 texcoord;
};
struct SurfaceOutput {
    vec3 Albedo;
    vec3 Normal;
    vec3 Emission;
    float Specular;
    float Gloss;
    float Alpha;
};
struct CustomSurfaceOutput {
    vec3 Albedo;
    vec3 Normal;
    vec3 Normal_;
    vec3 Emission;
    float Specular;
    float Alpha;
};
struct appdata_full {
    vec4 vertex;
    vec4 tangent;
    vec3 normal;
    vec4 texcoord;
    vec4 texcoord1;
    vec4 color;
};
struct Input {
    vec3 localPos;
    vec3 localNormal;
    vec3 worldNormal;
    vec4 weights;
    vec4 blendMask;
    vec4 color;
};
struct v2f_surf {
    vec4 pos;
    vec3 worldNormal;
    vec4 color;
    vec3 cust_localPos;
    vec3 cust_localNormal;
    vec4 cust_weights;
    vec4 cust_blendMask;
    vec3 normal;
    vec3 lightDir;
    vec3 viewDir;
    vec3 _LightCoord;
};
uniform mat4 _LightMatrix0;
uniform sampler2D _NormSpecPowerTex;
uniform float _NormalPower;
uniform mat4 _Object2World;
uniform vec3 _WorldSpaceCameraPos;
uniform vec4 _WorldSpaceLightPos0;

uniform vec4 unity_Scale;
void vert( inout appdata_full v, out Input o );
vec3 WorldSpaceViewDir( in vec4 v );
vec3 WorldSpaceLightDir( in vec4 v );
v2f_surf vert_surf( in appdata_full v );
void vert( inout appdata_full v, out Input o ) {
    int mat0;
    int mat12;
    vec3 matIdx;
    int idx;
    vec2 normSpec;
    mat0 = int( (v.texcoord1.x  / 4.00000) );
    mat12 = int( v.texcoord1.y  );
    matIdx = vec3( float( mat0 ), float( (mat12 / 256) ), float( (mat12 % 256) ));
    o.weights.xyz  = matIdx;
    idx = int( (((v.texcoord1.y  - float( mat12 )) * 10.0000) + 0.499900) );
    o.blendMask.xyz  = vec3( float( (idx == 0) ), float( (idx == 1) ), float( (idx == 2) ));
    normSpec = xll_tex2Dlod( _NormSpecPowerTex, vec4( (dot( o.blendMask, vec4( matIdx, 0.000000)) / 255.000), 0.100000, 0.000000, 0.000000)).xy ;
    o.weights.w  = (((normSpec.x  * 16.0000) - 8.00000) * _NormalPower);
    o.blendMask.w  = (normSpec.y  * 4.00000);
    v.normal = vec3( v.texcoord.x , v.texcoord.y , ((v.texcoord1.x  - float( (4 * mat0) )) - 2.00000));
    o.localPos = vec3( (v.vertex * 0.0500000));
    o.localNormal = v.normal;
}
vec3 WorldSpaceViewDir( in vec4 v ) {
    return (_WorldSpaceCameraPos.xyz  - ( _Object2World * v ).xyz );
}
vec3 WorldSpaceLightDir( in vec4 v ) {
    vec3 worldPos;
    worldPos = ( _Object2World * v ).xyz ;
    return (_WorldSpaceLightPos0.xyz  - worldPos);
}
v2f_surf vert_surf( in appdata_full v ) {
    Input customInputData;
    v2f_surf o;
    vec3 lightDir;
    vec3 viewDirForLight;
    vert( v, customInputData);
    o.cust_localPos = customInputData.localPos;
    o.cust_localNormal = customInputData.localNormal;
    o.cust_weights = customInputData.weights;
    o.cust_blendMask = customInputData.blendMask;
    o.pos = ( gl_ModelViewProjectionMatrix * v.vertex );
    o.worldNormal = ( xll_constructMat3( _Object2World) * v.normal );
    o.color = v.color;
    o.normal = ( xll_constructMat3( _Object2World) * (v.normal * unity_Scale.w ) );
    lightDir = WorldSpaceLightDir( v.vertex);
    o.lightDir = lightDir;
    viewDirForLight = WorldSpaceViewDir( v.vertex);
    o.viewDir = viewDirForLight;
    o._LightCoord = ( _LightMatrix0 * ( _Object2World * v.vertex ) ).xyz ;
    return o;
}
attribute vec4 TANGENT;
varying vec3 xlv_TEXCOORD0;
varying vec4 xlv_COLOR0;
varying vec3 xlv_TEXCOORD1;
varying vec3 xlv_TEXCOORD2;
varying vec4 xlv_TEXCOORD3;
varying vec4 xlv_TEXCOORD4;
varying vec3 xlv_TEXCOORD5;
varying vec3 xlv_TEXCOORD6;
varying vec3 xlv_TEXCOORD7;
varying vec3 xlv_TEXCOORD8;
void main() {
    v2f_surf xl_retval;
    appdata_full xlt_v;
    xlt_v.vertex = vec4( gl_Vertex);
    xlt_v.tangent = vec4( TANGENT);
    xlt_v.normal = vec3( gl_Normal);
    xlt_v.texcoord = vec4( gl_MultiTexCoord0);
    xlt_v.texcoord1 = vec4( gl_MultiTexCoord1);
    xlt_v.color = vec4( gl_Color);
    xl_retval = vert_surf( xlt_v);
    gl_Position = vec4( xl_retval.pos);
    xlv_TEXCOORD0 = vec3( xl_retval.worldNormal);
    xlv_COLOR0 = vec4( xl_retval.color);
    xlv_TEXCOORD1 = vec3( xl_retval.cust_localPos);
    xlv_TEXCOORD2 = vec3( xl_retval.cust_localNormal);
    xlv_TEXCOORD3 = vec4( xl_retval.cust_weights);
    xlv_TEXCOORD4 = vec4( xl_retval.cust_blendMask);
    xlv_TEXCOORD5 = vec3( xl_retval.normal);
    xlv_TEXCOORD6 = vec3( xl_retval.lightDir);
    xlv_TEXCOORD7 = vec3( xl_retval.viewDir);
    xlv_TEXCOORD8 = vec3( xl_retval._LightCoord);
}
/* NOTE: GLSL optimization failed
0:88(75): error: operator '%' is reserved in (null)
0:88(56): error: cannot construct `float' from a non-numeric data type
0:88(11): error: cannot construct `vec3' from a non-numeric data type
*/

#endif
#ifdef FRAGMENT
#extension GL_ARB_shader_texture_lod : enable
varying vec3 xlv_TEXCOORD8;
varying vec3 xlv_TEXCOORD7;
varying vec3 xlv_TEXCOORD6;
varying vec4 xlv_TEXCOORD4;
varying vec4 xlv_TEXCOORD3;
varying vec3 xlv_TEXCOORD2;
varying vec3 xlv_TEXCOORD1;
varying vec4 xlv_COLOR0;
varying vec3 xlv_TEXCOORD0;
vec4 xlat_mutable__SpecColor;
uniform vec4 _SpecColor;
vec4 xlat_mutable__LightColor0;
uniform vec4 _LightColor0;
uniform float _TileSize;
uniform float _SpecPower;
uniform sampler2D _SpecColorTex;
uniform float _Repetition;
uniform sampler2D _NormalMap;
uniform sampler2D _LightTextureB0;
uniform samplerCube _LightTexture0;
uniform float _Indent;
uniform sampler2D _DiffuseTex;
uniform sampler2D _DiamondSpecTex;
uniform float _DiamondSpecParam3;
uniform float _DiamondSpecParam2;
uniform float _DiamondSpecParam1;
void main ()
{
  xlat_mutable__SpecColor = _SpecColor;
  xlat_mutable__LightColor0 = _LightColor0;
  vec4 c;
  vec3 tmpvar_1;
  tmpvar_1 = max (((normalize (abs (xlv_TEXCOORD2)) - 0.2) * 7.0), vec3(0.0, 0.0, 0.0));
  vec3 tmpvar_2;
  tmpvar_2 = (tmpvar_1 / vec3(((tmpvar_1.x + tmpvar_1.y) + tmpvar_1.z)));
  float tmpvar_3;
  tmpvar_3 = (float(int(floor ((xlv_TEXCOORD3.x + 0.5)))) * _TileSize);
  float tmpvar_4;
  tmpvar_4 = (floor (tmpvar_3) * _TileSize);
  vec3 tmpvar_5;
  tmpvar_5.x = fract (tmpvar_3);
  tmpvar_5.y = fract (tmpvar_4);
  tmpvar_5.z = floor (tmpvar_4);
  float tmpvar_6;
  tmpvar_6 = (float(int(floor ((xlv_TEXCOORD3.y + 0.5)))) * _TileSize);
  float tmpvar_7;
  tmpvar_7 = (floor (tmpvar_6) * _TileSize);
  vec3 tmpvar_8;
  tmpvar_8.x = fract (tmpvar_6);
  tmpvar_8.y = fract (tmpvar_7);
  tmpvar_8.z = floor (tmpvar_7);
  float tmpvar_9;
  tmpvar_9 = (float(int(floor ((xlv_TEXCOORD3.z + 0.5)))) * _TileSize);
  float tmpvar_10;
  tmpvar_10 = (floor (tmpvar_9) * _TileSize);
  vec3 tmpvar_11;
  tmpvar_11.x = fract (tmpvar_9);
  tmpvar_11.y = fract (tmpvar_10);
  tmpvar_11.z = floor (tmpvar_10);
  vec2 tmpvar_12;
  tmpvar_12 = (tmpvar_5.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.xy / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_13;
  tmpvar_13.zw = vec2(0.0, 0.0);
  tmpvar_13.x = tmpvar_12.x;
  tmpvar_13.y = tmpvar_12.y;
  vec2 tmpvar_14;
  tmpvar_14 = (tmpvar_5.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.yz / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_15;
  tmpvar_15.zw = vec2(0.0, 0.0);
  tmpvar_15.x = tmpvar_14.x;
  tmpvar_15.y = tmpvar_14.y;
  vec2 tmpvar_16;
  tmpvar_16 = (tmpvar_5.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.zx / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_17;
  tmpvar_17.zw = vec2(0.0, 0.0);
  tmpvar_17.x = tmpvar_16.x;
  tmpvar_17.y = tmpvar_16.y;
  vec2 tmpvar_18;
  tmpvar_18 = (tmpvar_8.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.xy / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_19;
  tmpvar_19.zw = vec2(0.0, 0.0);
  tmpvar_19.x = tmpvar_18.x;
  tmpvar_19.y = tmpvar_18.y;
  vec2 tmpvar_20;
  tmpvar_20 = (tmpvar_8.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.yz / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_21;
  tmpvar_21.zw = vec2(0.0, 0.0);
  tmpvar_21.x = tmpvar_20.x;
  tmpvar_21.y = tmpvar_20.y;
  vec2 tmpvar_22;
  tmpvar_22 = (tmpvar_8.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.zx / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_23;
  tmpvar_23.zw = vec2(0.0, 0.0);
  tmpvar_23.x = tmpvar_22.x;
  tmpvar_23.y = tmpvar_22.y;
  vec2 tmpvar_24;
  tmpvar_24 = (tmpvar_11.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.xy / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_25;
  tmpvar_25.zw = vec2(0.0, 0.0);
  tmpvar_25.x = tmpvar_24.x;
  tmpvar_25.y = tmpvar_24.y;
  vec2 tmpvar_26;
  tmpvar_26 = (tmpvar_11.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.yz / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_27;
  tmpvar_27.zw = vec2(0.0, 0.0);
  tmpvar_27.x = tmpvar_26.x;
  tmpvar_27.y = tmpvar_26.y;
  vec2 tmpvar_28;
  tmpvar_28 = (tmpvar_11.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.zx / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_29;
  tmpvar_29.zw = vec2(0.0, 0.0);
  tmpvar_29.x = tmpvar_28.x;
  tmpvar_29.y = tmpvar_28.y;
  vec4 tmpvar_30;
  vec2 tmpvar_31;
  tmpvar_31 = (tmpvar_5.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.yz / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_32;
  tmpvar_32.zw = vec2(0.0, 0.0);
  tmpvar_32.x = tmpvar_31.x;
  tmpvar_32.y = tmpvar_31.y;
  tmpvar_30 = (texture2DLod (_NormalMap, tmpvar_32.xy, 0.0) - 0.5);
  vec4 tmpvar_33;
  vec2 tmpvar_34;
  tmpvar_34 = (tmpvar_5.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.zx / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_35;
  tmpvar_35.zw = vec2(0.0, 0.0);
  tmpvar_35.x = tmpvar_34.x;
  tmpvar_35.y = tmpvar_34.y;
  tmpvar_33 = (texture2DLod (_NormalMap, tmpvar_35.xy, 0.0) - 0.5);
  vec4 tmpvar_36;
  vec2 tmpvar_37;
  tmpvar_37 = (tmpvar_5.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.xy / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_38;
  tmpvar_38.zw = vec2(0.0, 0.0);
  tmpvar_38.x = tmpvar_37.x;
  tmpvar_38.y = tmpvar_37.y;
  tmpvar_36 = (texture2DLod (_NormalMap, tmpvar_38.xy, 0.0) - 0.5);
  vec3 tmpvar_39;
  tmpvar_39.x = 0.0;
  tmpvar_39.y = tmpvar_30.x;
  tmpvar_39.z = tmpvar_30.y;
  vec3 tmpvar_40;
  tmpvar_40.y = 0.0;
  tmpvar_40.x = tmpvar_33.y;
  tmpvar_40.z = tmpvar_33.x;
  vec3 tmpvar_41;
  tmpvar_41.z = 0.0;
  tmpvar_41.x = tmpvar_36.x;
  tmpvar_41.y = tmpvar_36.y;
  vec4 tmpvar_42;
  vec2 tmpvar_43;
  tmpvar_43 = (tmpvar_8.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.yz / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_44;
  tmpvar_44.zw = vec2(0.0, 0.0);
  tmpvar_44.x = tmpvar_43.x;
  tmpvar_44.y = tmpvar_43.y;
  tmpvar_42 = (texture2DLod (_NormalMap, tmpvar_44.xy, 0.0) - 0.5);
  vec4 tmpvar_45;
  vec2 tmpvar_46;
  tmpvar_46 = (tmpvar_8.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.zx / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_47;
  tmpvar_47.zw = vec2(0.0, 0.0);
  tmpvar_47.x = tmpvar_46.x;
  tmpvar_47.y = tmpvar_46.y;
  tmpvar_45 = (texture2DLod (_NormalMap, tmpvar_47.xy, 0.0) - 0.5);
  vec4 tmpvar_48;
  vec2 tmpvar_49;
  tmpvar_49 = (tmpvar_8.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.xy / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_50;
  tmpvar_50.zw = vec2(0.0, 0.0);
  tmpvar_50.x = tmpvar_49.x;
  tmpvar_50.y = tmpvar_49.y;
  tmpvar_48 = (texture2DLod (_NormalMap, tmpvar_50.xy, 0.0) - 0.5);
  vec3 tmpvar_51;
  tmpvar_51.x = 0.0;
  tmpvar_51.y = tmpvar_42.x;
  tmpvar_51.z = tmpvar_42.y;
  vec3 tmpvar_52;
  tmpvar_52.y = 0.0;
  tmpvar_52.x = tmpvar_45.y;
  tmpvar_52.z = tmpvar_45.x;
  vec3 tmpvar_53;
  tmpvar_53.z = 0.0;
  tmpvar_53.x = tmpvar_48.x;
  tmpvar_53.y = tmpvar_48.y;
  vec4 tmpvar_54;
  vec2 tmpvar_55;
  tmpvar_55 = (tmpvar_11.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.yz / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_56;
  tmpvar_56.zw = vec2(0.0, 0.0);
  tmpvar_56.x = tmpvar_55.x;
  tmpvar_56.y = tmpvar_55.y;
  tmpvar_54 = (texture2DLod (_NormalMap, tmpvar_56.xy, 0.0) - 0.5);
  vec4 tmpvar_57;
  vec2 tmpvar_58;
  tmpvar_58 = (tmpvar_11.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.zx / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_59;
  tmpvar_59.zw = vec2(0.0, 0.0);
  tmpvar_59.x = tmpvar_58.x;
  tmpvar_59.y = tmpvar_58.y;
  tmpvar_57 = (texture2DLod (_NormalMap, tmpvar_59.xy, 0.0) - 0.5);
  vec4 tmpvar_60;
  vec2 tmpvar_61;
  tmpvar_61 = (tmpvar_11.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.xy / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_62;
  tmpvar_62.zw = vec2(0.0, 0.0);
  tmpvar_62.x = tmpvar_61.x;
  tmpvar_62.y = tmpvar_61.y;
  tmpvar_60 = (texture2DLod (_NormalMap, tmpvar_62.xy, 0.0) - 0.5);
  vec3 tmpvar_63;
  tmpvar_63.x = 0.0;
  tmpvar_63.y = tmpvar_54.x;
  tmpvar_63.z = tmpvar_54.y;
  vec3 tmpvar_64;
  tmpvar_64.y = 0.0;
  tmpvar_64.x = tmpvar_57.y;
  tmpvar_64.z = tmpvar_57.x;
  vec3 tmpvar_65;
  tmpvar_65.z = 0.0;
  tmpvar_65.x = tmpvar_60.x;
  tmpvar_65.y = tmpvar_60.y;
  vec3 tmpvar_66;
  tmpvar_66 = ((((xlv_TEXCOORD4.x * (((tmpvar_39 * tmpvar_2.xxx) + (tmpvar_40 * tmpvar_2.yyy)) + (tmpvar_41 * tmpvar_2.zzz))) * 2.0) + ((xlv_TEXCOORD4.y * (((tmpvar_51 * tmpvar_2.xxx) + (tmpvar_52 * tmpvar_2.yyy)) + (tmpvar_53 * tmpvar_2.zzz))) * 2.0)) + ((xlv_TEXCOORD4.z * (((tmpvar_63 * tmpvar_2.xxx) + (tmpvar_64 * tmpvar_2.yyy)) + (tmpvar_65 * tmpvar_2.zzz))) * 2.0));
  vec3 tmpvar_67;
  tmpvar_67 = ((((((xlv_TEXCOORD4.x * (((texture2DLod (_DiffuseTex, tmpvar_13.xy, 0.0) * tmpvar_2.z) + (texture2DLod (_DiffuseTex, tmpvar_15.xy, 0.0) * tmpvar_2.x)) + (texture2DLod (_DiffuseTex, tmpvar_17.xy, 0.0) * tmpvar_2.y))) + (xlv_TEXCOORD4.y * (((texture2DLod (_DiffuseTex, tmpvar_19.xy, 0.0) * tmpvar_2.z) + (texture2DLod (_DiffuseTex, tmpvar_21.xy, 0.0) * tmpvar_2.x)) + (texture2DLod (_DiffuseTex, tmpvar_23.xy, 0.0) * tmpvar_2.y)))) + (xlv_TEXCOORD4.z * (((texture2DLod (_DiffuseTex, tmpvar_25.xy, 0.0) * tmpvar_2.z) + (texture2DLod (_DiffuseTex, tmpvar_27.xy, 0.0) * tmpvar_2.x)) + (texture2DLod (_DiffuseTex, tmpvar_29.xy, 0.0) * tmpvar_2.y)))).xyz * xlv_COLOR0.xyz) * (1.0 - xlv_COLOR0.w)) + (xlv_COLOR0.xyz * xlv_COLOR0.w));
  vec4 tmpvar_68;
  tmpvar_68.w = 1.0;
  tmpvar_68.x = tmpvar_66.x;
  tmpvar_68.y = tmpvar_66.y;
  tmpvar_68.z = tmpvar_66.z;
  vec3 tmpvar_69;
  tmpvar_69 = normalize ((xlv_TEXCOORD0 - (normalize (tmpvar_68).xyz * xlv_TEXCOORD3.w)));
  float tmpvar_70;
  tmpvar_70 = (xlv_TEXCOORD4.w * _SpecPower);
  vec4 tmpvar_71;
  tmpvar_71.w = 0.0;
  tmpvar_71.xyz = xlv_TEXCOORD3.xyz;
  vec4 tmpvar_72;
  tmpvar_72.yzw = vec3(0.1, 0.0, 0.0);
  tmpvar_72.x = (dot (xlv_TEXCOORD4, tmpvar_71) / 255.0);
  vec4 tmpvar_73;
  tmpvar_73 = texture2DLod (_SpecColorTex, tmpvar_72.xy, 0.0);
  vec3 tmpvar_74;
  tmpvar_74 = normalize (xlv_TEXCOORD6);
  vec3 tmpvar_75;
  tmpvar_75 = normalize (xlv_TEXCOORD7);
  float atten;
  atten = (texture2D (_LightTextureB0, vec2(dot (xlv_TEXCOORD8, xlv_TEXCOORD8))).w * textureCube (_LightTexture0, xlv_TEXCOORD8).w);
  vec4 c_i0;
  float m;
  m = tmpvar_73.x;
  if ((tmpvar_73.y > tmpvar_73.x)) {
    m = tmpvar_73.y;
  };
  if ((tmpvar_73.z > m)) {
    m = tmpvar_73.z;
  };
  if ((m > 0.03)) {
    float tmpvar_76;
    tmpvar_76 = (((((tmpvar_75.x * tmpvar_75.x) * tmpvar_75.y) + (tmpvar_75.z * tmpvar_75.x)) + (tmpvar_75.y * tmpvar_75.z)) * _DiamondSpecParam1);
    vec4 tmpvar_77;
    tmpvar_77.zw = vec2(0.0, 0.0);
    tmpvar_77.x = tmpvar_76;
    tmpvar_77.y = _DiamondSpecParam2;
    xlat_mutable__SpecColor.xyz = texture2DLod (_DiamondSpecTex, tmpvar_77.xy, 0.0).xyz;
    vec4 tmpvar_78;
    tmpvar_78.zw = vec2(0.0, 0.0);
    tmpvar_78.x = tmpvar_76;
    tmpvar_78.y = _DiamondSpecParam3;
    xlat_mutable__LightColor0.xyz = (_LightColor0.xyz * texture2DLod (_DiamondSpecTex, tmpvar_78.xy, 0.0).xyz);
  } else {
    if ((m > 0.0)) {
      xlat_mutable__SpecColor.xyz = (tmpvar_73.xyz * (1.0/(m)));
    } else {
      xlat_mutable__SpecColor.xyz = vec3(1.0, 1.0, 1.0);
    };
  };
  c_i0 = vec4(0.0, 0.0, 0.0, 0.0);
  c_i0.xyz = ((((tmpvar_67 * xlat_mutable__LightColor0.xyz) * dot (tmpvar_69, tmpvar_74)) + ((xlat_mutable__LightColor0.xyz * xlat_mutable__SpecColor.xyz) * (smoothstep (0.0, 1.0, pow (max (0.0, dot (tmpvar_69, normalize ((tmpvar_74 + tmpvar_75)))), (32.0 * tmpvar_70))) * tmpvar_70))) * atten);
  c_i0.w = 1.0;
  c = c_i0;
  c.w = 0.0;
  gl_FragData[0] = c;
}


#endif
"
}

SubProgram "d3d9 " {
Keywords { "POINT_COOKIE" }
Bind "vertex" Vertex
Bind "texcoord" TexCoord0
Bind "texcoord1" TexCoord1
Bind "color" Color
Matrix 0 [glstate_matrix_mvp]
Vector 12 [unity_Scale]
Vector 13 [_WorldSpaceCameraPos]
Vector 14 [_WorldSpaceLightPos0]
Matrix 4 [_Object2World]
Matrix 8 [_LightMatrix0]
Float 15 [_NormalPower]
SetTexture 0 [_NormSpecPowerTex] 2D
"vs_3_0
; 108 ALU, 2 TEX
dcl_position o0
dcl_texcoord0 o1
dcl_color0 o2
dcl_texcoord1 o3
dcl_texcoord2 o4
dcl_texcoord3 o5
dcl_texcoord4 o6
dcl_texcoord5 o7
dcl_texcoord6 o8
dcl_texcoord7 o9
dcl_texcoord8 o10
def c16, 0.25000000, 0.00000000, 1.00000000, 0.00390625
def c17, 256.00000000, 0.50000000, 10.00000000, 0.49990001
def c18, -0.49990001, 2.00000000, 0.00392157, 4.00000000
def c19, 0.10000000, 0.00000000, 16.00000000, -8.00000000
def c20, -2.00000000, 0.05000000, 0, 0
dcl_position0 v0
dcl_texcoord0 v3
dcl_texcoord1 v4
dcl_color0 v5
dcl_2d s0
abs r0.y, v4
frc r0.z, r0.y
slt r0.x, v4.y, c16.y
max r0.x, -r0, r0
slt r0.x, c16.y, r0
add r0.y, r0, -r0.z
add r0.w, -r0.x, c16.z
mul r0.z, r0.y, r0.w
mad r0.z, r0.x, -r0.y, r0
add r0.x, v4.y, -r0.z
mul r0.x, r0, c17.z
mul r1.y, r0.z, c16.w
slt r0.y, r0.x, c18.x
add r0.w, r0.x, c17
max r0.x, -r0.y, r0.y
abs r0.y, r0.w
slt r0.x, c16.y, r0
frc r0.w, r0.y
add r0.y, r0, -r0.w
add r1.x, -r0, c16.z
mul r0.w, r0.y, r1.x
mad r0.w, r0.x, -r0.y, r0
sge r0.y, c16, r0.w
sge r0.x, r0.w, c16.y
mul r0.x, r0, r0.y
sge r1.x, c16.z, r0.w
sge r0.y, r0.w, c16.z
mul r0.y, r0, r1.x
sge r1.x, r0.w, c18.y
sge r0.w, c18.y, r0
mul r0.z, r1.x, r0.w
abs r1.x, r1.y
frc r1.z, r1.x
add r1.x, r1, -r1.z
slt r0.w, r1.y, c16.y
frc r1.z, r1.y
mul r1.z, r1, c17.x
max r0.w, -r0, r0
slt r0.w, c16.y, r0
add r1.y, -r0.w, c16.z
mul r1.y, r1.x, r1
mad r1.y, r0.w, -r1.x, r1
abs r1.w, r1.z
add r1.x, r1.w, c17.y
mul r1.w, v4.x, c16.x
slt r0.w, r1.z, c16.y
frc r1.z, r1.x
slt r2.x, r1.w, c16.y
max r0.w, -r0, r0
add r1.x, r1, -r1.z
slt r0.w, c16.y, r0
add r1.z, -r0.w, c16
mul r1.z, r1.x, r1
mad r1.z, r0.w, -r1.x, r1
abs r2.y, r1.w
max r2.x, -r2, r2
slt r1.w, c16.y, r2.x
frc r2.x, r2.y
add r2.x, r2.y, -r2
add r2.z, -r1.w, c16
mul r2.y, r2.x, r2.z
mad r1.x, r1.w, -r2, r2.y
dp3 r0.w, r0, r1
mov r2.yz, c19.xxyw
mul r2.x, r0.w, c18.z
texldl r2.xy, r2.xyzz, s0
mad r0.w, r2.x, c19.z, c19
mul r1.w, r0, c15.x
mul r0.w, r2.y, c18
mov o6, r0
mov o5, r1
mad r1.x, -r1, c18.w, v4
add r1.z, r1.x, c20.x
mov r1.xy, v3
mul r2.xyz, r1, c12.w
dp4 r0.z, v0, c6
dp4 r0.x, v0, c4
dp4 r0.y, v0, c5
dp4 r0.w, v0, c7
mov o2, v5
dp4 o10.z, r0, c10
dp4 o10.y, r0, c9
dp4 o10.x, r0, c8
dp3 o1.z, r1, c6
dp3 o1.y, r1, c5
dp3 o1.x, r1, c4
mul o3.xyz, v0, c20.y
mov o4.xyz, r1
dp3 o7.z, r2, c6
dp3 o7.y, r2, c5
dp3 o7.x, r2, c4
add o8.xyz, -r0, c14
add o9.xyz, -r0, c13
dp4 o0.w, v0, c3
dp4 o0.z, v0, c2
dp4 o0.y, v0, c1
dp4 o0.x, v0, c0
"
}

SubProgram "opengl " {
Keywords { "DIRECTIONAL_COOKIE" }
"!!GLSL
#ifdef VERTEX
#define DIRECTIONAL_COOKIE 1
#define SHADER_API_OPENGL 1
#define SHADER_API_DESKTOP 1
vec4 xll_tex2Dlod(sampler2D s, vec4 coord) {
   return texture2DLod( s, coord.xy, coord.w);
}
mat3 xll_constructMat3( mat4 m) {
  return mat3( vec3( m[0]), vec3( m[1]), vec3( m[2]));
}
struct v2f_vertex_lit {
    vec2 uv;
    vec4 diff;
    vec4 spec;
};
struct v2f_img {
    vec4 pos;
    vec2 uv;
};
struct appdata_img {
    vec4 vertex;
    vec2 texcoord;
};
struct SurfaceOutput {
    vec3 Albedo;
    vec3 Normal;
    vec3 Emission;
    float Specular;
    float Gloss;
    float Alpha;
};
struct CustomSurfaceOutput {
    vec3 Albedo;
    vec3 Normal;
    vec3 Normal_;
    vec3 Emission;
    float Specular;
    float Alpha;
};
struct appdata_full {
    vec4 vertex;
    vec4 tangent;
    vec3 normal;
    vec4 texcoord;
    vec4 texcoord1;
    vec4 color;
};
struct Input {
    vec3 localPos;
    vec3 localNormal;
    vec3 worldNormal;
    vec4 weights;
    vec4 blendMask;
    vec4 color;
};
struct v2f_surf {
    vec4 pos;
    vec3 worldNormal;
    vec4 color;
    vec3 cust_localPos;
    vec3 cust_localNormal;
    vec4 cust_weights;
    vec4 cust_blendMask;
    vec3 normal;
    vec3 lightDir;
    vec3 viewDir;
    vec2 _LightCoord;
};
uniform mat4 _LightMatrix0;
uniform sampler2D _NormSpecPowerTex;
uniform float _NormalPower;
uniform mat4 _Object2World;
uniform vec3 _WorldSpaceCameraPos;
uniform vec4 _WorldSpaceLightPos0;

uniform vec4 unity_Scale;
void vert( inout appdata_full v, out Input o );
vec3 WorldSpaceViewDir( in vec4 v );
vec3 WorldSpaceLightDir( in vec4 v );
v2f_surf vert_surf( in appdata_full v );
void vert( inout appdata_full v, out Input o ) {
    int mat0;
    int mat12;
    vec3 matIdx;
    int idx;
    vec2 normSpec;
    mat0 = int( (v.texcoord1.x  / 4.00000) );
    mat12 = int( v.texcoord1.y  );
    matIdx = vec3( float( mat0 ), float( (mat12 / 256) ), float( (mat12 % 256) ));
    o.weights.xyz  = matIdx;
    idx = int( (((v.texcoord1.y  - float( mat12 )) * 10.0000) + 0.499900) );
    o.blendMask.xyz  = vec3( float( (idx == 0) ), float( (idx == 1) ), float( (idx == 2) ));
    normSpec = xll_tex2Dlod( _NormSpecPowerTex, vec4( (dot( o.blendMask, vec4( matIdx, 0.000000)) / 255.000), 0.100000, 0.000000, 0.000000)).xy ;
    o.weights.w  = (((normSpec.x  * 16.0000) - 8.00000) * _NormalPower);
    o.blendMask.w  = (normSpec.y  * 4.00000);
    v.normal = vec3( v.texcoord.x , v.texcoord.y , ((v.texcoord1.x  - float( (4 * mat0) )) - 2.00000));
    o.localPos = vec3( (v.vertex * 0.0500000));
    o.localNormal = v.normal;
}
vec3 WorldSpaceViewDir( in vec4 v ) {
    return (_WorldSpaceCameraPos.xyz  - ( _Object2World * v ).xyz );
}
vec3 WorldSpaceLightDir( in vec4 v ) {
    vec3 worldPos;
    worldPos = ( _Object2World * v ).xyz ;
    return _WorldSpaceLightPos0.xyz ;
}
v2f_surf vert_surf( in appdata_full v ) {
    Input customInputData;
    v2f_surf o;
    vec3 lightDir;
    vec3 viewDirForLight;
    vert( v, customInputData);
    o.cust_localPos = customInputData.localPos;
    o.cust_localNormal = customInputData.localNormal;
    o.cust_weights = customInputData.weights;
    o.cust_blendMask = customInputData.blendMask;
    o.pos = ( gl_ModelViewProjectionMatrix * v.vertex );
    o.worldNormal = ( xll_constructMat3( _Object2World) * v.normal );
    o.color = v.color;
    o.normal = ( xll_constructMat3( _Object2World) * (v.normal * unity_Scale.w ) );
    lightDir = WorldSpaceLightDir( v.vertex);
    o.lightDir = lightDir;
    viewDirForLight = WorldSpaceViewDir( v.vertex);
    o.viewDir = viewDirForLight;
    o._LightCoord = ( _LightMatrix0 * ( _Object2World * v.vertex ) ).xy ;
    return o;
}
attribute vec4 TANGENT;
varying vec3 xlv_TEXCOORD0;
varying vec4 xlv_COLOR0;
varying vec3 xlv_TEXCOORD1;
varying vec3 xlv_TEXCOORD2;
varying vec4 xlv_TEXCOORD3;
varying vec4 xlv_TEXCOORD4;
varying vec3 xlv_TEXCOORD5;
varying vec3 xlv_TEXCOORD6;
varying vec3 xlv_TEXCOORD7;
varying vec2 xlv_TEXCOORD8;
void main() {
    v2f_surf xl_retval;
    appdata_full xlt_v;
    xlt_v.vertex = vec4( gl_Vertex);
    xlt_v.tangent = vec4( TANGENT);
    xlt_v.normal = vec3( gl_Normal);
    xlt_v.texcoord = vec4( gl_MultiTexCoord0);
    xlt_v.texcoord1 = vec4( gl_MultiTexCoord1);
    xlt_v.color = vec4( gl_Color);
    xl_retval = vert_surf( xlt_v);
    gl_Position = vec4( xl_retval.pos);
    xlv_TEXCOORD0 = vec3( xl_retval.worldNormal);
    xlv_COLOR0 = vec4( xl_retval.color);
    xlv_TEXCOORD1 = vec3( xl_retval.cust_localPos);
    xlv_TEXCOORD2 = vec3( xl_retval.cust_localNormal);
    xlv_TEXCOORD3 = vec4( xl_retval.cust_weights);
    xlv_TEXCOORD4 = vec4( xl_retval.cust_blendMask);
    xlv_TEXCOORD5 = vec3( xl_retval.normal);
    xlv_TEXCOORD6 = vec3( xl_retval.lightDir);
    xlv_TEXCOORD7 = vec3( xl_retval.viewDir);
    xlv_TEXCOORD8 = vec2( xl_retval._LightCoord);
}
/* NOTE: GLSL optimization failed
0:88(75): error: operator '%' is reserved in (null)
0:88(56): error: cannot construct `float' from a non-numeric data type
0:88(11): error: cannot construct `vec3' from a non-numeric data type
*/

#endif
#ifdef FRAGMENT
#extension GL_ARB_shader_texture_lod : enable
varying vec2 xlv_TEXCOORD8;
varying vec3 xlv_TEXCOORD7;
varying vec3 xlv_TEXCOORD6;
varying vec4 xlv_TEXCOORD4;
varying vec4 xlv_TEXCOORD3;
varying vec3 xlv_TEXCOORD2;
varying vec3 xlv_TEXCOORD1;
varying vec4 xlv_COLOR0;
varying vec3 xlv_TEXCOORD0;
vec4 xlat_mutable__SpecColor;
uniform vec4 _SpecColor;
vec4 xlat_mutable__LightColor0;
uniform vec4 _LightColor0;
uniform float _TileSize;
uniform float _SpecPower;
uniform sampler2D _SpecColorTex;
uniform float _Repetition;
uniform sampler2D _NormalMap;
uniform sampler2D _LightTexture0;
uniform float _Indent;
uniform sampler2D _DiffuseTex;
uniform sampler2D _DiamondSpecTex;
uniform float _DiamondSpecParam3;
uniform float _DiamondSpecParam2;
uniform float _DiamondSpecParam1;
void main ()
{
  xlat_mutable__SpecColor = _SpecColor;
  xlat_mutable__LightColor0 = _LightColor0;
  vec4 c;
  vec3 tmpvar_1;
  tmpvar_1 = max (((normalize (abs (xlv_TEXCOORD2)) - 0.2) * 7.0), vec3(0.0, 0.0, 0.0));
  vec3 tmpvar_2;
  tmpvar_2 = (tmpvar_1 / vec3(((tmpvar_1.x + tmpvar_1.y) + tmpvar_1.z)));
  float tmpvar_3;
  tmpvar_3 = (float(int(floor ((xlv_TEXCOORD3.x + 0.5)))) * _TileSize);
  float tmpvar_4;
  tmpvar_4 = (floor (tmpvar_3) * _TileSize);
  vec3 tmpvar_5;
  tmpvar_5.x = fract (tmpvar_3);
  tmpvar_5.y = fract (tmpvar_4);
  tmpvar_5.z = floor (tmpvar_4);
  float tmpvar_6;
  tmpvar_6 = (float(int(floor ((xlv_TEXCOORD3.y + 0.5)))) * _TileSize);
  float tmpvar_7;
  tmpvar_7 = (floor (tmpvar_6) * _TileSize);
  vec3 tmpvar_8;
  tmpvar_8.x = fract (tmpvar_6);
  tmpvar_8.y = fract (tmpvar_7);
  tmpvar_8.z = floor (tmpvar_7);
  float tmpvar_9;
  tmpvar_9 = (float(int(floor ((xlv_TEXCOORD3.z + 0.5)))) * _TileSize);
  float tmpvar_10;
  tmpvar_10 = (floor (tmpvar_9) * _TileSize);
  vec3 tmpvar_11;
  tmpvar_11.x = fract (tmpvar_9);
  tmpvar_11.y = fract (tmpvar_10);
  tmpvar_11.z = floor (tmpvar_10);
  vec2 tmpvar_12;
  tmpvar_12 = (tmpvar_5.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.xy / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_13;
  tmpvar_13.zw = vec2(0.0, 0.0);
  tmpvar_13.x = tmpvar_12.x;
  tmpvar_13.y = tmpvar_12.y;
  vec2 tmpvar_14;
  tmpvar_14 = (tmpvar_5.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.yz / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_15;
  tmpvar_15.zw = vec2(0.0, 0.0);
  tmpvar_15.x = tmpvar_14.x;
  tmpvar_15.y = tmpvar_14.y;
  vec2 tmpvar_16;
  tmpvar_16 = (tmpvar_5.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.zx / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_17;
  tmpvar_17.zw = vec2(0.0, 0.0);
  tmpvar_17.x = tmpvar_16.x;
  tmpvar_17.y = tmpvar_16.y;
  vec2 tmpvar_18;
  tmpvar_18 = (tmpvar_8.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.xy / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_19;
  tmpvar_19.zw = vec2(0.0, 0.0);
  tmpvar_19.x = tmpvar_18.x;
  tmpvar_19.y = tmpvar_18.y;
  vec2 tmpvar_20;
  tmpvar_20 = (tmpvar_8.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.yz / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_21;
  tmpvar_21.zw = vec2(0.0, 0.0);
  tmpvar_21.x = tmpvar_20.x;
  tmpvar_21.y = tmpvar_20.y;
  vec2 tmpvar_22;
  tmpvar_22 = (tmpvar_8.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.zx / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_23;
  tmpvar_23.zw = vec2(0.0, 0.0);
  tmpvar_23.x = tmpvar_22.x;
  tmpvar_23.y = tmpvar_22.y;
  vec2 tmpvar_24;
  tmpvar_24 = (tmpvar_11.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.xy / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_25;
  tmpvar_25.zw = vec2(0.0, 0.0);
  tmpvar_25.x = tmpvar_24.x;
  tmpvar_25.y = tmpvar_24.y;
  vec2 tmpvar_26;
  tmpvar_26 = (tmpvar_11.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.yz / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_27;
  tmpvar_27.zw = vec2(0.0, 0.0);
  tmpvar_27.x = tmpvar_26.x;
  tmpvar_27.y = tmpvar_26.y;
  vec2 tmpvar_28;
  tmpvar_28 = (tmpvar_11.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.zx / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_29;
  tmpvar_29.zw = vec2(0.0, 0.0);
  tmpvar_29.x = tmpvar_28.x;
  tmpvar_29.y = tmpvar_28.y;
  vec4 tmpvar_30;
  vec2 tmpvar_31;
  tmpvar_31 = (tmpvar_5.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.yz / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_32;
  tmpvar_32.zw = vec2(0.0, 0.0);
  tmpvar_32.x = tmpvar_31.x;
  tmpvar_32.y = tmpvar_31.y;
  tmpvar_30 = (texture2DLod (_NormalMap, tmpvar_32.xy, 0.0) - 0.5);
  vec4 tmpvar_33;
  vec2 tmpvar_34;
  tmpvar_34 = (tmpvar_5.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.zx / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_35;
  tmpvar_35.zw = vec2(0.0, 0.0);
  tmpvar_35.x = tmpvar_34.x;
  tmpvar_35.y = tmpvar_34.y;
  tmpvar_33 = (texture2DLod (_NormalMap, tmpvar_35.xy, 0.0) - 0.5);
  vec4 tmpvar_36;
  vec2 tmpvar_37;
  tmpvar_37 = (tmpvar_5.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.xy / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_38;
  tmpvar_38.zw = vec2(0.0, 0.0);
  tmpvar_38.x = tmpvar_37.x;
  tmpvar_38.y = tmpvar_37.y;
  tmpvar_36 = (texture2DLod (_NormalMap, tmpvar_38.xy, 0.0) - 0.5);
  vec3 tmpvar_39;
  tmpvar_39.x = 0.0;
  tmpvar_39.y = tmpvar_30.x;
  tmpvar_39.z = tmpvar_30.y;
  vec3 tmpvar_40;
  tmpvar_40.y = 0.0;
  tmpvar_40.x = tmpvar_33.y;
  tmpvar_40.z = tmpvar_33.x;
  vec3 tmpvar_41;
  tmpvar_41.z = 0.0;
  tmpvar_41.x = tmpvar_36.x;
  tmpvar_41.y = tmpvar_36.y;
  vec4 tmpvar_42;
  vec2 tmpvar_43;
  tmpvar_43 = (tmpvar_8.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.yz / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_44;
  tmpvar_44.zw = vec2(0.0, 0.0);
  tmpvar_44.x = tmpvar_43.x;
  tmpvar_44.y = tmpvar_43.y;
  tmpvar_42 = (texture2DLod (_NormalMap, tmpvar_44.xy, 0.0) - 0.5);
  vec4 tmpvar_45;
  vec2 tmpvar_46;
  tmpvar_46 = (tmpvar_8.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.zx / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_47;
  tmpvar_47.zw = vec2(0.0, 0.0);
  tmpvar_47.x = tmpvar_46.x;
  tmpvar_47.y = tmpvar_46.y;
  tmpvar_45 = (texture2DLod (_NormalMap, tmpvar_47.xy, 0.0) - 0.5);
  vec4 tmpvar_48;
  vec2 tmpvar_49;
  tmpvar_49 = (tmpvar_8.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.xy / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_50;
  tmpvar_50.zw = vec2(0.0, 0.0);
  tmpvar_50.x = tmpvar_49.x;
  tmpvar_50.y = tmpvar_49.y;
  tmpvar_48 = (texture2DLod (_NormalMap, tmpvar_50.xy, 0.0) - 0.5);
  vec3 tmpvar_51;
  tmpvar_51.x = 0.0;
  tmpvar_51.y = tmpvar_42.x;
  tmpvar_51.z = tmpvar_42.y;
  vec3 tmpvar_52;
  tmpvar_52.y = 0.0;
  tmpvar_52.x = tmpvar_45.y;
  tmpvar_52.z = tmpvar_45.x;
  vec3 tmpvar_53;
  tmpvar_53.z = 0.0;
  tmpvar_53.x = tmpvar_48.x;
  tmpvar_53.y = tmpvar_48.y;
  vec4 tmpvar_54;
  vec2 tmpvar_55;
  tmpvar_55 = (tmpvar_11.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.yz / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_56;
  tmpvar_56.zw = vec2(0.0, 0.0);
  tmpvar_56.x = tmpvar_55.x;
  tmpvar_56.y = tmpvar_55.y;
  tmpvar_54 = (texture2DLod (_NormalMap, tmpvar_56.xy, 0.0) - 0.5);
  vec4 tmpvar_57;
  vec2 tmpvar_58;
  tmpvar_58 = (tmpvar_11.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.zx / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_59;
  tmpvar_59.zw = vec2(0.0, 0.0);
  tmpvar_59.x = tmpvar_58.x;
  tmpvar_59.y = tmpvar_58.y;
  tmpvar_57 = (texture2DLod (_NormalMap, tmpvar_59.xy, 0.0) - 0.5);
  vec4 tmpvar_60;
  vec2 tmpvar_61;
  tmpvar_61 = (tmpvar_11.xy + (((0.00195313 * _Indent) + (fract ((xlv_TEXCOORD1.xy / _Repetition)) * (1.0 - (0.00390626 * _Indent)))) * _TileSize));
  vec4 tmpvar_62;
  tmpvar_62.zw = vec2(0.0, 0.0);
  tmpvar_62.x = tmpvar_61.x;
  tmpvar_62.y = tmpvar_61.y;
  tmpvar_60 = (texture2DLod (_NormalMap, tmpvar_62.xy, 0.0) - 0.5);
  vec3 tmpvar_63;
  tmpvar_63.x = 0.0;
  tmpvar_63.y = tmpvar_54.x;
  tmpvar_63.z = tmpvar_54.y;
  vec3 tmpvar_64;
  tmpvar_64.y = 0.0;
  tmpvar_64.x = tmpvar_57.y;
  tmpvar_64.z = tmpvar_57.x;
  vec3 tmpvar_65;
  tmpvar_65.z = 0.0;
  tmpvar_65.x = tmpvar_60.x;
  tmpvar_65.y = tmpvar_60.y;
  vec3 tmpvar_66;
  tmpvar_66 = ((((xlv_TEXCOORD4.x * (((tmpvar_39 * tmpvar_2.xxx) + (tmpvar_40 * tmpvar_2.yyy)) + (tmpvar_41 * tmpvar_2.zzz))) * 2.0) + ((xlv_TEXCOORD4.y * (((tmpvar_51 * tmpvar_2.xxx) + (tmpvar_52 * tmpvar_2.yyy)) + (tmpvar_53 * tmpvar_2.zzz))) * 2.0)) + ((xlv_TEXCOORD4.z * (((tmpvar_63 * tmpvar_2.xxx) + (tmpvar_64 * tmpvar_2.yyy)) + (tmpvar_65 * tmpvar_2.zzz))) * 2.0));
  vec3 tmpvar_67;
  tmpvar_67 = ((((((xlv_TEXCOORD4.x * (((texture2DLod (_DiffuseTex, tmpvar_13.xy, 0.0) * tmpvar_2.z) + (texture2DLod (_DiffuseTex, tmpvar_15.xy, 0.0) * tmpvar_2.x)) + (texture2DLod (_DiffuseTex, tmpvar_17.xy, 0.0) * tmpvar_2.y))) + (xlv_TEXCOORD4.y * (((texture2DLod (_DiffuseTex, tmpvar_19.xy, 0.0) * tmpvar_2.z) + (texture2DLod (_DiffuseTex, tmpvar_21.xy, 0.0) * tmpvar_2.x)) + (texture2DLod (_DiffuseTex, tmpvar_23.xy, 0.0) * tmpvar_2.y)))) + (xlv_TEXCOORD4.z * (((texture2DLod (_DiffuseTex, tmpvar_25.xy, 0.0) * tmpvar_2.z) + (texture2DLod (_DiffuseTex, tmpvar_27.xy, 0.0) * tmpvar_2.x)) + (texture2DLod (_DiffuseTex, tmpvar_29.xy, 0.0) * tmpvar_2.y)))).xyz * xlv_COLOR0.xyz) * (1.0 - xlv_COLOR0.w)) + (xlv_COLOR0.xyz * xlv_COLOR0.w));
  vec4 tmpvar_68;
  tmpvar_68.w = 1.0;
  tmpvar_68.x = tmpvar_66.x;
  tmpvar_68.y = tmpvar_66.y;
  tmpvar_68.z = tmpvar_66.z;
  vec3 tmpvar_69;
  tmpvar_69 = normalize ((xlv_TEXCOORD0 - (normalize (tmpvar_68).xyz * xlv_TEXCOORD3.w)));
  float tmpvar_70;
  tmpvar_70 = (xlv_TEXCOORD4.w * _SpecPower);
  vec4 tmpvar_71;
  tmpvar_71.w = 0.0;
  tmpvar_71.xyz = xlv_TEXCOORD3.xyz;
  vec4 tmpvar_72;
  tmpvar_72.yzw = vec3(0.1, 0.0, 0.0);
  tmpvar_72.x = (dot (xlv_TEXCOORD4, tmpvar_71) / 255.0);
  vec4 tmpvar_73;
  tmpvar_73 = texture2DLod (_SpecColorTex, tmpvar_72.xy, 0.0);
  vec3 tmpvar_74;
  tmpvar_74 = normalize (xlv_TEXCOORD7);
  float atten;
  atten = texture2D (_LightTexture0, xlv_TEXCOORD8).w;
  vec4 c_i0;
  float m;
  m = tmpvar_73.x;
  if ((tmpvar_73.y > tmpvar_73.x)) {
    m = tmpvar_73.y;
  };
  if ((tmpvar_73.z > m)) {
    m = tmpvar_73.z;
  };
  if ((m > 0.03)) {
    float tmpvar_75;
    tmpvar_75 = (((((tmpvar_74.x * tmpvar_74.x) * tmpvar_74.y) + (tmpvar_74.z * tmpvar_74.x)) + (tmpvar_74.y * tmpvar_74.z)) * _DiamondSpecParam1);
    vec4 tmpvar_76;
    tmpvar_76.zw = vec2(0.0, 0.0);
    tmpvar_76.x = tmpvar_75;
    tmpvar_76.y = _DiamondSpecParam2;
    xlat_mutable__SpecColor.xyz = texture2DLod (_DiamondSpecTex, tmpvar_76.xy, 0.0).xyz;
    vec4 tmpvar_77;
    tmpvar_77.zw = vec2(0.0, 0.0);
    tmpvar_77.x = tmpvar_75;
    tmpvar_77.y = _DiamondSpecParam3;
    xlat_mutable__LightColor0.xyz = (_LightColor0.xyz * texture2DLod (_DiamondSpecTex, tmpvar_77.xy, 0.0).xyz);
  } else {
    if ((m > 0.0)) {
      xlat_mutable__SpecColor.xyz = (tmpvar_73.xyz * (1.0/(m)));
    } else {
      xlat_mutable__SpecColor.xyz = vec3(1.0, 1.0, 1.0);
    };
  };
  c_i0 = vec4(0.0, 0.0, 0.0, 0.0);
  c_i0.xyz = ((((tmpvar_67 * xlat_mutable__LightColor0.xyz) * dot (tmpvar_69, xlv_TEXCOORD6)) + ((xlat_mutable__LightColor0.xyz * xlat_mutable__SpecColor.xyz) * (smoothstep (0.0, 1.0, pow (max (0.0, dot (tmpvar_69, normalize ((xlv_TEXCOORD6 + tmpvar_74)))), (32.0 * tmpvar_70))) * tmpvar_70))) * atten);
  c_i0.w = 1.0;
  c = c_i0;
  c.w = 0.0;
  gl_FragData[0] = c;
}


#endif
"
}

SubProgram "d3d9 " {
Keywords { "DIRECTIONAL_COOKIE" }
Bind "vertex" Vertex
Bind "texcoord" TexCoord0
Bind "texcoord1" TexCoord1
Bind "color" Color
Matrix 0 [glstate_matrix_mvp]
Vector 12 [unity_Scale]
Vector 13 [_WorldSpaceCameraPos]
Vector 14 [_WorldSpaceLightPos0]
Matrix 4 [_Object2World]
Matrix 8 [_LightMatrix0]
Float 15 [_NormalPower]
SetTexture 0 [_NormSpecPowerTex] 2D
"vs_3_0
; 107 ALU, 2 TEX
dcl_position o0
dcl_texcoord0 o1
dcl_color0 o2
dcl_texcoord1 o3
dcl_texcoord2 o4
dcl_texcoord3 o5
dcl_texcoord4 o6
dcl_texcoord5 o7
dcl_texcoord6 o8
dcl_texcoord7 o9
dcl_texcoord8 o10
def c16, 0.25000000, 0.00000000, 1.00000000, 0.00390625
def c17, 256.00000000, 0.50000000, 10.00000000, 0.49990001
def c18, -0.49990001, 2.00000000, 0.00392157, 4.00000000
def c19, 0.10000000, 0.00000000, 16.00000000, -8.00000000
def c20, -2.00000000, 0.05000000, 0, 0
dcl_position0 v0
dcl_texcoord0 v3
dcl_texcoord1 v4
dcl_color0 v5
dcl_2d s0
abs r0.y, v4
frc r0.z, r0.y
slt r0.x, v4.y, c16.y
max r0.x, -r0, r0
slt r0.x, c16.y, r0
add r0.y, r0, -r0.z
add r0.w, -r0.x, c16.z
mul r0.z, r0.y, r0.w
mad r0.z, r0.x, -r0.y, r0
add r0.x, v4.y, -r0.z
mul r0.x, r0, c17.z
mul r1.y, r0.z, c16.w
slt r0.y, r0.x, c18.x
add r0.w, r0.x, c17
max r0.x, -r0.y, r0.y
abs r0.y, r0.w
slt r0.x, c16.y, r0
frc r0.w, r0.y
add r0.y, r0, -r0.w
add r1.x, -r0, c16.z
mul r0.w, r0.y, r1.x
mad r0.w, r0.x, -r0.y, r0
sge r0.y, c16, r0.w
sge r0.x, r0.w, c16.y
mul r0.x, r0, r0.y
sge r1.x, c16.z, r0.w
sge r0.y, r0.w, c16.z
mul r0.y, r0, r1.x
sge r1.x, r0.w, c18.y
sge r0.w, c18.y, r0
mul r0.z, r1.x, r0.w
abs r1.x, r1.y
frc r1.z, r1.x
add r1.x, r1, -r1.z
slt r0.w, r1.y, c16.y
frc r1.z, r1.y
mul r1.z, r1, c17.x
max r0.w, -r0, r0
slt r0.w, c16.y, r0
add r1.y, -r0.w, c16.z
mul r1.y, r1.x, r1
mad r1.y, r0.w, -r1.x, r1
abs r1.w, r1.z
add r1.x, r1.w, c17.y
mul r1.w, v4.x, c16.x
slt r0.w, r1.z, c16.y
frc r1.z, r1.x
slt r2.x, r1.w, c16.y
max r0.w, -r0, r0
add r1.x, r1, -r1.z
slt r0.w, c16.y, r0
add r1.z, -r0.w, c16
mul r1.z, r1.x, r1
mad r1.z, r0.w, -r1.x, r1
abs r2.y, r1.w
max r2.x, -r2, r2
slt r1.w, c16.y, r2.x
frc r2.x, r2.y
add r2.x, r2.y, -r2
add r2.z, -r1.w, c16
mul r2.y, r2.x, r2.z
mad r1.x, r1.w, -r2, r2.y
dp3 r0.w, r0, r1
mov r2.yz, c19.xxyw
mul r2.x, r0.w, c18.z
texldl r2.xy, r2.xyzz, s0
mad r0.w, r2.x, c19.z, c19
mul r1.w, r0, c15.x
mul r0.w, r2.y, c18
mov o6, r0
mov o5, r1
mad r1.x, -r1, c18.w, v4
add r1.z, r1.x, c20.x
mov r1.xy, v3
mul r2.xyz, r1, c12.w
dp4 r0.z, v0, c6
dp4 r0.x, v0, c4
dp4 r0.y, v0, c5
dp4 r0.w, v0, c7
mov o2, v5
dp4 o10.y, r0, c9
dp4 o10.x, r0, c8
dp3 o1.z, r1, c6
dp3 o1.y, r1, c5
dp3 o1.x, r1, c4
mul o3.xyz, v0, c20.y
mov o4.xyz, r1
dp3 o7.z, r2, c6
dp3 o7.y, r2, c5
dp3 o7.x, r2, c4
mov o8.xyz, c14
add o9.xyz, -r0, c13
dp4 o0.w, v0, c3
dp4 o0.z, v0, c2
dp4 o0.y, v0, c1
dp4 o0.x, v0, c0
"
}

}
Program "fp" {
// Fragment combos: 5
//   d3d9 - ALU: 186 to 196, TEX: 42 to 44, FLOW: 2 to 2
SubProgram "opengl " {
Keywords { "POINT" }
"!!GLSL"
}

SubProgram "d3d9 " {
Keywords { "POINT" }
Vector 0 [_LightColor0]
Float 1 [_SpecPower]
Float 2 [_TileSize]
Float 3 [_Repetition]
Float 4 [_Indent]
Float 5 [_DiamondSpecParam1]
Float 6 [_DiamondSpecParam2]
Float 7 [_DiamondSpecParam3]
SetTexture 0 [_DiffuseTex] 2D
SetTexture 1 [_NormalMap] 2D
SetTexture 2 [_SpecColorTex] 2D
SetTexture 3 [_LightTexture0] 2D
SetTexture 4 [_DiamondSpecTex] 2D
"ps_3_0
; 192 ALU, 43 TEX, 2 FLOW
dcl_2d s0
dcl_2d s1
dcl_2d s2
dcl_2d s3
dcl_2d s4
def c8, 0.00195313, 0.00390625, 1.00000000, 0.50000000
def c9, 0.00000000, -0.20000000, 7.00000000, -0.50000000
def c10, 2.00000000, 0.00392157, 0.10000000, 0.00000000
def c11, 0.02999878, 0.00000000, 1.00000000, 32.00000000
def c12, 2.00000000, 3.00000000, 0, 0
dcl_texcoord0 v0.xyz
dcl_color0 v1
dcl_texcoord1 v2.xyz
dcl_texcoord2 v3.xyz
dcl_texcoord3 v4
dcl_texcoord4 v5
dcl_texcoord6 v6.xyz
dcl_texcoord7 v7.xyz
dcl_texcoord8 v8.xyz
add r0.w, v4.x, c8
frc r1.x, r0.w
add r1.x, r0.w, -r1
abs r1.y, r1.x
abs r0.xyz, v3
dp3 r0.w, r0, r0
rsq r0.w, r0.w
mad r0.xyz, r0.w, r0, c9.y
frc r1.z, r1.y
add r0.w, r1.y, -r1.z
cmp r0.w, r1.x, r0, -r0
mul r1.x, r0.w, c2
frc r2.x, r1
add r1.x, r1, -r2
mul r1.x, r1, c2
mul r0.xyz, r0, c9.z
max r0.xyz, r0, c9.x
add r0.w, r0.x, r0.y
add r0.w, r0.z, r0
rcp r0.w, r0.w
mul r10.xyz, r0, r0.w
mov r0.x, c4
frc r2.y, r1.x
rcp r1.w, c3.x
mad r0.w, -r0.x, c8.y, c8.z
mul r1.xy, v2.zxzw, r1.w
frc r0.xy, r1
mul r1.xy, r0, r0.w
mov r0.x, c4
mad r11.xy, c8.x, r0.x, r1
mul r1.xy, v2.yzzw, r1.w
frc r2.zw, r1.xyxy
mad r6.xy, r11, c2.x, r2
mov r6.z, c9.x
texldl r0.xy, r6.xyzz, s1
add r0.xz, r0.yyxw, c9.w
mov r0.y, c9.x
mul r1.xyz, r10.y, r0
mul r2.zw, r0.w, r2
mov r0.x, c4
mad r8.zw, c8.x, r0.x, r2
add r0.x, v4.y, c8.w
frc r0.y, r0.x
add r2.z, r0.x, -r0.y
abs r2.w, r2.z
mad r4.xy, r8.zwzw, c2.x, r2
mov r4.z, c9.x
texldl r0.xy, r4.xyzz, s1
add r0.yz, r0.xxyw, c9.w
mov r0.x, c9
mad r1.xyz, r10.x, r0, r1
frc r3.x, r2.w
add r0.z, r2.w, -r3.x
cmp r0.z, r2, r0, -r0
mul r0.xy, v2, r1.w
mul r1.w, r0.z, c2.x
frc r0.xy, r0
mul r0.zw, r0.w, r0.xyxy
mov r0.x, c4
mad r13.xy, c8.x, r0.x, r0.zwzw
frc r8.x, r1.w
add r0.x, r1.w, -r8
mul r0.z, r0.x, c2.x
frc r8.y, r0.z
mad r5.xy, r13, c2.x, r2
mov r5.z, c9.x
texldl r0.xy, r5.xyzz, s1
mad r3.xy, r11, c2.x, r8
mov r3.z, c9.x
texldl r2.xy, r3.xyzz, s1
add r0.w, v4.z, c8
mov r2.z, c9.x
texldl r3.xyz, r3.xyzz, s0
add r0.xy, r0, c9.w
mov r0.z, c9.x
mad r0.xyz, r10.z, r0, r1
add r1.xz, r2.yyxw, c9.w
mov r1.y, c9.x
mul r7.xyz, r10.y, r1
frc r1.x, r0.w
add r0.w, r0, -r1.x
mad r2.xy, r8.zwzw, c2.x, r8
texldl r1.xy, r2.xyzz, s1
abs r1.w, r0
add r1.yz, r1.xxyw, c9.w
mov r1.x, c9
mad r9.xyz, r10.x, r1, r7
frc r2.w, r1
add r1.z, r1.w, -r2.w
cmp r0.w, r0, r1.z, -r1.z
texldl r2.xyz, r2.xyzz, s0
mad r1.xy, r13, c2.x, r8
mov r1.z, c9.x
texldl r7.xy, r1.xyzz, s1
mul r0.w, r0, c2.x
frc r8.x, r0.w
add r0.w, r0, -r8.x
mul r0.w, r0, c2.x
frc r8.y, r0.w
mul r2.xyz, r10.x, r2
texldl r1.xyz, r1.xyzz, s0
mad r1.xyz, r10.z, r1, r2
texldl r2.xyz, r4.xyzz, s0
mad r1.xyz, r10.y, r3, r1
texldl r3.xyz, r5.xyzz, s0
mul r2.xyz, r10.x, r2
mad r2.xyz, r10.z, r3, r2
texldl r3.xyz, r6.xyzz, s0
mad r2.xyz, r10.y, r3, r2
mul r1.xyz, v5.y, r1
mov r4.xyz, v4
mad r1.xyz, v5.x, r2, r1
add r7.xy, r7, c9.w
mov r7.z, c9.x
mad r7.xyz, r10.z, r7, r9
mul r7.xyz, v5.y, r7
mad r12.xyz, v5.x, r0, r7
mad r7.xy, r8.zwzw, c2.x, r8
mad r9.xy, r11, c2.x, r8
mov r9.z, c9.x
texldl r11.xy, r9.xyzz, s1
mov r7.z, c9.x
texldl r3.xyz, r7.xyzz, s0
add r0.xz, r11.yyxw, c9.w
mov r0.y, c9.x
mul r11.xyz, r10.y, r0
texldl r0.xy, r7.xyzz, s1
add r0.yz, r0.xxyw, c9.w
mov r0.x, c9
mad r11.xyz, r10.x, r0, r11
mad r8.xy, r13, c2.x, r8
mov r8.z, c9.x
mul r2.xyz, r10.x, r3
texldl r3.xyz, r8.xyzz, s0
mad r2.xyz, r10.z, r3, r2
texldl r3.xyz, r9.xyzz, s0
mad r2.xyz, r3, r10.y, r2
mad r1.xyz, v5.z, r2, r1
texldl r13.xy, r8.xyzz, s1
mul r1.xyz, r1, v1
mul r2.xyz, v1, v1.w
add r0.xy, r13, c9.w
mov r0.z, c9.x
mad r0.xyz, r10.z, r0, r11
mad r0.xyz, v5.z, r0, r12
mul r0.xyz, r0, c10.x
mov r0.w, c8.z
dp4 r0.w, r0, r0
rsq r0.w, r0.w
mul r0.xyz, r0.w, r0
mul r0.xyz, v4.w, -r0
add r0.xyz, r0, v0
dp3 r0.w, r0, r0
rsq r0.w, r0.w
mul r0.xyz, r0.w, r0
dp3 r0.w, v5, r4
mul r4.x, r0.w, c10.y
mov r4.yz, c10.xzww
texldl r5.xyz, r4.xyzz, s2
add_pp r0.w, r5.y, -r5.x
cmp_pp r1.w, -r0, r5.x, r5.y
add r0.w, -v1, c8.z
mad r3.xyz, r1, r0.w, r2
add_pp r2.w, r5.z, -r1
cmp_pp r1.y, -r2.w, r1.w, r5.z
dp3_pp r0.w, v6, v6
rsq_pp r0.w, r0.w
dp3_pp r1.x, v7, v7
mul_pp r4.xyz, r0.w, v6
rsq_pp r0.w, r1.x
dp3 r1.x, v8, v8
mul_pp r6.xyz, r0.w, v7
texld r1.x, r1.x, s3
mov_pp r2.xyz, c0
mul r0.w, v5, c1.x
mov_pp r2.w, r1.x
if_gt r1.y, c11.x
mul_pp r1.y, r6.z, r6.x
mul_pp r1.x, r6, r6
mad_pp r1.x, r6.y, r1, r1.y
mad_pp r1.x, r6.y, r6.z, r1
mul r2.x, r1, c5
mov r2.z, c9.x
mov r2.y, c7.x
texldl r1.xyz, r2.xyzz, s4
mov r2.z, c9.x
mov r2.y, c6.x
texldl r5.xyz, r2.xyzz, s4
mul_pp r2.xyz, r1, c0
else
cmp_pp r1.w, -r1.y, c11.y, c11.z
rcp_pp r1.x, r1.y
mul_pp r1.xyz, r5, r1.x
abs_pp r1.w, r1
cmp_pp r5.xyz, -r1.w, c8.z, r1
endif
add_pp r1.xyz, r4, r6
dp3_pp r1.w, r1, r1
rsq_pp r1.w, r1.w
mul_pp r1.xyz, r1.w, r1
dp3_pp r1.x, r0, r1
mul_pp r4.w, r0, c11
max_pp r3.w, r1.x, c9.x
pow_pp_sat r1, r3.w, r4.w
mad_pp r1.y, -r1.x, c12.x, c12
mul_pp r1.x, r1, r1
mul_pp r1.x, r1, r1.y
mul_pp r0.w, r1.x, r0
mul_pp r1.xyz, r2, r5
mul_pp r1.xyz, r1, r0.w
mul_pp r2.xyz, r3, r2
dp3_pp r0.x, r0, r4
mad_pp r0.xyz, r2, r0.x, r1
mul_pp oC0.xyz, r0, r2.w
mov_pp oC0.w, c9.x
"
}

SubProgram "opengl " {
Keywords { "DIRECTIONAL" }
"!!GLSL"
}

SubProgram "d3d9 " {
Keywords { "DIRECTIONAL" }
Vector 0 [_LightColor0]
Float 1 [_SpecPower]
Float 2 [_TileSize]
Float 3 [_Repetition]
Float 4 [_Indent]
Float 5 [_DiamondSpecParam1]
Float 6 [_DiamondSpecParam2]
Float 7 [_DiamondSpecParam3]
SetTexture 0 [_DiffuseTex] 2D
SetTexture 1 [_NormalMap] 2D
SetTexture 2 [_SpecColorTex] 2D
SetTexture 3 [_DiamondSpecTex] 2D
"ps_3_0
; 186 ALU, 42 TEX, 2 FLOW
dcl_2d s0
dcl_2d s1
dcl_2d s2
dcl_2d s3
def c8, 0.00195313, 0.00390625, 1.00000000, 0.50000000
def c9, 0.00000000, -0.20000000, 7.00000000, -0.50000000
def c10, 2.00000000, 0.00392157, 0.10000000, 0.00000000
def c11, 0.02999878, 0.00000000, 1.00000000, 32.00000000
def c12, 2.00000000, 3.00000000, 0, 0
dcl_texcoord0 v0.xyz
dcl_color0 v1
dcl_texcoord1 v2.xyz
dcl_texcoord2 v3.xyz
dcl_texcoord3 v4
dcl_texcoord4 v5
dcl_texcoord6 v6.xyz
dcl_texcoord7 v7.xyz
add r0.w, v4.x, c8
frc r1.x, r0.w
add r1.x, r0.w, -r1
abs r1.y, r1.x
abs r0.xyz, v3
dp3 r0.w, r0, r0
rsq r0.w, r0.w
mad r0.xyz, r0.w, r0, c9.y
frc r1.z, r1.y
add r0.w, r1.y, -r1.z
cmp r0.w, r1.x, r0, -r0
mul r1.x, r0.w, c2
frc r2.x, r1
add r1.x, r1, -r2
mul r1.x, r1, c2
mul r0.xyz, r0, c9.z
max r0.xyz, r0, c9.x
add r0.w, r0.x, r0.y
add r0.w, r0.z, r0
rcp r0.w, r0.w
mul r10.xyz, r0, r0.w
mov r0.x, c4
frc r2.y, r1.x
rcp r1.w, c3.x
mad r0.w, -r0.x, c8.y, c8.z
mul r1.xy, v2.zxzw, r1.w
frc r0.xy, r1
mul r1.xy, r0, r0.w
mov r0.x, c4
mad r11.xy, c8.x, r0.x, r1
mul r1.xy, v2.yzzw, r1.w
frc r2.zw, r1.xyxy
mul r2.zw, r0.w, r2
mad r6.xy, r11, c2.x, r2
mov r6.z, c9.x
texldl r0.xy, r6.xyzz, s1
add r0.xz, r0.yyxw, c9.w
mov r0.y, c9.x
mul r1.xyz, r10.y, r0
mov r0.x, c4
mad r8.zw, c8.x, r0.x, r2
add r0.x, v4.y, c8.w
frc r0.y, r0.x
add r2.z, r0.x, -r0.y
abs r2.w, r2.z
mad r4.xy, r8.zwzw, c2.x, r2
mov r4.z, c9.x
texldl r0.xy, r4.xyzz, s1
add r0.yz, r0.xxyw, c9.w
mov r0.x, c9
mad r1.xyz, r10.x, r0, r1
frc r3.x, r2.w
add r0.z, r2.w, -r3.x
cmp r0.z, r2, r0, -r0
mul r0.xy, v2, r1.w
mul r1.w, r0.z, c2.x
frc r0.xy, r0
mul r0.zw, r0.w, r0.xyxy
mov r0.x, c4
mad r13.xy, c8.x, r0.x, r0.zwzw
frc r8.x, r1.w
add r0.x, r1.w, -r8
mul r0.z, r0.x, c2.x
frc r8.y, r0.z
mad r3.xy, r11, c2.x, r8
mad r5.xy, r13, c2.x, r2
mov r5.z, c9.x
mov r3.z, c9.x
texldl r2.xy, r3.xyzz, s1
texldl r0.xy, r5.xyzz, s1
add r0.w, v4.z, c8
mov r2.z, c9.x
texldl r3.xyz, r3.xyzz, s0
add r0.xy, r0, c9.w
mov r0.z, c9.x
mad r0.xyz, r10.z, r0, r1
add r1.xz, r2.yyxw, c9.w
mov r1.y, c9.x
mul r7.xyz, r10.y, r1
frc r1.x, r0.w
add r0.w, r0, -r1.x
mad r2.xy, r8.zwzw, c2.x, r8
texldl r1.xy, r2.xyzz, s1
abs r1.w, r0
add r1.yz, r1.xxyw, c9.w
mov r1.x, c9
mad r9.xyz, r10.x, r1, r7
frc r2.w, r1
add r1.z, r1.w, -r2.w
cmp r0.w, r0, r1.z, -r1.z
texldl r2.xyz, r2.xyzz, s0
mad r1.xy, r13, c2.x, r8
mov r1.z, c9.x
texldl r7.xy, r1.xyzz, s1
mul r0.w, r0, c2.x
frc r8.x, r0.w
add r0.w, r0, -r8.x
mul r0.w, r0, c2.x
frc r8.y, r0.w
mul r2.xyz, r10.x, r2
texldl r1.xyz, r1.xyzz, s0
mad r1.xyz, r10.z, r1, r2
texldl r2.xyz, r4.xyzz, s0
mad r1.xyz, r10.y, r3, r1
texldl r3.xyz, r5.xyzz, s0
mul r2.xyz, r10.x, r2
mad r2.xyz, r10.z, r3, r2
texldl r3.xyz, r6.xyzz, s0
mad r2.xyz, r10.y, r3, r2
mul r1.xyz, v5.y, r1
mad r1.xyz, v5.x, r2, r1
mov r4.xyz, v4
add r7.xy, r7, c9.w
mov r7.z, c9.x
mad r7.xyz, r10.z, r7, r9
mul r7.xyz, v5.y, r7
mad r12.xyz, v5.x, r0, r7
mad r7.xy, r8.zwzw, c2.x, r8
mad r9.xy, r11, c2.x, r8
mov r9.z, c9.x
texldl r11.xy, r9.xyzz, s1
mov r7.z, c9.x
texldl r3.xyz, r7.xyzz, s0
add r0.xz, r11.yyxw, c9.w
mov r0.y, c9.x
mul r11.xyz, r10.y, r0
texldl r0.xy, r7.xyzz, s1
add r0.yz, r0.xxyw, c9.w
mov r0.x, c9
mad r11.xyz, r10.x, r0, r11
mad r8.xy, r13, c2.x, r8
mov r8.z, c9.x
mul r2.xyz, r10.x, r3
texldl r13.xy, r8.xyzz, s1
texldl r3.xyz, r8.xyzz, s0
mad r3.xyz, r10.z, r3, r2
texldl r2.xyz, r9.xyzz, s0
mad r3.xyz, r2, r10.y, r3
mad r1.xyz, v5.z, r3, r1
mul r1.xyz, r1, v1
mul r3.xyz, v1, v1.w
add r0.xy, r13, c9.w
mov r0.z, c9.x
mad r0.xyz, r10.z, r0, r11
mad r0.xyz, v5.z, r0, r12
mul r0.xyz, r0, c10.x
mov r0.w, c8.z
dp4 r0.w, r0, r0
rsq r0.w, r0.w
mul r0.xyz, r0.w, r0
mul r0.xyz, v4.w, -r0
add r0.xyz, r0, v0
dp3 r0.w, r0, r0
rsq r0.w, r0.w
mul r0.xyz, r0.w, r0
dp3 r0.w, v5, r4
mul r2.x, r0.w, c10.y
mov r2.yz, c10.xzww
texldl r2.xyz, r2.xyzz, s2
add_pp r0.w, r2.y, -r2.x
cmp_pp r1.w, -r0, r2.x, r2.y
add r0.w, -v1, c8.z
mad r4.xyz, r1, r0.w, r3
add_pp r1.x, r2.z, -r1.w
cmp_pp r1.w, -r1.x, r1, r2.z
dp3_pp r0.w, v7, v7
rsq_pp r0.w, r0.w
mul_pp r1.xyz, r0.w, v7
mov_pp r3.xyz, c0
mul r0.w, v5, c1.x
if_gt r1.w, c11.x
mul_pp r2.x, r1.z, r1
mul_pp r1.w, r1.x, r1.x
mad_pp r1.w, r1.y, r1, r2.x
mad_pp r1.w, r1.y, r1.z, r1
mul r2.x, r1.w, c5
mov r2.z, c9.x
mov r2.y, c7.x
texldl r3.xyz, r2.xyzz, s3
mov r2.z, c9.x
mov r2.y, c6.x
texldl r2.xyz, r2.xyzz, s3
mul_pp r3.xyz, r3, c0
else
rcp_pp r2.w, r1.w
cmp_pp r1.w, -r1, c11.y, c11.z
mul_pp r2.xyz, r2, r2.w
abs_pp r1.w, r1
cmp_pp r2.xyz, -r1.w, c8.z, r2
endif
add_pp r1.xyz, v6, r1
dp3_pp r1.w, r1, r1
rsq_pp r1.w, r1.w
mul_pp r1.xyz, r1.w, r1
dp3_pp r1.x, r0, r1
mul_pp r3.w, r0, c11
max_pp r2.w, r1.x, c9.x
pow_pp_sat r1, r2.w, r3.w
mad_pp r1.y, -r1.x, c12.x, c12
mul_pp r1.x, r1, r1
mul_pp r1.x, r1, r1.y
mul_pp r0.w, r1.x, r0
mul_pp r1.xyz, r3, r2
mul_pp r2.xyz, r1, r0.w
mul_pp r1.xyz, r4, r3
dp3_pp r0.x, r0, v6
mad_pp oC0.xyz, r1, r0.x, r2
mov_pp oC0.w, c9.x
"
}

SubProgram "opengl " {
Keywords { "SPOT" }
"!!GLSL"
}

SubProgram "d3d9 " {
Keywords { "SPOT" }
Vector 0 [_LightColor0]
Float 1 [_SpecPower]
Float 2 [_TileSize]
Float 3 [_Repetition]
Float 4 [_Indent]
Float 5 [_DiamondSpecParam1]
Float 6 [_DiamondSpecParam2]
Float 7 [_DiamondSpecParam3]
SetTexture 0 [_DiffuseTex] 2D
SetTexture 1 [_NormalMap] 2D
SetTexture 2 [_SpecColorTex] 2D
SetTexture 3 [_LightTexture0] 2D
SetTexture 4 [_LightTextureB0] 2D
SetTexture 5 [_DiamondSpecTex] 2D
"ps_3_0
; 196 ALU, 44 TEX, 2 FLOW
dcl_2d s0
dcl_2d s1
dcl_2d s2
dcl_2d s3
dcl_2d s4
dcl_2d s5
def c8, 0.00195313, 0.00390625, 1.00000000, 0.50000000
def c9, 0.00000000, -0.20000000, 7.00000000, -0.50000000
def c10, 2.00000000, 0.00392157, 0.10000000, 0.00000000
def c11, 0.00000000, 1.00000000, 0.02999878, 32.00000000
def c12, 2.00000000, 3.00000000, 0, 0
dcl_texcoord0 v0.xyz
dcl_color0 v1
dcl_texcoord1 v2.xyz
dcl_texcoord2 v3.xyz
dcl_texcoord3 v4
dcl_texcoord4 v5
dcl_texcoord6 v6.xyz
dcl_texcoord7 v7.xyz
dcl_texcoord8 v8
add r0.w, v4.x, c8
frc r1.x, r0.w
add r1.x, r0.w, -r1
abs r1.y, r1.x
abs r0.xyz, v3
dp3 r0.w, r0, r0
rsq r0.w, r0.w
mad r0.xyz, r0.w, r0, c9.y
frc r1.z, r1.y
add r0.w, r1.y, -r1.z
cmp r0.w, r1.x, r0, -r0
mul r1.x, r0.w, c2
frc r2.x, r1
add r1.x, r1, -r2
mul r1.x, r1, c2
mul r0.xyz, r0, c9.z
max r0.xyz, r0, c9.x
add r0.w, r0.x, r0.y
add r0.w, r0.z, r0
rcp r0.w, r0.w
mul r10.xyz, r0, r0.w
mov r0.x, c4
frc r2.y, r1.x
rcp r1.w, c3.x
mad r0.w, -r0.x, c8.y, c8.z
mul r1.xy, v2.zxzw, r1.w
frc r0.xy, r1
mul r1.xy, r0, r0.w
mov r0.x, c4
mad r11.xy, c8.x, r0.x, r1
mul r1.xy, v2.yzzw, r1.w
frc r2.zw, r1.xyxy
mad r6.xy, r11, c2.x, r2
mov r6.z, c9.x
texldl r0.xy, r6.xyzz, s1
add r0.xz, r0.yyxw, c9.w
mov r0.y, c9.x
mul r1.xyz, r10.y, r0
mul r2.zw, r0.w, r2
mov r0.x, c4
mad r8.zw, c8.x, r0.x, r2
add r0.x, v4.y, c8.w
frc r0.y, r0.x
add r2.z, r0.x, -r0.y
abs r2.w, r2.z
mad r4.xy, r8.zwzw, c2.x, r2
mov r4.z, c9.x
texldl r0.xy, r4.xyzz, s1
add r0.yz, r0.xxyw, c9.w
mov r0.x, c9
mad r1.xyz, r10.x, r0, r1
frc r3.x, r2.w
add r0.z, r2.w, -r3.x
cmp r0.z, r2, r0, -r0
mul r0.xy, v2, r1.w
mul r1.w, r0.z, c2.x
frc r0.xy, r0
mul r0.zw, r0.w, r0.xyxy
mov r0.x, c4
mad r13.xy, c8.x, r0.x, r0.zwzw
frc r8.x, r1.w
add r0.x, r1.w, -r8
mul r0.z, r0.x, c2.x
frc r8.y, r0.z
mad r5.xy, r13, c2.x, r2
mov r5.z, c9.x
texldl r0.xy, r5.xyzz, s1
mad r3.xy, r11, c2.x, r8
mov r3.z, c9.x
texldl r2.xy, r3.xyzz, s1
add r0.w, v4.z, c8
mov r2.z, c9.x
texldl r3.xyz, r3.xyzz, s0
add r0.xy, r0, c9.w
mov r0.z, c9.x
mad r0.xyz, r10.z, r0, r1
add r1.xz, r2.yyxw, c9.w
mov r1.y, c9.x
mul r7.xyz, r10.y, r1
frc r1.x, r0.w
add r0.w, r0, -r1.x
mad r2.xy, r8.zwzw, c2.x, r8
texldl r1.xy, r2.xyzz, s1
abs r1.w, r0
add r1.yz, r1.xxyw, c9.w
mov r1.x, c9
mad r9.xyz, r10.x, r1, r7
frc r2.w, r1
add r1.z, r1.w, -r2.w
cmp r0.w, r0, r1.z, -r1.z
texldl r2.xyz, r2.xyzz, s0
mad r1.xy, r13, c2.x, r8
mov r1.z, c9.x
texldl r7.xy, r1.xyzz, s1
mul r0.w, r0, c2.x
frc r8.x, r0.w
add r0.w, r0, -r8.x
mul r0.w, r0, c2.x
frc r8.y, r0.w
mul r2.xyz, r10.x, r2
texldl r1.xyz, r1.xyzz, s0
mad r1.xyz, r10.z, r1, r2
mad r1.xyz, r10.y, r3, r1
texldl r2.xyz, r4.xyzz, s0
texldl r3.xyz, r5.xyzz, s0
mul r2.xyz, r10.x, r2
mad r2.xyz, r10.z, r3, r2
texldl r3.xyz, r6.xyzz, s0
mad r2.xyz, r10.y, r3, r2
mul r1.xyz, v5.y, r1
mad r1.xyz, v5.x, r2, r1
add r7.xy, r7, c9.w
mov r7.z, c9.x
mad r7.xyz, r10.z, r7, r9
mul r7.xyz, v5.y, r7
mad r12.xyz, v5.x, r0, r7
mad r7.xy, r8.zwzw, c2.x, r8
mad r9.xy, r11, c2.x, r8
mov r9.z, c9.x
texldl r11.xy, r9.xyzz, s1
mov r7.z, c9.x
texldl r3.xyz, r7.xyzz, s0
add r0.xz, r11.yyxw, c9.w
mov r0.y, c9.x
mul r11.xyz, r10.y, r0
texldl r0.xy, r7.xyzz, s1
add r0.yz, r0.xxyw, c9.w
mov r0.x, c9
mad r11.xyz, r10.x, r0, r11
mad r8.xy, r13, c2.x, r8
mov r8.z, c9.x
mul r2.xyz, r10.x, r3
texldl r13.xy, r8.xyzz, s1
texldl r3.xyz, r8.xyzz, s0
mad r3.xyz, r10.z, r3, r2
texldl r4.xyz, r9.xyzz, s0
mad r3.xyz, r4, r10.y, r3
mad r1.xyz, v5.z, r3, r1
mul r1.xyz, r1, v1
mov r2.xyz, v4
mul r3.xyz, v1, v1.w
add r0.xy, r13, c9.w
mov r0.z, c9.x
mad r0.xyz, r10.z, r0, r11
mad r0.xyz, v5.z, r0, r12
mul r0.xyz, r0, c10.x
mov r0.w, c8.z
dp4 r0.w, r0, r0
rsq r0.w, r0.w
mul r0.xyz, r0.w, r0
mul r0.xyz, v4.w, -r0
add r0.xyz, r0, v0
dp3 r0.w, r0, r0
rsq r0.w, r0.w
mul r0.xyz, r0.w, r0
dp3 r0.w, v5, r2
mul r2.x, r0.w, c10.y
mov r2.yz, c10.xzww
texldl r2.xyz, r2.xyzz, s2
add_pp r0.w, r2.y, -r2.x
cmp_pp r1.w, -r0, r2.x, r2.y
add r0.w, -v1, c8.z
mad r4.xyz, r1, r0.w, r3
add_pp r2.w, r2.z, -r1
cmp_pp r1.y, -r2.w, r1.w, r2.z
dp3_pp r0.w, v6, v6
rsq_pp r0.w, r0.w
dp3_pp r1.x, v7, v7
mul_pp r5.xyz, r0.w, v6
rsq_pp r0.w, r1.x
rcp r1.x, v8.w
mad r7.xy, v8, r1.x, c8.w
mul_pp r6.xyz, r0.w, v7
dp3 r1.x, v8, v8
texld r0.w, r7, s3
cmp r1.z, -v8, c11.x, c11.y
mul_pp r0.w, r1.z, r0
texld r1.x, r1.x, s4
mul_pp r2.w, r0, r1.x
mov_pp r3.xyz, c0
mul r0.w, v5, c1.x
if_gt r1.y, c11.z
mul_pp r1.y, r6.z, r6.x
mul_pp r1.x, r6, r6
mad_pp r1.x, r6.y, r1, r1.y
mad_pp r1.x, r6.y, r6.z, r1
mul r2.x, r1, c5
mov r2.z, c9.x
mov r2.y, c7.x
texldl r1.xyz, r2.xyzz, s5
mov r2.z, c9.x
mov r2.y, c6.x
texldl r2.xyz, r2.xyzz, s5
mul_pp r3.xyz, r1, c0
else
cmp_pp r1.w, -r1.y, c11.x, c11.y
rcp_pp r1.x, r1.y
mul_pp r1.xyz, r2, r1.x
abs_pp r1.w, r1
cmp_pp r2.xyz, -r1.w, c8.z, r1
endif
add_pp r1.xyz, r5, r6
dp3_pp r1.w, r1, r1
rsq_pp r1.w, r1.w
mul_pp r1.xyz, r1.w, r1
dp3_pp r1.x, r0, r1
mul_pp r4.w, r0, c11
max_pp r3.w, r1.x, c9.x
pow_pp_sat r1, r3.w, r4.w
mad_pp r1.y, -r1.x, c12.x, c12
mul_pp r1.x, r1, r1
mul_pp r1.x, r1, r1.y
mul_pp r0.w, r1.x, r0
mul_pp r1.xyz, r3, r2
mul_pp r1.xyz, r1, r0.w
mul_pp r2.xyz, r4, r3
dp3_pp r0.x, r0, r5
mad_pp r0.xyz, r2, r0.x, r1
mul_pp oC0.xyz, r0, r2.w
mov_pp oC0.w, c9.x
"
}

SubProgram "opengl " {
Keywords { "POINT_COOKIE" }
"!!GLSL"
}

SubProgram "d3d9 " {
Keywords { "POINT_COOKIE" }
Vector 0 [_LightColor0]
Float 1 [_SpecPower]
Float 2 [_TileSize]
Float 3 [_Repetition]
Float 4 [_Indent]
Float 5 [_DiamondSpecParam1]
Float 6 [_DiamondSpecParam2]
Float 7 [_DiamondSpecParam3]
SetTexture 0 [_DiffuseTex] 2D
SetTexture 1 [_NormalMap] 2D
SetTexture 2 [_SpecColorTex] 2D
SetTexture 3 [_LightTextureB0] 2D
SetTexture 4 [_LightTexture0] CUBE
SetTexture 5 [_DiamondSpecTex] 2D
"ps_3_0
; 192 ALU, 44 TEX, 2 FLOW
dcl_2d s0
dcl_2d s1
dcl_2d s2
dcl_2d s3
dcl_cube s4
dcl_2d s5
def c8, 0.00195313, 0.00390625, 1.00000000, 0.50000000
def c9, 0.00000000, -0.20000000, 7.00000000, -0.50000000
def c10, 2.00000000, 0.00392157, 0.10000000, 0.00000000
def c11, 0.02999878, 0.00000000, 1.00000000, 32.00000000
def c12, 2.00000000, 3.00000000, 0, 0
dcl_texcoord0 v0.xyz
dcl_color0 v1
dcl_texcoord1 v2.xyz
dcl_texcoord2 v3.xyz
dcl_texcoord3 v4
dcl_texcoord4 v5
dcl_texcoord6 v6.xyz
dcl_texcoord7 v7.xyz
dcl_texcoord8 v8.xyz
add r0.w, v4.x, c8
frc r1.x, r0.w
add r1.x, r0.w, -r1
abs r1.y, r1.x
abs r0.xyz, v3
dp3 r0.w, r0, r0
rsq r0.w, r0.w
mad r0.xyz, r0.w, r0, c9.y
frc r1.z, r1.y
add r0.w, r1.y, -r1.z
cmp r0.w, r1.x, r0, -r0
mul r1.x, r0.w, c2
frc r2.x, r1
add r1.x, r1, -r2
mul r1.x, r1, c2
mul r0.xyz, r0, c9.z
max r0.xyz, r0, c9.x
add r0.w, r0.x, r0.y
add r0.w, r0.z, r0
rcp r0.w, r0.w
mul r10.xyz, r0, r0.w
mov r0.x, c4
frc r2.y, r1.x
rcp r1.w, c3.x
mad r0.w, -r0.x, c8.y, c8.z
mul r1.xy, v2.zxzw, r1.w
frc r0.xy, r1
mul r1.xy, r0, r0.w
mov r0.x, c4
mad r11.xy, c8.x, r0.x, r1
mul r1.xy, v2.yzzw, r1.w
frc r2.zw, r1.xyxy
mad r6.xy, r11, c2.x, r2
mov r6.z, c9.x
texldl r0.xy, r6.xyzz, s1
add r0.xz, r0.yyxw, c9.w
mov r0.y, c9.x
mul r1.xyz, r10.y, r0
mul r2.zw, r0.w, r2
mov r0.x, c4
mad r8.zw, c8.x, r0.x, r2
add r0.x, v4.y, c8.w
frc r0.y, r0.x
add r2.z, r0.x, -r0.y
abs r2.w, r2.z
mad r4.xy, r8.zwzw, c2.x, r2
mov r4.z, c9.x
texldl r0.xy, r4.xyzz, s1
add r0.yz, r0.xxyw, c9.w
mov r0.x, c9
mad r1.xyz, r10.x, r0, r1
frc r3.x, r2.w
add r0.z, r2.w, -r3.x
cmp r0.z, r2, r0, -r0
mul r0.xy, v2, r1.w
mul r1.w, r0.z, c2.x
frc r0.xy, r0
mul r0.zw, r0.w, r0.xyxy
mov r0.x, c4
mad r13.xy, c8.x, r0.x, r0.zwzw
frc r8.x, r1.w
add r0.x, r1.w, -r8
mul r0.z, r0.x, c2.x
frc r8.y, r0.z
mad r5.xy, r13, c2.x, r2
mov r5.z, c9.x
texldl r0.xy, r5.xyzz, s1
mad r3.xy, r11, c2.x, r8
mov r3.z, c9.x
texldl r2.xy, r3.xyzz, s1
add r0.w, v4.z, c8
mov r2.z, c9.x
texldl r3.xyz, r3.xyzz, s0
add r0.xy, r0, c9.w
mov r0.z, c9.x
mad r0.xyz, r10.z, r0, r1
add r1.xz, r2.yyxw, c9.w
mov r1.y, c9.x
mul r7.xyz, r10.y, r1
frc r1.x, r0.w
add r0.w, r0, -r1.x
mad r2.xy, r8.zwzw, c2.x, r8
texldl r1.xy, r2.xyzz, s1
abs r1.w, r0
add r1.yz, r1.xxyw, c9.w
mov r1.x, c9
mad r9.xyz, r10.x, r1, r7
frc r2.w, r1
add r1.z, r1.w, -r2.w
cmp r0.w, r0, r1.z, -r1.z
texldl r2.xyz, r2.xyzz, s0
mad r1.xy, r13, c2.x, r8
mov r1.z, c9.x
texldl r7.xy, r1.xyzz, s1
mul r0.w, r0, c2.x
frc r8.x, r0.w
add r0.w, r0, -r8.x
mul r0.w, r0, c2.x
frc r8.y, r0.w
mul r2.xyz, r10.x, r2
texldl r1.xyz, r1.xyzz, s0
mad r1.xyz, r10.z, r1, r2
mad r1.xyz, r10.y, r3, r1
texldl r2.xyz, r4.xyzz, s0
texldl r3.xyz, r5.xyzz, s0
mul r2.xyz, r10.x, r2
mad r2.xyz, r10.z, r3, r2
texldl r3.xyz, r6.xyzz, s0
mad r2.xyz, r10.y, r3, r2
mul r1.xyz, v5.y, r1
mad r1.xyz, v5.x, r2, r1
add r7.xy, r7, c9.w
mov r7.z, c9.x
mad r7.xyz, r10.z, r7, r9
mul r7.xyz, v5.y, r7
mad r12.xyz, v5.x, r0, r7
mad r7.xy, r8.zwzw, c2.x, r8
mad r9.xy, r11, c2.x, r8
mov r9.z, c9.x
texldl r11.xy, r9.xyzz, s1
mov r7.z, c9.x
texldl r3.xyz, r7.xyzz, s0
add r0.xz, r11.yyxw, c9.w
mov r0.y, c9.x
mul r11.xyz, r10.y, r0
texldl r0.xy, r7.xyzz, s1
add r0.yz, r0.xxyw, c9.w
mov r0.x, c9
mad r11.xyz, r10.x, r0, r11
mad r8.xy, r13, c2.x, r8
mov r8.z, c9.x
mul r2.xyz, r10.x, r3
texldl r13.xy, r8.xyzz, s1
texldl r3.xyz, r8.xyzz, s0
mad r3.xyz, r10.z, r3, r2
texldl r4.xyz, r9.xyzz, s0
mad r3.xyz, r4, r10.y, r3
mad r1.xyz, v5.z, r3, r1
mul r1.xyz, r1, v1
mov r2.xyz, v4
mul r3.xyz, v1, v1.w
add r0.xy, r13, c9.w
mov r0.z, c9.x
mad r0.xyz, r10.z, r0, r11
mad r0.xyz, v5.z, r0, r12
mul r0.xyz, r0, c10.x
mov r0.w, c8.z
dp4 r0.w, r0, r0
rsq r0.w, r0.w
mul r0.xyz, r0.w, r0
mul r0.xyz, v4.w, -r0
add r0.xyz, r0, v0
dp3 r0.w, r0, r0
rsq r0.w, r0.w
mul r0.xyz, r0.w, r0
dp3 r0.w, v5, r2
mul r2.x, r0.w, c10.y
mov r2.yz, c10.xzww
texldl r2.xyz, r2.xyzz, s2
add_pp r0.w, r2.y, -r2.x
cmp_pp r1.w, -r0, r2.x, r2.y
add r0.w, -v1, c8.z
mad r4.xyz, r1, r0.w, r3
add_pp r2.w, r2.z, -r1
cmp_pp r1.y, -r2.w, r1.w, r2.z
dp3_pp r0.w, v6, v6
rsq_pp r0.w, r0.w
dp3_pp r1.x, v7, v7
rsq_pp r1.x, r1.x
mul_pp r5.xyz, r0.w, v6
mul_pp r6.xyz, r1.x, v7
dp3 r1.x, v8, v8
texld r0.w, v8, s4
texld r1.x, r1.x, s3
mul r2.w, r1.x, r0
mov_pp r3.xyz, c0
mul r0.w, v5, c1.x
if_gt r1.y, c11.x
mul_pp r1.y, r6.z, r6.x
mul_pp r1.x, r6, r6
mad_pp r1.x, r6.y, r1, r1.y
mad_pp r1.x, r6.y, r6.z, r1
mul r2.x, r1, c5
mov r2.z, c9.x
mov r2.y, c7.x
texldl r1.xyz, r2.xyzz, s5
mov r2.z, c9.x
mov r2.y, c6.x
texldl r2.xyz, r2.xyzz, s5
mul_pp r3.xyz, r1, c0
else
cmp_pp r1.w, -r1.y, c11.y, c11.z
rcp_pp r1.x, r1.y
mul_pp r1.xyz, r2, r1.x
abs_pp r1.w, r1
cmp_pp r2.xyz, -r1.w, c8.z, r1
endif
add_pp r1.xyz, r5, r6
dp3_pp r1.w, r1, r1
rsq_pp r1.w, r1.w
mul_pp r1.xyz, r1.w, r1
dp3_pp r1.x, r0, r1
mul_pp r4.w, r0, c11
max_pp r3.w, r1.x, c9.x
pow_pp_sat r1, r3.w, r4.w
mad_pp r1.y, -r1.x, c12.x, c12
mul_pp r1.x, r1, r1
mul_pp r1.x, r1, r1.y
mul_pp r0.w, r1.x, r0
mul_pp r1.xyz, r3, r2
mul_pp r1.xyz, r1, r0.w
mul_pp r2.xyz, r4, r3
dp3_pp r0.x, r0, r5
mad_pp r0.xyz, r2, r0.x, r1
mul_pp oC0.xyz, r0, r2.w
mov_pp oC0.w, c9.x
"
}

SubProgram "opengl " {
Keywords { "DIRECTIONAL_COOKIE" }
"!!GLSL"
}

SubProgram "d3d9 " {
Keywords { "DIRECTIONAL_COOKIE" }
Vector 0 [_LightColor0]
Float 1 [_SpecPower]
Float 2 [_TileSize]
Float 3 [_Repetition]
Float 4 [_Indent]
Float 5 [_DiamondSpecParam1]
Float 6 [_DiamondSpecParam2]
Float 7 [_DiamondSpecParam3]
SetTexture 0 [_DiffuseTex] 2D
SetTexture 1 [_NormalMap] 2D
SetTexture 2 [_SpecColorTex] 2D
SetTexture 3 [_LightTexture0] 2D
SetTexture 4 [_DiamondSpecTex] 2D
"ps_3_0
; 187 ALU, 43 TEX, 2 FLOW
dcl_2d s0
dcl_2d s1
dcl_2d s2
dcl_2d s3
dcl_2d s4
def c8, 0.00195313, 0.00390625, 1.00000000, 0.50000000
def c9, 0.00000000, -0.20000000, 7.00000000, -0.50000000
def c10, 2.00000000, 0.00392157, 0.10000000, 0.00000000
def c11, 0.02999878, 0.00000000, 1.00000000, 32.00000000
def c12, 2.00000000, 3.00000000, 0, 0
dcl_texcoord0 v0.xyz
dcl_color0 v1
dcl_texcoord1 v2.xyz
dcl_texcoord2 v3.xyz
dcl_texcoord3 v4
dcl_texcoord4 v5
dcl_texcoord6 v6.xyz
dcl_texcoord7 v7.xyz
dcl_texcoord8 v8.xy
add r0.w, v4.x, c8
frc r1.x, r0.w
add r1.x, r0.w, -r1
abs r1.y, r1.x
abs r0.xyz, v3
dp3 r0.w, r0, r0
rsq r0.w, r0.w
mad r0.xyz, r0.w, r0, c9.y
frc r1.z, r1.y
add r0.w, r1.y, -r1.z
cmp r0.w, r1.x, r0, -r0
mul r1.x, r0.w, c2
frc r2.x, r1
add r1.x, r1, -r2
mul r1.x, r1, c2
mul r0.xyz, r0, c9.z
max r0.xyz, r0, c9.x
add r0.w, r0.x, r0.y
add r0.w, r0.z, r0
rcp r0.w, r0.w
mul r10.xyz, r0, r0.w
mov r0.x, c4
frc r2.y, r1.x
rcp r1.w, c3.x
mad r0.w, -r0.x, c8.y, c8.z
mul r1.xy, v2.zxzw, r1.w
frc r0.xy, r1
mul r1.xy, r0, r0.w
mov r0.x, c4
mad r11.xy, c8.x, r0.x, r1
mul r1.xy, v2.yzzw, r1.w
frc r2.zw, r1.xyxy
mul r2.zw, r0.w, r2
mad r6.xy, r11, c2.x, r2
mov r6.z, c9.x
texldl r0.xy, r6.xyzz, s1
add r0.xz, r0.yyxw, c9.w
mov r0.y, c9.x
mul r1.xyz, r10.y, r0
mov r0.x, c4
mad r8.zw, c8.x, r0.x, r2
add r0.x, v4.y, c8.w
frc r0.y, r0.x
add r2.z, r0.x, -r0.y
abs r2.w, r2.z
mad r4.xy, r8.zwzw, c2.x, r2
mov r4.z, c9.x
texldl r0.xy, r4.xyzz, s1
add r0.yz, r0.xxyw, c9.w
mov r0.x, c9
mad r1.xyz, r10.x, r0, r1
frc r3.x, r2.w
add r0.z, r2.w, -r3.x
cmp r0.z, r2, r0, -r0
mul r0.xy, v2, r1.w
mul r1.w, r0.z, c2.x
frc r0.xy, r0
mul r0.zw, r0.w, r0.xyxy
mov r0.x, c4
mad r13.xy, c8.x, r0.x, r0.zwzw
frc r8.x, r1.w
add r0.x, r1.w, -r8
mul r0.z, r0.x, c2.x
frc r8.y, r0.z
mad r3.xy, r11, c2.x, r8
mad r5.xy, r13, c2.x, r2
mov r5.z, c9.x
mov r3.z, c9.x
texldl r2.xy, r3.xyzz, s1
texldl r0.xy, r5.xyzz, s1
add r0.w, v4.z, c8
mov r2.z, c9.x
texldl r3.xyz, r3.xyzz, s0
add r0.xy, r0, c9.w
mov r0.z, c9.x
mad r0.xyz, r10.z, r0, r1
add r1.xz, r2.yyxw, c9.w
mov r1.y, c9.x
mul r7.xyz, r10.y, r1
frc r1.x, r0.w
add r0.w, r0, -r1.x
mad r2.xy, r8.zwzw, c2.x, r8
texldl r1.xy, r2.xyzz, s1
abs r1.w, r0
add r1.yz, r1.xxyw, c9.w
mov r1.x, c9
mad r9.xyz, r10.x, r1, r7
frc r2.w, r1
add r1.z, r1.w, -r2.w
cmp r0.w, r0, r1.z, -r1.z
texldl r2.xyz, r2.xyzz, s0
mad r1.xy, r13, c2.x, r8
mov r1.z, c9.x
texldl r7.xy, r1.xyzz, s1
mul r0.w, r0, c2.x
frc r8.x, r0.w
add r0.w, r0, -r8.x
mul r0.w, r0, c2.x
frc r8.y, r0.w
mul r2.xyz, r10.x, r2
texldl r1.xyz, r1.xyzz, s0
mad r1.xyz, r10.z, r1, r2
texldl r2.xyz, r4.xyzz, s0
mad r1.xyz, r10.y, r3, r1
texldl r3.xyz, r5.xyzz, s0
mul r2.xyz, r10.x, r2
mad r2.xyz, r10.z, r3, r2
texldl r3.xyz, r6.xyzz, s0
mad r2.xyz, r10.y, r3, r2
mul r1.xyz, v5.y, r1
mad r1.xyz, v5.x, r2, r1
mov r4.xyz, v4
add r7.xy, r7, c9.w
mov r7.z, c9.x
mad r7.xyz, r10.z, r7, r9
mul r7.xyz, v5.y, r7
mad r12.xyz, v5.x, r0, r7
mad r7.xy, r8.zwzw, c2.x, r8
mad r9.xy, r11, c2.x, r8
mov r9.z, c9.x
texldl r11.xy, r9.xyzz, s1
mov r7.z, c9.x
texldl r3.xyz, r7.xyzz, s0
add r0.xz, r11.yyxw, c9.w
mov r0.y, c9.x
mul r11.xyz, r10.y, r0
texldl r0.xy, r7.xyzz, s1
add r0.yz, r0.xxyw, c9.w
mov r0.x, c9
mad r11.xyz, r10.x, r0, r11
mad r8.xy, r13, c2.x, r8
mov r8.z, c9.x
mul r2.xyz, r10.x, r3
texldl r13.xy, r8.xyzz, s1
texldl r3.xyz, r8.xyzz, s0
mad r3.xyz, r10.z, r3, r2
texldl r2.xyz, r9.xyzz, s0
mad r3.xyz, r2, r10.y, r3
mad r1.xyz, v5.z, r3, r1
mul r1.xyz, r1, v1
mul r3.xyz, v1, v1.w
add r0.xy, r13, c9.w
mov r0.z, c9.x
mad r0.xyz, r10.z, r0, r11
mad r0.xyz, v5.z, r0, r12
mul r0.xyz, r0, c10.x
mov r0.w, c8.z
dp4 r0.w, r0, r0
rsq r0.w, r0.w
mul r0.xyz, r0.w, r0
mul r0.xyz, v4.w, -r0
add r0.xyz, r0, v0
dp3 r0.w, r0, r0
rsq r0.w, r0.w
mul r0.xyz, r0.w, r0
dp3 r0.w, v5, r4
mul r2.x, r0.w, c10.y
mov r2.yz, c10.xzww
texldl r2.xyz, r2.xyzz, s2
add_pp r0.w, r2.y, -r2.x
cmp_pp r1.w, -r0, r2.x, r2.y
add r0.w, -v1, c8.z
mad r4.xyz, r1, r0.w, r3
add_pp r1.x, r2.z, -r1.w
cmp_pp r1.w, -r1.x, r1, r2.z
dp3_pp r0.w, v7, v7
rsq_pp r0.w, r0.w
mul_pp r1.xyz, r0.w, v7
mov_pp r3.xyz, c0
texld r0.w, v8, s3
mul r2.w, v5, c1.x
if_gt r1.w, c11.x
mul_pp r2.x, r1.z, r1
mul_pp r1.w, r1.x, r1.x
mad_pp r1.w, r1.y, r1, r2.x
mad_pp r1.w, r1.y, r1.z, r1
mul r2.x, r1.w, c5
mov r2.z, c9.x
mov r2.y, c7.x
texldl r3.xyz, r2.xyzz, s4
mov r2.z, c9.x
mov r2.y, c6.x
texldl r2.xyz, r2.xyzz, s4
mul_pp r3.xyz, r3, c0
else
rcp_pp r3.w, r1.w
cmp_pp r1.w, -r1, c11.y, c11.z
mul_pp r2.xyz, r2, r3.w
abs_pp r1.w, r1
cmp_pp r2.xyz, -r1.w, c8.z, r2
endif
add_pp r1.xyz, v6, r1
dp3_pp r1.w, r1, r1
rsq_pp r1.w, r1.w
mul_pp r1.xyz, r1.w, r1
dp3_pp r1.x, r0, r1
mul_pp r4.w, r2, c11
max_pp r3.w, r1.x, c9.x
pow_pp_sat r1, r3.w, r4.w
mad_pp r1.y, -r1.x, c12.x, c12
mul_pp r1.x, r1, r1
mul_pp r1.x, r1, r1.y
mul_pp r1.w, r1.x, r2
mul_pp r1.xyz, r3, r2
mul_pp r2.xyz, r1, r1.w
mul_pp r1.xyz, r4, r3
dp3_pp r0.x, r0, v6
mad_pp r0.xyz, r1, r0.x, r2
mul_pp oC0.xyz, r0, r0.w
mov_pp oC0.w, c9.x
"
}

}
	}

#LINE 215

    }
    Fallback "Diffuse"
}
	
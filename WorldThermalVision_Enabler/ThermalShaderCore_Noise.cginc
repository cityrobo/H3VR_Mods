
struct appdata
{
	float4 vertex : POSITION;
	float3 normal : NORMAL;
};

struct v2f
{
	float4 vertex : SV_POSITION;
	float4 posWorld : TEXCOORD0;
	float3 normalDir : TEXCOORD1;
};

sampler2D _ThermalColorLUT;

float random(float2 uv, float scale)
{
    uv = uv * float2(_SinTime.x, _SinTime.y);
    return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453123) * scale;
}

v2f vert (appdata v)
{
	v2f o;
	o.vertex = UnityObjectToClipPos(v.vertex);
	o.posWorld = mul(unity_ObjectToWorld, v.vertex);
	o.normalDir = normalize( mul( float4( v.normal, 0.0 ), unity_WorldToObject ).xyz );

	return o;
}

fixed4 thermal_frag (v2f i,float thermalPowExponent,float maxTemperature,float thermalConstantTemperature, float noiseScale) : SV_Target
{
	float3 viewDirection = normalize( _WorldSpaceCameraPos.xyz - i.posWorld.xyz );
	float3 normalDirection = i.normalDir;

	float dotProduct = saturate(dot(viewDirection,normalDirection));
	float temperature = pow(dotProduct, thermalPowExponent);
	temperature = lerp(0, maxTemperature,temperature) + + thermalConstantTemperature;
    temperature += random(i.posWorld.xy, noiseScale);
	half4 col = tex2D(_ThermalColorLUT,temperature);
	return col;
}
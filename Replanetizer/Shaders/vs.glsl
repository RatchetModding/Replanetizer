#version 330 core

// Input vertex data, different for all executions of this shader.
layout(location = 0) in vec3 vertexPosition_modelspace;
layout(location = 1) in vec3 vertexNormal;
layout(location = 2) in vec2 vertexUV;
layout(location = 3) in vec4 vertexRGBA;
layout(location = 4) in float vertexTerrainLight;

struct Light {
	vec4 color1;
	vec4 direction1;
	vec4 color2;
	vec4 direction2;
};

// Allocate as many as can appear in any level
#define ALLOCATED_LIGHTS 20
layout(std140) uniform lights{
	Light light[ALLOCATED_LIGHTS];
};

// Output data ; will be interpolated for each fragment.
out vec2 UV;
out vec3 lightColor;
out float fogBlend;

// Values that stay constant for the whole mesh.
uniform mat4 WorldToView;
uniform mat4 ModelToWorld;
uniform int levelObjectType;
uniform int lightIndex;
uniform int useFog;
uniform float fogNearDistance;
uniform float fogFarDistance;
uniform float fogNearIntensity;
uniform float fogFarIntensity;
uniform vec4 staticColor;

/*
 * Seems to correspond with the game, I don't know though I just tested random things.
 */
float get_depth() {
	return 0.5f * (gl_Position.z - gl_DepthRange.far) / gl_DepthRange.far;
}

/*
 * Degree 2 Taylor Expansion of exp function
 * Closest to what is used in the game that I could figure out
 */
float quick_exp(float x) {
	float y = x * x;
	float z = y * x;
	return 1.0f + x + 0.5f * y + 0.1666f * z;
}

void main() {
	// Output position of the vertex, in clip space : MVP * position
	gl_Position = WorldToView * (ModelToWorld * vec4(vertexPosition_modelspace, 1.0f));

	vec3 normal = normalize((ModelToWorld * vec4(vertexNormal, 0.0f)).xyz);

	// UV of the vertex. No special space for this one.
	UV = vertexUV;

	lightColor = vec3(1.0f);

	int index = lightIndex;
	vec3 diffuseColor = vec3(0.0f);

	if (levelObjectType == 1) {
		index = min(ALLOCATED_LIGHTS - 1, int(vertexTerrainLight));
	}

	Light l = light[index];

	diffuseColor += max(0.0f, -dot(l.direction1.xyz, normal)) * l.color1.xyz;
	diffuseColor += max(0.0f, -dot(l.direction2.xyz, normal)) * l.color2.xyz;

	if (levelObjectType == 1 || levelObjectType == 3) {
		lightColor = vertexRGBA.xyz + diffuseColor;
	}
	else if (levelObjectType == 2 || levelObjectType == 4) {
		lightColor = staticColor.xyz + diffuseColor;
	}

	fogBlend = 0.0f;

	if (useFog == 1 && levelObjectType >= 1 && levelObjectType <= 4) {
		float depth = clamp((fogFarDistance - quick_exp(get_depth())) / (fogFarDistance - fogNearDistance), 0.0f, 1.0f);

		fogBlend = 1.0f - clamp(mix(fogFarIntensity, fogNearIntensity, depth), 0.0f, 1.0f);
	}
}

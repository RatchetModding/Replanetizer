#version 330 core

// Input vertex data, different for all executions of this shader.
layout(location = 0) in vec3 vertexPosition;
layout(location = 1) in vec3 vertexNormal;
layout(location = 2) in vec2 vertexUV;
layout(location = 3) in vec4 vertexBoneIndex; //ivec4 and uvec4 does not work, OpenGL always converts the bytes into floats
layout(location = 4) in vec4 vertexBoneWeight;

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
uniform mat4 worldToView;
uniform mat4 modelToWorld;
uniform mat4 bones[128];
uniform int lightIndex;
uniform int useFog;
uniform vec4 fogParams;
uniform vec4 staticColor;

void main() {
    vec4 position = vec4(0.0f);
    vec4 normal = vec4(0.0f);
    vec4 basePosition = vec4(vertexPosition, 1.0f);
    vec4 baseNormal = vec4(vertexNormal, 0.0f);
    for (int i = 0; i < 4; i++) {
        int index = int(vertexBoneIndex[i]);
        position += (bones[index] * basePosition) * vertexBoneWeight[i];
        normal += (bones[index] * baseNormal) * vertexBoneWeight[i];
    }

	// Output position of the vertex, in clip space : MVP * position
	gl_Position = worldToView * (modelToWorld * position);

	normal = normalize(modelToWorld * normal);

	// UV of the vertex. No special space for this one.
	UV = vertexUV;

    // Light color is precomputed on PS3 but we do it here instead.
    vec3 directionalLight = vec3(0.0f);

    if (lightIndex >= 0) {
        Light l = light[lightIndex];
        directionalLight += max(0.0f, -dot(l.direction1, normal)) * l.color1.xyz;
        directionalLight += max(0.0f, -dot(l.direction2, normal)) * l.color2.xyz;
    }

    vec3 diffuseLight = staticColor.xyz;

    lightColor = mix(diffuseLight, directionalLight, 0.5f);

	fogBlend = 0.0f;

	if (useFog == 1) {
        float depth = gl_Position.w - fogParams.x;

        depth = clamp(depth * fogParams.y, 0.0f, 1.0f);

		fogBlend = fogParams.z + depth * fogParams.w;
	}
}

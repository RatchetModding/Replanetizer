#version 330 core

// Input vertex data, different for all executions of this shader.
layout(location = 0) in vec3 vertexPosition_modelspace;
layout(location = 1) in vec3 vertexNormal;
layout(location = 2) in vec2 vertexUV;
layout(location = 3) in vec4 vertexRGBA;

layout(std140) uniform lights {
    vec4 color1;
    vec4 direction1;
    vec4 color2;
    vec4 direction2;
};

// Output data ; will be interpolated for each fragment.
out vec2 UV;
out vec4 DiffuseColor;
out vec4 BakedColor;

// Values that stay constant for the whole mesh.
uniform mat4 WorldToView;
uniform mat4 ModelToWorld;
uniform int levelObjectType;

void main(){
    // Output position of the vertex, in clip space : MVP * position
    gl_Position = WorldToView * (ModelToWorld * vec4(vertexPosition_modelspace, 1.0f));

    vec3 normal = normalize((ModelToWorld * vec4(vertexNormal, 0.0f)).xyz);

    // UV of the vertex. No special space for this one.
    UV = vertexUV;

    DiffuseColor = vec4(0.0f,0.0f,0.0f,1.0f);

    if (levelObjectType == 1 || levelObjectType == 3) {
        BakedColor = vertexRGBA;
    }

    DiffuseColor += vec4(max(0.0f,-dot(direction1.xyz,normal)) * color1.xyz,1.0f);
    DiffuseColor += vec4(max(0.0f,-dot(direction2.xyz,normal)) * color2.xyz,1.0f);
}
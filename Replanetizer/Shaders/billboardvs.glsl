#version 330 core

// Input vertex data, different for all executions of this shader.
layout(location = 0) in vec2 vertexPos;

// Values that stay constant for the whole mesh.
uniform mat4 WorldToView;
uniform vec3 up;
uniform vec3 right;
uniform vec3 position;

void main() {
    // Output position of the vertex, in clip space : MVP * position
    vec3 baseWorldPos = position + right * vertexPos.x + up * vertexPos.y;

    gl_Position = WorldToView * (vec4(baseWorldPos, 1.0f));
}

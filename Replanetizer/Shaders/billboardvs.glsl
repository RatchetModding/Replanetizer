#version 330 core

// Input vertex data, different for all executions of this shader.
layout(location = 0) in vec3 vertexPos;

// Values that stay constant for the whole mesh.
uniform mat4 WorldToView;
uniform vec3 up;
uniform vec3 right;
uniform vec3 position;

void main() {
    // Output position of the vertex, in clip space : MVP * position
    //vec3 right = normalize(vec3(WorldToView[0][0], WorldToView[1][0], WorldToView[2][0]));
    //vec3 up = normalize(vec3(WorldToView[0][1], WorldToView[1][1], WorldToView[2][1]));
    vec3 baseWorldPos = position + right * vertexPos.x + up * vertexPos.y;

    gl_Position = WorldToView * (vec4(baseWorldPos, 1.0f));
}

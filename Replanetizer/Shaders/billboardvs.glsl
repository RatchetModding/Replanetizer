#version 330 core

// Input vertex data, different for all executions of this shader.
layout(location = 0) in vec2 vertexPos;
layout(location = 1) in vec2 vertexUV;

// Output data ; will be interpolated for each fragment.
out vec2 UV;

// Values that stay constant for the whole mesh.
uniform mat4 worldToView;
uniform vec3 up;
uniform vec3 right;
uniform vec3 position;

void main() {
    // Output position of the vertex, in clip space : MVP * position
    vec3 baseWorldPos = position + right * vertexPos.x + up * vertexPos.y;

    gl_Position = worldToView * (vec4(baseWorldPos, 1.0f));

    // UV of the vertex. No special space for this one.
	UV = vertexUV;
}

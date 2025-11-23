#version 330 core

// Input vertex data, different for all executions of this shader.
layout(location = 0) in vec3 vertexPosition_modelspace;
layout(location = 1) in vec4 vertexColors;

// Output data ; will be interpolated for each fragment.
out vec4 diffuseColors;
out float fogBlend;

// Values that stay constant for the whole mesh.
uniform mat4 worldToView;
uniform int useFog;
uniform vec4 fogParams;

void main() {
    // Output position of the vertex, in clip space : MVP * position
    gl_Position = worldToView * vec4(vertexPosition_modelspace, 1.0f);

    // UV of the vertex. No special space for this one.
    diffuseColors = vertexColors;

    fogBlend = 0.0f;

    if (useFog == 1) {
        float depth = gl_Position.w - fogParams.x;

        depth = clamp(depth * fogParams.y, 0.0f, 1.0f);

        fogBlend = fogParams.z + depth * fogParams.w;
    }
}

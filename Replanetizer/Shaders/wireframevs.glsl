#version 330 core

// Input vertex data, different for all executions of this shader.
layout(location = 0) in vec3 vertexPosition_modelspace;

// Values that stay constant for the whole mesh.
uniform mat4 modelToWorld;

void main() {
	// Output position of the vertex, in clip space : MVP * position
	gl_Position = modelToWorld * vec4(vertexPosition_modelspace, 1.0f);
}

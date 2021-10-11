#version 330 core

// Input vertex data, different for all executions of this shader.
layout(location = 0) in vec3 vertexPosition_modelspace;

// Output data ; will be interpolated for each fragment.
out vec4 DiffuseColor;

// Values that stay constant for the whole mesh.
uniform mat4 WorldToView;
uniform mat4 ModelToWorld;
uniform vec4 incolor;

void main() {
	// Output position of the vertex, in clip space : MVP * position
	gl_Position = WorldToView * (ModelToWorld * vec4(vertexPosition_modelspace, 1.0f));

	// UV of the vertex. No special space for this one.
	DiffuseColor = incolor;
}

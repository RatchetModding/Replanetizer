#version 330 core

// Input vertex data, different for all executions of this shader.
layout(location = 0) in vec3 Vertex;
layout(location = 1) in vec2 vertexUV;
layout(location = 2) in vec4 idx;
layout(location = 3) in vec4 Weight;

// Output data ; will be interpolated for each fragment.
out vec2 UV;

// Values that stay constant for the whole mesh.
uniform mat4 MVP;
uniform mat4 Bone[100];

void main(){
	
	vec4 newPosition = vec4(0.0);
	int index = 0;

	for (int i = 0; i < 4; i++)
	{
		index = int(idx[i]);
		newPosition += (Bone[index] * vec4(Vertex, 1.0)) * Weight[i];
	}
	gl_Position = MVP * newPosition;
	UV = vertexUV;
}
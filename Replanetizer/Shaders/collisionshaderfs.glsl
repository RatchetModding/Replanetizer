#version 330 core

// Interpolated values from the vertex shaders
in vec4 diffuseColors;

// Ouput data
out vec4 color;

void main(){
	color = diffuseColors / 255.0f;
}
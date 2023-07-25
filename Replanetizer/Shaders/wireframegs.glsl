#version 330 core

layout(triangles) in;
layout(triangle_strip, max_vertices = 3) out;

uniform mat4 worldToView;

out vec3 dist;

void main() {
    float length10 = length(gl_in[1].gl_Position.xyz - gl_in[0].gl_Position.xyz);
    float length21 = length(gl_in[2].gl_Position.xyz - gl_in[1].gl_Position.xyz);
    float length02 = length(gl_in[0].gl_Position.xyz - gl_in[2].gl_Position.xyz);

    vec3 scaling = vec3(min(length10, length02), min(length10, length21), min(length21, length02));
    float lineWidth = 8.0f;

    dist = vec3(scaling.x / lineWidth, 0.0f, 0.0f);
    gl_Position = worldToView * gl_in[0].gl_Position;
    EmitVertex();

    dist = vec3(0.0f, scaling.y / lineWidth, 0.0f);
    gl_Position = worldToView * gl_in[1].gl_Position;
    EmitVertex();

    dist = vec3(0.0f, 0.0f, scaling.z / lineWidth);
    gl_Position = worldToView * gl_in[2].gl_Position;
    EmitVertex();

    EndPrimitive();
}

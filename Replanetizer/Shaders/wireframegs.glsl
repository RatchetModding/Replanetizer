#version 330 core

layout(triangles) in;
layout(triangle_strip, max_vertices = 3) out;

uniform vec2 resolution;

noperspective out vec3 dist;

void main() {
    vec2 p0 = 0.5f * resolution * gl_in[0].gl_Position.xy;
    vec2 p1 = 0.5f * resolution * gl_in[1].gl_Position.xy;
    vec2 p2 = 0.5f * resolution * gl_in[2].gl_Position.xy;
    vec2 v0 = p2 - p1;
    vec2 v1 = p2 - p0;
    vec2 v2 = p1 - p0;
    float area = abs(v1.x * v2.y - v1.y * v2.x);

    float lineWidth = 16.0f;

    dist = vec3(area / (lineWidth * length(v0)), 0.0f, 0.0f);
    gl_Position = gl_in[0].gl_Position;
    EmitVertex();

    dist = 0.5f * vec3(0.0f, area / (lineWidth * length(v1)), 0.0f);
    gl_Position = gl_in[1].gl_Position;
    EmitVertex();

    dist = 0.5f * vec3(0.0f, 0.0f, area / (lineWidth * length(v2)));
    gl_Position = gl_in[2].gl_Position;
    EmitVertex();

    EndPrimitive();
}

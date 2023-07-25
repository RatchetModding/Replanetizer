#version 330 core

layout(lines) in;
layout(triangle_strip, max_vertices = 4) out;

void main() {
    vec4 line0 = gl_in[0].gl_Position;
    vec4 line1 = gl_in[1].gl_Position;

    float lineWidth = 0.1f;

    vec4 quadTL = line0 + vec4(0.0f, -lineWidth, 0.0f, 0.0f);
    vec4 quadTR = line1 + vec4(0.0f, -lineWidth, 0.0f, 0.0f);
    vec4 quadBL = line0 + vec4(0.0f, lineWidth, 0.0f, 0.0f);
    vec4 quadBR = line1 + vec4(0.0f, lineWidth, 0.0f, 0.0f);

    gl_Position = quadTL;
    EmitVertex();

    gl_Position = quadBL;
    EmitVertex();

    gl_Position = quadTR;
    EmitVertex();

    gl_Position = quadBR;
    EmitVertex();

    EndPrimitive();
}

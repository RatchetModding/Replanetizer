#version 330 core

// Interpolated values from the vertex shaders
flat in uvec4 collisionMetadata;
in float fogBlend;
in vec3 v_worldPos;
in vec3 v_cameraPos;

// Ouput data
layout(location = 0) out vec4 color;
layout(location = 1) out int id;
uniform vec4 fogColor;
uniform vec3 materialColors[32];

vec3 materialColor(uint material)
{
    if (gl_FrontFacing == false)
        return vec3(0.5f);

    return materialColors[material & 0x1Fu];
}

float stripePattern(vec2 position, vec2 direction, float spacing, float width)
{
    float phase = dot(position, direction) / spacing;
    float distanceToLine = abs(fract(phase) - 0.5f);
    float antiAlias = fwidth(phase);
    return 1.0 - smoothstep(width, width + antiAlias, distanceToLine);
}

void main() {
    vec3 N = normalize(cross(dFdy(v_worldPos), dFdx(v_worldPos)));
    vec3 lightDir = normalize(v_cameraPos - v_worldPos);

    float ambient = 0.4f;
    float diffuse = max(abs(dot(N, lightDir)), 0.0f);

    float brightness = ambient + (diffuse * 0.5f);

    uint collisionType = collisionMetadata.x;
    uint geometryCategory = collisionMetadata.y;
    vec3 metadataColor;

    if (geometryCategory == 1u || geometryCategory == 4u) {
        uint material = collisionType & 0x1Fu;
        uint group = (collisionType >> 5u) & 0x3u;
        bool ignoreCameraCollision = (collisionType & 0x80u) != 0u;

        metadataColor = materialColor(material);

        float groupPattern = 0.0f;
        if (group != 0u)
        {
            vec2 stripeDirection = vec2(0.0f);

            if (group == 1u)
                stripeDirection = vec2(0.0f, 1.0f);
            else if (group == 2u)
                stripeDirection = vec2(1.0f, 0.0f);
            else if (group == 3u)
                stripeDirection = vec2(1.0f, 1.0f);

            vec3 absN = abs(N);
            vec2 projection = (absN.y >= absN.x && absN.y >= absN.z) ? v_worldPos.xz : (absN.x >= absN.z) ? v_worldPos.yz : v_worldPos.xy;

            groupPattern = stripePattern(projection, stripeDirection, 0.15f, 0.12f);
        }

        metadataColor *= mix(1.0f, 0.85f, groupPattern);
    }
    else if (geometryCategory == 2u) {
        metadataColor = (gl_FrontFacing) ? vec3(0.5f, 0.5f, 1.0f) : vec3(0.5f);
    }
    else if (geometryCategory == 3u) {
        metadataColor = (gl_FrontFacing) ? vec3(1.0f, 0.25f, 0.75f) : vec3(0.5f);
    }
    else if (geometryCategory == 5u) {
        metadataColor = vec3(1.0f, 0.0f, 1.0f);
    }
    else {
        metadataColor = vec3(0.0f);
    }

    color = vec4(metadataColor * brightness, 1.0);

    color = vec4(clamp(color.rgb, 0.0, 1.0), 1.0);
    color.xyz = (fogColor.xyz - color.xyz) * fogBlend + color.xyz;

    id = int((17u << 24u) | (geometryCategory << 8u) | collisionType);
}

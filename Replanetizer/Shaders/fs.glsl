#version 330 core

// Interpolated values from the vertex shaders
in vec2 UV;
in vec4 DiffuseColor;
in vec4 BakedColor;

// Ouput data
layout (location = 0) out vec4 color;
layout (location = 1) out int id;

// Values that stay constant for the whole mesh.
uniform sampler2D myTextureSampler;
uniform int useFog;
uniform vec4 fogColor;
uniform float fogNearDistance;
uniform float fogFarDistance;
uniform float fogNearIntensity;
uniform float fogFarIntensity;
uniform int levelObjectType;
uniform int levelObjectNumber;
uniform vec4 staticColor;

float get_depth() {
    float ndcDepth = (2.0 * gl_FragCoord.z - gl_DepthRange.near - gl_DepthRange.far) /
        (gl_DepthRange.far - gl_DepthRange.near);
    float clipDepth = ndcDepth / gl_FragCoord.w;
    return (clipDepth * 0.5f) + 0.5f;
}

/*
 * Degree 2 Taylor Expansion of exp function
 * Closest to what is used in the game that I could figure out
 */
float quick_exp(float x) {
    float y = x * x;
    float z = y * x;
    return 1.0f + x + 0.5f * y + 0.1666f * z;
}

/*
 * We use one shader for all shading types.
 * Terrain (1): RGBAs + DiffuseColor
 * Shrubs (2): StaticColor + DiffuseColor
 * Ties (3): Colorbytes + DiffuseColor
 * Mobies (4): StaticColor + DiffuseColor
 *
 * Weights of all these colors and the fog are probably not exact atm.
 *
 * Some Notes about the rendering in the game:
 * - Ratchets helmet treats light in the opposite direction, i.e.
 *   if a directional light points up, it will be treated as if it was pointing down.
 * - Ratchets helmet and the ship seem to be the only objects that
 *   have specular highlights.
 * - Terrain stores the information about which directional light is used on a per vertex
 *   basis. Replanetizer uses the light specified by the first vertex as in most cases all
 *   vertices have the same light and because the shaders are not capable of switching
 *   lights for each vertex.
 * - Fog seems to be twice as bright for ties.
 */
void main(){
    //color of the texture at the specified UV
    vec4 textureColor = texture(myTextureSampler, UV);

    if (textureColor.w < 0.1f) discard;

    vec4 ambient = vec4(1.0f);

    if (levelObjectType == 2 || levelObjectType == 4) {
        ambient = staticColor;
    } else if (levelObjectType == 1 || levelObjectType == 3) {
        ambient = BakedColor;
    }

    vec4 diffuse = vec4(0.0f);

    if (levelObjectType >= 1 && levelObjectType <= 4) {
        diffuse = DiffuseColor;
    }

    color.xyz = textureColor.xyz * (diffuse.xyz + ambient.xyz);
    color.w = textureColor.w;

    if (useFog == 1) {
        float depth = clamp((fogFarDistance - quick_exp(get_depth())) / (fogFarDistance - fogNearDistance), 0.0f, 1.0f);

        float intensity = clamp(depth * fogNearIntensity + (1.0f - depth) * fogFarIntensity, 0.0f, 1.0f);

        if (levelObjectType == 3) {
            color.xyz = mix(2.0f * fogColor.xyz, color.xyz, intensity);
        } else {
            color.xyz = mix(fogColor.xyz, color.xyz, intensity);
        }
    }

    id = (levelObjectType << 24) | levelObjectNumber;
}
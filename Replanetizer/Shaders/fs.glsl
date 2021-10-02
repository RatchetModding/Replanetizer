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
 * - Ratchets Helmet and the Ship treat light in the opposite direction, i.e.
 *   if a directional light points up, it will be treated as if it was pointing down.
 *   This seems to only apply to these two and these two are the only object that seem to
 *   have specular highlights.
 * - Terrain uses directional lights aswell but it is unknown where the corresponding
 *   integer is stored. In general, the terrain uses the same directional lights as
 *   the objects around them.
 * - Mobies and Shrubs need their vertex normals to always be flipped while the same
 *   does not seem to hold for ties. In fact, sometimes not both lights in a pair
 *   of directional lights seem to be used for ties.
 */
void main(){
    //color of the texture at the specified UV
    vec4 textureColor = texture(myTextureSampler, UV);

    vec4 ambient = vec4(0.5f);

    if (levelObjectType == 2 || levelObjectType == 4) {
        ambient = staticColor;
    } else if (levelObjectType == 1 || levelObjectType == 3) {
        ambient = BakedColor;
    } else if (levelObjectType == 11) {
        ambient = vec4(1.0f);
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

        color.xyz = intensity * color.xyz + (1.0f - intensity) * fogColor.xyz;
    }

    id = (levelObjectType << 24) | levelObjectNumber;
}
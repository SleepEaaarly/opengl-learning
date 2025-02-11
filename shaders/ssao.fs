#version 330 core

out float FragColor;
in vec2 texCoords;

uniform sampler2D gPositionDepth;
uniform sampler2D gNormal;
uniform sampler2D texNoise;

uniform vec3 samples[64];
uniform mat4 projection;

const vec2 noiseScale = vec2(800.0 / 4.0, 600.0 / 4.0);

int kernalSize = 64;
float radius = 1.0f;

void main() {
    vec3 normal = texture(gNormal, texCoords).rgb;
    vec3 rand_vec = texture(texNoise, texCoords * noiseScale).rgb;
    vec3 tangent = normalize(rand_vec - dot(normal, rand_vec) * normal);
    vec3 bitangent = cross(normal, tangent);
    vec3 fragPos = texture(gPositionDepth, texCoords).rgb;

    mat3 TBN = mat3(tangent, bitangent, normal);

    float occlusion = 0.f;

    for (int i = 0; i < kernalSize; i++) {
        vec3 sam = TBN * samples[i];
        sam = fragPos + sam * radius;

        vec4 offset = vec4(sam, 1.0);
        offset = projection * offset;
        offset.xyz /= offset.w;
        offset.xyz = offset.xyz * 0.5 + 0.5;

        // Both values are negative
        float sam_proj_depth = -texture(gPositionDepth, offset.xy).a;    // Because in view coords, positive z axis points to the opposite direction of camera.
        float sam_depth = sam.z;

        float range_check = smoothstep(0.0, 1.0, radius / abs(fragPos.z - sam_proj_depth));
        occlusion += ((sam_proj_depth > sam_depth) ? 1.0 : 0.0) * range_check;
    }
    
    occlusion /= kernalSize;
    
    // To multiply ambient light directly
    occlusion = 1.0 - occlusion;
    FragColor = occlusion;
}

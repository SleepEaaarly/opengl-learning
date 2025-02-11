#version 330 core

in vec2 texCoords;

uniform sampler2D depthMap;
uniform float near_plane;
uniform float far_plane;

float LinearizeDepth(float depth) {     // only used for shadowMap visualization
    float z = depth * 2.0 - 1.0; // Back to NDC 
    return (2.0 * near_plane * far_plane) / (far_plane + near_plane - z * (far_plane - near_plane));
}

void main() {
    float depth = texture(depthMap, texCoords).r;
    // gl_FragColor = vec4(vec3(LinearizeDepth(depth)), 1.0f); // perspec depth
    gl_FragColor = vec4(vec3(depth), 1.0f); // ortho depth
}
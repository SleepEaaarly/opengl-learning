#version 330 core

out vec4 FragColor;
in vec2 texCoords;

uniform sampler2D scene;
uniform sampler2D bloomBlur;
uniform bool bloom;
uniform float exposure;

void main() {
    const float gamma = 2.2;
    vec3 scene_color = texture(scene, texCoords).rgb;
    vec3 blur_color = texture(bloomBlur, texCoords).rgb;

    vec3 hdr_color = scene_color;
    if (bloom)
        hdr_color += blur_color;

    vec3 result = vec3(1.0) - exp(-hdr_color * exposure);

    result = pow(result, vec3(1. / gamma));

    FragColor = vec4(result, 1.0f);
}

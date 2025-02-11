#version 330 core

out vec4 FragColor;

in vec3 stPoint;
in vec2 texCoords;
in vec3 sphereCoord;

uniform sampler2D backMap;
uniform float radius;

const float alpha_upper = 0.1;
const float stepSize = 0.05f;

float sphereAlpha(vec3 rayCoord) {
    float dist = length(rayCoord - sphereCoord);
    float weight = 1.0 - dist / radius;
    return mix(0.0, alpha_upper, weight);
}

void main() {
    vec3 endPoint = texture(backMap, texCoords).xyz;
    vec3 dir = endPoint - stPoint;
    float allLen = length(dir);

    vec3 deltaDir = normalize(dir) * stepSize;
    float deltaDirLen = length(deltaDir);
    vec3 rayCoord = stPoint;

    vec4 colorAcum = vec4(0.0);
    float alphaAcum = 0.0;
    vec3 sample_color = vec3(1.0, 0.0, 0.0);

    float len = 0.0;

    while(len < allLen && alphaAcum < 1.0) {
        float sample_alpha = sphereAlpha(rayCoord);
        colorAcum.rgb += (1.0 - colorAcum.a) * sample_color * sample_alpha;
        colorAcum.a += (1.0 - colorAcum.a) * sample_alpha;
        
        rayCoord += deltaDir;
        len += deltaDirLen;
    }
    

    FragColor = colorAcum;
}

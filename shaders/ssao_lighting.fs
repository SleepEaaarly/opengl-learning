#version 330 core

out vec4 fragColor;
in vec2 texCoords;

uniform sampler2D gPositionDepth;
uniform sampler2D gNormal;
uniform sampler2D gAlbedo;
uniform sampler2D ssao;

struct Light {
    vec3 Position;
    vec3 Color;
    
    float Linear;
    float Quadratic;
};
uniform Light light;

void main() {
    vec3 fragPos = texture(gPositionDepth, texCoords).rgb;
    vec3 normal = texture(gNormal, texCoords).rgb;
    vec3 diffuse_color = texture(gAlbedo, texCoords).rgb;
    float ambientOcclusion = texture(ssao, texCoords).r;

    // vec3 ambient = vec3(0.3 * ambientOcclusion);
    vec3 ambient = vec3(0.3);
    vec3 viewDir = normalize(-fragPos);

    vec3 lightDir = normalize(light.Position - fragPos);
    vec3 diffuse = max(dot(normal, lightDir), 0.0) * diffuse_color * light.Color;
    
    vec3 halfwayDir = normalize(viewDir + lightDir);
    float spec = pow(max(0.0, dot(halfwayDir, normal)), 8.0);
    vec3 specular = spec * light.Color;

    float distance = length(light.Position - fragPos);
    float attenuation = 1.0 / (1.0 + light.Linear * distance + light.Quadratic * distance * distance);
    
    vec3 lighting = (ambient + diffuse + specular) * attenuation;

    fragColor = vec4(lighting, 1.0);
}

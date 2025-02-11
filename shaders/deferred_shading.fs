#version 330 core

out vec4 FragColor;
in vec2 texCoords;

uniform sampler2D gPosition;
uniform sampler2D gNormal;
uniform sampler2D gAlbedoSpec;

uniform vec3 viewPos;

struct Light {
    vec3 position;
    vec3 color;

    float linear;
    float quadratic;
};

const int NR_LIGHTS = 32;
uniform Light lights[NR_LIGHTS];

void main() {
    // Retrieve data from gbuffer
    vec3 FragPos = texture(gPosition, texCoords).rgb;
    vec3 Normal = texture(gNormal, texCoords).rgb;
    vec3 Diffuse = texture(gAlbedoSpec, texCoords).rgb;
    float Specular = texture(gAlbedoSpec, texCoords).a;
    
    // Then calculate lighting as usual
    vec3 lighting  = Diffuse * 0.1; // hard-coded ambient component
    vec3 viewDir  = normalize(viewPos - FragPos);
    for(int i = 0; i < NR_LIGHTS; ++i) {
        // Diffuse
        vec3 lightDir = normalize(lights[i].position - FragPos);
        vec3 diffuse = max(dot(Normal, lightDir), 0.0) * Diffuse * lights[i].color;
        // Specular
        vec3 halfwayDir = normalize(lightDir + viewDir);  
        float spec = pow(max(dot(Normal, halfwayDir), 0.0), 16.0);
        vec3 specular = lights[i].color * spec * Specular;
        // Attenuation
        float distance = length(lights[i].position - FragPos);
        float attenuation = 1.0 / (1.0 + lights[i].linear * distance + lights[i].quadratic * distance * distance);
        diffuse *= attenuation;
        specular *= attenuation;
        lighting += diffuse + specular;
    }  
    // FragColor = vec4(vec3(Specular, Specular, Specular), 1.0);
    FragColor = vec4(lighting, 1.0);
}

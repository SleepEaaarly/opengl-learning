#version 330 core

layout (location = 0) out vec4 FragColor;
layout (location = 1) out vec4 BrightColor;

in VS_OUT {
    vec3 fragPos;
    vec3 normal;
    vec2 texCoords;
} fs_in;

struct Light {
    vec3 position;
    vec3 color;
};

uniform Light lights[4];
uniform sampler2D diffuseTexture;
uniform vec3 viewPos;


void main() {
    float amb_cof = 0.0f, dif_cof = 1.0f, spe_cof = 2.0f;
    vec3 color = texture(diffuseTexture, fs_in.texCoords).rgb;

    // ambient light
    vec3 ambient = amb_cof * color;

    vec3 view_dir = normalize(viewPos - fs_in.fragPos);
    vec3 normal = normalize(fs_in.normal);      
    vec3 lighting = vec3(0.0);  
    for (int i = 0; i < 4; i++) {
        vec3 light_dir = normalize(lights[i].position - fs_in.fragPos);
        vec3 light_color = lights[i].color;

        // diffuse light
        float dif = max(0.0, dot(light_dir, normal)) * dif_cof;
        vec3 diffuse = dif * light_color * color;

        // specular light
        vec3 half_dir = normalize(light_dir + view_dir);
        float spe = max(0.0, pow(dot(half_dir, normal), 32)) * spe_cof;
        vec3 specular = spe * light_color * color;

        // result
        float distance = length(fs_in.fragPos - lights[i].position);
        vec3 result = (diffuse + specular) / distance / distance;
        lighting += result;
    }

    vec3 result = ambient + lighting;
    // turn into grey graph
    float brightness = dot(result, vec3(0.2126, 0.7152, 0.0722));
    if (brightness > 1.0f) {
        BrightColor = vec4(result, 1.0f);
    } else {
        BrightColor = vec4(0.0f, 0.0f, 0.0f, 1.0f);
    }

    FragColor = vec4(result, 1.0f);
}


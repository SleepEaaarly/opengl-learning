#version 330 core

out vec4 FragColor;
in vec2 TexCoords;
in vec3 WorldPos;
in vec3 Normal;

uniform sampler2D albedoMap;
uniform sampler2D normalMap;
uniform sampler2D metallicMap;
uniform sampler2D roughnessMap;
uniform sampler2D aoMap;

uniform vec3 lightPositions[4];
uniform vec3 lightColors[4];

uniform vec3 camPos;

const float PI = 3.14159265359;

vec3 getNormalFromNormalMap();
vec3 fresnelSchlick(float cosTheta, vec3 F0);
float DistributionGGX(vec3 N, vec3 H, float roughness);
float GeometrySchlickGGX(float NdotV, float roughness);
float GeometrySmith(vec3 N, vec3 V, vec3 L, float roughness);


void main() {
    vec3 albedo = pow(texture(albedoMap, TexCoords).rgb, vec3(2.2));
    vec3 N = getNormalFromNormalMap();
    float metallic = texture(metallicMap, TexCoords).r;
    float roughness = texture(roughnessMap, TexCoords).r;
    float ao = texture(aoMap, TexCoords).r;

    vec3 V = normalize(camPos - WorldPos);

    vec3 F0 = vec3(0.04);
    F0 = mix(F0, albedo, metallic);

    vec3 Lo = vec3(0.0);
    for (int i = 0; i < 4; i++) {
        vec3 L = normalize(lightPositions[i] - WorldPos);
        vec3 H = normalize(V + L);
        float distance = length(lightPositions[i] - WorldPos);
        vec3 radiance = lightColors[i] / (distance * distance);

        float NDF = DistributionGGX(N, H, roughness);
        vec3 F = fresnelSchlick(max(0.0, dot(H, V)), F0);
        float G = GeometrySmith(N, V, L, roughness);

        vec3 numerator = NDF * F * G;
        float denominator = 4.0 * max(0.0, dot(N, L)) * max(0.0, dot(N, V)) + 0.0001;
        vec3 specular = numerator / denominator;

        vec3 ks = F;
        vec3 kd = vec3(1.0) - ks;
        
        // multiply kD by the inverse metalness such that only non-metals 
        // have diffuse lighting, or a linear blend if partly metal (pure metals
        // have no diffuse light).
        kd *= 1.0 - metallic;

        // note that we already multiplied the BRDF by the Fresnel (kS) so we won't multiply by kS again
        Lo += (kd * albedo / PI + specular) * radiance * dot(N, L);
    }

    // ambient lighting (note that the next IBL tutorial will replace 
    // this ambient lighting with environment lighting).    
    vec3 ambient = vec3(0.03) * albedo * ao;

    vec3 color = ambient + Lo;

    // HDR tonemapping
    color = color / (color + vec3(1.0));
    // gamma correct
    color = pow(color, vec3(1.0 / 2.2));
    
    FragColor = vec4(color, 1.0);
}


vec3 getNormalFromNormalMap() {
    vec3 normal = texture(normalMap, TexCoords).xyz * 2.0 - 1.0;

    vec3 delta_w1 = dFdx(WorldPos);
    vec3 delta_w2 = dFdy(WorldPos);
    float delta_t1 = dFdx(TexCoords).t;
    float delta_t2 = dFdy(TexCoords).t;

    vec3 N = normalize(Normal);
    vec3 T = normalize(delta_w1 * delta_t2 - delta_w2 * delta_t1);
    T = normalize(T - N * dot(N, T));
    vec3 B = cross(N, T);
    mat3 TBN = mat3(T, B, N);

    return normalize(TBN * normal);
}


vec3 fresnelSchlick(float cosTheta, vec3 F0) {
    return F0 + (1.0 - F0) * pow(clamp(1 - cosTheta, 0.0, 1.0), 5);
}

float DistributionGGX(vec3 N, vec3 H, float roughness) {
    float a = roughness * roughness;
    float NdotH = dot(N, H);
    float NdotH2 = NdotH * NdotH;
    float temp = a / (NdotH2 * (a * a - 1) + 1);
    return temp * temp / PI;
}

float GeometrySchlickGGX(float NdotV, float roughness) {
    float r = roughness + 1;
    float k = r * r / 8;
    return NdotV / (NdotV * (1.0 - k) + k);
}

float GeometrySmith(vec3 N, vec3 V, vec3 L, float roughness) {
    float NdotV = max(0.0, dot(N, V));
    float NdotL = max(0.0, dot(N, L));

    return GeometrySchlickGGX(NdotV, roughness) * GeometrySchlickGGX(NdotL, roughness);
}
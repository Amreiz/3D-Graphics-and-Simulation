#version 330

uniform vec4 uEyePosition;
uniform vec4 uAmbient;

in vec4 oNormal;
in vec4 oSurfacePosition;

out vec4 FragColour;

struct LightProperties {
	vec4 Position;
	vec3 AmbientLight;
	vec3 DiffuseLight;
	vec3 SpecularLight;
};

uniform LightProperties uLight;

struct MaterialProperties {
	vec3 AmbientReflectivity;
	vec3 DiffuseReflectivity;
	vec3 SpecularReflectivity;
	float Shininess;
};

uniform MaterialProperties uMaterial;


void main()
{
	vec4 lightDir = normalize(uLight.Position - oSurfacePosition);

	vec4 eyeDirection = normalize(uEyePosition - oSurfacePosition);
	vec4 reflectedVector = reflect(-lightDir, oNormal);

	float specularFactor = pow(max(dot( reflectedVector, eyeDirection), 0.0), uMaterial.Shininess);
	float diffuseFactor = max(dot(oNormal, lightDir), 0);

FragColour = vec4(uLight.AmbientLight * uMaterial.AmbientReflectivity +
uLight.DiffuseLight * uMaterial.DiffuseReflectivity * diffuseFactor +
uLight.SpecularLight * uMaterial.SpecularReflectivity * specularFactor, 1);}

#version 330

uniform vec4 uEyePosition;
uniform sampler2D uTexture;




void main()
{
		for(int i = 0; i < 3; ++i)
		{
			vec4 lightDir = normalize(uLight[i].Position - oSurfacePosition);
			vec4 reflectedVector = reflect(-lightDir, oNormal);
			float diffuseFactor = max(dot(oNormal, lightDir), 0);
			float specularFactor = pow(max(dot( reflectedVector, eyeDirection), 0.0), uMaterial.Shininess);
			FragColour = FragColour + vec4(uLight[i].AmbientLight * uMaterial.AmbientReflectivity + uLight[i].DiffuseLight * uMaterial.DiffuseReflectivity * diffuseFactor + 
			uLight[i].SpecularLight * uMaterial.SpecularReflectivity * specularFactor, 1);
		}
}
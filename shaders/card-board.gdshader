shader_type canvas_item;

uniform bool doMix;
uniform vec4 mixColor:source_color;

void fragment() 
{
	if (distance(UV, vec2(0.5,0.5)) > 0.5) 
	{
		COLOR = vec4(0.0);
	}
	else 
	{
		if(doMix)
		{
			COLOR = mix(texture(TEXTURE,UV), mixColor, 0.5f);
		}
		else{
			COLOR = texture(TEXTURE,UV);
		}
		
	}
}

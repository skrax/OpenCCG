shader_type canvas_item;

uniform bool doMix;
uniform bool drawOutline;
uniform vec4 mixColor:source_color;
uniform vec4 outlineColor:source_color;

void fragment() 
{
	float d = distance(UV, vec2(0.5,0.5));
	
	if (d > 0.5) 
	{
		COLOR = vec4(0.0);
	}
	else if (drawOutline && d > 0.49)
	{
		COLOR = outlineColor;
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

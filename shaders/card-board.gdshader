shader_type canvas_item;


uniform bool drawOutline;
uniform vec4 outlineColor:source_color;

uniform bool doMix;
uniform int iteration_times = 5;
uniform float speed = 1;
uniform vec4 color :source_color = vec4(0.0, .35, .5, 1.0);
uniform float intensity = .005;

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
		    lowp vec2 p = mod(UV*TAU, TAU)-250.0;
			lowp vec2 i = vec2(p);
			lowp float c = 1.0;
			lowp float t = TIME * speed;

			for (int n = 0; n < iteration_times; n++) 
			{
				i = p + vec2(cos(t - i.x) + sin(t + i.y), sin(t - i.y) + cos(t + i.x));
				c += 1.0/length(vec2(p.x / (sin(i.x+t)/intensity),p.y / (cos(i.y+t)/intensity)));
			}
			c /= float(iteration_times);
			c = 1.17-pow(c, 1.4);
			vec4 colour = vec4(pow(abs(c), 8.0));
				colour = clamp(colour + color, 0.0, 1.0);
   	
			COLOR = mix(texture(TEXTURE, UV), colour, 0.4);
		}
		else{
			COLOR = texture(TEXTURE,UV);
		}
		
	}
}

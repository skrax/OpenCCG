shader_type canvas_item;

uniform bool drawOutline;
uniform vec4 outlineColor:source_color;

void fragment() 
{
	if (drawOutline &&
	(UV.x < 0.01 || UV.x > 0.99 || UV.y < 0.01 || UV.y > 0.99))
	{
		COLOR = outlineColor;
	}
	else 
	{
		COLOR = texture(TEXTURE,UV);
	}
}

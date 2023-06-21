using Godot;

namespace OpenCCG;

public partial class CounterLabel : Label
{
    private int _value;

    public int Value
    {
        get => _value;
        set
        {
            _value = value;
            Text = $"{value}";
        }
    }
}
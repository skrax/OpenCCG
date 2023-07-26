using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;

namespace OpenCCG.GameBoard;

[GlobalClass]
public partial class AnimationQueue : Node
{
    private readonly Queue<Func<Task>> _animationQueue = new();
    private Task _currentAnimation = Task.CompletedTask;

    public override void _Process(double delta)
    {
        if (_currentAnimation.IsCompleted)
        {
            if (_animationQueue.TryDequeue(out var c))
            {
                _currentAnimation = c();
            }
        }
    }

    public void Enqueue(Func<Task> task)
    {
        _animationQueue.Enqueue(task);
    }
}
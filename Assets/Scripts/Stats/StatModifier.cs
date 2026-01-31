using System;
using System.Threading;
using UnityEngine;


// this is all so the "CountdownTimer" referenced in the "abstract class StatModifier : IDisposable" below can work
public abstract class Timer
{
    protected float initialTime;
    public float Time { get; set; }
    public bool IsRunning { get; protected set; }

    public float Progress => Time / initialTime;

    public Action OnTimerStart = delegate { };
    public Action OnTimerStop = delegate { };

    protected Timer(float value)
    {
        initialTime = value;
        IsRunning = false;
    }

    public void Start()
    {
        Time = initialTime;
        if (!IsRunning)
        {
            IsRunning = true;
            OnTimerStart.Invoke();
        }
    }

    public void Stop()
    {
        if (IsRunning)
        {
            IsRunning = false;
            OnTimerStop.Invoke();
        }
    }

    public void Resume() => IsRunning = true;
    public void Pause() => IsRunning = false;

    public abstract void Tick(float deltaTime);
}

public class CountdownTimer : Timer
{
    public CountdownTimer(float value) : base(value) { }

    public override void Tick(float deltaTime)
    {
        if (IsRunning && Time > 0)
        {
            Time -= deltaTime;
        }

        if (IsRunning && Time <= 0)
        {
            Stop();
        }
    }

    public bool IsFinished => Time <= 0;

    public void Reset() => Time = initialTime;

    public void Reset(float newTime)
    {
        initialTime = newTime;
        Reset();
    }
}

//back to the stat modifier code

public class BasicStatModifier : StatModifier
{
    readonly StatType type;
    readonly Func<int, int> operation;

    public BasicStatModifier(StatType type, float duration, Func<int, int> operation) : base(duration)
    {
        this.type = type;
        this.operation = operation;
    }

    public override void Handle(object sender, Query query)
    {
        if (query.StatType == type)
        {
            query.Value = operation(query.Value);
        }
    }
}

public abstract class StatModifier : IDisposable
{
    public event Action<StatModifier> OnDispose = delegate { };

    //this is the added timer stuff needed so the update function in StatsMediator works
    readonly CountdownTimer timer;

    protected StatModifier(float duration)
    {
        timer = new CountdownTimer(duration);
        timer.OnTimerStop += Dispose;
        timer.Start();
    }


    //this is also so the "modifier.Update(deltaTime) in StatsMediator works
    public void Update(float deltaTime) => timer?.Tick(deltaTime);



    public abstract void Handle(object sender, Query query);

    public void Dispose()
    {
        OnDispose.Invoke(obj:this);
    }
}

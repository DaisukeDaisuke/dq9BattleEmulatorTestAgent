using dq9BattleEmulatorTestAgent;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

internal class JoypadScheduler
{
    private readonly DesmumeInstance _instance;
    private readonly ConcurrentQueue<JoypadCommand> _joypadQueue = new();
    private readonly CancellationTokenSource _cts = new();

    public JoypadScheduler(DesmumeInstance instance)
    {
        _instance = instance;
    }

    public void PushInput(string key, int delayAfterMs = 1000)
    {
        _joypadQueue.Enqueue(new JoypadInputCommand(key, delayAfterMs));
    }

    public void PushSleep(int delayMs)
    {
        _joypadQueue.Enqueue(new JoypadSleepCommand(delayMs));
    }

    public void Start()
    {
         Task.Run(() => WorkerLoop(1, _cts.Token));   
    }

    private async Task WorkerLoop(int id, CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            if (_joypadQueue.TryDequeue(out JoypadCommand command))
            {
                await command.ExecuteAsync(_instance, token);

                if (command.DelayAfterMs > 0)
                {
                    try
                    {
                        await Task.Delay(command.DelayAfterMs, token);
                    }
                    catch (TaskCanceledException)
                    {
                        return;
                    }
                }
            }
            else
            {
                await Task.Delay(50, token);
            }
        }
    }

    public void Stop()
    {
        _cts.Cancel();
    }
}

internal abstract class JoypadCommand
{
    public int DelayAfterMs { get; }

    protected JoypadCommand(int delayAfterMs)
    {
        DelayAfterMs = delayAfterMs;
    }

    public abstract Task ExecuteAsync(DesmumeInstance instance, CancellationToken token);
}

internal class JoypadInputCommand : JoypadCommand
{
    public string Key { get; }

    public JoypadInputCommand(string key, int delayAfterMs) : base(delayAfterMs)
    {
        Key = key;
    }

    public override async Task ExecuteAsync(DesmumeInstance instance, CancellationToken token)
    {
        await instance.ProcessKeyAsync(Key, token);
    }
}


internal class JoypadSleepCommand : JoypadCommand
{
    public JoypadSleepCommand(int delayMs) : base(delayMs) { }

    public override Task ExecuteAsync(DesmumeInstance instance, CancellationToken token)
    {
        // 実行時は何もせず待機のみ
        return Task.CompletedTask;
    }
}


internal static class Joypad
{
    public const string Left = "left";
    public const string Right = "right";
    public const string Down = "down";
    public const string Up = "up";
    public const string Select = "select";
    public const string Start = "start";
    public const string A = "A";
    public const string B = "B";
    public const string Y = "Y";
    public const string X = "X";
    public const string L = "L";
    public const string R = "R";
    // 他のキーも定義
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;



namespace Aegis
{
    /// <summary>
    /// 이 클래스는 System.Threading.Tasks.Task의 각 메서드 기능과 완전히 동일합니다.
    /// 다만, Task 내에서 발생되는 Exception을 핸들링하여 정보를 Logger로 보내는 기능이 추가되어있습니다.
    /// Task 내에서 TaskCanceledException이 발생할 경우에는 어떠한 동작도 하지 않습니다.
    /// </summary>
    public static class AegisTask
    {
        public static Task Run(Action action)
        {
            return Task.Run(() =>
            {
                try
                {
                    action();
                }
                catch (TaskCanceledException)
                {
                }
                catch (Exception e)
                {
                    Logger.Write(LogType.Err, 1, e.ToString());
                }
            });
        }


        public static Task<TResult> Run<TResult>(Func<Task<TResult>> function)
        {
            return Task<TResult>.Run<TResult>(() =>
            {
                try
                {
                    return function();
                }
                catch (TaskCanceledException)
                {
                }
                catch (Exception e)
                {
                    Logger.Write(LogType.Err, 1, e.ToString());
                }
                return null;
            });
        }


        public static Task Run(Action action, CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                try
                {
                    action();
                }
                catch (TaskCanceledException)
                {
                }
                catch (Exception e)
                {
                    Logger.Write(LogType.Err, 1, e.ToString());
                }
            }, cancellationToken);
        }


        public static Task<TResult> Run<TResult>(Func<Task<TResult>> function, CancellationToken cancellationToken)
        {
            return Task<TResult>.Run<TResult>(() =>
            {
                try
                {
                    return function();
                }
                catch (TaskCanceledException)
                {
                }
                catch (Exception e)
                {
                    Logger.Write(LogType.Err, 1, e.ToString());
                }
                return null;
            }, cancellationToken);
        }


        public static Task RunPeriodically(Int32 period, CancellationToken cancellationToken, Action action)
        {
            return Task.Run(async () =>
            {
                while (cancellationToken.IsCancellationRequested == false)
                {
                    try
                    {
                        await Delay(period, cancellationToken);
                        action();
                    }
                    catch (TaskCanceledException)
                    {
                        break;
                    }
                    catch (Exception e)
                    {
                        Logger.Write(LogType.Err, 1, e.ToString());
                    }
                }
            });
        }


        public static Task Delay(int millisecondsDelay)
        {
            return Task.Delay(millisecondsDelay);
        }


        public static Task Delay(TimeSpan delay)
        {
            return Task.Delay(delay);
        }


        public static Task Delay(int millisecondsDelay, CancellationToken cancellationToken)
        {
            return Task.Delay(millisecondsDelay, cancellationToken);
        }


        public static Task Delay(TimeSpan delay, CancellationToken cancellationToken)
        {
            return Task.Delay(delay, cancellationToken);
        }
    }
}

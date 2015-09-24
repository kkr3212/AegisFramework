using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;



namespace Aegis.Threading
{
    /// <summary>
    /// 이 클래스는 System.Threading.Tasks.Task의 각 메서드 기능과 완전히 동일합니다.
    /// 다만, Task 내에서 발생되는 Exception을 핸들링하여 정보를 Logger로 보내는 기능이 추가되어있습니다.
    /// Task 내에서 TaskCanceledException이 발생할 경우에는 어떠한 동작도 하지 않습니다.
    /// </summary>
    public static class AegisTask
    {
        private static Int32 _taskCount = 0;

        /// <summary>
        /// 현재 실행중인 Task의 갯수를 가져옵니다.
        /// </summary>
        public static Int32 TaskCount { get { return _taskCount; } }





        public static Task Run(Action action)
        {
            return Task.Run(() =>
            {
                Interlocked.Increment(ref _taskCount);
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
                Interlocked.Decrement(ref _taskCount);
            });
        }


        public static Task<TResult> Run<TResult>(Func<Task<TResult>> function)
        {
            return Task<TResult>.Run<TResult>(() =>
            {
                Interlocked.Increment(ref _taskCount);
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
                Interlocked.Decrement(ref _taskCount);
                return null;
            });
        }


        public static Task Run(Action action, CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                Interlocked.Increment(ref _taskCount);
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
                Interlocked.Decrement(ref _taskCount);
            }, cancellationToken);
        }


        public static Task<TResult> Run<TResult>(Func<Task<TResult>> function, CancellationToken cancellationToken)
        {
            return Task<TResult>.Run<TResult>(() =>
            {
                Interlocked.Increment(ref _taskCount);
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
                Interlocked.Decrement(ref _taskCount);
                return null;
            }, cancellationToken);
        }


        public static Thread RunPeriodically(Int32 period, CancellationToken cancellationToken, Action action)
        {
            Thread thread = new Thread(async () =>
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
            thread.Start();

            return thread;
        }


        public static Task RunPeriodically(Int32 period, Func<Boolean> action)
        {
            return Task.Run(async () =>
            {
                Interlocked.Increment(ref _taskCount);
                while (true)
                {
                    try
                    {
                        await Delay(period);
                        if (action() == false)
                            break;
                    }
                    catch (Exception e)
                    {
                        Logger.Write(LogType.Err, 1, e.ToString());
                    }
                }
                Interlocked.Decrement(ref _taskCount);
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

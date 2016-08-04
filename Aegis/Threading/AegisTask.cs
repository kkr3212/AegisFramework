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
        private static int _taskCount = 0;

        /// <summary>
        /// 현재 실행중인 Task의 갯수를 가져옵니다.
        /// </summary>
        public static int TaskCount { get { return _taskCount; } }





        public static void SafeAction(Action action, Func<Exception, bool> actionOnFail = null)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                if (actionOnFail != null)
                {
                    SafeAction(() =>
                    {
                        //  로그출력 여부는 Delegator의 리턴값으로 결정한다.
                        if (actionOnFail(e) == false)
                            Logger.Err(LogMask.Aegis, e.ToString());
                    }, null);
                }
                //  actionOnFail이 정의되지 않은 경우 항상 로그를 출력한다.
                else
                    Logger.Err(LogMask.Aegis, e.ToString());
            }
        }


        public static TResult SafeAction<TResult>(Func<TResult> action, Func<Exception, bool> actionOnFail = null)
        {
            try
            {
                return action();
            }
            catch (Exception e)
            {
                if (actionOnFail != null)
                {
                    SafeAction(() =>
                    {
                        //  로그출력 여부는 Delegator의 리턴값으로 결정한다.
                        if (actionOnFail(e) == false)
                            Logger.Err(LogMask.Aegis, e.ToString());
                    }, null);
                }
                //  actionOnFail이 정의되지 않은 경우 항상 로그를 출력한다.
                else
                    Logger.Err(LogMask.Aegis, e.ToString());
            }

            return default(TResult);
        }


        public static Task Run(Action action)
        {
            return Task.Run(() =>
            {
                Interlocked.Increment(ref _taskCount);
                SafeAction(action, (e) =>
                {
                    return (e is TaskCanceledException) == true;
                });
                Interlocked.Decrement(ref _taskCount);
            });
        }


        public static Task<TResult> Run<TResult>(Func<Task<TResult>> action)
        {
            return Task<TResult>.Run<TResult>(() =>
            {
                Interlocked.Increment(ref _taskCount);
                SafeAction<Task<TResult>>(() =>
                {
                    return action();
                }, (e) =>
                {
                    return (e is TaskCanceledException) == true;
                });
                Interlocked.Decrement(ref _taskCount);
                return null;
            });
        }


        public static Task Run(Action action, CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                Interlocked.Increment(ref _taskCount);
                SafeAction(action, (e) =>
                {
                    return (e is TaskCanceledException) == true;
                });
                Interlocked.Decrement(ref _taskCount);
            }, cancellationToken);
        }


        public static Task<TResult> Run<TResult>(Func<Task<TResult>> action, CancellationToken cancellationToken)
        {
            return Task<TResult>.Run<TResult>(() =>
            {
                Interlocked.Increment(ref _taskCount);
                SafeAction<Task<TResult>>(() =>
                {
                    return action();
                }, (e) =>
                {
                    return (e is TaskCanceledException) == true;
                });
                Interlocked.Decrement(ref _taskCount);
                return null;
            }, cancellationToken);
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

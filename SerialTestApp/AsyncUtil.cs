using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SerialTestApp
{
    /// <summary>
    /// Helper class for calling async methods.
    /// </summary>
    internal class AsyncUtil
    {
        private static readonly TaskFactory _taskFactory =
            new TaskFactory(
                CancellationToken.None,
                TaskCreationOptions.None,
                TaskContinuationOptions.None,
                TaskScheduler.Default);


        /// <summary>
        /// Executes an asynchronous method which has a <c>void</c> return type
        /// synchronously.
        /// </summary>
        /// <param name="task">The asynchronous method to execute.</param>
        /// <remarks><para>The method can be called as follows:
        /// </para>
        /// <code>
        /// AsyncUtil.RunSync(()=> AsyncMethod());
        /// </code>
        /// </remarks>
        public static void RunSync(Func<Task> task) =>
            _taskFactory.StartNew(task)
            .Unwrap()
            .GetAwaiter()
            .GetResult();


        /// <summary>
        /// Executes an asynchronous method which has a <typeparamref name="TResult"/> return
        /// type synchronously.
        /// </summary>
        /// <typeparam name="TResult">The return type of the asynchronous method.</typeparam>
        /// <param name="task">The asynchronous method to execute.</param>
        /// <returns>The result of the asynchronous method.</returns>
        /// <remarks><para>
        /// The method can be called as follows:
        /// </para>
        /// <code>
        /// AsyncUtil.RunSync(()=>AyncMethod{T}());
        /// </code>
        /// </remarks>
        public static TResult RunAsync<TResult>(Func<Task<TResult>> task) =>
            _taskFactory.StartNew(task)
            .Unwrap()
            .GetAwaiter()
            .GetResult();
    }
}

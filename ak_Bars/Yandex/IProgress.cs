﻿using System;

namespace ak_Bars.Yandex
{
    /// <summary>
    /// Represents interface to notify UI about async operation progress.
    /// </summary>
    public interface IProgress
    {
        /// <summary>
        /// Updates the progress value.
        /// </summary>
        /// <param name="current">The current value.</param>
        /// <param name="total">The total value.</param>
        void UpdateProgress(long current, long total, string filename);
    }

    /// <summary>
    /// Represents entity to notify UI about async operation progress.
    /// </summary>
    public class AsyncProgress : IProgress
    {
        /// <summary>
        /// The progress action.
        /// </summary>
        private readonly Action<long, long, string> progressAction;

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncProgress"/> class.
        /// </summary>
        /// <param name="progressAction">The progress action.</param>
        public AsyncProgress(Action<long, long, string> progressAction)
        {
            this.progressAction = progressAction;
        }

        /// <summary>
        /// Updates the progress value.
        /// </summary>
        /// <param name="current">The current value.</param>
        /// <param name="total">The total value.</param>
        public void UpdateProgress(long current, long total, string filename)
        {
            this.progressAction(current, total, filename);
        }
    }
}

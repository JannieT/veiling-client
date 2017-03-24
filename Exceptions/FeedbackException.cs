using System;

namespace Veiling
{
    public abstract class FeedbackException : Exception
    {
        public abstract string Feedback { get; }

        public FeedbackException()
        {
        }

        public FeedbackException(string message)
            : base(message)
        {
        }

        public FeedbackException(string message, Exception inner)
            : base(message, inner)
        {
        }

    }

}

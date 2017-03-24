using System;

namespace Veiling
{
    public class NoNetworkException : FeedbackException
    {
        public override string Feedback { get; } = "Geen data konneksie";

        public NoNetworkException()
        {
        }

        public NoNetworkException(string message)
            : base(message)
        {
        }

        public NoNetworkException(string message, Exception inner)
            : base(message, inner)
        {
        }

    }
}

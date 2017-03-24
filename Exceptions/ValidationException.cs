using System;

namespace Veiling
{
    public class ValidationException : FeedbackException
    {
        public override string Feedback { get; } = "Die inligting lyk nie reg nie. Ons kan dit nie verwerk nie.";

        public ValidationException()
        {
        }

        public ValidationException(string message)
            : base(message)
        {
        }

        public ValidationException(string message, Exception inner)
            : base(message, inner)
        {
        }

    }
}

using System;

namespace Veiling
{
    public class AuthenticationException: FeedbackException
    {
        public override string Feedback { get; } = "Versoek verwerp. Kyk by jou verstellings en maak seker jou api sleutel is reg ingetik.";

        public AuthenticationException()
        {
        }

        public AuthenticationException(string message)
            : base(message)
        {
        }

        public AuthenticationException(string message, Exception inner)
            : base(message, inner)
        {
        }

    }
}

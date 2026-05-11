using System.Threading.Tasks;
using Abp.Dependency;
using Castle.Core.Logging;

namespace BelKhidmah.Otp
{
    public class NullEmailOtpSender : IEmailOtpSender, ITransientDependency
    {
        public ILogger Logger { get; set; } = NullLogger.Instance;

        public Task SendAsync(string recipient, string code)
        {
            Logger.WarnFormat("[OTP-EMAIL] TO={0} CODE={1} — wire up a real IEmailOtpSender implementation.", recipient, code);
            return Task.CompletedTask;
        }
    }

    public class NullSmsOtpSender : ISmsOtpSender, ITransientDependency
    {
        public ILogger Logger { get; set; } = NullLogger.Instance;

        public Task SendAsync(string recipient, string code)
        {
            Logger.WarnFormat("[OTP-SMS] TO={0} CODE={1} — wire up a real ISmsOtpSender implementation.", recipient, code);
            return Task.CompletedTask;
        }
    }
}

using System.Threading.Tasks;

namespace BelKhidmah.Otp
{
    public interface IOtpSender
    {
        Task SendAsync(string recipient, string code);
    }

    public interface IEmailOtpSender : IOtpSender { }

    public interface ISmsOtpSender : IOtpSender { }
}

using BelKhidmah.Debugging;

namespace BelKhidmah
{
    public class BelKhidmahConsts
    {
        public const string LocalizationSourceName = "BelKhidmah";

        public const string ConnectionStringName = "Default";

        public const bool MultiTenancyEnabled = true;


        /// <summary>
        /// Default pass phrase for SimpleStringCipher decrypt/encrypt operations
        /// </summary>
        public static readonly string DefaultPassPhrase =
            DebugHelper.IsDebug ? "gsKxGZ012HLL3MI5" : "d4e6355dcffd428c82e940514dc5a663";
    }
}

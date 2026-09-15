namespace BelKhidmah.Authentication.Biometric
{
    public enum BiometricErrorCode
    {
        BiometricKeyNotFound         = 601,
        InvalidOrExpiredChallenge    = 602,
        SignatureVerificationFailed  = 603,
        AccountNotActive             = 604,
        NonceRequired                = 605,
        SignatureRequired            = 606
    }
}

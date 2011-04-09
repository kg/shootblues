using System;
using System.Runtime.InteropServices;
using System.Text;
using Security.WinTrust;

namespace Security.WinTrust {
    public enum WinTrustDataUIChoice : uint {
        All = 1,
        None = 2,
        NoBad = 3,
        NoGood = 4
    }

    public enum WinTrustDataRevocationChecks : uint {
        None = 0x00000000,
        WholeChain = 0x00000001
    }

    public enum WinTrustDataChoice : uint {
        File = 1,
        Catalog = 2,
        Blob = 3,
        Signer = 4,
        Certificate = 5
    }

    public enum WinTrustDataStateAction : uint {
        Ignore = 0x00000000,
        Verify = 0x00000001,
        Close = 0x00000002,
        AutoCache = 0x00000003,
        AutoCacheFlush = 0x00000004
    }

    [FlagsAttribute]
    public enum WinTrustDataProvFlags : uint {
        UseIe4TrustFlag = 0x00000001,
        NoIe4ChainFlag = 0x00000002,
        NoPolicyUsageFlag = 0x00000004,
        RevocationCheckNone = 0x00000010,
        RevocationCheckEndCert = 0x00000020,
        RevocationCheckChain = 0x00000040,
        RevocationCheckChainExcludeRoot = 0x00000080,
        SaferFlag = 0x00000100,
        HashOnlyFlag = 0x00000200,
        UseDefaultOsverCheck = 0x00000400,
        LifetimeSigningFlag = 0x00000800,
        CacheOnlyUrlRetrieval = 0x00001000      // affects CRL retrieval and AIA retrieval
    }

    public enum WinTrustDataUIContext : uint {
        Execute = 0,
        Install = 1
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public class WinTrustFileInfo : IDisposable {
        public UInt32 StructSize = (UInt32)Marshal.SizeOf(typeof(WinTrustFileInfo));
        public readonly IntPtr pszFilePath;            // required, file name to be verified
        public IntPtr hFile = IntPtr.Zero;             // optional, open handle to FilePath
        public IntPtr pgKnownSubject = IntPtr.Zero;    // optional, subject type if it is known

        public WinTrustFileInfo (String filePath) {
            pszFilePath = Marshal.StringToCoTaskMemAuto(filePath);
        }
        public void Dispose () {
            Marshal.FreeCoTaskMem(pszFilePath);
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public class WinTrustData : IDisposable {
        public UInt32 StructSize = (UInt32)Marshal.SizeOf(typeof(WinTrustData));
        public IntPtr PolicyCallbackData = IntPtr.Zero;
        public IntPtr SIPClientData = IntPtr.Zero;
        // required: UI choice
        public WinTrustDataUIChoice UIChoice = WinTrustDataUIChoice.None;
        // required: certificate revocation check options
        public WinTrustDataRevocationChecks RevocationChecks = WinTrustDataRevocationChecks.None;
        // required: which structure is being passed in?
        public readonly WinTrustDataChoice UnionChoice;
        // individual file
        public readonly IntPtr FileInfoPtr;
        public WinTrustDataStateAction StateAction = WinTrustDataStateAction.Ignore;
        public IntPtr StateData = IntPtr.Zero;
        public String URLReference = null;
        public WinTrustDataProvFlags ProvFlags = WinTrustDataProvFlags.SaferFlag;
        public WinTrustDataUIContext UIContext = WinTrustDataUIContext.Execute;

        public WinTrustData (String fileName) {
            WinTrustFileInfo wtfiData = new WinTrustFileInfo(fileName);
            FileInfoPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(WinTrustFileInfo)));
            Marshal.StructureToPtr(wtfiData, FileInfoPtr, false);
            UnionChoice = WinTrustDataChoice.File;
        }

        public void Dispose () {
            Marshal.FreeCoTaskMem(FileInfoPtr);
        }
    }

    public enum WinVerifyTrustResult : uint {
        Success = 0,
        /// <summary>
        /// A system-level error occurred while verifying trust. 
        /// </summary>
        TRUST_E_SYSTEM_ERROR = 0x80096001,
        /// <summary>
        /// The certificate for the signer of the message is invalid or not found. 
        /// </summary>
        TRUST_E_NO_SIGNER_CERT = 0x80096002,
        /// <summary>
        /// One of the counter signatures was invalid. 
        /// </summary>
        TRUST_E_COUNTER_SIGNER = 0x80096003,
        /// <summary>
        /// The signature of the certificate cannot be verified. 
        /// </summary>
        TRUST_E_CERT_SIGNATURE = 0x80096004,
        /// <summary>
        /// The timestamp signature and/or certificate could not be verified or is malformed. 
        /// </summary>
        TRUST_E_TIME_STAMP = 0x80096005,
        /// <summary>
        /// The digital signature of the object did not verify. 
        /// </summary>
        TRUST_E_BAD_DIGEST = 0x80096010,
        /// <summary>
        /// A certificate's basic constraint extension has not been observed. 
        /// </summary>
        TRUST_E_BASIC_CONSTRAINTS = 0x80096019,
        /// <summary>
        /// The certificate does not meet or contain the Authenticode(tm) financial extensions. 
        /// </summary>
        TRUST_E_FINANCIAL_CRITERIA = 0x8009601E,
        /// <summary>
        /// Unknown trust provider. 
        /// </summary>
        TRUST_E_PROVIDER_UNKNOWN = 0x800B0001,
        /// <summary>
        /// The trust verification action specified is not supported by the specified trust provider. 
        /// </summary>
        TRUST_E_ACTION_UNKNOWN = 0x800B0002,
        /// <summary>
        /// The form specified for the subject is not one supported or known by the specified trust provider. 
        /// </summary>
        TRUST_E_SUBJECT_FORM_UNKNOWN = 0x800B0003,
        /// <summary>
        /// The subject is not trusted for the specified action. 
        /// </summary>
        TRUST_E_SUBJECT_NOT_TRUSTED = 0x800B0004,
        /// <summary>
        /// No signature was present in the subject. 
        /// </summary>
        TRUST_E_NOSIGNATURE = 0x800B0100,
        /// <summary>
        /// A required certificate is not within its validity period when verifying against the current system clock or the timestamp in the signed file. 
        /// </summary>
        CERT_E_EXPIRED = 0x800B0101,
        /// <summary>
        /// The validity periods of the certification chain do not nest correctly. 
        /// </summary>
        CERT_E_VALIDITYPERIODNESTING = 0x800B0102,
        /// <summary>
        /// A certificate that can only be used as an end-entity is being used as a CA or visa versa. 
        /// </summary>
        CERT_E_ROLE = 0x800B0103,
        /// <summary>
        /// A path length constraint in the certification chain has been violated. 
        /// </summary>
        CERT_E_PATHLENCONST = 0x800B0104,
        /// <summary>
        /// A certificate contains an unknown extension that is marked 'critical'. 
        /// </summary>
        CERT_E_CRITICAL = 0x800B0105,
        /// <summary>
        /// A certificate being used for a purpose other than the ones specified by its CA. 
        /// </summary>
        CERT_E_PURPOSE = 0x800B0106,
        /// <summary>
        /// A parent of a given certificate in fact did not issue that child certificate. 
        /// </summary>
        CERT_E_ISSUERCHAINING = 0x800B0107,
        /// <summary>
        /// A certificate is missing or has an empty value for an important field, such as a subject or issuer name. 
        /// </summary>
        CERT_E_MALFORMED = 0x800B0108,
        /// <summary>
        /// A certificate chain processed, but terminated in a root certificate which is not trusted by the trust provider. 
        /// </summary>
        CERT_E_UNTRUSTEDROOT = 0x800B0109,
        /// <summary>
        /// A certificate chain could not be built to a trusted root authority. 
        /// </summary>
        CERT_E_CHAINING = 0x800B010A,
        /// <summary>
        /// Generic trust failure. 
        /// </summary>
        TRUST_E_FAIL = 0x800B010B,
        /// <summary>
        /// A certificate was explicitly revoked by its issuer. 
        /// </summary>
        CERT_E_REVOKED = 0x800B010C,
        /// <summary>
        /// The certification path terminates with the test root which is not trusted with the current policy settings. 
        /// </summary>
        CERT_E_UNTRUSTEDTESTROOT = 0x800B010D,
        /// <summary>
        /// The revocation process could not continue - the certificate(s) could not be checked. 
        /// </summary>
        CERT_E_REVOCATION_FAILURE = 0x800B010E,
        /// <summary>
        /// The certificate's CN name does not match the passed value. 
        /// </summary>
        CERT_E_CN_NO_MATCH = 0x800B010F,
        /// <summary>
        /// The certificate is not valid for the requested usage. 
        /// </summary>
        CERT_E_WRONG_USAGE = 0x800B0110,
        /// <summary>
        /// The certificate was explicitly marked as untrusted by the user. 
        /// </summary>
        TRUST_E_EXPLICIT_DISTRUST = 0x800B0111,
        /// <summary>
        /// A certification chain processed correctly, but one of the CA certificates is not trusted by the policy provider. 
        /// </summary>
        CERT_E_UNTRUSTEDCA = 0x800B0112,
        /// <summary>
        /// The certificate has invalid policy. 
        /// </summary>
        CERT_E_INVALID_POLICY = 0x800B0113,
        /// <summary>
        /// The certificate has an invalid name. The name is not included in the permitted list or is explicitly excluded. 
        /// </summary>
        CERT_E_INVALID_NAME = 0x800B0114
    }

    public static class WinTrust {
        public static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);
        // GUID of the action to perform
        public const string WINTRUST_ACTION_GENERIC_VERIFY_V2 = "{00AAC56B-CD44-11d0-8CC2-00C04FC295EE}";

        [DllImport("wintrust.dll", ExactSpelling = true, SetLastError = false, CharSet = CharSet.Unicode)]
        public static extern WinVerifyTrustResult WinVerifyTrust (
            [In] IntPtr hwnd,
            [In] [MarshalAs(UnmanagedType.LPStruct)] Guid pgActionID,
            [In] WinTrustData pWVTData
        );

        public static bool VerifyEmbeddedSignature (string fileName) {
            WinTrustData wtd = new WinTrustData(fileName);
            Guid guidAction = new Guid(WINTRUST_ACTION_GENERIC_VERIFY_V2);
            WinVerifyTrustResult result = WinVerifyTrust(INVALID_HANDLE_VALUE, guidAction, wtd);
            bool ret = (result == WinVerifyTrustResult.Success);
            return ret;
        }
    }
}

namespace ShootBlues {
    public static class Authenticode {
        const uint FORMAT_MESSAGE_ALLOCATE_BUFFER = 0x00000100;
        const uint FORMAT_MESSAGE_IGNORE_INSERTS = 0x00000200;
        const uint FORMAT_MESSAGE_FROM_SYSTEM = 0x00001000;

        [DllImport("kernel32.dll", CharSet=CharSet.Unicode)]
        static extern uint FormatMessage (
            uint dwFlags, IntPtr lpSource,
            uint dwMessageId, uint dwLanguageId, 
            [Out] StringBuilder lpBuffer,
            uint nSize, IntPtr lpArguments
        );

        public static bool CheckSignature (IntPtr ownerWindow, string filename, bool enableUi, out string errorMessage) {
            using (var wtd = new WinTrustData(filename) {
                UIChoice = enableUi ? WinTrustDataUIChoice.All : WinTrustDataUIChoice.None,
                UIContext = WinTrustDataUIContext.Execute,
                RevocationChecks = WinTrustDataRevocationChecks.WholeChain,
                StateAction = WinTrustDataStateAction.Ignore,
                ProvFlags = WinTrustDataProvFlags.RevocationCheckChain
            }) {
                var trustResult = WinTrust.WinVerifyTrust(
                    ownerWindow, new Guid(WinTrust.WINTRUST_ACTION_GENERIC_VERIFY_V2), wtd
                );

                if (trustResult == WinVerifyTrustResult.Success) {
                    errorMessage = null;
                    return true;
                } else {
                    var sb = new StringBuilder(1024);
                    var charCount = FormatMessage(
                        FORMAT_MESSAGE_FROM_SYSTEM | FORMAT_MESSAGE_IGNORE_INSERTS, 
                        IntPtr.Zero, (uint)trustResult, 0,
                        sb, (uint)sb.Capacity, IntPtr.Zero
                    );

                    errorMessage = sb.ToString(0, (int)charCount);
                    return false;
                }
            }
        }
    }
}
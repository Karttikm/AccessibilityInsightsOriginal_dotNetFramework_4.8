// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using AccessibilityInsights.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using static AccessibilityInsights.Win32.NativeMethods;

namespace AccessibilityInsights.SetupLibrary
{
    internal class TrustVerifier : IDisposable
    {
        // This list intentionally excludes the CN, since we are validating only the organization
        private static readonly IReadOnlyCollection<string> TrustedCertIssuerEndings = new List<string>
        {
            ", O=Microsoft Corporation, L=Redmond, S=Washington, C=US",
        };

        // used to keep the file handle open for the lifetime of the object
        // Thus preventing modification after the file has been verified.
        private FileStream _file;

        public bool IsVerified { get; }

        public TrustVerifier(Stream stream)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));

            string tempFilePath = CreateTempFileFromStream(stream);
            IsVerified = VerifyFile(tempFilePath);
        }

        public TrustVerifier(string filePath)
        {
            try
            {
                IsVerified = VerifyFile(filePath);
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }
#pragma warning restore CA1031 // Do not catch general exception types
        }

        private static string CreateTempFileFromStream(Stream stream)
        {
            string tempFile = Path.GetTempFileName();

            stream.Position = 0;
            using (FileStream outStream = File.Open(tempFile, FileMode.Truncate))
            {
                stream.CopyTo(outStream);
            }

            return tempFile;
        }


        private bool VerifyFile(string filePath)
        {
            return IsFileTrusted(filePath) && IsFileIssuingOrganizationTrusted(filePath);
        }

        private bool IsFileTrusted(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException(nameof(filePath));

            _file = File.OpenRead(filePath);

            using (var fileInfo = new WinTrustFileInfo(filePath))
            using (var winTrustData = new WinTrustData(fileInfo))
            {
                var result = WinVerifyTrust(IntPtr.Zero, WINTRUST_ACTION_GENERIC_VERIFY_V2, winTrustData);
                return result == WinVerifyTrustResult.Success;
            }
        }

        public static bool IsFileIssuingOrganizationTrusted(string filePath)
        {
            // Try to load certificate using X509CertificateLoader if available (recommended API).
            // Fall back to legacy CreateFromSignedFile when loader isn't present.
            X509Certificate2 cert2 = null;
            try
            {
                cert2 = LoadCertificateFromSignedFileUsingLoader(filePath) ?? LoadCertificateFromSignedFileFallback(filePath);
                using (cert2)
                {
                    string issuer = cert2.Issuer;
                    foreach (string trustedCertIssuerEnding in TrustedCertIssuerEndings)
                    {
                        if (issuer.EndsWith(trustedCertIssuerEnding, StringComparison.Ordinal))
                        {
                            return true;
                        }
                    }
                }
            }
            catch
            {
                // If certificate loading fails, treat as untrusted.
            }

            return false;
        }

        private static X509Certificate2 LoadCertificateFromSignedFileUsingLoader(string filePath)
        {
            // Attempt to find a runtime-provided X509CertificateLoader type with a suitable method via reflection.
            // This avoids hard compile-time dependency while using the recommended API when available.
            foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type loaderType = null;
                try
                {
                    loaderType = asm.GetType("System.Security.Cryptography.X509Certificates.X509CertificateLoader", throwOnError: false, ignoreCase: false);
                }
                catch
                {
                    continue;
                }

                if (loaderType == null)
                    continue;

                // Find a static method that accepts a single string and returns a certificate (X509Certificate or X509Certificate2).
                foreach (MethodInfo mi in loaderType.GetMethods(BindingFlags.Public | BindingFlags.Static))
                {
                    var parms = mi.GetParameters();
                    if (parms.Length == 1 && parms[0].ParameterType == typeof(string))
                    {
                        if (typeof(X509Certificate2).IsAssignableFrom(mi.ReturnType) || typeof(X509Certificate).IsAssignableFrom(mi.ReturnType))
                        {
                            try
                            {
                                object result = mi.Invoke(null, new object[] { filePath });
                                if (result is X509Certificate2 xc2)
                                {
                                    return xc2;
                                }
                                if (result is X509Certificate xc)
                                {
                                    return new X509Certificate2(xc);
                                }
                            }
                            catch
                            {
                                // ignore and continue searching
                            }
                        }
                    }
                }
            }

            return null;
        }

        private static X509Certificate2 LoadCertificateFromSignedFileFallback(string filePath)
        {
            // Fallback to legacy API if loader not found.
            // Keep the obsolete usage scoped and documented.
#pragma warning disable SYSLIB0057 // Loading certificate data through the constructor or Import is obsolete. Use X509CertificateLoader instead to load certificates.
            using (X509Certificate xc = X509Certificate.CreateFromSignedFile(filePath))
            {
                return new X509Certificate2(xc);
            }
#pragma warning restore SYSLIB0057
        }

        ~TrustVerifier()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_file != null)
            {
                _file.Dispose();
                _file = null;
            }
        }
    } // class
} // namespace

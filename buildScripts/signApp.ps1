param (
	[string]$certFile,
	[string]$outputDir
)

$Source = @"
    using System;
    using System.IO;
    using System.Security.Cryptography;
    using System.Text;

    namespace AES
    {
        public static class AesDecryption
        {
            private const int KeyIterations = 1000;
            private const int KeySize = 256;
            private const int BlockSize = 128;

            #region Methods
            public static byte[] Encrypt(byte[] content, string password, string salt)
            {
                var passwordBytes = Encoding.UTF8.GetBytes(password);
                var saltBytes = Encoding.UTF8.GetBytes(salt);
             
                byte[] encryptedBytes;
                using (var ms = new MemoryStream())
                {
                    using (var aes = new RijndaelManaged())
                    {
                        aes.KeySize = KeySize;
                        aes.BlockSize = BlockSize;

                        var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, KeyIterations);
                        aes.Key = key.GetBytes(aes.KeySize / 8);
                        aes.IV = key.GetBytes(aes.BlockSize / 8);

                        aes.Mode = CipherMode.CBC;

                        using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                        {
                            cs.Write(content, 0, content.Length);
                            cs.Close();
                        }
                        encryptedBytes = ms.ToArray();
                    }
                }

                return encryptedBytes;
            }

            public static byte[] Decrypt(byte[] content, string password, string salt)
            {
                var passwordBytes = Encoding.UTF8.GetBytes(password);
                var saltBytes = Encoding.UTF8.GetBytes(salt);

                byte[] decryptedBytes;
                using (var ms = new MemoryStream())
                {
                    using (var aes = new RijndaelManaged())
                    {
                        aes.KeySize = KeySize;
                        aes.BlockSize = BlockSize;

                        var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, KeyIterations);
                        aes.Key = key.GetBytes(aes.KeySize / 8);
                        aes.IV = key.GetBytes(aes.BlockSize / 8);

                        aes.Mode = CipherMode.CBC;

                        using (var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write))
                        {
                            cs.Write(content, 0, content.Length);
                            cs.Close();
                        }
                        decryptedBytes = ms.ToArray();
                    }
                }

                return decryptedBytes;
            }

            public static void EncryptFile(string path, string password, string salt)
            {
                var content = File.ReadAllBytes(path);
                var encrypted = Encrypt(content, password, salt);
                File.WriteAllBytes(path, encrypted);
            }

            public static void DecryptFile(string path, string password, string salt)
            {
                var content = File.ReadAllBytes(path);
                var encrypted = Decrypt(content, password, salt);
                File.WriteAllBytes(path, encrypted);
            }
            #endregion
        }
    }
"@

Add-Type -TypeDefinition $Source -Language CSharp

# Use double AES decryption
[AES.AesDecryption]::DecryptFile("$PSScriptRoot\$certFile", $env:certEncryptPw, $env:certEncryptSalt)
[AES.AesDecryption]::DecryptFile("$PSScriptRoot\$certFile", $env:certEncryptPw, $env:certEncryptSalt)
Write-Output "Decrypted certificate $PSScriptRoot\$certFile"

Get-ChildItem "$outputDir\$env:configuration" -Filter *.exe |
ForEach-Object {
    $path = $_.FullName
	
	$output = & "$env:signtoolLocation" sign /f $PSScriptRoot\$certFile /p $env:certPw $path
	
    Write-Output $output	

	Write-Output "Signed $path"
}

# Start-Sleep -Seconds 2147483

Remove-Item $PSScriptRoot\$certFile -Force
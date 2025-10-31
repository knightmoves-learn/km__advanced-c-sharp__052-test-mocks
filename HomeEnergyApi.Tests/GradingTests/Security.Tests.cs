using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using HomeEnergyApi.Security;

public class SecurityTests
{
    public string testKey = "This_must_be_exactly_32_chars_01";
    public string testIv = "This_must_be_16_";

    [Fact]
    public void CanCreateValueEncryptor()
    {
        ValueEncryptor valueEncryptor = new ValueEncryptor(BuildMockConfiguration());

        Assert.True(valueEncryptor != null,
            "ValueEncryptor could not be created when passing a valid 'IConfiguration' to its constructor");
    }

    [Fact]
    public void EncryptReturnsValidEncryption()
    {
        ValueEncryptor valueEncryptor = new ValueEncryptor(BuildMockConfiguration());
        string toEncrypt = "Encrypt me!";
        string result = valueEncryptor.Encrypt(toEncrypt);
        string expected = TestEncrypt(testKey, testIv, toEncrypt);

        Assert.True(result == expected,
            $"ValueEncryptor did not return the expected encrypted string from 'Encrypt()' method\nExpected:{expected}\nReceived:{result}");
    }
    
    [Fact]
    public void EncryptReturnsValidEncryptionNull()
    {
        ValueEncryptor valueEncryptor = new ValueEncryptor(BuildMockConfiguration());
        string toEncrypt = "default";
        string result = valueEncryptor.Encrypt(null);
        string expected = TestEncrypt(testKey, testIv, toEncrypt);

        Assert.True(result == expected,
            $"ValueEncryptor did not return the expected encrypted string from 'Encrypt()' method\nExpected:{expected}\nReceived:{result}");
    }

    [Fact]
    public void EncryptReturnsValidDecryption()
    {
        ValueEncryptor valueEncryptor = new ValueEncryptor(BuildMockConfiguration());
        string toDecrypt = "SIo3pWbHXjqmY7RidpOwa1Iegqkdr4gogKk1NvN9gIw=";
        string expected = TestDecrypt(testKey, testIv, toDecrypt);
        valueEncryptor.ValueDecrypted += (cipher, plaintext) =>
        {
            Assert.Equal(toDecrypt, cipher);
            Assert.Equal(expected, plaintext);
        };


        string result = valueEncryptor.Decrypt(toDecrypt);

        Assert.True(result == expected,
            $"ValueEncryptor did not return the expected decrypted string from 'Decrypt()' method\nExpected:{expected}\nReceived:{result}");
    }

    public IConfigurationRoot BuildMockConfiguration()
    {
        var mockConfigStub = new Dictionary<string, string> {
            {"AES:Key", testKey},
            {"AES:InitializationVector", testIv}
        };

        var mockConfig = new ConfigurationBuilder()
            .AddInMemoryCollection(mockConfigStub)
            .Build();

        return mockConfig;
    }

    public string TestEncrypt(string key, string iv, string expected)
    {
        using var aesAlg = Aes.Create();
        aesAlg.Key = Encoding.UTF8.GetBytes(key);
        aesAlg.IV = Encoding.UTF8.GetBytes(iv);

        var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

        byte[] encrypted;
        using (MemoryStream msEncrypt = new MemoryStream())
        {
            using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
            {
                using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                {
                    swEncrypt.Write(expected);
                }
            }

            encrypted = msEncrypt.ToArray();
        }

        return Convert.ToBase64String(encrypted);
    }

    public string TestDecrypt(string key, string iv, string toDecrypt)
    {
        if (key.Length != 32 || iv.Length != 16)
        {
            throw new ArgumentException("Key must be 32 bytes and IV must be 16 bytes long.");
        }

        using var aesAlg = Aes.Create();
        aesAlg.Key = Encoding.UTF8.GetBytes(key);
        aesAlg.IV = Encoding.UTF8.GetBytes(iv);

        var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

        string plaintext;
        byte[] cipherBytes = Convert.FromBase64String(toDecrypt);

        using (MemoryStream msDecrypt = new MemoryStream(cipherBytes))
        {
            using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
            {
                using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                {
                    plaintext = srDecrypt.ReadToEnd();
                }
            }
        }

        return plaintext;
    }
}

using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace TikSync
{
    public class SecurityCoreTikSync
    {
        private readonly string _apiKey;
        private readonly string _deviceId;
        private readonly string _fingerprint;
        private readonly byte[] _primaryKey;
        private readonly byte[] _secondaryKey;

        public SecurityCoreTikSync(string apiKey)
        {
            _apiKey = apiKey;
            _deviceId = "tsd_" + Guid.NewGuid().ToString("N");
            _fingerprint = ComputeFingerprint(_deviceId);
            _primaryKey = DeriveKey(apiKey, _deviceId, "tiksync-primary-v1");
            _secondaryKey = DeriveKey(apiKey, _deviceId, "tiksync-secondary-v1");
        }

        private static byte[] DeriveKey(string apiKey, string deviceId, string salt)
        {
            using var sha = SHA256.Create();
            var input = Encoding.UTF8.GetBytes($"{apiKey}|{deviceId}|{salt}");
            return sha.ComputeHash(input);
        }

        private static string ComputeFingerprint(string deviceId)
        {
            using var sha = SHA256.Create();
            var data = Encoding.UTF8.GetBytes(
                deviceId +
                Environment.OSVersion.Platform +
                System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture +
                "1.0.0" +
                Environment.MachineName +
                HexEncode(SHA256.HashData(Encoding.UTF8.GetBytes(Environment.UserName)))
            );
            return HexEncode(sha.ComputeHash(data));
        }

        public Dictionary<string, string> Sign(string method, string path, string body)
        {
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
            var nonce = GenerateNonce();

            var payload = $"{method.ToUpper()}|{path}|{timestamp}|{nonce}|{body}";
            var primarySig = HmacSha256(_primaryKey, payload);
            var secondaryInput = $"{primarySig}|{nonce}";
            var secondarySig = HmacSha512(_secondaryKey, secondaryInput);

            var combined = $"ts1.{primarySig[..16]}.{secondarySig[..24]}";

            return new Dictionary<string, string>
            {
                ["signature"] = combined,
                ["timestamp"] = timestamp,
                ["nonce"] = nonce,
            };
        }

        public string GenerateToken()
        {
            var epoch = DateTimeOffset.UtcNow.ToUnixTimeSeconds() / 180;
            var nonceBytes = RandomNumberGenerator.GetBytes(8);
            var nonceHex = HexEncode(nonceBytes);

            var payload = $"{epoch}|{nonceHex}|{_deviceId}";
            var masterKey = DeriveKey(_apiKey, _deviceId, "tiksync-token-master-v1");
            var sig = HmacSha256(masterKey, payload);

            return $"tst1.{epoch}.{nonceHex}.{sig[..32]}";
        }

        public Dictionary<string, string> GetConnectHeaders()
        {
            var signed = Sign("GET", "/v1/connect", "");
            return new Dictionary<string, string>
            {
                ["x-ts-device"] = _deviceId,
                ["x-ts-signature"] = signed["signature"],
                ["x-ts-timestamp"] = signed["timestamp"],
                ["x-ts-nonce"] = signed["nonce"],
                ["x-ts-token"] = GenerateToken(),
                ["x-ts-fingerprint"] = _fingerprint,
                ["x-ts-version"] = "1.0.0",
            };
        }

        public string DeviceId => _deviceId;

        private static string GenerateNonce()
        {
            return HexEncode(RandomNumberGenerator.GetBytes(16));
        }

        private static string HmacSha256(byte[] key, string data)
        {
            using var hmac = new HMACSHA256(key);
            return HexEncode(hmac.ComputeHash(Encoding.UTF8.GetBytes(data)));
        }

        private static string HmacSha512(byte[] key, string data)
        {
            using var hmac = new HMACSHA512(key);
            return HexEncode(hmac.ComputeHash(Encoding.UTF8.GetBytes(data)));
        }

        private static string HexEncode(byte[] bytes)
        {
            var sb = new StringBuilder(bytes.Length * 2);
            foreach (var b in bytes) sb.Append(b.ToString("x2"));
            return sb.ToString();
        }
    }
}

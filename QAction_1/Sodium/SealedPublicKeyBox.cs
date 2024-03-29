﻿namespace Skyline.Protocol.Sodium
{
	using System;
	using System.Security.Cryptography;
	using System.Text;

	using static Skyline.Protocol.Sodium.Interop.Libsodium;

	/// <summary> Create and Open SealedPublicKeyBoxes. </summary>
	public static class SealedPublicKeyBox
	{
		public const int RecipientPublicKeyBytes = crypto_box_curve25519xsalsa20poly1305_PUBLICKEYBYTES;
		public const int RecipientSecretKeyBytes = crypto_box_curve25519xsalsa20poly1305_SECRETKEYBYTES;
		private const int CryptoBoxSealbytes = crypto_box_curve25519xsalsa20poly1305_PUBLICKEYBYTES + crypto_box_curve25519xsalsa20poly1305_MACBYTES;

		/// <summary> Creates a SealedPublicKeyBox</summary>
		/// <param name="message">The message.</param>
		/// <param name="recipientKeyPair">The recipientKeyPair key pair (uses only the public key).</param>
		/// <returns>The anonymously encrypted message.</returns>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		/// <exception cref="CryptographicException"></exception>
		public static byte[] Create(string message, KeyPair recipientKeyPair)
		{
			return Create(Encoding.UTF8.GetBytes(message), recipientKeyPair.PublicKey);
		}

		/// <summary> Creates a SealedPublicKeyBox</summary>
		/// <param name="message">The message.</param>
		/// <param name="recipientKeyPair">The recipientKeyPair key pair (uses only the public key).</param>
		/// <returns>The anonymously encrypted message.</returns>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		/// <exception cref="CryptographicException"></exception>
		public static byte[] Create(byte[] message, KeyPair recipientKeyPair)
		{
			return Create(message, recipientKeyPair.PublicKey);
		}

		/// <summary> Creates a SealedPublicKeyBox</summary>
		/// <param name="message">The message.</param>
		/// <param name="recipientPublicKey">The 32 byte recipient's public key.</param>
		/// <returns>The anonymously encrypted message.</returns>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		/// <exception cref="CryptographicException"></exception>
		public static byte[] Create(string message, byte[] recipientPublicKey)
		{
			return Create(Encoding.UTF8.GetBytes(message), recipientPublicKey);
		}

		/// <summary> Creates a SealedPublicKeyBox</summary>
		/// <param name="message">The message.</param>
		/// <param name="recipientPublicKey">The 32 byte recipient's public key.</param>
		/// <returns>The anonymously encrypted message.</returns>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		/// <exception cref="CryptographicException"></exception>
		public static byte[] Create(byte[] message, byte[] recipientPublicKey)
		{
			if (recipientPublicKey == null || recipientPublicKey.Length != RecipientPublicKeyBytes)
				throw new ArgumentOutOfRangeException(nameof(recipientPublicKey), recipientPublicKey?.Length ?? 0, $"recipientPublicKey must be {RecipientPublicKeyBytes} bytes in length.");

			var buffer = new byte[message.Length + CryptoBoxSealbytes];

			SodiumCore.Initialize();
			var ret = crypto_box_seal(buffer, message, (ulong)message.Length, recipientPublicKey);

			if (ret != 0)
				throw new CryptographicException("Failed to create SealedBox");

			return buffer;
		}

		/// <summary>Opens a SealedPublicKeyBox</summary>
		/// <param name="cipherText">Hex-encoded cipherText to be opened.</param>
		/// <param name="recipientKeyPair">The recipient's key pair.</param>
		/// <returns>The decrypted message.</returns>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		/// <exception cref="CryptographicException"></exception>
		public static byte[] Open(string cipherText, KeyPair recipientKeyPair)
		{
			return Open(Utilities.HexToBinary(cipherText), recipientKeyPair.PrivateKey, recipientKeyPair.PublicKey);
		}

		/// <summary>Opens a SealedPublicKeyBox</summary>
		/// <param name="cipherText">The cipherText to be opened.</param>
		/// <param name="recipientKeyPair">The recipient's key pair.</param>
		/// <returns>The decrypted message.</returns>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		/// <exception cref="CryptographicException"></exception>
		public static byte[] Open(byte[] cipherText, KeyPair recipientKeyPair)
		{
			return Open(cipherText, recipientKeyPair.PrivateKey, recipientKeyPair.PublicKey);
		}

		/// <summary>Opens a SealedPublicKeyBox</summary>
		/// <param name="cipherText">Hex-encoded cipherText to be opened.</param>
		/// <param name="recipientSecretKey">The recipient's secret key.</param>
		/// <param name="recipientPublicKey">The recipient's public key.</param>
		/// <returns>The decrypted message.</returns>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		/// <exception cref="CryptographicException"></exception>
		public static byte[] Open(string cipherText, byte[] recipientSecretKey, byte[] recipientPublicKey)
		{
			return Open(Utilities.HexToBinary(cipherText), recipientSecretKey, recipientPublicKey);
		}

		/// <summary>Opens a SealedPublicKeyBox</summary>
		/// <param name="cipherText">The cipherText to be opened.</param>
		/// <param name="recipientSecretKey">The recipient's secret key.</param>
		/// <param name="recipientPublicKey">The recipient's public key.</param>
		/// <returns>The decrypted message.</returns>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		/// <exception cref="CryptographicException"></exception>
		public static byte[] Open(byte[] cipherText, byte[] recipientSecretKey, byte[] recipientPublicKey)
		{
			if (recipientSecretKey == null || recipientSecretKey.Length != RecipientSecretKeyBytes)
				throw new ArgumentOutOfRangeException(nameof(recipientPublicKey), recipientSecretKey?.Length ?? 0, $"recipientSecretKey must be {RecipientSecretKeyBytes} bytes in length.");
			if (recipientPublicKey == null || recipientPublicKey.Length != RecipientPublicKeyBytes)
				throw new ArgumentOutOfRangeException(nameof(recipientPublicKey), recipientPublicKey?.Length ?? 0, $"recipientPublicKey must be {RecipientPublicKeyBytes} bytes in length.");

			if (cipherText.Length < CryptoBoxSealbytes)
				throw new CryptographicException("Failed to open SealedBox");

			var buffer = new byte[cipherText.Length - CryptoBoxSealbytes];

			SodiumCore.Initialize();
			var ret = crypto_box_seal_open(buffer, cipherText, (ulong)cipherText.Length, recipientPublicKey,
				recipientSecretKey);

			if (ret != 0)
				throw new CryptographicException("Failed to open SealedBox");

			return buffer;
		}
	}
}

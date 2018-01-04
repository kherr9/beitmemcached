using System;
using NUnit.Framework;

namespace BeIT.MemCached.IntegrationTests
{
    public class MemcachedClientTests
    {
        private static readonly string ClientName = nameof(MemcachedClientTests);

        private MemcachedClient _client;

        static MemcachedClientTests()
        {
            MemcachedClient.Setup(ClientName, new[] { "localhost" });
        }

        [SetUp]
        public void SetUp()
        {
            _client = MemcachedClient.GetInstance(ClientName);
        }

        [TearDown]
        public void TearDown()
        {
            _client.FlushAll();
        }

        #region Set

        [Test]
        public void TestSet_WhenKeyNotExists()
        {
            // Arrange
            var key = GenerateKey();
            var value = GenerateBytes();

            // Act
            var result = _client.Set(key, value);

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(value, _client.Get(key));
        }

        [Test]
        public void TestSet_WhenKeyExists()
        {
            // Arrange
            (var key, _) = GenerateAndAddKey(_client);
            var newValue = GenerateBytes();

            // Act
            var result = _client.Set(key, newValue);

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(newValue, _client.Get(key), "should replace value");
        }

        #endregion

        private string GenerateKey()
        {
            return $"key_{Guid.NewGuid()}";
        }

        private byte[] GenerateBytes()
        {
            return Guid.NewGuid().ToByteArray();
        }

        private (string, byte[]) GenerateAndAddKey(MemcachedClient client)
        {
            var key = GenerateKey();
            var value = GenerateBytes();

            Assert.IsTrue(client.Add(key, value));

            return (key, value);
        }
    }
}

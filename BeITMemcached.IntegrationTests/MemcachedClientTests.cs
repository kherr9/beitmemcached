using System;
using System.Linq;
using NUnit.Framework;

namespace BeIT.MemCached.IntegrationTests
{
    /// <summary>
    /// docker run -d -p 11211:11211 --name memcached memcached
    /// </summary>
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

        #region Set(string, object)

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

        #region Add(string, object)

        [Test]
        public void TestAdd_WhenKeyNotExists()
        {
            // Arrange
            var key = GenerateKey();
            var value = GenerateBytes();

            // Act
            var result = _client.Add(key, value);

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(value, _client.Get(key));
        }

        [Test]
        public void TestAdd_WhenKeyExists()
        {
            // Arrange
            (var key, var existingValue) = GenerateAndAddKey(_client);
            var newValue = GenerateBytes();

            // Act
            var result = _client.Add(key, newValue);

            // Assert
            Assert.IsFalse(result);
            Assert.AreEqual(existingValue, _client.Get(key), "value should not be replaced");
        }

        #endregion

        #region Replace(string, object)

        [Test]
        public void TestReplace_WhenKeyNotExists()
        {
            // Arrange
            var key = GenerateKey();
            var value = GenerateBytes();

            // Act
            var result = _client.Replace(key, value);

            // Assert
            Assert.IsFalse(result);
            Assert.IsNull(_client.Get(key));
        }

        [Test]
        public void TestReplace_WhenKeyExists()
        {
            // Arrange
            (var key, _) = GenerateAndAddKey(_client);
            var newValue = GenerateBytes();

            // Act
            var result = _client.Replace(key, newValue);

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(newValue, _client.Get(key), "value should not be replaced");
        }

        #endregion

        #region Get(string)

        [Test]
        public void TestGet_WhenKeyNotExists()
        {
            // Arrange
            var key = GenerateKey();

            // Act
            var result = _client.Get(key);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public void TestGet_WhenKeyExists()
        {
            // Arrange
            (var key, var value) = GenerateAndAddKey(_client);

            // Act
            var result = _client.Get(key);

            Assert.AreEqual(value, result);
        }

        #endregion

        #region Get(string[])

        [Test]
        public void TestGets_WhenNoKeysExist()
        {
            // Arrange
            var keys = new[]
            {
                GenerateKey(),
                GenerateKey()
            };

            var values = new byte[][]
            {
                null,
                null
            };

            // Act
            var result = _client.Get(keys);

            // Assert
            CollectionAssert.AreEqual(values, result);
        }

        [Test]
        public void TestGets_WhenAllKeysExist()
        {
            // Arrange
            (var key1, var value1) = GenerateAndAddKey(_client);
            (var key2, var value2) = GenerateAndAddKey(_client);

            var keys = new[] { key1, key2 };
            var values = new[] { value1, value2 };

            // Act
            var result = _client.Get(keys);

            // Assert
            CollectionAssert.AreEqual(values, result);
        }

        [Test]
        public void TestGets_WhenSomeKeysExist()
        {
            // Arrange
            (var key1, var value1) = GenerateAndAddKey(_client);
            var key2 = GenerateKey();
            (var key3, var value3) = GenerateAndAddKey(_client);

            var keys = new[]
            {
                key1,
                key2,
                key3
            };

            var values = new[]
            {
                value1,
                null,
                value3
            };

            // Act
            var result = _client.Get(keys);

            // Assert
            CollectionAssert.AreEqual(values, result);
        }

        #endregion

        #region Delete(string)

        [Test]
        public void TestDelete_WhenKeyNotExists()
        {
            // Arrange
            var key = GenerateKey();

            // Act
            var result = _client.Delete(key);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void TestDelete_WhenKeyExists()
        {
            // Arrange
            (var key, _) = GenerateAndAddKey(_client);

            // Act
            var result = _client.Delete(key);

            // Assert
            Assert.IsTrue(result);
            Assert.IsNull(_client.Get(key));
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

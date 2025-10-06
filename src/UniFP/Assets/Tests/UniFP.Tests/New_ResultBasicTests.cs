using NUnit.Framework;
using UniFP;

namespace UniFP.Tests
{
    /// <summary>
    /// Result core functionality tests.
    /// Covers Success/Failure creation, core properties, and factory methods.
    /// </summary>
    public class ResultBasicTests
    {
        #region Success Tests

        [Test]
        public void Success_CreatesSuccessResult()
        {
            var result = Result<int>.Success(42);

            Assert.IsTrue(result.IsSuccess);
            Assert.IsFalse(result.IsFailure);
            Assert.AreEqual(42, result.Value);
        }

        [Test]
        public void FromValue_CreatesSuccessResult()
        {
            var result = Result.FromValue(100);

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(100, result.Value);
        }

        [Test]
        public void Success_WithReferenceType_WorksCorrectly()
        {
            var obj = new TestClass { Value = 123 };
            var result = Result<TestClass>.Success(obj);

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(123, result.Value.Value);
            Assert.AreSame(obj, result.Value);
        }

        #endregion

        #region Failure Tests

        [Test]
        public void Failure_WithErrorCode_CreatesFailureResult()
        {
            var result = Result<int>.Failure(ErrorCode.InvalidInput);

            Assert.IsTrue(result.IsFailure);
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(ErrorCode.InvalidInput, result.ErrorCode);
        }

        [Test]
        public void Failure_WithString_CreatesFailureResult()
        {
            var result = Result<int>.Failure("custom error");

            Assert.IsTrue(result.IsFailure);
            Assert.That(result.GetErrorMessage(), Does.Contain("custom error"));
        }

        [Test]
        public void FromError_WithErrorCode_CreatesFailureResult()
        {
            var result = Result.FromError<int>(ErrorCode.NotFound);

            Assert.IsTrue(result.IsFailure);
            Assert.AreEqual(ErrorCode.NotFound, result.ErrorCode);
        }

        #endregion

        #region Try Tests

        [Test]
        public void Try_WithSuccessfulFunc_ReturnsSuccess()
        {
            var result = Result.TryFromValue(() => 10 + 20);

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(30, result.Value);
        }

        [Test]
        public void Try_WithException_ReturnsFailure()
        {
            var result = Result.TryFromValue<int>(() => throw new System.Exception("test error"));

            Assert.IsTrue(result.IsFailure);
            Assert.That(result.GetErrorMessage(), Does.Contain("test error"));
        }

        [Test]
        public void Try_WithDivisionByZero_ReturnsFailure()
        {
            var result = Result.TryFromValue(() =>
            {
                int a = 10;
                int b = 0;
                if (b == 0) throw new System.DivideByZeroException();
                return a / b;
            });

            Assert.IsTrue(result.IsFailure);
        }

        #endregion

        #region Equality Tests

        [Test]
        public void Equals_SameSuccessValues_ReturnsTrue()
        {
            var result1 = Result<int>.Success(42);
            var result2 = Result<int>.Success(42);

            Assert.AreEqual(result1, result2);
        }

        [Test]
        public void Equals_DifferentValues_ReturnsFalse()
        {
            var result1 = Result<int>.Success(42);
            var result2 = Result<int>.Success(100);

            Assert.AreNotEqual(result1, result2);
        }

        [Test]
        public void Equals_SameErrorCode_ReturnsTrue()
        {
            var result1 = Result<int>.Failure(ErrorCode.NotFound);
            var result2 = Result<int>.Failure(ErrorCode.NotFound);

            Assert.AreEqual(result1, result2);
        }

        #endregion

        #region Helper Class

        class TestClass
        {
            public int Value { get; set; }
        }

        #endregion
    }
}

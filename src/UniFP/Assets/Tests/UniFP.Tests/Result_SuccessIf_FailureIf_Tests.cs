using NUnit.Framework;
using UniFP;
#if UNIFP_UNITASK
using Cysharp.Threading.Tasks;
#endif

namespace UniFP.Tests
{
    /// <summary>
    /// SuccessIf / FailureIf 팩토리 메서드 테스트
    /// </summary>
    [TestFixture]
    public class Result_SuccessIf_FailureIf_Tests
    {
        #region SuccessIf Tests

        [Test]
        public void SuccessIf_WithTrueCondition_ReturnsSuccess()
        {
            // Arrange
            var user = new TestUser { Age = 20 };

            // Act
            var result = Result.SuccessIf(
                user.Age >= 18,
                user,
                ErrorCode.ValidationFailed
            );

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(user, result.Value);
        }

        [Test]
        public void SuccessIf_WithFalseCondition_ReturnsFailure()
        {
            // Arrange
            var user = new TestUser { Age = 15 };

            // Act
            var result = Result.SuccessIf(
                user.Age >= 18,
                user,
                ErrorCode.ValidationFailed
            );

            // Assert
            Assert.IsTrue(result.IsFailure);
            Assert.AreEqual(ErrorCode.ValidationFailed, result.ErrorCode);
        }

        [Test]
        public void SuccessIf_WithCustomMessage_ReturnsFailureWithMessage()
        {
            // Arrange
            var user = new TestUser { Age = 15 };

            // Act
            var result = Result.SuccessIf(
                user.Age >= 18,
                user,
                ErrorCode.ValidationFailed
            );

            // Assert
            Assert.IsTrue(result.IsFailure);
            Assert.AreEqual(ErrorCode.ValidationFailed, result.ErrorCode);
        }

        [Test]
        public void SuccessIf_WithFunc_EvaluatesCondition()
        {
            // Arrange
            var user = new TestUser { Age = 20 };
            bool conditionEvaluated = false;

            // Act
            var result = Result.SuccessIf(
                () => { conditionEvaluated = true; return user.Age >= 18; },
                user,
                ErrorCode.ValidationFailed
            );

            // Assert
            Assert.IsTrue(conditionEvaluated);
            Assert.IsTrue(result.IsSuccess);
        }

        #endregion

        #region FailureIf Tests

        [Test]
        public void FailureIf_WithFalseCondition_ReturnsSuccess()
        {
            // Arrange
            var user = new TestUser { IsBanned = false };

            // Act
            var result = Result.FailureIf(
                user.IsBanned,
                user,
                ErrorCode.Forbidden
            );

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(user, result.Value);
        }

        [Test]
        public void FailureIf_WithTrueCondition_ReturnsFailure()
        {
            // Arrange
            var user = new TestUser { IsBanned = true };

            // Act
            var result = Result.FailureIf(
                user.IsBanned,
                user,
                ErrorCode.Forbidden
            );

            // Assert
            Assert.IsTrue(result.IsFailure);
            Assert.AreEqual(ErrorCode.Forbidden, result.ErrorCode);
        }

        [Test]
        public void FailureIf_WithCustomMessage_ReturnsFailureWithMessage()
        {
            // Arrange
            var user = new TestUser { IsBanned = true };

            // Act
            var result = Result.FailureIf(
                user.IsBanned,
                user,
                ErrorCode.Forbidden
            );

            // Assert
            Assert.IsTrue(result.IsFailure);
            Assert.AreEqual(ErrorCode.Forbidden, result.ErrorCode);
        }

        [Test]
        public void FailureIf_WithFunc_EvaluatesCondition()
        {
            // Arrange
            var user = new TestUser { IsBanned = false };
            bool conditionEvaluated = false;

            // Act
            var result = Result.FailureIf(
                () => { conditionEvaluated = true; return user.IsBanned; },
                user,
                ErrorCode.Forbidden
            );

            // Assert
            Assert.IsTrue(conditionEvaluated);
            Assert.IsTrue(result.IsSuccess);
        }

        #endregion

        #region Chaining Tests

        [Test]
        public void SuccessIf_And_FailureIf_CanBeChained()
        {
            // Arrange
            var user = new TestUser { Age = 20, IsBanned = false };

            // Act
            var result = Result.SuccessIf(user.Age >= 18, user, ErrorCode.ValidationFailed)
                .Then(u => Result.FailureIf(u.IsBanned, u, ErrorCode.Forbidden))
                .Then(u => Result<TestUser>.Success(u));

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(user, result.Value);
        }

        [Test]
        public void SuccessIf_FailsEarly_InChain()
        {
            // Arrange
            var user = new TestUser { Age = 15, IsBanned = false };

            // Act
            var result = Result.SuccessIf(user.Age >= 18, user, ErrorCode.ValidationFailed)
                .Then(u => Result.FailureIf(u.IsBanned, u, ErrorCode.Forbidden));

            // Assert
            Assert.IsTrue(result.IsFailure);
            Assert.AreEqual(ErrorCode.ValidationFailed, result.ErrorCode);
        }

        [Test]
        public void FailureIf_FailsInMiddle_OfChain()
        {
            // Arrange
            var user = new TestUser { Age = 20, IsBanned = true };

            // Act
            var result = Result.SuccessIf(user.Age >= 18, user, ErrorCode.ValidationFailed)
                .Then(u => Result.FailureIf(u.IsBanned, u, ErrorCode.Forbidden))
                .Then(u => Result<TestUser>.Success(u));

            // Assert
            Assert.IsTrue(result.IsFailure);
            Assert.AreEqual(ErrorCode.Forbidden, result.ErrorCode);
        }

        #endregion

        #region Helper Class

        private class TestUser
        {
            public int Age { get; set; }
            public bool IsBanned { get; set; }
        }

        #endregion
    }
}

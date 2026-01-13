using NUnit.Framework;
using UniFP;

namespace UniFP.Tests
{
    /// <summary>
    /// CombineValues 별칭 메서드 테스트
    /// </summary>
    [TestFixture]
    public class Result_CombineValues_Tests
    {
        #region CombineValues 2 Results

        [Test]
        public void CombineValues_TwoSuccesses_ReturnsSuccessWithTuple()
        {
            // Arrange
            var result1 = Result<int>.Success(10);
            var result2 = Result<string>.Success("Hello");

            // Act
            var combined = Result.CombineValues(result1, result2);

            // Assert
            Assert.IsTrue(combined.IsSuccess);
            Assert.AreEqual((10, "Hello"), combined.Value);
        }

        [Test]
        public void CombineValues_FirstFailure_ReturnsFirstFailure()
        {
            // Arrange
            var result1 = Result<int>.Failure(ErrorCode.NotFound);
            var result2 = Result<string>.Success("Hello");

            // Act
            var combined = Result.CombineValues(result1, result2);

            // Assert
            Assert.IsTrue(combined.IsFailure);
            Assert.AreEqual(ErrorCode.NotFound, combined.ErrorCode);
        }

        [Test]
        public void CombineValues_SecondFailure_ReturnsSecondFailure()
        {
            // Arrange
            var result1 = Result<int>.Success(10);
            var result2 = Result<string>.Failure(ErrorCode.InvalidOperation);

            // Act
            var combined = Result.CombineValues(result1, result2);

            // Assert
            Assert.IsTrue(combined.IsFailure);
            Assert.AreEqual(ErrorCode.InvalidOperation, combined.ErrorCode);
        }

        [Test]
        public void CombineValues_IsSameAsCombine()
        {
            // Arrange
            var result1 = Result<int>.Success(42);
            var result2 = Result<string>.Success("Test");

            // Act
            var combined1 = Result.Combine(result1, result2);
            var combined2 = Result.CombineValues(result1, result2);

            // Assert
            Assert.AreEqual(combined1.IsSuccess, combined2.IsSuccess);
            Assert.AreEqual(combined1.Value, combined2.Value);
        }

        #endregion

        #region CombineValues 3 Results

        [Test]
        public void CombineValues_ThreeSuccesses_ReturnsSuccessWithTuple()
        {
            // Arrange
            var result1 = Result<int>.Success(1);
            var result2 = Result<string>.Success("Two");
            var result3 = Result<bool>.Success(true);

            // Act
            var combined = Result.CombineValues(result1, result2, result3);

            // Assert
            Assert.IsTrue(combined.IsSuccess);
            Assert.AreEqual((1, "Two", true), combined.Value);
        }

        [Test]
        public void CombineValues_ThreeResults_FirstFailure_ReturnsFailure()
        {
            // Arrange
            var result1 = Result<int>.Failure(ErrorCode.Unknown);
            var result2 = Result<string>.Success("Two");
            var result3 = Result<bool>.Success(true);

            // Act
            var combined = Result.CombineValues(result1, result2, result3);

            // Assert
            Assert.IsTrue(combined.IsFailure);
            Assert.AreEqual(ErrorCode.Unknown, combined.ErrorCode);
        }

        #endregion

        #region CombineValues 4 Results

        [Test]
        public void CombineValues_FourSuccesses_ReturnsSuccessWithTuple()
        {
            // Arrange
            var result1 = Result<int>.Success(10);
            var result2 = Result<string>.Success("Hello");
            var result3 = Result<bool>.Success(false);
            var result4 = Result<float>.Success(3.14f);

            // Act
            var combined = Result.CombineValues(result1, result2, result3, result4);

            // Assert
            Assert.IsTrue(combined.IsSuccess);
            Assert.AreEqual((10, "Hello", false, 3.14f), combined.Value);
        }

        [Test]
        public void CombineValues_FourResults_LastFailure_ReturnsFailure()
        {
            // Arrange
            var result1 = Result<int>.Success(10);
            var result2 = Result<string>.Success("Hello");
            var result3 = Result<bool>.Success(false);
            var result4 = Result<float>.Failure(ErrorCode.Timeout);

            // Act
            var combined = Result.CombineValues(result1, result2, result3, result4);

            // Assert
            Assert.IsTrue(combined.IsFailure);
            Assert.AreEqual(ErrorCode.Timeout, combined.ErrorCode);
        }

        #endregion

        #region Real-World Usage Pattern

        [Test]
        public void CombineValues_ValidationScenario_WorksCorrectly()
        {
            // Arrange - 사용자 입력 검증 시나리오
            var nameValidation = ValidateName("John Doe");
            var emailValidation = ValidateEmail("john@example.com");
            var ageValidation = ValidateAge(25);

            // Act
            var userCreation = Result.CombineValues(nameValidation, emailValidation, ageValidation)
                .Map(tuple => new UserDto
                {
                    Name = tuple.Item1,
                    Email = tuple.Item2,
                    Age = tuple.Item3
                });

            // Assert
            Assert.IsTrue(userCreation.IsSuccess);
            Assert.AreEqual("John Doe", userCreation.Value.Name);
            Assert.AreEqual("john@example.com", userCreation.Value.Email);
            Assert.AreEqual(25, userCreation.Value.Age);
        }

        [Test]
        public void CombineValues_ValidationScenario_FailsOnInvalidEmail()
        {
            // Arrange
            var nameValidation = ValidateName("John Doe");
            var emailValidation = ValidateEmail("invalid-email"); // 잘못된 이메일
            var ageValidation = ValidateAge(25);

            // Act
            var userCreation = Result.CombineValues(nameValidation, emailValidation, ageValidation)
                .Map(tuple => new UserDto
                {
                    Name = tuple.Item1,
                    Email = tuple.Item2,
                    Age = tuple.Item3
                });

            // Assert
            Assert.IsTrue(userCreation.IsFailure);
            Assert.AreEqual(ErrorCode.ValidationFailed, userCreation.ErrorCode);
        }

        [Test]
        public void CombineValues_WithChaining_BuildsComplexObject()
        {
            // Arrange
            var basicInfo = Result.CombineValues(
                ValidateName("Alice"),
                ValidateAge(30)
            );

            var contactInfo = Result.CombineValues(
                ValidateEmail("alice@example.com"),
                ValidatePhone("+1234567890")
            );

            // Act
            var user = basicInfo
                .Then(info => contactInfo.Map(contact => (info, contact)))
                .Map(tuple => new UserDto
                {
                    Name = tuple.info.Item1,
                    Age = tuple.info.Item2,
                    Email = tuple.contact.Item1,
                    Phone = tuple.contact.Item2
                });

            // Assert
            Assert.IsTrue(user.IsSuccess);
            Assert.AreEqual("Alice", user.Value.Name);
            Assert.AreEqual(30, user.Value.Age);
            Assert.AreEqual("alice@example.com", user.Value.Email);
            Assert.AreEqual("+1234567890", user.Value.Phone);
        }

        #endregion

        #region Helper Methods

        private Result<string> ValidateName(string name)
        {
            return Result.SuccessIf(
                !string.IsNullOrWhiteSpace(name),
                name,
                ErrorCode.ValidationFailed
            );
        }

        private Result<string> ValidateEmail(string email)
        {
            return Result.SuccessIf(
                !string.IsNullOrWhiteSpace(email) && email.Contains("@"),
                email,
                ErrorCode.ValidationFailed
            );
        }

        private Result<int> ValidateAge(int age)
        {
            return Result.SuccessIf(
                age >= 0 && age <= 150,
                age,
                ErrorCode.ValidationFailed
            );
        }

        private Result<string> ValidatePhone(string phone)
        {
            return Result.SuccessIf(
                !string.IsNullOrWhiteSpace(phone) && phone.StartsWith("+"),
                phone,
                ErrorCode.ValidationFailed
            );
        }

        private class UserDto
        {
            public string Name { get; set; }
            public string Email { get; set; }
            public int Age { get; set; }
            public string Phone { get; set; }
        }

        #endregion
    }
}

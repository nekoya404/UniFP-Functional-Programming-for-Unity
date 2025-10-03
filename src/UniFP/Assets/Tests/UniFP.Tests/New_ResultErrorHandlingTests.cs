using NUnit.Framework;
using UniFP;

namespace UniFP.Tests
{
    /// <summary>
    /// Error handling tests.
    /// Exercises Recover, IfFailed, Match, and Catch behaviors.
    /// </summary>
    public class ResultErrorHandlingTests
    {
        #region Recover Tests

        [Test]
        public void Recover_OnFailure_ReturnsDefaultValue()
        {
            var result = Result.FromError<int>(ErrorCode.NotFound)
                .Recover((ErrorCode e) => 0);

            Assert.AreEqual(0, result);
        }

        [Test]
        public void Recover_OnSuccess_ReturnsOriginalValue()
        {
            var result = Result.FromValue(42)
                .Recover((ErrorCode e) => 0);

            Assert.AreEqual(42, result);
        }

        [Test]
        public void Recover_WithCustomMessage_UsesErrorCodeFallback()
        {
            var result = Result<int>.Failure("error")
        .Recover(_ => -1);

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(-1, result.Value);
        }

        [Test]
        public void Recover_CanInspectError()
        {
            ErrorCode capturedError = ErrorCode.Unknown;

            var result = Result.FromError<int>(ErrorCode.NotFound)
                .Recover((ErrorCode e) =>
                {
                    capturedError = e;
                    return 0;
                });

            Assert.AreEqual(ErrorCode.NotFound, capturedError);
        }

        #endregion

        #region IfFailed Tests

        [Test]
        public void IfFailed_OnFailure_ReturnsAlternative()
        {
            var primary = Result<int>.Failure(ErrorCode.NotFound);
            var fallback = Result<int>.Success(42);

            var result = primary.IfFailed(fallback);

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(42, result.Value);
        }

        [Test]
        public void IfFailed_OnSuccess_ReturnsPrimary()
        {
            var primary = Result<int>.Success(100);
            var fallback = Result<int>.Success(42);

            var result = primary.IfFailed(fallback);

            Assert.AreEqual(100, result.Value);
        }

        [Test]
        public void IfFailed_WithFunc_LazilyEvaluatesAlternative()
        {
            bool alternativeCalled = false;

            var result = Result<int>.Success(100)
                .IfFailed(() =>
                {
                    alternativeCalled = true;
                    return Result<int>.Success(42);
                });

            Assert.IsFalse(alternativeCalled); // Not called for success
        }

        [Test]
        public void IfFailed_ChainsFallbacks()
        {
            var result = Result<int>.Failure(ErrorCode.NotFound)
                .IfFailed(Result<int>.Failure(ErrorCode.NetworkError))
                .IfFailed(Result<int>.Success(42));

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(42, result.Value);
        }

        #endregion

        #region Match Tests

        [Test]
        public void Match_OnSuccess_ExecutesSuccessFunc()
        {
            var result = Result.FromValue(42)
                .Match(
                    onSuccess: x => x * 2,
                    onFailure: (ErrorCode e) => 0
                );

            Assert.AreEqual(84, result);
        }

        [Test]
        public void Match_OnFailure_ExecutesFailureFunc()
        {
            var result = Result.FromError<int>(ErrorCode.NotFound)
                .Match(
                    onSuccess: x => x * 2,
                    onFailure: (ErrorCode e) => -1
                );

            Assert.AreEqual(-1, result);
        }

        [Test]
        public void Match_Void_OnSuccess_ExecutesSuccessAction()
        {
            int captured = 0;

            Result.FromValue(42).Match(
                onSuccess: x => captured = x,
                onFailure: (ErrorCode e) => captured = -1
            );

            Assert.AreEqual(42, captured);
        }

        [Test]
        public void Match_Void_OnFailure_ExecutesFailureAction()
        {
            ErrorCode capturedError = ErrorCode.Unknown;

            Result.FromError<int>(ErrorCode.NotFound).Match(
                onSuccess: x => { },
                onFailure: (ErrorCode e) => capturedError = e
            );

            Assert.AreEqual(ErrorCode.NotFound, capturedError);
        }

        [Test]
        public void Match_ChangesReturnType()
        {
            var result = Result.FromValue(42)
                .Match(
                    onSuccess: x => $"Value: {x}",
                    onFailure: (ErrorCode e) => $"Error: {e}"
                );

            Assert.AreEqual("Value: 42", result);
        }

        #endregion

        #region Catch Tests

        [Test]
        public void Catch_MatchingError_HandlesError()
        {
            var result = Result<int>.Failure("timeout error")
                .Catch(
                    errorPredicate: msg => msg.Contains("timeout"),
                    handler: msg => Result<int>.Success(42)
                );

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(42, result.Value);
        }

        [Test]
        public void Catch_NonMatchingError_PreservesError()
        {
            var result = Result<int>.Failure("network error")
                .Catch(
                    errorPredicate: msg => msg.Contains("timeout"),
                    handler: msg => Result<int>.Success(42)
                );

            Assert.IsTrue(result.IsFailure);
            Assert.That(result.ErrorMessage, Does.Contain("network error"));
        }

        [Test]
        public void Catch_OnSuccess_DoesNothing()
        {
            var result = Result<int>.Success(100)
                .Catch(
                    errorPredicate: msg => true,
                    handler: msg => Result<int>.Success(42)
                );

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(100, result.Value);
        }

        [Test]
        public void Catch_CanChainMultiple()
        {
            var result = Result<int>.Failure("database error")
                .Catch(
                    errorPredicate: msg => msg.Contains("network"),
                    handler: msg => Result<int>.Success(1)
                )
                .Catch(
                    errorPredicate: msg => msg.Contains("database"),
                    handler: msg => Result<int>.Success(2)
                );

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(2, result.Value);
        }

        #endregion

        #region Railway Pattern Tests

        [Test]
        public void RailwayPattern_AllSuccess_ReturnsSuccess()
        {
            var result = ValidateEmail("test@example.com")
                .Then(email => ValidateAge("25")
                    .Map(age => new User { Email = email, Age = age }));

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual("test@example.com", result.Value.Email);
            Assert.AreEqual(25, result.Value.Age);
        }

        [Test]
        public void RailwayPattern_EmailFails_StopsEarly()
        {
            bool ageCalled = false;

            var result = ValidateEmail("invalid-email")
                .Then(email =>
                {
                    ageCalled = true;
                    return Result<User>.Success(new User());
                });

            Assert.IsTrue(result.IsFailure);
            Assert.IsFalse(ageCalled);
        }

        [Test]
        public void RailwayPattern_WithRecovery()
        {
            var result = ValidateAge("-5")
                .Recover((ErrorCode e) => 18); // Default to adult age

            Assert.AreEqual(18, result);
        }

        #endregion

        #region Helper Methods

        Result<string> ValidateEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
                return Result<string>.Failure(ErrorCode.InvalidInput);
            if (!email.Contains("@"))
                return Result<string>.Failure(ErrorCode.ValidationFailed);
            return Result<string>.Success(email);
        }

        Result<int> ValidateAge(string ageStr)
        {
            if (!int.TryParse(ageStr, out var age))
                return Result<int>.Failure(ErrorCode.InvalidInput);
            if (age < 0 || age > 150)
                return Result<int>.Failure(ErrorCode.ValidationFailed);
            return Result<int>.Success(age);
        }

        class User
        {
            public string Email { get; set; }
            public int Age { get; set; }
        }

        #endregion
    }
}

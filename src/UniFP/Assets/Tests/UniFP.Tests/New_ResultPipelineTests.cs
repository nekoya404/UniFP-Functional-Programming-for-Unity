using NUnit.Framework;
using UniFP;

namespace UniFP.Tests
{
    /// <summary>
    /// Pipeline chaining tests.
    /// Focuses on Then, Map, Filter, and Do operators.
    /// </summary>
    public class ResultPipelineTests
    {
        #region Then Tests

        [Test]
        public void Then_OnSuccess_ExecutesFunction()
        {
            var result = Result.FromValue(10)
                .Then(x => Result<int>.Success(x * 2));

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(20, result.Value);
        }

        [Test]
        public void Then_OnFailure_SkipsFunction()
        {
            var result = Result.FromError<int>(ErrorCode.Unknown)
                .Then(x => Result<int>.Success(x * 2));

            Assert.IsTrue(result.IsFailure);
            Assert.AreEqual(ErrorCode.Unknown, result.ErrorCode);
        }

        [Test]
        public void Then_Chains_MultipleOperations()
        {
            var result = Result.FromValue(5)
                .Then(x => Result<int>.Success(x + 1))
                .Then(x => Result<int>.Success(x * 2))
                .Then(x => Result<int>.Success(x - 2));

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(10, result.Value); // (5+1)*2-2 = 10
        }

        [Test]
        public void Then_PropagatesFailure()
        {
            var result = Result.FromValue(10)
                .Then(x => Result<int>.Success(x * 2))
                .Then(x => Result<int>.Failure(ErrorCode.ValidationFailed))
                .Then(x => Result<int>.Success(x + 100));

            Assert.IsTrue(result.IsFailure);
            Assert.AreEqual(ErrorCode.ValidationFailed, result.ErrorCode);
        }

        #endregion

        #region Map Tests

        [Test]
        public void Map_OnSuccess_TransformsValue()
        {
            var result = Result.FromValue(10)
                .Map(x => x * 2);

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(20, result.Value);
        }

        [Test]
        public void Map_OnFailure_SkipsTransformation()
        {
            var result = Result.FromError<int>(ErrorCode.Unknown)
                .Map(x => x * 2);

            Assert.IsTrue(result.IsFailure);
            Assert.AreEqual(ErrorCode.Unknown, result.ErrorCode);
        }

        [Test]
        public void Map_ChangesType()
        {
            var result = Result.FromValue(42)
                .Map(x => $"Value: {x}");

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual("Value: 42", result.Value);
        }

        [Test]
        public void Map_ChainsMultipleTimes()
        {
            var result = Result.FromValue(10)
                .Map(x => x * 2)
                .Map(x => x + 5)
                .Map(x => x.ToString());

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual("25", result.Value);
        }

        #endregion

        #region Filter Tests

        [Test]
        public void Filter_PassesPredicate_ReturnsSuccess()
        {
            var result = Result.FromValue(10)
                .Filter(x => x > 0, ErrorCode.ValidationFailed);

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(10, result.Value);
        }

        [Test]
        public void Filter_FailsPredicate_ReturnsFailure()
        {
            var result = Result.FromValue(-5)
                .Filter(x => x > 0, ErrorCode.ValidationFailed);

            Assert.IsTrue(result.IsFailure);
            Assert.AreEqual(ErrorCode.ValidationFailed, result.ErrorCode);
        }

        [Test]
        public void Filter_MultipleConditions_AllMustPass()
        {
            var result = Result.FromValue(15)
                .Filter(x => x > 0, ErrorCode.ValidationFailed)
                .Filter(x => x < 100, ErrorCode.ValidationFailed)
                .Filter(x => x % 5 == 0, ErrorCode.ValidationFailed);

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(15, result.Value);
        }

        [Test]
        public void Filter_StopsAtFirstFailure()
        {
            var callCount = 0;

            var result = Result.FromValue(150)
                .Filter(x => x > 0, ErrorCode.ValidationFailed)
                .Filter(x => x < 100, ErrorCode.ValidationFailed) // Fails here
                .Filter(x =>
                {
                    callCount++;
                    return x % 5 == 0;
                }, ErrorCode.ValidationFailed);

            Assert.IsTrue(result.IsFailure);
            Assert.AreEqual(0, callCount); // Third filter not called
        }

        #endregion

        #region Do Tests

        [Test]
        public void Do_OnSuccess_ExecutesAction()
        {
            int sideEffect = 0;

            var result = Result.FromValue(10)
                .Do(x => sideEffect = x * 2);

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(10, result.Value); // Value unchanged
            Assert.AreEqual(20, sideEffect); // Side effect executed
        }

        [Test]
        public void Do_OnFailure_SkipsAction()
        {
            int sideEffect = 0;

            var result = Result.FromError<int>(ErrorCode.Unknown)
                .Do(x => sideEffect = x * 2);

            Assert.IsTrue(result.IsFailure);
            Assert.AreEqual(0, sideEffect); // Side effect not executed
        }

        [Test]
        public void Do_PreservesValue()
        {
            var result = Result.FromValue(42)
                .Do(x => { /* do nothing */ })
                .Map(x => x * 2);

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(84, result.Value);
        }

        #endregion

        #region Complex Pipeline Tests

        [Test]
        public void ComplexPipeline_AllOperators()
        {
            int doCounter = 0;

            var result = Result.FromValue("10")
                .Do(x => doCounter++)
                .Then(ParseInt)
                .Do(x => doCounter++)
                .Filter(x => x > 0, ErrorCode.ValidationFailed)
                .Map(x => x * 2)
                .Do(x => doCounter++)
                .Filter(x => x < 100, ErrorCode.ValidationFailed);

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(20, result.Value);
            Assert.AreEqual(3, doCounter);
        }

        [Test]
        public void ComplexPipeline_FailsAtMiddle()
        {
            int doCounter = 0;

            var result = Result.FromValue("10")
                .Do(x => doCounter++)
                .Then(ParseInt)
                .Filter(x => x > 100, ErrorCode.ValidationFailed) // Fails here
                .Do(x => doCounter++) // Should not execute
                .Map(x => x * 2);

            Assert.IsTrue(result.IsFailure);
            Assert.AreEqual(1, doCounter); // Only first Do executed
        }

        #endregion

        #region Helper Methods

        Result<int> ParseInt(string s)
        {
            if (int.TryParse(s, out var value))
                return Result<int>.Success(value);
            return Result<int>.Failure(ErrorCode.InvalidInput);
        }

        #endregion
    }
}

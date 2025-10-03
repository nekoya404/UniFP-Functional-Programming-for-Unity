using NUnit.Framework;
using UniFP;
using System;
#if UNIFP_UNITASK
using Cysharp.Threading.Tasks;
#endif

namespace UniFP.Tests
{
    /// <summary>
    /// Finally 확장 메서드 테스트
    /// </summary>
    [TestFixture]
    public class Result_Finally_Tests
    {
        #region Synchronous Finally Tests

        [Test]
        public void Finally_WithSuccess_ExecutesFinallyBlock()
        {
            // Arrange
            var result = Result<int>.Success(42);
            bool finallyExecuted = false;

            // Act
            var output = result.Finally(r =>
            {
                finallyExecuted = true;
                return r.Match(
                    value => value * 2,
                    error => 0
                );
            });

            // Assert
            Assert.IsTrue(finallyExecuted);
            Assert.AreEqual(84, output);
        }

        [Test]
        public void Finally_WithFailure_ExecutesFinallyBlock()
        {
            // Arrange
            var result = Result<int>.Failure(ErrorCode.Unknown);
            bool finallyExecuted = false;

            // Act
            var output = result.Finally(r =>
            {
                finallyExecuted = true;
                return r.Match(
                    value => value * 2,
                    error => -1
                );
            });

            // Assert
            Assert.IsTrue(finallyExecuted);
            Assert.AreEqual(-1, output);
        }

        [Test]
        public void Finally_WithAction_ExecutesSideEffect()
        {
            // Arrange
            var result = Result<int>.Success(42);
            int capturedValue = 0;

            // Act
            result.Finally(r =>
            {
                r.Match(
                    value => { capturedValue = value; },
                    error => { capturedValue = -1; }
                );
            });

            // Assert
            Assert.AreEqual(42, capturedValue);
        }

        [Test]
        public void Finally_AlwaysExecutes_RegardlessOfResult()
        {
            // Arrange
            var successResult = Result<int>.Success(100);
            var failureResult = Result<int>.Failure(ErrorCode.Unknown);
            int executionCount = 0;

            // Act
            successResult.Finally(_ => { executionCount++; });
            failureResult.Finally(_ => { executionCount++; });

            // Assert
            Assert.AreEqual(2, executionCount);
        }

        #endregion

        #region Cleanup Pattern Tests

        [Test]
        public void Finally_UsedForResourceCleanup_WorksCorrectly()
        {
            // Arrange
            var resource = new TestResource();

            // Act
            var result = Result<TestResource>.Success(resource)
                .Do(r => r.DoWork())
                .Finally(r =>
                {
                    // 리소스 정리 (성공/실패 관계없이)
                    r.Match(
                        res => res.Dispose(),
                        _ => resource.Dispose()
                    );
                    return r;
                });

            // Assert
            Assert.IsTrue(resource.IsDisposed);
        }

        [Test]
        public void Finally_InRailwayPattern_PreservesResult()
        {
            // Arrange
            int finallyCallCount = 0;

            // Act
            var result = Result<int>.Success(10)
                .Map(x => x * 2)
                .Finally(r => { finallyCallCount++; return r; })
                .Map(x => x + 5)
                .Finally(r => { finallyCallCount++; return r; });

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(25, result.Value);
            Assert.AreEqual(2, finallyCallCount);
        }

        #endregion

        #region Error Handling Pattern Tests

        [Test]
        public void Finally_WithLogging_LogsBothSuccessAndFailure()
        {
            // Arrange
            var logger = new TestLogger();

            // Act - Success case
            Result<int>.Success(42)
                .Finally(r =>
                {
                    r.Match(
                        value => logger.Log($"Operation succeeded with value: {value}"),
                        error => logger.Log($"Operation failed with error: {error}")
                    );
                });

            // Act - Failure case
            Result<int>.Failure(ErrorCode.NotFound, "Item not found")
                .Finally(r =>
                {
                    r.Match(
                        value => logger.Log($"Operation succeeded with value: {value}"),
                        error => logger.Log($"Operation failed with error: {error}")
                    );
                });

            // Assert
            Assert.AreEqual(2, logger.Logs.Count);
            Assert.IsTrue(logger.Logs[0].Contains("succeeded"));
            Assert.IsTrue(logger.Logs[1].Contains("failed"));
        }

        #endregion

#if UNIFP_UNITASK
        #region Async Finally Tests (UniTask)

        [Test]
        public async UniTask Finally_Async_WithUniTask_ExecutesFinallyBlock()
        {
            // Arrange
            var task = UniTask.FromResult(Result<int>.Success(42));
            bool finallyExecuted = false;

            // Act
            var output = await task.Finally(r =>
            {
                finallyExecuted = true;
                return r.Match(
                    value => value * 2,
                    error => 0
                );
            });

            // Assert
            Assert.IsTrue(finallyExecuted);
            Assert.AreEqual(84, output);
        }

        [Test]
        public async UniTask Finally_AsyncToAsync_WorksCorrectly()
        {
            // Arrange
            var task = UniTask.FromResult(Result<int>.Success(10));
            int finallyValue = 0;

            // Act
            var result = await task.Finally(async r =>
            {
                await UniTask.Delay(10);
                finallyValue = r.Value;
                return r;
            });

            // Assert
            Assert.AreEqual(10, finallyValue);
            Assert.IsTrue(result.IsSuccess);
        }

        [Test]
        public async UniTask Finally_SyncToAsync_WorksCorrectly()
        {
            // Arrange
            var result = Result<int>.Success(20);
            int asyncFinallyValue = 0;

            // Act
            var output = await result.Finally(async r =>
            {
                await UniTask.Delay(10);
                asyncFinallyValue = r.Value * 2;
                return asyncFinallyValue;
            });

            // Assert
            Assert.AreEqual(40, output);
        }

        #endregion
#endif

        #region Helper Classes

        private class TestResource : IDisposable
        {
            public bool IsDisposed { get; private set; }

            public void DoWork()
            {
                // 작업 수행
            }

            public void Dispose()
            {
                IsDisposed = true;
            }
        }

        private class TestLogger
        {
            public System.Collections.Generic.List<string> Logs { get; } = new();

            public void Log(string message)
            {
                Logs.Add(message);
            }
        }

        #endregion
    }
}

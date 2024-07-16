using System.Threading.Tasks;
using NSubstitute;
using ResultsTests.Helpers;
using Xunit;

namespace ResultsTests
{
    public class AsyncDnDApiTests
    {
        private readonly ID20 _d20;
        private readonly AsyncDnDApi _asyncApi;
        private readonly Enemy _invalidArmourEnemy = new() { ArmourClass = -1 };
        private readonly Enemy _validEnemy = new() { ArmourClass = 20 };
        
        public AsyncDnDApiTests()
        {
            _d20 = Substitute.For<ID20>();
            _asyncApi = new AsyncDnDApi(_d20);
        }
        
        [Fact]
        public async Task When_Null_Provided_AsyncAttackEnemy_Should_Fail()
        {
            var result = await _asyncApi.AsyncAttack(null);
            Assert.False(result);
        }

        [Fact]
        public async Task When_Null_Provided_AsyncAttackEnemy_Should_Fail_With_Correct_ErrorCode()
        {
            var result = await _asyncApi.AsyncAttack(null);
            if (result)
            {
                Assert.Fail("Async attack should fail if null enemy is provided.");
            }
            else
            {
                Assert.True(result.ErrorCode == ErrorCodeRepository.AsyncDnDApiErrors.AsyncAttack.NoEnemyProvided);
            }
        }
        
        [Fact]
        public async Task When_Enemy_With_Negative_ArmourClass_Provided_AsyncAttackEnemy_Should_Fail()
        {
            var result = await _asyncApi.AsyncAttack(_invalidArmourEnemy);
            Assert.False(result);
        }
        
        [Fact]
        public async Task When_Enemy_With_Negative_ArmourClass_Provided_AsyncAttackEnemy_Should_Fail_With_Correct_ErrorCode()
        {
            var result = await _asyncApi.AsyncAttack(_invalidArmourEnemy);
            if(result.Succeeded)
            {
                Assert.Fail("Async attack should fail if enemy has negative ArmourClass.");
            }
            else
            {
                Assert.True(result.ErrorCode == ErrorCodeRepository.AsyncDnDApiErrors.AsyncAttack.InvalidEnemyArmourClass);
            }
        }
        
        [Fact]
        public async Task When_AttackEnemy_Succeeded_And_Roll_Was_Less_Than_10_Response_Should_Be_CriticalMiss()
        {
            _d20.Roll().Returns(5);
            var result = await _asyncApi.AsyncAttack(_validEnemy);
            if(result.Succeeded)
            {
                Assert.True(result.Value == AttackResult.CriticalMiss);
            }
            else
            {
                Assert.Fail("Expected the attack to succeed when enemy provided is valid.");
            }
        }

        #if RESULT_CALLBACKS
        [Fact]
        public async Task When_Null_Provided_AsyncAttackEnemy_Should_Fail_With_Correct_ErrorCode_Calbacks()
        {
            (await _asyncApi.AsyncAttack(null))
                .IfSuccessful(_ => 
                    Assert.Fail("Async attack should fail if null enemy is provided."))
                .IfError(errorCode =>
                {
                    Assert.True(errorCode == ErrorCodeRepository.AsyncDnDApiErrors.AsyncAttack.NoEnemyProvided);
                });
        }
        
        [Fact]
        public async Task When_Enemy_With_Negative_ArmourClass_Provided_AsyncAttackEnemy_Should_Fail_With_Correct_ErrorCode_Callbacks()
        {
            (await _asyncApi.AsyncAttack(_invalidArmourEnemy))
                .IfSuccessful(_ => 
                    Assert.Fail("Async attack should fail if enemy has negative ArmourClass."))
                .IfError(errorCode =>
                {
                    Assert.True(errorCode == ErrorCodeRepository.AsyncDnDApiErrors.AsyncAttack.InvalidEnemyArmourClass);
                });
        }
        
        [Fact]
        public async Task When_AttackEnemy_Succeeded_And_Roll_Was_Less_Than_10_Response_Should_Be_CriticalMiss_Callbacks()
        {
            _d20.Roll().Returns(5);
            (await _asyncApi.AsyncAttack(_validEnemy))
                .IfSuccessful(result =>
                {
                    Assert.True(result == AttackResult.CriticalMiss);
                })
                .IfError(_ =>
                {
                    Assert.Fail("Expected the attack to succeed when enemy provided is valid.");
                });
        }
        #endif
    }
}
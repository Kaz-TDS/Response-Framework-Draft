using NSubstitute;
using ResultsTests.Helpers;

namespace ResultsTests
{
    public class DnDApiTests
    {
        private readonly ID20 _d20;
        private readonly DnDApi _api;
        private readonly Enemy _invalidArmourEnemy = new() { ArmourClass = -1, Distance = 5 };
        private readonly Enemy _validEnemy = new() { ArmourClass = 20, Distance = 5 };

        public DnDApiTests()
        {
            _d20 = Substitute.For<ID20>();
            _api = new DnDApi(_d20);
        }

        [Fact]
        public void When_Enemy_Provided_CanAttack_Should_Succeed()
        {
            var enemy = new Enemy();
            
            Assert.True(_api.CanAttackEnemy(enemy));
        }

        [Fact]
        public void When_Null_Provided_CanAttack_Should_Fail()
        {
            Assert.False(_api.CanAttackEnemy(null));
        }

        [Fact]
        public void When_Null_Provided_CanAttack_Should_Fail_With_Correct_ErrorCode()
        {
            Assert.True(
                _api.CanAttackEnemy(null).ErrorCode == 
                ResultsFactory.DnDApi.CanAttackEnemy.NoEnemyProvidedErrorCode);
        }

        [Fact]
        public void When_Null_Provided_AttackEnemy_Should_Fail()
        {
            Assert.False(_api.AttackTheEnemy(null));
        }

        [Fact]
        public void When_Null_Provided_AttackEnemy_Should_Fail_With_Correct_ErrorCode()
        {
            Assert.True(_api.AttackTheEnemy(null).ErrorCode ==
                        ResultsFactory.DnDApi.AttackTheEnemy.NoEnemyProvidedErrorCode);
        }

        [Fact]
        public void When_Enemy_With_Negative_ArmourClass_Provided_AttackEnemy_Should_Fail()
        {
            Assert.False(_api.AttackTheEnemy(_invalidArmourEnemy));
        }
        
        [Fact]
        public void When_Enemy_With_Negative_ArmourClass_Provided_AttackEnemy_Should_Fail_With_Correct_ErrorCode()
        {
            Assert.True(_api.AttackTheEnemy(_invalidArmourEnemy).ErrorCode ==
                         ResultsFactory.DnDApi.AttackTheEnemy.InvalidEnemyArmourclassErrorCode);
        }

        [Fact]
        public void When_Valid_Enemy_Provided_AttackEnemy_Should_Succeed()
        {
            Assert.True(_api.AttackTheEnemy(_validEnemy));
        }

        [Fact]
        public void When_AttackEnemy_Succeeded_And_Roll_Was_Less_Than_10_Response_Should_Be_CriticalMiss()
        {
            _d20.Roll().Returns(5);
            Assert.True(_api.AttackTheEnemy(_validEnemy).Response == AttackResult.CriticalMiss);
        }
        
        [Fact]
        public void When_AttackEnemy_Succeeded_And_Roll_Was_Less_Than_20_But_More_Than_10_Response_Should_Be_Miss()
        {
            _d20.Roll().Returns(15);
            Assert.True(_api.AttackTheEnemy(_validEnemy).Response == AttackResult.Miss);
        }
        
        [Fact]
        public void When_AttackEnemy_Succeeded_And_Roll_Was_More_Than_20_But_Less_Than_30_Response_Should_Be_Hit()
        {
            _d20.Roll().Returns(25);
            Assert.True(_api.AttackTheEnemy(_validEnemy).Response == AttackResult.Hit);
        }
        
        [Fact]
        public void When_AttackEnemy_Succeeded_And_Roll_Was_More_Than_30_Response_Should_Be_CriticalHit()
        {
            _d20.Roll().Returns(31);
            Assert.True(_api.AttackTheEnemy(_validEnemy).Response == AttackResult.CriticalHit);
        }
    }
}
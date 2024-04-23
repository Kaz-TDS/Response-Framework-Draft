using ResultsLib;
using TDS.Results;

namespace ResultsTests.Helpers
{
    public class DnDApi
    {
        private readonly ID20 _die;

        public DnDApi(ID20 die)
        {
            _die = die;
        }
        
        [ErrorResult(errorCode: 1, errorMessage: "No enemy provided")]
        public Result CanAttackEnemy(Enemy enemy)
        {
            if (enemy == null)
            {
                return ResultsFactory.DnDApi.CanAttackEnemy.NoEnemyProvided;
            }
            return Result.Success();
        }

        [ErrorResult(errorCode: 1, errorMessage: "No enemy provided")]
        [ErrorResult(errorCode: 2, errorMessage: "Invalid enemy Armour Class")]
        [ErrorResult(errorCode: 3, errorMessage: "What the hell")]
        public Result<AttackResult> AttackTheEnemy(Enemy enemy)
        {
            if (enemy == null)
            {
                return ResultsFactory.DnDApi.AttackTheEnemy.NoEnemyProvided(AttackResult.Miss);
            }
            
            if (enemy.ArmourClass < 0)
            {
                return ResultsFactory.DnDApi.AttackTheEnemy.InvalidEnemyArmourClass(AttackResult.Miss);
            }

            var rollResult = _die.Roll();

            var adjustedResult = rollResult - enemy.ArmourClass;
            var attackResult = adjustedResult switch
            {
                < -10 => AttackResult.CriticalMiss,
                < 0 => AttackResult.Miss,
                < 10 => AttackResult.Hit,
                _ => AttackResult.CriticalHit
            };

            return Result<AttackResult>.Success(attackResult);
        }
    }

    public class Enemy
    {
        public int ArmourClass;
    }

    public enum AttackResult
    {
        Undefined = 0,
        CriticalMiss = 1,
        Miss = 2,
        Hit = 7,
        CriticalHit = -2
    }
}
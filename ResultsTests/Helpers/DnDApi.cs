using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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

        [ErrorResult(errorCode: 1, errorMessage:"No enemies provided")]
        [ErrorResult(errorCode: 2, errorMessage:"No instance processor provided")]
        [ErrorResult(errorCode: 3, errorMessage:"Encountered a null enemy")]
        public Result<List<T>> ProcessMultipleEnemies<T>(List<Enemy> enemies, Action<Enemy> instanceProcessMethod) where T : struct
        {
            if (enemies == null || enemies.Count == 0)
            {
                return ResultsFactory.DnDApi.ProcessMultipleEnemies.NoEnemiesProvided(new List<T>());
            }

            if (instanceProcessMethod == null)
            {
                return ResultsFactory.DnDApi.ProcessMultipleEnemies.NoInstanceProcessorProvided(new List<T>());
            }

            foreach (var enemy in enemies)
            {
                if (enemy != null)
                {
                    instanceProcessMethod(enemy);
                }

                if (enemy == null)
                {
                    return ResultsFactory.DnDApi.ProcessMultipleEnemies.EncounteredANullEnemy(new List<T>());
                }
            }
            
            return Result<List<T>>.Success(default);
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

    public class AsyncDnDApi
    {
        private readonly ID20 _die;

        public AsyncDnDApi(ID20 die)
        {
            _die = die;
        }
        
        [ErrorResult(errorCode: 1, errorMessage: "No enemy provided")]
        [ErrorResult(errorCode: 2, errorMessage: "Invalid enemy Armour Class")]
        public async Task<Result<AttackResult>> AsyncAttack(Enemy enemy)
        {
            await Task.Delay(1000);
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
}
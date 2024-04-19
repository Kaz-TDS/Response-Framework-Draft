using ResultsLib;
using TDS.Results;

namespace ResultsTests.Helpers
{

    public class DnDApi
    {
        // [ErrorResult(errorCode: 1,errorMessage: "No enemy provided")]
        // [ErrorResult(errorCode: 2, errorMessage: "Invalid enemy ArmourClass")]
        // public Result<AttackResult> AttackTheEnemy(Enemy enemy)
        // {
        //     if (enemy == null)
        //     {
        //         return ResultsFactory.DnDApi.AttackTheEnemy.NoEnemyProvided;
        //     }
        //
        //     if (enemy.ArmourClass < 0)
        //     {
        //         return ResultsFactory.DnDApi.AttackTheEnemy.InvalidEnemyArmourClass;
        //     }
        //     
        //     var d20 = new System.Random();
        //     var roll = 1 + d20.Next(20);
        //
        //     var adjustedResult = roll - enemy.ArmourClass;
        //
        //     var result = adjustedResult switch
        //     {
        //         > 10 => Result<AttackResult>.Success(AttackResult.CriticalHit),
        //         >= 0 => Result<AttackResult>.Success(AttackResult.Hit),
        //         <= -10 => Result<AttackResult>.Success(AttackResult.CriticalMiss),
        //         < 0 => Result<AttackResult>.Success(AttackResult.Miss)
        //     };
        //
        //     return result;
        // }

        [ErrorResult(errorCode: 1, errorMessage: "No enemy provided")]
        [ErrorResult(errorCode: 2, errorMessage: "Invalid enemy ArmourClass")]
        [ErrorResult(errorCode: 3, errorMessage: "What the hell")]
        public Result AttackTheEnemy(Enemy enemy)
        {
            try
            {
                if (enemy == null)
                {
                    return ResultsFactory.DnDApi.AttackTheEnemy.NoEnemyProvided;
                }

                if (enemy.ArmourClass < 0)
                {
                    return ResultsFactory.DnDApi.AttackTheEnemy.InvalidEnemyArmourclass;
                }
            }
            catch (Exception e)
            {
                return ResultsFactory.DnDApi.AttackTheEnemy.WhatTheHell;
            }
            
            return Result.Success();
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
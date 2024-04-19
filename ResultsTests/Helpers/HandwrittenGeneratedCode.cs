// using TDS.Results;
//
// namespace ResultsTests.Helpers
// {
// #if DEBUG
//     public static partial class ResultsFactory
//     {
//         public static DnDApiResultsFactory DnDApi => new();
//     }
//
//     public partial class DnDApiResultsFactory
//     {
//         public AttackTheEnemyErrors AttackTheEnemy => new();
//
//         public class AttackTheEnemyErrors
//         {
//             public Result<AttackResult> NoEnemyProvided
//                 => new()
//                 {
//                     Succeeded = false, ErrorCode = 1,
//                     Response = default, ErrorMessage = "No enemy provided"
//                 };
//
//             public Result<AttackResult> InvalidEnemyArmourClass
//                 => new()
//                 {
//                     Succeeded = false, ErrorCode = 2,
//                     Response = default, ErrorMessage = "Invalid enemy ArmourClass"
//                 };
//         }
//     }
// }
// #else
//     public static class ResultsFactory
//     {
//         public static DnDApiResultsFactory DnDApi => new();
//     }
//
//     public class DnDApiResultsFactory
//     {
//         public AttackTheEnemyErrors AttackTheEnemy => new();
//
//         public class AttackTheEnemyErrors
//         {
//             public Result<AttackResult> NoEnemyProvided
//                 => new()
//                 {
//                     Succeeded = false, ErrorCode = 1,
//                     Response = default
//                 };
//
//             public Result<AttackResult> InvalidEnemyArmourClass
//                 => new()
//                 {
//                     Succeeded = false, ErrorCode = 2,
//                     Response = default
//                 };
//         }
//     }
// #endif

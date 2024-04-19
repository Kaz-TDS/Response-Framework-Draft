using ResultsTests.Helpers;

namespace ResultsTests
{
    public class DndApiTests
    {
        [Fact]
        public void Test1()
        {
            var dndapi = new DnDApi();
            var result = dndapi.AttackTheEnemy(null);
            Assert.False(result.Succeeded);
            Assert.Equal(1, result.ErrorCode);
        }
    }
}
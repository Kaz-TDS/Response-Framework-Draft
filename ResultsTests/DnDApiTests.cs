using ResultsTests.Helpers;

namespace ResultsTests;

public class DndApiTests
{
    [Fact]
    public void Test1()
    {
        var dndapi = new DnDApi();
        if (dndapi.AttackTheEnemy(default))
        {
            
        }

        var result = dndapi.AttackTheEnemy(new Enemy());
        switch (result.ErrorCode)
        {
            
        }
    }
}
using XCDESave;

namespace XCDESaveEditor.Gui.Flows;

internal sealed class SetMoney
{
    public void Run(XCDESaveData saveData, uint newAmount)
    {
        saveData.Money = newAmount;
    }
}

internal sealed class SetNoponstones
{
    public void Run(XCDESaveData saveData, uint newAmount)
    {
        saveData.Noponstones = newAmount;
    }
}

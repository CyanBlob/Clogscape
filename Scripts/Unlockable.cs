public class Unlockable
{
    private bool Unlocked = false;

    public bool IsUnlocked()
    {
        return Unlocked;
    }

    public void Unlock()
    {
        Unlocked = true;
    }
}
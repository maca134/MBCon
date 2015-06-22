namespace PlayerCheck.Drivers
{
    public interface IDriver
    {
        void SetConfig(DriverSettings settings);
        bool CheckPlayer(Player player);
    }
}

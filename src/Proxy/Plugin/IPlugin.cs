namespace Proxy.Plugin
{
    public interface IPlugin
    {
        string Name { get; }
        string Author { get; }
        void Init(IApi api, string dllpath);
        void Kill();
    }
}

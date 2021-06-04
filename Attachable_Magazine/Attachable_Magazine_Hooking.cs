using Deli.Setup;

namespace Cityrobo
{
    public class Attachable_Magazine_Hooking : DeliBehaviour
    {

        private readonly Attachable_Magazine_Hooks _hooks;
        public Attachable_Magazine_Hooking()
        {
            Logger.LogInfo("Attachable_Magazine_Hooks loaded!");
            _hooks = new Attachable_Magazine_Hooks();
            _hooks.Hook();
        }
        private void OnDestroy()
        {
            _hooks?.Unhook();
        }
    }
}

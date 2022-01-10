using Deli.Setup;

namespace Cityrobo
{
	public class SnappyBoltActionTrigger_Plugin : DeliBehaviour
	{
		private readonly SnappyBoltActionTrigger_Hooks _hooks;

		public SnappyBoltActionTrigger_Plugin()
		{
			Logger.LogInfo("SnappyBoltActionTrigger loaded!");
			_hooks = new SnappyBoltActionTrigger_Hooks();
			_hooks.Hook();
		}

		private void OnDestroy()
		{
			_hooks?.Unhook();
		}
	}
}
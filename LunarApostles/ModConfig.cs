using BepInEx.Configuration;

namespace LunarApostles
{
  internal class ModConfig
  {
    public static ConfigEntry<bool> enableAdaptiveArmor;
    public static ConfigEntry<float> sBaseHealth;
    public static ConfigEntry<float> sLevelHealth;
    public static ConfigEntry<float> oBaseHealth;
    public static ConfigEntry<float> oLevelHealth;
    public static ConfigEntry<float> baseDamage;
    public static ConfigEntry<float> levelDamage;

    public static void InitConfig(ConfigFile config)
    {
      enableAdaptiveArmor = config.Bind("General", "Adaptive Armor", true, "Enable Adaptive Armor on the scavs (same armor mithrix has)");
      sBaseHealth = config.Bind("Stats", "Base HP", 500f, "Vanilla: 3k-4k depending on scav (they also didnt have adaptive armor)");
      sLevelHealth = config.Bind("Stats", " Level HP", 150f, "Health gained per level. Vanilla: 1k-1.2k depending on scav");
      baseDamage = config.Bind("Stats", "Base Damage", 8f, "Vanilla: 4");
      levelDamage = config.Bind("Stats", "Level Damage", 1.6f, "Damage gained per level. Vanilla: 0.8");
      oBaseHealth = config.Bind("Other Stats", "Tracking Orb Base Health", 180f, "Vanilla: 75");
      oLevelHealth = config.Bind("Other Stats", "Tracking Orb Health", 54f, "Health gained per level. Vanilla: 22");
    }
  }
}

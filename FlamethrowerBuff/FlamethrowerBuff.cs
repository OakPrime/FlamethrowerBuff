using BepInEx;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine;

namespace FlamethrowerBuff
{

    //This is an example plugin that can be put in BepInEx/plugins/ExamplePlugin/ExamplePlugin.dll to test out.
    //It's a small plugin that adds a relatively simple item to the game, and gives you that item whenever you press F2.

    //This attribute is required, and lists metadata for your plugin.
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]

    //This is the main declaration of our plugin class. BepInEx searches for all classes inheriting from BaseUnityPlugin to initialize on startup.
    //BaseUnityPlugin itself inherits from MonoBehaviour, so you can use this as a reference for what you can declare and use in your plugin class: https://docs.unity3d.com/ScriptReference/MonoBehaviour.html
    public class FlamethrowerBuff : BaseUnityPlugin
    {
        //The Plugin GUID should be a unique ID for this plugin, which is human readable (as it is used in places like the config).
        //If we see this PluginGUID as it is on thunderstore, we will deprecate this mod. Change the PluginAuthor and the PluginName !
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "OakPrime";
        public const string PluginName = "FlamethrowerBuff";
        public const string PluginVersion = "1.0.0";

        //The Awake() method is run at the very start when the game is initialized.
        public void Awake()
        {
            Log.Init(Logger);
            try
            {
                RoR2.Skills.SkillDef flameDef = Addressables.LoadAssetAsync<RoR2.Skills.SkillDef>("RoR2/Base/Mage/MageBodyFlamethrower.asset").WaitForCompletion();
                if (flameDef)
                {
                    flameDef.canceledFromSprinting = false;
                    flameDef.cancelSprintingOnActivation = true;
                };
                IL.EntityStates.Mage.Weapon.Flamethrower.FireGauntlet += (il) =>
                {
                    ILCursor c = new ILCursor(il);
                    c.TryGotoNext(
                        x => x.MatchLdarg(out _),
                        x => x.MatchLdfld(out _),
                        x => x.MatchCallOrCallvirt(out _)
                    );
                    c.Index += 2;
                    c.Emit(OpCodes.Ldc_R4, 1.5f);
                    c.Emit(OpCodes.Mul);
                };
                IL.EntityStates.Mage.Weapon.Flamethrower.OnEnter += (il) =>
                {
                    ILCursor c = new ILCursor(il);
                    c.Emit(OpCodes.Ldarg_0);
                    c.Emit(OpCodes.Ldarg_0);
                    c.Emit(OpCodes.Ldfld, typeof(EntityStates.Mage.Weapon.Flamethrower).GetField("flamethrowerEffectPrefab"));
                    c.EmitDelegate<Func<GameObject, GameObject>>((prefab) =>
                    {
                        prefab.transform.localScale = new Vector3(prefab.transform.localScale.x, prefab.transform.localScale.y, 1.5f);
                        return prefab;
                    });
                    c.Emit(OpCodes.Stfld, typeof(EntityStates.Mage.Weapon.Flamethrower).GetField("flamethrowerEffectPrefab"));
                };
            }
            catch (Exception e)
            {
                Logger.LogError(e.Message + " - " + e.StackTrace);
            };
        }
    }
}

using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Fallout4;
using Mutagen.Bethesda.FormKeys.Fallout4;

namespace AttachmentDemystifier
{
    public class Program
    {
        public static async Task<int> Main(string[] args)
        {
            return await SynthesisPipeline.Instance
                .AddPatch<IFallout4Mod, IFallout4ModGetter>(RunPatch)
                .SetTypicalOpen(GameRelease.Fallout4, "YourPatcher.esp")
                .Run(args);
        }

        public static void RunPatch(IPatcherState<IFallout4Mod, IFallout4ModGetter> state)
        {
            foreach (var objModGetter in state.LoadOrder.PriorityOrder.AObjectModification().WinningOverrides())
            {
                // Rich weapon attachment descriptions.
                if (objModGetter is IWeaponModificationGetter weaponModGetter)
                {
                    string newDescription = "";
                    foreach (var propertyMod in weaponModGetter.Properties)
                    {
                        if (propertyMod is IObjectModFormLinkFloatPropertyGetter<Weapon.Property> formModFloat)
                        {
                            if (formModFloat.Property == Weapon.Property.DamageTypeValues)
                            {
                                newDescription += (formModFloat.Value < 0 ? "Decreases " : "Increases ") + "damage by " + (formModFloat.FunctionType == ObjectModProperty.FloatFunctionType.MultAndAdd ? (Math.Abs(Math.Round(formModFloat.Value * 100)) + "%") : Math.Abs(Math.Round(formModFloat.Value))) + ". ";
                            }
                        }
                        else if (propertyMod is IObjectModFormLinkIntPropertyGetter<Weapon.Property> formModInt)
                        {
                            if (formModInt.Property == Weapon.Property.Ammo)
                            {
                                if (formModInt.Record.TryResolve<IAmmunitionGetter>(state.LinkCache, out var ammoGetter))
                                    newDescription += $"Rechambers weapon to {ammoGetter.ShortName}. ";
                                else
                                    newDescription += "Rechambers weapon to a new ammo type. ";
                            }
                            else if (formModInt.Property == Weapon.Property.Enchantments)
                            {
                                if (formModInt.Record.TryResolve<IObjectEffectGetter>(state.LinkCache, out var effectGetter))
                                {
                                    foreach (var effectEntryGetter in effectGetter.Effects)
                                    {
                                        if (effectEntryGetter.BaseEffect.TryResolve(state.LinkCache, out var magicEffectGetter))
                                        {
                                            if (magicEffectGetter.HasKeyword(Fallout4.Keyword.DamageTypeEnergy))
                                                newDescription += $"Deals {effectEntryGetter.Data!.Magnitude * effectEntryGetter.Data.Duration} energy damage" + (effectEntryGetter.Data.Duration > 1 ? $" over {effectEntryGetter.Data.Duration}s" : "") + ". ";
                                            else if (magicEffectGetter.HasKeyword(Fallout4.Keyword.DamageTypeRadiation))
                                                newDescription += $"Deals {effectEntryGetter.Data!.Magnitude * effectEntryGetter.Data.Duration} radiation damage" + (effectEntryGetter.Data.Duration > 1 ? $" over {effectEntryGetter.Data.Duration}s" : "") + ". ";
                                            else if (magicEffectGetter.HasKeyword(Fallout4.Keyword.DamageTypeAcid))
                                                newDescription += $"Deals {effectEntryGetter.Data!.Magnitude * effectEntryGetter.Data.Duration} acid damage" + (effectEntryGetter.Data.Duration > 1 ? $" over {effectEntryGetter.Data.Duration}s" : "") + ". ";
                                            else if (magicEffectGetter.HasKeyword(Fallout4.Keyword.DamageTypeCyro))
                                                newDescription += $"Deals {effectEntryGetter.Data!.Magnitude * effectEntryGetter.Data.Duration} cryo damage" + (effectEntryGetter.Data.Duration > 1 ? $" over {effectEntryGetter.Data.Duration}s" : "") + ". ";
                                            else if (magicEffectGetter.HasKeyword(Fallout4.Keyword.DamageTypeElectricty))
                                                newDescription += $"Deals {effectEntryGetter.Data!.Magnitude * effectEntryGetter.Data.Duration} shock damage" + (effectEntryGetter.Data.Duration > 1 ? $" over {effectEntryGetter.Data.Duration}s" : "") + ". ";
                                            else if (magicEffectGetter.HasKeyword(Fallout4.Keyword.DamageTypeFire) || magicEffectGetter.HasKeyword(Fallout4.Keyword.DamageTypeLaserFire))
                                                newDescription += $"Deals {effectEntryGetter.Data!.Magnitude * effectEntryGetter.Data.Duration} fire damage" + (effectEntryGetter.Data.Duration > 1 ? $" over {effectEntryGetter.Data.Duration}s" : "") + ". ";
                                            else if (magicEffectGetter.HasKeyword(Fallout4.Keyword.DamageTypePoison))
                                                newDescription += $"Deals {effectEntryGetter.Data!.Magnitude * effectEntryGetter.Data.Duration} poison damage" + (effectEntryGetter.Data.Duration > 1 ? $" over {effectEntryGetter.Data.Duration}s" : "") + ". ";
                                        }
                                    }
                                }
                            }
                        }
                        else if (propertyMod is IObjectModEnumPropertyGetter<Weapon.Property> enumMod)
                        {
                            if(enumMod.Property == Weapon.Property.SoundLevel)
                            {
                                if (enumMod.EnumIntValue == 2 || enumMod.EnumIntValue == 4)
                                    newDescription += "Suppresses weapon. ";
                            }
                        }
                        else if (propertyMod is IObjectModBoolPropertyGetter<Weapon.Property> boolMod)
                        {
                            if (boolMod.Property == Weapon.Property.IsAutomatic)
                            {
                                newDescription += "Changes firing mode to " + (boolMod.Value ? "automatic" : "semi-automatic") + ". ";
                            }
                        }
                        else if (propertyMod is IObjectModIntPropertyGetter<Weapon.Property> intMod)
                        {
                            if (intMod.Property == Weapon.Property.AmmoCapacity)
                            {
                                bool isDecrease = intMod.Value < 0;
                                string descSnippet = intMod.FunctionType == ObjectModProperty.FloatFunctionType.Set ? "Changes ammunition capacity to " : isDecrease ? "Decreases ammunition capacity by " : "Increases ammunition capacity by ";

                                newDescription += descSnippet + $"{Math.Abs(intMod.Value)}. ";
                            }
                            else if (intMod.Property == Weapon.Property.NumProjectiles)
                            {
                                if (intMod.FunctionType == ObjectModProperty.FloatFunctionType.Add)
                                    newDescription += (intMod.Value < 0 ? "Decreases " : "Increases ") + $"projectile count by {intMod.Value}. ";
                                else if (intMod.FunctionType == ObjectModProperty.FloatFunctionType.Set)
                                    newDescription += $"Changes projectile count to {intMod.Value}. ";
                            }
                        }
                        else if (propertyMod is IObjectModFloatPropertyGetter<Weapon.Property> floatMod)
                        {
                            double modValue = floatMod.FunctionType == ObjectModProperty.FloatFunctionType.MultAndAdd ? Math.Abs(Math.Round(floatMod.Value * 100)) : Math.Abs(Math.Round(floatMod.Value));
                            if (modValue == 0)
                                continue;

                            bool addSnippet = true;
                            bool isDecrease = floatMod.Value < 0;
                            string descSnippet = "";
                            switch (floatMod.Property)
                            {
                                case Weapon.Property.AttackDamage:
                                    if (floatMod.FunctionType == ObjectModProperty.FloatFunctionType.Set)
                                        continue;

                                    descSnippet += "damage ";
                                    break;
                                case Weapon.Property.AttackActionPointCost:
                                    if (floatMod.FunctionType == ObjectModProperty.FloatFunctionType.Set)
                                        continue;

                                    descSnippet += "action point cost ";
                                    break;
                                case Weapon.Property.AmmoCapacity:
                                    descSnippet += "ammunition capacity ";
                                    break;
                                case Weapon.Property.CriticalDamageMult:
                                    if (floatMod.FunctionType == ObjectModProperty.FloatFunctionType.Set)
                                        continue;

                                    descSnippet += "critical damage ";
                                    break;
                                case Weapon.Property.Weight:
                                    if (floatMod.FunctionType == ObjectModProperty.FloatFunctionType.Set)
                                        continue;

                                    descSnippet += "weight ";
                                    break;
                                /*case Weapon.Property.Value:
                                    descSnippet += "value ";
                                    break;*/
                                case Weapon.Property.ReloadSpeed:
                                    if (floatMod.FunctionType == ObjectModProperty.FloatFunctionType.Set)
                                        continue;

                                    descSnippet += "reload speed ";
                                    isDecrease = !isDecrease;
                                    break;
                                case Weapon.Property.AttackDelaySec:
                                    if (floatMod.FunctionType == ObjectModProperty.FloatFunctionType.Set)
                                        continue;

                                    descSnippet += "firing delay ";
                                    break;
                                case Weapon.Property.Speed:
                                    if (floatMod.FunctionType == ObjectModProperty.FloatFunctionType.Set)
                                        continue;

                                    descSnippet += "firing rate ";
                                    break;
                                case Weapon.Property.AimModelRecoilMinDegPerShot:
                                    if (floatMod.FunctionType == ObjectModProperty.FloatFunctionType.Set)
                                        continue;

                                    descSnippet += "sighted recoil ";
                                    break;
                                case Weapon.Property.AimModelRecoilHipMult:
                                    if (floatMod.FunctionType == ObjectModProperty.FloatFunctionType.Set)
                                        continue;

                                    descSnippet += "hip-fire recoil ";
                                    break;
                                case Weapon.Property.AimModelConeIronSightsMultiplier:
                                    if (floatMod.FunctionType == ObjectModProperty.FloatFunctionType.Set)
                                        continue;

                                    descSnippet += "sighted accuracy ";
                                    isDecrease = !isDecrease;
                                    break;
                                case Weapon.Property.AimModelMinConeDegrees:
                                    if (floatMod.FunctionType == ObjectModProperty.FloatFunctionType.Set)
                                        continue;

                                    descSnippet += "hip-fire accuracy ";
                                    isDecrease = !isDecrease;
                                    break;
                                case Weapon.Property.Reach:
                                    if (floatMod.FunctionType == ObjectModProperty.FloatFunctionType.Set)
                                        continue;

                                    descSnippet += "weapon reach ";
                                    break;
                                case Weapon.Property.SecondaryDamage:
                                    descSnippet += "bash damage ";
                                    break;
                                case Weapon.Property.AimModelBaseStability:
                                    if (floatMod.FunctionType == ObjectModProperty.FloatFunctionType.Set)
                                        continue;

                                    descSnippet += "aim stability ";
                                    break;
                                /*case Weapon.Property.MinRange:
                                    descSnippet = floatMod.FunctionType == ObjectModProperty.FloatFunctionType.Set ? "Changes " : isDecrease ? "Decreases " : "Increases ";
                                    descSnippet += "effective weapon range. ";
                                    newDescription += descSnippet;
                                    continue;*/

                                default:
                                    addSnippet = false;
                                    break;
                            }

                            if (addSnippet)
                            {
                                descSnippet = (floatMod.FunctionType == ObjectModProperty.FloatFunctionType.Set ? "Changes " : isDecrease ? "Decreases " : "Increases ") + descSnippet;
                                descSnippet += floatMod.FunctionType == ObjectModProperty.FloatFunctionType.Set ? "to " : "by ";
                                descSnippet += floatMod.FunctionType == ObjectModProperty.FloatFunctionType.MultAndAdd ? $"{Math.Abs(Math.Round(floatMod.Value * 100))}%. " : $"{Math.Abs(Math.Round(floatMod.Value))}. ";
                                newDescription += descSnippet;
                            }
                        }
                    }

                    newDescription.Trim();
                    if (newDescription.Length > 0)
                        state.PatchMod.ObjectModifications.GetOrAddAsOverride(objModGetter).Description = newDescription;
                }
                /*else if (objModGetter is IArmorModificationGetter armorModGetter)
                {
                    string newDescription = "";
                    foreach (var propertyMod in armorModGetter.Properties)
                    {

                    }
                }*/
            }
        }
    }
}

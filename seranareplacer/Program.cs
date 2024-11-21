using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins.Order;
using Noggog;
using FluentResults;

namespace seranareplacer
{
    public class Program
    {
        public static async Task<int> Main(string[] args)
        {
            return await SynthesisPipeline.Instance
                .AddPatch<ISkyrimMod, ISkyrimModGetter>(RunPatch)
                .SetTypicalOpen(GameRelease.SkyrimSE, "YourPatcher.esp")
                .Run(args);
        }
        public static readonly ModKey ModKey = ModKey.FromNameAndExtension("Dawnguard.esm");

        public static void RunPatch(IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            ILinkCache linkCache = state.LinkCache;
            var formLink = new FormLink<INpcGetter>(FormKey.Factory("002B6C:Dawnguard.esm"));

            if (formLink.TryResolve(linkCache, out var serana))
            {
                if (serana.Name == null)
                {
                    return;
                }
                if (serana.ShortName == null)
                {
                    return;
                }
                if(serana.FaceMorph == null)
                {
                    return;
                }
                if(serana.FaceParts == null)
                {
                    return;
                }
                Console.WriteLine("found serana - " + serana.EditorID);
                foreach (var npcgetter in state.LoadOrder.PriorityOrder.Npc().WinningOverrides())
                {
                    var modifiedNpc = state.PatchMod.Npcs.GetOrAddAsOverride(npcgetter);
                    modifiedNpc.ObjectBounds.DeepCopyIn(serana.ObjectBounds);
                    modifiedNpc.Configuration.DeepCopyIn(serana.Configuration);
                    modifiedNpc.Name = new Mutagen.Bethesda.Strings.TranslatedString(serana.Name.TargetLanguage);
                    modifiedNpc.Name.Set(serana.Name.TargetLanguage,serana.Name.String);
                    modifiedNpc.ShortName = new Mutagen.Bethesda.Strings.TranslatedString(serana.ShortName.TargetLanguage);
                    modifiedNpc.ShortName.Set(serana.ShortName.TargetLanguage, serana.ShortName.String);
                    modifiedNpc.HeadParts.Clear();
                    foreach( var part in serana.HeadParts)
                    {
                        modifiedNpc.HeadParts.Add(part);
                    }
                    modifiedNpc.HairColor.SetTo(serana.HairColor);
                    modifiedNpc.DefaultOutfit.SetTo(serana.DefaultOutfit);
                    modifiedNpc.FaceMorph = new NpcFaceMorph();
                    modifiedNpc.FaceMorph.DeepCopyIn(serana.FaceMorph);
                    modifiedNpc.TextureLighting = serana.TextureLighting;

                    modifiedNpc.FaceParts = new NpcFaceParts();
                    modifiedNpc.FaceParts.DeepCopyIn(serana.FaceParts);
                    modifiedNpc.TintLayers.Clear();
                    modifiedNpc.TintLayers.Combine(serana.TintLayers);

                }

            }
        }
    }
}

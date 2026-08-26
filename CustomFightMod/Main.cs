using Il2Cpp;
using Il2CppAK.Wwise;
using Il2CppI2.Loc;
using Il2CppInterop.Runtime.Injection;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppNocturne;
using Il2CppSystem.Collections.Generic;
using MelonLoader;
using MelonLoader.Utils;
using Newtonsoft.Json;
using System.IO;
using UnityEngine;
using UnityEngine.AddressableAssets;
using static Il2CppNocturne.AudioHook;

namespace CustomFightMod
{
    public sealed class Main : MelonMod
    {
        public static string MOD_FOLDER;

        public static bool initate = false;
        public static bool isCustom = false;

        public static int showCount = 0;

        public static string currentSongName = "";
        public static System.Collections.Generic.Dictionary<string, Infos> infosDico = new System.Collections.Generic.Dictionary<string, Infos>();
        public static System.Collections.Generic.Dictionary<string, string> pathFolderDico = new System.Collections.Generic.Dictionary<string, string>();

        public static List<int> customArcadeIndex = new List<int>();

        public static Dictionary<string, uint> pauseEventRefs = new Dictionary<string, uint>();
        public static Dictionary<string, uint> resumeEventRefs = new Dictionary<string, uint>();
        public static Dictionary<string, uint> stopEventRefs = new Dictionary<string, uint>();

        public static Il2CppAK.Wwise.Event currentPauseEvent = new Il2CppAK.Wwise.Event();
        public static Il2CppAK.Wwise.Event currentResumeEvent = new Il2CppAK.Wwise.Event();
        public static Il2CppAK.Wwise.Event currentStopEvent = new Il2CppAK.Wwise.Event();

        public override void OnInitializeMelon()
        {
            MelonLogger.Msg(System.ConsoleColor.DarkGray, "Mod Init...");

            MOD_FOLDER = Path.Combine(MelonEnvironment.ModsDirectory, "CustomFight");

            MelonLogger.Msg(System.ConsoleColor.DarkGray, "Mod Folder loading...");
            if (!Directory.Exists(MOD_FOLDER))
            {
                Directory.CreateDirectory(MOD_FOLDER);
                MelonLogger.Msg(System.ConsoleColor.Yellow, "Dossier mod créé ");
            }

            MelonLogger.Msg(System.ConsoleColor.Green, "Mod Folder load");
            MelonLogger.Msg(System.ConsoleColor.DarkGray, "Injecting IL2CPP...");

            ClassInjector.RegisterTypeInIl2Cpp<OverrideCustomAnimator>();

            MelonLogger.Msg(System.ConsoleColor.Green, "Injecting Complete");

            HarmonyLib.Harmony harmony = new HarmonyLib.Harmony("CustomFightMod");
            harmony.PatchAll();

            MelonLogger.Msg(System.ConsoleColor.Green, "Mod Init Complete");
        }

        // -------------- SPRITES -------------- //

        static Sprite loadCustomSprite(string path, string name, int sizeX, int sizeZ, float pixelPerUnit)
        {
            Texture2D texture = new Texture2D(sizeX, sizeZ, TextureFormat.RGBA32, false, false);

            texture.filterMode = FilterMode.Point;

            byte[] pngBytes = File.ReadAllBytes(path);

            ImageConversion.LoadImage(texture, pngBytes);

            Sprite sprite = Sprite.Create(
                texture,
                new Rect(0, 0, texture.width, texture.height),
                Vector2.zero,
                pixelPerUnit
            );

            sprite.name = name;

            return sprite;
        }

        static System.Collections.Generic.Dictionary<string, Sprite> getSpriteAnimation(Infos infos, string pathFolder)
        {
            System.Collections.Generic.Dictionary<string, Sprite> sprites = new System.Collections.Generic.Dictionary<string, Sprite>();

            foreach (var kv in infos.animation)
            {
                if (kv.Value == string.Empty) continue;

                Sprite sprite = loadCustomSprite
                    (
                        Path.Combine(pathFolder, kv.Value),
                        kv.Value.Replace(".png", ""),
                        infos.animation_size_x,
                        infos.animation_size_y,
                        infos.animation_pixel_per_unit
                    );

                if (sprite == null) continue;

                sprites.Add(kv.Key, sprite);
            }

            return sprites;
        }

        // -------------- LOAD FIGHTS -------------- //
        static void loadCategories(GenericArcadeMenuV2 menu)
        {
            ArcadeDatabase arcadeDatabase = menu.arcadeDatabase;

            int categoryIndex = 2;

            string[] subFolders = Directory.GetDirectories(MOD_FOLDER);

            CombatPlayerScore score = new CombatPlayerScore();
            score.score = 0;
            score.totalNotes = 1;
            score.maxScore = 1;
            score.combo = 0;
            score.difficulties = new List<NocturneDifficulty>();
            score.grades = new List<NoteGradeScore>();

            foreach (string category in subFolders)
            {

                string categoryName = Path.GetFileName(category);

                MelonLogger.Msg(System.ConsoleColor.DarkGray, "NEW CATEGORY ADD - " + categoryName + " :");

                NocturneString categoryDisplayName = new NocturneString(new LocalizedString("Custom Category"), categoryName);
                NocturneString shortDisplayName = new NocturneString(new LocalizedString("Custom"), "Custom " + (categoryIndex - 1));
                List<ArcadeSongInfo> arcadeSongs = new List<ArcadeSongInfo>();

                int songIndex = 0;

                string[] subSubFolders = Directory.GetDirectories(Path.Combine(MOD_FOLDER, category));

                foreach (string songFolder in subSubFolders)
                {
                    string songInfo = Path.Combine(songFolder, "songinfo.json");

                    if (!File.Exists(songInfo)) continue;

                    string data = File.ReadAllText(songInfo);

                    Infos infos = JsonConvert.DeserializeObject<Infos>(data);

                    ArcadeSongInfo arcadeSongInfo = createSongInfo(songIndex, songFolder, infos);

                    if (arcadeSongInfo == null)
                    {
                        MelonLogger.Error("{} n'a pas pu être initialisé car des données sont manquantes !", infos.songname);
                        continue;
                    }

                    if (!GameDataManager.Scores.HasAnyScore(arcadeSongInfo.DisplayName))
                    {
                        GameDataManager.Scores.TryRecordScore(arcadeSongInfo.DisplayName, 0, score);
                        GameDataManager.SaveFile.SaveScores();
                    }

                    arcadeSongs.Add(arcadeSongInfo);

                    MelonLogger.Msg(System.ConsoleColor.DarkGray, "  - " + infos.songname);

                    songIndex++;
                }

                if (arcadeSongs.Count < 1)
                {
                    MelonLogger.Msg(System.ConsoleColor.Yellow, "Empty category found - " + categoryName + " : not created");
                    continue;
                }

                createArcadeCategory(arcadeDatabase, arcadeSongs, categoryDisplayName, shortDisplayName, categoryIndex);

                categoryIndex++;
            }

            initate = true;
        }

        public static void createArcadeCategory(ArcadeDatabase database, List<ArcadeSongInfo> arcadeSongs, NocturneString categoryDisplayName, NocturneString shortDisplayName, int categoryIndex)
        {
            ArcadeCategory category = new ArcadeCategory();

            category.unlockCondition = new ProgressFlagCondition();

            category.categoryDisplayName = categoryDisplayName;

            category.shortDisplayName = shortDisplayName;

            category.index = categoryIndex;

            category.arcadeSongs = arcadeSongs;

            category.isDebugOnly = false;
            category.isDemoAchievement = false;

            database.songCategories.Add(category);
            customArcadeIndex.Add(categoryIndex);
        }

        static ArcadeSongInfo createSongInfo(int songIndex, string songFolder, Infos infos)
        {
            // ------- SONGINFO ------- //

            ArcadeSongInfo songInfo = new ArcadeSongInfo();

            songInfo.SetIndex(songIndex);
            songInfo.songType = ArcadeSongType.Song;
            songInfo.soundBanks = new List<Bank>();

            songInfo.preCombatHook = EmptyHook;
            songInfo.postCombatHook = EmptyHook;

            // ------- BANK ------- //

            string bankPath = Path.Combine(
                songFolder,
                infos.soundbank
            );

            AKRESULT result = AkSoundEngine.LoadBank(
                bankPath,
                null,
                null,
                out uint bankId
            );

            if (result != AKRESULT.AK_Success && result != AKRESULT.AK_BankAlreadyLoaded) return null;

            WwiseBankReference bankRef = new WwiseBankReference();
            bankRef.objectName = "Nocturne_Custom_Music";
            bankRef.name = "Nocturne_Custom_Music";
            bankRef.id = bankId;

            Bank myBank = new Bank();
            myBank.WwiseObjectReference = bankRef;

            songInfo.soundBanks.Add(myBank);

            // ------- EVENT ------- //

            WwiseEventReference playEventRef = ScriptableObject.CreateInstance<WwiseEventReference>();
            playEventRef.id = infos.playevent_id;
            playEventRef.name = "Play_CustomCombatMusic";
            playEventRef.objectName = "Play_CustomCombatMusic";

            Il2CppAK.Wwise.Event playEvent = new Il2CppAK.Wwise.Event();
            playEvent.ObjectReference = playEventRef;

            // ------- SONGDATA ------- //

            SongData songData = createSongData(infos, songFolder, myBank, playEvent);

            songInfo.songData = songData;

            if (!pauseEventRefs.ContainsKey(songData.songName))
                pauseEventRefs.Add(songData.songName, infos.pauseevent_id);

            if (!resumeEventRefs.ContainsKey(songData.songName))
                resumeEventRefs.Add(songData.songName, infos.resumeevent_id);

            if (!stopEventRefs.ContainsKey(songData.songName))
                stopEventRefs.Add(songData.songName, infos.stopevent_id);

            // ------- SPRITE - ICON ------- //

            string icon = Path.Combine(songFolder, infos.icon);

            songInfo.sprite = loadCustomSprite(icon, "Custom - Icon", infos.icon_size_x, infos.icon_size_y, infos.pixel_per_unit);

            songInfo.lore = new NocturneString(new LocalizedString("Custom Lore"), "This is a custom song added via modding.");

            return songInfo;
        }

        static SongData createSongData(Infos infos, string songFolder, Bank myBank, Il2CppAK.Wwise.Event playEvent)
        {
            SongData songData = ScriptableObject.CreateInstance<SongData>();

            songData.songName = infos.songname;
            songData.name = infos.songname;

            songData.soundBank = myBank;
            songData.playEvent = playEvent;
            songData.overridePlayEventInArcade = false;

            songData.timingType = SongTimingType.Continuous;
            songData.overrideMaxBpm = false;
            songData.SongSplitMeasure = 0;
            songData.melodyChangeMeasure = 0;
            songData.setMelodyIndex = false;
            songData.RandomizeSequence = false;

            string beatmap = Path.Combine(songFolder, infos.beatmap);
            string sm = File.ReadAllText(beatmap);

            TextAsset asset = new TextAsset(sm);
            var beatmaps = new Il2CppReferenceArray<TextAsset>(1);
            beatmaps[0] = asset;

            songData.beatmaps = beatmaps;

            songData.Enemies = new List<EnemyData>();
            songData.Enemies.Add(createEnemyData(infos.animation_placeholder));

            infosDico.Add(infos.songname, infos);
            pathFolderDico.Add(infos.songname, songFolder);

            songData.combatPrefabs = new List<CombatSpawnPrefab>();
            songData.onInitializeCombat = new AudioHook();
            songData.onSongStart = new AudioHook();

            songData.overridePlayerStats = false;
            songData.overrideHighScoreKey = false;
            songData.overrideColumnSpeeds = false;
            songData.Items = new Il2CppReferenceArray<ItemData>(0);
            songData.showInArcade = true;
            songData.hideStaggerBonus = false;
            songData.tapNoteTypeRemapper = new List<TapNoteTypeRemapper>();
            songData.Cues = new List<SongSectionMusicCue>();

            return songData;
        }

        static EnemyData createEnemyData(string placeholder)
        {
            EnemyData enemyData = new EnemyData();

            AssetReferenceGameObject key = new AssetReferenceGameObject(placeholder);

            enemyData.addressableArtPrefab = key;

            return enemyData;
        }

        // -------------- HARMONY -------------- //

        [HarmonyLib.HarmonyPatch(typeof(GenericArcadeMenuV2), "Awake")]
        class MenuInit
        {
            static void Postfix(GenericArcadeMenuV2 __instance)
            {
                if (__instance == null) return;
                if (__instance.arcadeDatabase == null) return;
                if (!initate){
                    loadCategories(__instance);
                }
            }
        }

        [HarmonyLib.HarmonyPatch(typeof(CombatNoteFieldView), "ShowColumns")]
        class FiveColumn
        {
            static void Postfix(CombatOptions combatOptions)
            {
                if (combatOptions == null) return;

                if (!infosDico.ContainsKey(currentSongName)) return;

                if (infosDico[currentSongName] == null) return;

                if (!infosDico[currentSongName].five_columns) return;

                AnimationProperty animationProperty = new AnimationProperty();

                animationProperty.name = "Five Instant";
                animationProperty.intValue = 0;
                animationProperty.boolValue = false;
                animationProperty.floatValue = 0;
                animationProperty.type = AnimationProperty.PropertyType.Trigger;

                combatOptions.columnAnimatorSettings.Clear();
                combatOptions.columnAnimatorSettings.Add(animationProperty);

                if (showCount == 0)
                {
                    showCount++;
                    CombatNoteFieldView combatNoteFieldView = UnityEngine.Object.FindObjectOfType<CombatNoteFieldView>();
                    combatNoteFieldView.ShowColumns(combatOptions);
                }
                else 
                {
                    showCount++;
                    if (showCount == 3)
                        showCount = 0;
                }
            }
        }

        [HarmonyLib.HarmonyPatch(typeof(CombatEnemyView), "Start")]
        class InitNewEnemy
        { 
            static void Postfix(CombatEnemyView __instance)
            {
                if (!isCustom) 
                {
                    //SpriteRenderer rd = __instance.spriteRenderer;
                    //OverrideCustomAnimator animator = rd.gameObject.AddComponent<OverrideCustomAnimator>();
                    //animator.baseEnemy = rd;
                    return;
                };

                if (__instance != null)
                {                
                    if (__instance.spriteRenderer != null)
                    {
                        if (__instance.spriteRenderer.gameObject.GetComponent<OverrideCustomAnimator>() != null) return;

                        SpriteRenderer rd = __instance.spriteRenderer;

                        rd.gameObject.layer = 0;

                        GameObject customGO = new GameObject("CustomSprite");
                        customGO.layer = 16;

                        var transform = customGO.transform;
                        transform.SetParent(
                            __instance.enemyParent.transform,
                            false
                        );

                        transform.localPosition = new Vector3
                            (
                                -(int)(160 / 3.25 / 2),
                                transform.localPosition.y,
                                transform.localPosition.z
                            );
                        transform.localRotation = Quaternion.identity;
                        transform.localScale = Vector3.one;

                        SpriteRenderer customSpriterenderer = customGO.AddComponent<SpriteRenderer>();

                        customSpriterenderer.material = rd.material;
                        customSpriterenderer.sortingOrder = 1;

                        OverrideCustomAnimator animator = rd.gameObject.AddComponent<OverrideCustomAnimator>();
                        animator.baseEnemy = rd;
                        animator.newEnemy = customSpriterenderer;
                        animator.sprites = getSpriteAnimation
                            (
                                infosDico[currentSongName],
                                pathFolderDico[currentSongName]
                            );
                    }
                }
            }
        }

        [HarmonyLib.HarmonyPatch(typeof(AudioController), "PauseCombat")]
        class CombatPaused
        { 
            static void Postfix(AudioController __instance)
            {
                if (currentPauseEvent != null)
                    AudioController.PostAudioEvent(currentPauseEvent);
            }
        }

        [HarmonyLib.HarmonyPatch(typeof(AudioController), "ResumeCombat")]
        class CombatResumed
        { 
            static void Postfix(AudioController __instance)
            {
                if (currentResumeEvent != null)
                    AudioController.PostAudioEvent(currentResumeEvent);
            }
        }

        [HarmonyLib.HarmonyPatch(typeof(AudioController), "StopCombat")]
        class CombatStopped
        { 
            static void Postfix(AudioController __instance)
            {
                if (currentStopEvent != null){
                    AudioController.PostAudioEvent(currentStopEvent);
                }
            }
        }

        [HarmonyLib.HarmonyPatch(typeof(ArcadeMenuV2), "ArcadeSongGroup_OnClick")]
        class CombatStart
        { 
            static void Postfix(ArcadeSongGroup songGroup, int melodyIndex)
            {
                if (songGroup == null) return;

                if (!customArcadeIndex.Contains(songGroup.CategoryIndex)) {
                    isCustom = false;
                    return; 
                }

                if (infosDico.ContainsKey(songGroup.CurrentSong.songData.songName) && pathFolderDico.ContainsKey(songGroup.CurrentSong.songData.songName))
                {
                    currentSongName = songGroup.CurrentSong.songData.songName;
                }

                isCustom = true;

                WwiseEventReference stopEventRef = ScriptableObject.CreateInstance<WwiseEventReference>();
                WwiseEventReference pauseEventRef = ScriptableObject.CreateInstance<WwiseEventReference>();
                WwiseEventReference resumeEventRef = ScriptableObject.CreateInstance<WwiseEventReference>();

                if (pauseEventRefs.ContainsKey(songGroup.CurrentSong.songData.songName)) {
                    uint id = pauseEventRefs[songGroup.CurrentSong.songData.songName];

                    pauseEventRef.id = id;
                    pauseEventRef.name = "Pause_CustomCombatMusic";
                    pauseEventRef.objectName = "Pause_CustomCombatMusic";

                    currentPauseEvent.ObjectReference = pauseEventRef;
                }

                if (resumeEventRefs.ContainsKey(songGroup.CurrentSong.songData.songName)) {
                    uint id = resumeEventRefs[songGroup.CurrentSong.songData.songName];

                    resumeEventRef.id = id;
                    resumeEventRef.name = "Resume_CustomCombatMusic";
                    resumeEventRef.objectName = "Resume_CustomCombatMusic";

                    currentResumeEvent.ObjectReference = resumeEventRef;
                }

                if (stopEventRefs.ContainsKey(songGroup.CurrentSong.songData.songName)) {
                    uint id = stopEventRefs[songGroup.CurrentSong.songData.songName];

                    stopEventRef.id = id;
                    stopEventRef.name = "Stop_CustomCombatMusic";
                    stopEventRef.objectName = "Stop_CustomCombatMusic";

                    currentStopEvent.ObjectReference = stopEventRef;
                }
            }
        }
    }
}

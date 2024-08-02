//using UnityEngine;
//using UnityEngine.Events;
//using System;
//using System.IO;
//using System.Runtime.Serialization.Formatters.Binary;

//[CreateAssetMenu(fileName = "NewGameState", menuName = "Trombone Hero/Game State (old)")]
//public class GameState_old : ScriptableObject
//{
//    public bool showDebug;
//    public LevelCollection levelCollection;
//    public int continues;
//    public string saveFolderName = "SavedGames";
//    public string saveFileName = "NewSave";
//    public int currentLevelIndex;
//    public string[] passwords;
//    [Min(0)] public int arcadeHighscoreCapacity = 5;

//    [Header("Saved values")]
//    [SerializeField] private int unlockedLevelCount;
//    [SerializeField] private HighScoreInfos[] levelHighscores;
//    [SerializeField] private HighScoreInfos[] arcadeHighscores;
//    [SerializeField] private GameSettingsInfo settings;

//    public UnityEvent<GameSettingsInfo> onChangeGameSettings;

//    private LevelScoreInfo[] currentLevelScores;

//    public GameSettingsInfo Settings
//    {
//        get => settings;
//        set
//        {
//            if (!value.Equals(settings))
//            {
//                settings = value;
//                onChangeGameSettings.Invoke(settings);
//                SaveState();
//            }
//        }
//    }

//    public HighScoreInfos[] ArcadeHighScores
//    {
//        get
//        {
//            if (arcadeHighscores != null)
//            {
//                int length = arcadeHighscores.Length;
//                HighScoreInfos[] hs = new HighScoreInfos[length];
//                Array.Copy(arcadeHighscores, hs, length);
//                return hs;
//            }
//            return null;
//        }
//    }

//    public HighScoreInfos[] LevelHighScores
//    {
//        get
//        {
//            if (levelHighscores != null)
//            {
//                int length = levelHighscores.Length;
//                HighScoreInfos[] hs = new HighScoreInfos[length];
//                Array.Copy(levelHighscores, hs, length);
//                return hs;
//            }
//            return null;
//        }
//    }

//    public int LevelCount => levelCollection != null ? levelCollection.Count : 0;
//    public string[] LevelNames => levelCollection != null ? levelCollection.LevelNames : new string[0];

//    public int CurrentLevelNumber
//    {
//        get
//        {
//            return Mathf.Clamp(currentLevelIndex + 1, 1, LevelCount);
//        }
//        set
//        {
//            int levelNumber = Mathf.Clamp(value, 1, LevelCount);
//            currentLevelIndex = levelNumber - 1;
//        }
//    }

//    public Level CurrentLevel => levelCollection.levels[currentLevelIndex];
//    public LevelScoreInfo CurrentLevelScore => currentLevelScores[currentLevelIndex];
//    public int CurrentArcadeScore
//    {
//        get
//        {
//            int arcadeScore = 0;
//            if (currentLevelScores != null)
//                foreach (LevelScoreInfo levelScore in currentLevelScores)
//                    arcadeScore += levelScore.Total;
//            return arcadeScore;
//        }
//    }

//    public void ClearCurrentScore()
//    {
//        currentLevelScores = new LevelScoreInfo[LevelCount];
//        SaveState();
//    }

//    public void ClearHighscores()
//    {
//        levelHighscores = new HighScoreInfos[LevelCount];
//        arcadeHighscores = new HighScoreInfos[arcadeHighscoreCapacity];
//    }

//    public void SetArcadeHighscoreCount(int n)
//    {
//        if (n < 0) n = 0;
//        if (arcadeHighscores == null) arcadeHighscores = new HighScoreInfos[n];
//        else Array.Resize(ref arcadeHighscores, n);
//    }

//    public void UnlockLevel(int levelIndex, int batchSize = 1)
//    {
//        if (batchSize < 1) return;
//        int unlockToLevelNumber;
//        // Unlock one level
//        if (batchSize == 1) unlockToLevelNumber = levelIndex + 1;
//        // Unlock level batch
//        else unlockToLevelNumber = (1 + levelIndex / batchSize) * batchSize;
//        // Limit to level count
//        unlockToLevelNumber = Mathf.Clamp(unlockToLevelNumber, 0, LevelCount);
//        // Update if new value is higher
//        if (unlockedLevelCount < unlockToLevelNumber)
//        {
//            unlockedLevelCount = unlockToLevelNumber;
//            SaveState();
//        }
//    }

//    public int GetUnlockedLevelCount()
//    {
//        return unlockedLevelCount;
//    }

//    public void SetLevelScore(int levelIndex, LevelScoreInfo scoreInfo)
//    {
//        if (currentLevelScores == null || levelIndex < 0 || levelIndex >= currentLevelScores.Length) return;
//        else currentLevelScores[levelIndex] = scoreInfo;
//    }

//    public bool IsLevelHighscore(int score, int levelIndex)
//    {
//        if (levelIndex < 0 || levelIndex >= LevelCount) return false;
//        else return levelHighscores[levelIndex].score < score;
//    }

//    public void SetLevelHighscore(int score, int levelIndex, string playerName)
//    {
//        if (levelIndex < 0 || levelIndex >= LevelCount) return;
//        else
//        {
//            if (levelIndex >= levelHighscores.Length) Array.Resize(ref levelHighscores, levelIndex + 1);
//            levelHighscores[levelIndex] = new HighScoreInfos(playerName, score);
//        }
//    }

//    public bool IsArcadeHighscore(int score)
//    {
//        if (arcadeHighscores == null) return false;
//        int lowestHighscore = arcadeHighscores[arcadeHighscores.Length - 1].score;
//        return lowestHighscore < score;
//    }

//    public int GetArcadeScoreRank(int score)
//    {
//        if (arcadeHighscores != null)
//        {
//            for (int i = 0, iend = arcadeHighscores.Length; i < iend; i++)
//            {
//                if (arcadeHighscores[i].score < score)
//                    return i;
//            }
//        }
//        return int.MaxValue;
//    }

//    public void SetArcadeHighscore(int score, int rank, string playerName)
//    {
//        if (arcadeHighscores == null || rank < 0 || rank >= arcadeHighscores.Length) return;
//        // Move inferior scores down a rank
//        for (int i = arcadeHighscores.Length - 1; i > rank; i--)
//            arcadeHighscores[i] = arcadeHighscores[i - 1];
//        // Add new score
//        arcadeHighscores[rank] = new HighScoreInfos(playerName, score);
//    }

//    private string SavePath => Application.dataPath + "/" + saveFolderName + "/" + saveFileName + ".dat";

//    public void SaveState()
//    {
//        BinaryFormatter bf = new BinaryFormatter();
//        FileStream file = File.Create(SavePath);
//        SaveData data = new SaveData();
//        data.arcadeHighScores = arcadeHighscores;
//        data.currentScoreDetail = currentLevelScores;
//        data.levelHighScores = levelHighscores;
//        data.settings = settings;
//        bf.Serialize(file, data);
//        file.Close();
//        if (showDebug) Debug.Log("Game data saved to " + SavePath);
//    }

//    public bool TryLoadState()
//    {
//        if (File.Exists(SavePath))
//        {
//            BinaryFormatter bf = new BinaryFormatter();
//            FileStream file =
//                       File.Open(SavePath, FileMode.Open);
//            SaveData data = (SaveData)bf.Deserialize(file);
//            file.Close();
//            arcadeHighscores = data.arcadeHighScores;
//            currentLevelScores = data.currentScoreDetail;
//            levelHighscores = data.levelHighScores;
//            settings = data.settings;
//            if (showDebug) Debug.Log("Load success from " + SavePath);
//            return true;
//        }
//        else
//        {
//            if (showDebug) Debug.Log("Load failure from " + SavePath);
//            return false;
//        }
//    }
//}

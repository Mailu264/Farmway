using UnityEngine;

namespace Farmway.Infrastructure
{
    public interface ISaveService
    {
        bool HasSave { get; }
        // Выставляется главным меню: «Продолжить» = true
        bool LoadRequested { get; set; }
        // Сейв, который надо применить при старте геймплея (null если новая игра)
        SaveData Pending { get; }

        void Save(SaveData data);
        void LoadPending();
    }

    public class SaveService : ISaveService
    {
        private const string SaveKey = "farmway_save";

        public bool HasSave => PlayerPrefs.HasKey(SaveKey);
        public bool LoadRequested { get; set; }
        public SaveData Pending { get; private set; }

        public void Save(SaveData data)
        {
            PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(data));
            PlayerPrefs.Save();
        }

        public void LoadPending()
        {
            Pending = null;

            if (!LoadRequested || !HasSave)
                return;

            var json = PlayerPrefs.GetString(SaveKey);
            Pending = JsonUtility.FromJson<SaveData>(json);

            if (Pending == null)
                Debug.LogError("[Save] Не удалось прочитать сейв — начинаю новую игру");
        }
    }
}

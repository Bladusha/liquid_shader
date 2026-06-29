using TMPro;
using UnityEngine;

public class PanelInputAutoSave : MonoBehaviour
{
    [SerializeField] private string panelId = "MainPanel";

    private TMP_InputField[] inputFields;

    private void Awake()
    {
        inputFields = GetComponentsInChildren<TMP_InputField>(true);
    }

    private void OnEnable()
    {
        LoadData();
    }

    private void OnDisable()
    {
        SaveData();
    }

    public void SaveData()
    {
        if (inputFields == null)
        {
            return;
        }

        foreach (TMP_InputField field in inputFields)
        {
            if (field == null)
            {
                continue;
            }

            PlayerPrefs.SetString(GetKey(field), field.text);
        }

        PlayerPrefs.Save();
    }

    public void LoadData()
    {
        if (inputFields == null)
        {
            return;
        }

        foreach (TMP_InputField field in inputFields)
        {
            if (field == null)
            {
                continue;
            }

            string key = GetKey(field);
            if (PlayerPrefs.HasKey(key))
            {
                field.text = PlayerPrefs.GetString(key);
            }
        }
    }

    public void ClearSavedData()
    {
        if (inputFields == null)
        {
            return;
        }

        foreach (TMP_InputField field in inputFields)
        {
            if (field == null)
            {
                continue;
            }

            PlayerPrefs.DeleteKey(GetKey(field));
            field.text = string.Empty;
        }

        PlayerPrefs.Save();
    }

    private string GetKey(TMP_InputField field)
    {
        return $"{panelId}_{field.gameObject.name}";
    }
}

using UnityEngine;

[System.Serializable]
public class HealthArmorService : Initializable {
    [SerializeField] private CanvasHealthArmor canvasHealthArmor;
    [SerializeField] private View canvasDeath;

    public void Initialize() {
        canvasHealthArmor.Show();
        canvasDeath.Hide();
    }
    public void ShowDeath() { canvasDeath.Show(); } 
    public void SetMaxHealth(int value) { canvasHealthArmor.SetMaxHealth(value); }
    public void SetMaxArmor(int value) { canvasHealthArmor.SetMaxArmor(value); }
    public void SetHealth(int value) { canvasHealthArmor.SetHealth(value); }
    public void SetArmor(int value) { canvasHealthArmor.SetArmor(value);; }
    
}
using UnityEngine;
using UnityEngine.UI;

public class CanvasHealthArmor : View {
    [SerializeField] private Slider sliderArmor;
    [SerializeField] private Slider sliderHealth;
    
    public void SetMaxHealth(int value) { sliderHealth.maxValue = value; }
    public void SetMaxArmor(int value) { sliderArmor.maxValue = value; }
    public void SetHealth(int value) { sliderHealth.value = value; }
    public void SetArmor(int value) { sliderArmor.value = value; }
}

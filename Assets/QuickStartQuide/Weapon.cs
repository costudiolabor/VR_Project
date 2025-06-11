using UnityEngine;

public class Weapon : View {
    public float weaponSpeed = 15.0f;
    public float weaponLife = 3.0f;
    public float weaponCooldown = 1.0f;
    public int weaponAmmo = 15;

    public Bullet weaponBullet;
    public Transform weaponFirePosition;
}
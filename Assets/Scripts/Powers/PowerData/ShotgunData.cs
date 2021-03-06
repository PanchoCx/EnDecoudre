using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ShotgunData", menuName = "PowerData/ShotgunData")]

public class ShotgunData : PowerData
{   
    [Tooltip("The radius of the shot")]
    public int radius;

    [Tooltip("Enemies between -angle and +angle will be damaged, with 0 being the forward direction of the player")]
    public int angle;

    [Tooltip("The damage taken by each enemy hit by the cone")]
    public int damage;

    public GameObject bulletPrefab;
    public override string ToString()
    {
        return string.Format("ShotgunData : radius = {0}, angle = {1}, damage = {2}, cooldown = {3}, total charges = {4}", radius, angle, damage, cooldown, totalCharges);
    }
}
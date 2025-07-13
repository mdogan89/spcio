using UnityEngine;
public enum PowerupType
{
    None,
    SpeedBoost,
    Shield,
    DoublePoints,
    Gun,
    Ghost,
    Magnet,
    Piranha,
}
public class Powerup : MonoBehaviour
{
    public PowerupType powerupType;
    private void Update()
    {
        // Rotate the powerup for visual effect
        transform.Rotate(Vector3.up, 50f * Time.deltaTime);
    }
}

using UnityEngine;

namespace HeavenVr;

public class WeaponSwapper: MonoBehaviour
{
    private PlayerCard previousCard;
    private GameObject currentWeapon;
    
    public static void Create(GameObject gameObject)
    {
        gameObject.AddComponent<WeaponSwapper>();
    }

    private void Update()
    {
        if (RM.mechController == null || RM.mechController.deck == null) return;

        var currentCard = RM.mechController.deck.GetCardInHand(0);
        if (currentCard != previousCard)
        {
            if (currentWeapon)
            {
                currentWeapon.SetActive(false);
            }

            currentWeapon = transform.Find(GetWeaponTransformName(currentCard)).gameObject;
            currentWeapon.SetActive(true);
            previousCard = currentCard;
        }
    }

    private string GetWeaponTransformName(PlayerCard card)
    {
        switch (card.data.cardID)
        {
            case "PISTOL":
            {
                return "Pistol";
            }
            case "RIFLE":
            {
                return "Rifle";
            }
            case "MACHINEGUN":
            {
                return "MachineGun";
            }
            case "UZI":
            {
                return "Uzi";
            }
            case "SHOTGUN":
            {
                return "Shotgun";
            }
            case "ROCKETLAUNCHER":
            {
                return "Shotgun"; // TODO rocket launcher.
            }
            case "KATANA":
            case "FISTS":
            default:
            {
                return "Katana"; // TODO fist.
            }
        }
    }
}
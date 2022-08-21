using System;
using UnityEngine;

namespace HeavenVr;

public class WeaponSwapper: MonoBehaviour
{
    private PlayerCard _previousCard;
    private GameObject _currentWeapon;
    
    public static void Create(GameObject gameObject)
    {
        gameObject.AddComponent<WeaponSwapper>();
    }

    private void Update()
    {
        if (RM.mechController == null || RM.mechController.deck == null) return;

        var currentCard = RM.mechController.deck.GetCardInHand(0);

        if (currentCard == _previousCard) return;

        if (_currentWeapon)
        {
            _currentWeapon.SetActive(false);
        }

        _currentWeapon = transform.Find(GetWeaponTransformName(currentCard)).gameObject;
        if (_currentWeapon)
        {
            _currentWeapon.SetActive(true);
        }
        _previousCard = currentCard;
    }

    private static string GetWeaponTransformName(PlayerCard card)
    {
        return card.data.cardID switch
        {
            "PISTOL" => "Pistol",
            "RIFLE" => "Rifle",
            "MACHINEGUN" => "MachineGun",
            "UZI" => "Uzi",
            "SHOTGUN" => "Shotgun",
            "ROCKETLAUNCHER" => "RocketLauncher",
            "FISTS" => "",
            "RAPTURE" => "Katana", // TODO Book of Life model. I think it's this ID, but not sure.
            "KATANA" => "Katana",
            _ => throw new ArgumentOutOfRangeException(card.data.cardID, "Couldn't find a model the selected weapon")
        };
    }
}
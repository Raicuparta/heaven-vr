using System;
using HeavenVr.Helpers;
using UnityEngine;

namespace HeavenVr.Weapons;

public class WeaponSwapper : MonoBehaviour
{
    private GameObject _currentWeapon;
    private PlayerCard _previousCard;

    public static void Create(GameObject gameObject)
    {
        gameObject.AddComponent<WeaponSwapper>();
    }

    private void Update()
    {
        var currentCard = GetCurrentCard();

        if (currentCard == _previousCard) return;

        _previousCard = currentCard;

        if (_currentWeapon) _currentWeapon.SetActive(false);

        var currentWeaponTransformName = GetWeaponTransformName(currentCard);

        _currentWeapon = GetWeaponObject(currentWeaponTransformName);
        if (_currentWeapon) _currentWeapon.SetActive(true);
    }

    private static PlayerCard GetCurrentCard()
    {
        return PauseHelper.IsPaused() ? null : RM.mechController.deck.GetCardInHand(0);
    }

    private GameObject GetWeaponObject(string transformName)
    {
        return transformName == null ? null : transform.Find(transformName).gameObject;
    }

    private static string GetWeaponTransformName(PlayerCard card)
    {
        if (card == null) return null;

        return card.data.cardID switch
        {
            "PISTOL" => "Pistol",
            "RIFLE" => "Rifle",
            "MACHINEGUN" => "MachineGun",
            "UZI" => "Uzi",
            "SHOTGUN" => "Shotgun",
            "ROCKETLAUNCHER" => "RocketLauncher",
            "FISTS" => null, // TODO fists model.
            "RAPTURE" => "BookOfLife",
            "KATANA" => "Katana",
            _ => throw new ArgumentOutOfRangeException(card.data.cardID, "Couldn't find a model the selected weapon")
        };
    }
}
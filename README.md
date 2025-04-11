# CounterstrikeSharp - Quake Sounds

[![UpdateManager Compatible](https://img.shields.io/badge/CS2-UpdateManager-darkgreen)](https://github.com/Kandru/cs2-update-manager/)
[![GitHub release](https://img.shields.io/github/release/Kandru/cs2-quake-sounds?include_prereleases=&sort=semver&color=blue)](https://github.com/Kandru/cs2-quake-sounds/releases/)
[![License](https://img.shields.io/badge/License-GPLv3-blue)](#license)
[![issues - cs2-map-modifier](https://img.shields.io/github/issues/Kandru/cs2-quake-sounds)](https://github.com/Kandru/cs2-quake-sounds/issues)
[![](https://www.paypalobjects.com/en_US/i/btn/btn_donateCC_LG.gif)](https://www.paypal.com/donate/?hosted_button_id=C2AVYKGVP9TRG)

This is a simple Quake Sound plugin for your server. It supports all types of sounds - only a workshop addon is necessesary.

## Installation

1. Download and extract the latest release from the [GitHub releases page](https://github.com/Kandru/cs2-quake-sounds/releases/).
2. Move the "QuakeSounds" folder to the `/addons/counterstrikesharp/plugins/` directory.
3. Download and install [MultiAddonManager](https://github.com/Source2ZE/MultiAddonManager)
4. Add at least one Workshop Addon with Quake Sounds to the configuration of the MultiAddonManager. I provide the following for use: https://steamcommunity.com/sharedfiles/filedetails/?id=3461824328
5. Restart the server.

Updating is even easier: simply overwrite all plugin files and they will be reloaded automatically. To automate updates please use our [CS2 Update Manager](https://github.com/Kandru/cs2-update-manager/).


## Configuration

This plugin automatically creates a readable JSON configuration file. This configuration file can be found in `/addons/counterstrikesharp/configs/plugins/QuakeSounds/QuakeSounds.json`.

```json

```


## Commands

### quakesounds (Server Console Only)

Ability to run sub-commands:

#### reload

Reloads the configuration.

#### disable

Disables the sounds instantly and remembers this state.

#### enable

Enables the sounds instantly and remembers this state.

## Compile Yourself

Clone the project:

```bash
git clone https://github.com/Kandru/cs2-quake-sounds.git
```

Go to the project directory

```bash
  cd cs2-quake-sounds
```

Install dependencies

```bash
  dotnet restore
```

Build debug files (to use on a development game server)

```bash
  dotnet build
```

Build release files (to use on a production game server)

```bash
  dotnet publish
```

## FAQ

### Which sound should be played when? Can you provide examples?

Please refer to the example configuration found above or search the internet. However, here's a list you can refer to:

3 Frags → TRIPLE KILL
5 Frags → MULTI KILL
6 Frags → RAMPAGE
7 Frags → KILLING SPREE
8 Frags → DOMINATING
9 Frags → IMPRESSIVE
10 Frags → UNSTOPPABLE
11 Frags → OUTSTANDING
12 Frags → MEGA KILL
13 Frags → ULTRA KILL
14 Frags → EAGLE EYE
15 Frags → OWNAGE
16 Frags → COMBO KING
17 Frags → MANIAC
18 Frags → LUDICROUS KILL
19 Frags → BULLSEYE
20 Frags → EXCELLENT
21 Frags → PANCAKE
22 Frags → HEAD HUNTER
23 Frags → UNREAL
24 Frags → ASSASSIN
25 Frags → WICKED SICK
26 Frags → MASSACRE
27 Frags → KILLING MACHINE
28 Frags → MONSTER KILL
29 Frags → HOLY SHIT
30 Frags → G O D L I K E

### Where can I find sounds?

Sounds are spread over the internet. We provide a Workshop Addon which you can use: https://steamcommunity.com/sharedfiles/filedetails/?id=3461824328 which contains the following sounds:

#### Male Sounds

```
QuakeSoundsD.Combowhore
QuakeSoundsD.Dominating
QuakeSoundsD.Doublekill
QuakeSoundsD.Firstblood
QuakeSoundsD.Godlike
QuakeSoundsD.Haha
QuakeSoundsD.Hattrick
QuakeSoundsD.Headhunter
QuakeSoundsD.Headshot
QuakeSoundsD.Holyshit
QuakeSoundsD.Humiliation
QuakeSoundsD.Impressive
QuakeSoundsD.Killingspree
QuakeSoundsD.Ludicrouskill
QuakeSoundsD.Megakill
QuakeSoundsD.Monsterkill
QuakeSoundsD.Multikill
QuakeSoundsD.Perfect
QuakeSoundsD.Play
QuakeSoundsD.Prepare
QuakeSoundsD.Rampage
QuakeSoundsD.Teamkiller
QuakeSoundsD.Triplekill
QuakeSoundsD.Ultrakill
QuakeSoundsD.Unstoppable
QuakeSoundsD.Wickedsick
```

#### Female Sounds

```
QuakeSoundsF.Bottomfeeder
QuakeSoundsF.Dominating
QuakeSoundsF.Firstblood
QuakeSoundsF.Godlike
QuakeSoundsF.Headshot
QuakeSoundsF.Holyshit
QuakeSoundsF.Humiliation
QuakeSoundsF.Killingspree
QuakeSoundsF.Monsterkill
QuakeSoundsF.Multikill
QuakeSoundsF.Prepare
QuakeSoundsF.Rampage
QuakeSoundsF.Ultrakill
QuakeSoundsF.Unstoppable
QuakeSoundsF.Wickedsick
```

## License

Released under [GPLv3](/LICENSE) by [@Kandru](https://github.com/Kandru).

## Authors

- [@derkalle4](https://www.github.com/derkalle4)
- [@jmgraeffe](https://www.github.com/jmgraeffe)

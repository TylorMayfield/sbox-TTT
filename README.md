<h1><img src="ui/traitor-icon.png" alt="TTT logo" height="200"/></h1>

# [s&box TTT](https://cigarlounge.github.io/)

[![License](https://img.shields.io/badge/license-agreement-red)](https://github.com/CigarLounge/sbox-TTT/blob/main/LICENSE.md)
[![Discord](https://img.shields.io/discord/949508550118481970?label=discord)](https://discord.gg/rrsrakF8N3)
![GitHub Repo stars](https://img.shields.io/github/stars/CigarLounge/sbox-TTT?style=social)

TTT is a social deduction multiplayer game built in [s&box](https://sbox.facepunch.com/news). It aims to capture the spirit of classic [Trouble in Terrorist Town](https://ttt.badking.net/) while adding modern quality-of-life improvements, moderation tools, and a few server-friendly extras.

## Links

- Join the community on [Discord](https://discord.gg/rrsrakF8N3)
- Watch development updates on [YouTube](https://www.youtube.com/channel/UCk2IAm1j9o_3GWrqf537gNg)
- Browse screenshots and project info on the [website](https://cigarlounge.github.io/)

## Gameplay

Core gameplay is built around classic TTT roles, deduction, corpse investigation, traitor equipment, and round-based social chaos. Recent additions also include configurable body hanging, RDM reporting, a public tribunal system, AFK handling, and expanded round-end summaries.

### Server Config

#### Admin Access

Admin access can be managed with a local `admins.json` allowlist in the game data folder. The game also honors s&box engine admins, and the in-game admin console accepts either source.

Use these commands to manage the local allowlist:

- `ttt_admin_add <steamid64>`
- `ttt_admin_remove <steamid64>`
- `ttt_admin_list`

#### Body Hanging

Body hanging permissions are controlled by the saved server convar `ttt_hang_body_roles`.
Set it to a comma-separated list of role names, type names, or class names such as `traitor,detective`.
Use `all` or `*` to allow every role to hang bodies.

#### Movement and Bunny Hopping

Movement and bhop behavior are controlled by:

- `ttt_bhop_enabled`
- `ttt_bhop_autojump`
- `ttt_bhop_air_acceleration`
- `ttt_bhop_air_control`
- `ttt_bhop_ground_friction`
- `ttt_bhop_speed_cap_multiplier`

Set `ttt_bhop_speed_cap_multiplier` to `0` to fully remove the bhop jump speed cap.

#### AFK Handling

AFK behavior is controlled by:

- `ttt_afk_timer`
- `ttt_afk_auto_kick`
- `ttt_afk_fun_death`
- `ttt_afk_kick_delay`

When enabled, AFK players can be forced to self-destruct with a theatrical death before being moved to spectator or kicked.

#### Karma

Karma affects both outgoing damage and movement speed.
The minimum movement speed penalty floor is controlled by `ttt_karma_min_speed_scale`.

#### RDM, Tribunal, and Punishments

Public RDM tribunal voting is controlled by:

- `ttt_tribunal_enabled`
- `ttt_tribunal_vote_seconds`
- `ttt_tribunal_min_votes`
- `ttt_tribunal_required_ratio`

When enabled, open RDM reports can be reviewed by the server through the in-game Global Tribunal page.

Guilty RDM punishments are controlled by:

- `ttt_rdm_guilty_punishments`
- `ttt_rdm_slay_rounds`
- `ttt_rdm_damage_scale`
- `ttt_rdm_damage_rounds`
- `ttt_rdm_ban_minutes`

Supported punishment values are `slay`, `half_damage`, `kick`, and `ban`.

## [Contributing](https://github.com/CigarLounge/sbox-TTT/wiki/Contributing)

## [License](https://github.com/CigarLounge/sbox-TTT/blob/main/LICENSE.md)

## Special Thanks
[Thanks to Bad King Urgrain for creating "Trouble in Terrorist Town"](https://ttt.badking.net/) for [Garry's Mod](https://gmod.facepunch.com/) for which sbox-TTT takes inspiration from.

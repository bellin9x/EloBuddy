﻿using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu.Values;
using SharpDX;

namespace ezEvade
{
    public static class Situation
    {
        private static AIHeroClient myHero => Player.Instance;

        static Situation()
        {

        }

        public static bool CheckTeam(this Obj_AI_Base unit)
        {
            return unit.Team != Player.Instance.Team || Evade.devModeOn;
        }

        public static bool CheckTeam(this GameObject unit)
        {
            return unit.Team != Player.Instance.Team || Evade.devModeOn;
        }

        public static bool CheckTeam(this Obj_GeneralParticleEmitter emitter)
        {
            return emitter.Name.ToLower().Contains("red") ||
                  (emitter.Name.ToLower().Contains("green") || emitter.Name.ToLower().Contains("ally")) && Evade.devModeOn ||
                  !emitter.Name.ToLower().Contains("green") && !emitter.Name.ToLower().Contains("ally");
        }

        public static string EmitterColor()
        {
            return Evade.devModeOn ? "green" : "red";
        }

        public static string EmitterTeam()
        {
            return Evade.devModeOn ? "ally" : "enemy";
        }

        public static bool isNearEnemy(this Vector2 pos, float distance, bool alreadyNear = true)
        {
            if (ObjectCache.menuCache.cache["PreventDodgingNearEnemy"].Cast<CheckBox>().CurrentValue)
            {
                var curDistToEnemies = ObjectCache.myHeroCache.serverPos2D.GetDistanceToChampions();
                var posDistToEnemies = pos.GetDistanceToChampions();

                if (curDistToEnemies < distance)
                {
                    if (curDistToEnemies > posDistToEnemies)
                    {
                        return true;
                    }
                }
                else
                {
                    if (posDistToEnemies < distance)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static bool IsUnderTurret(this Vector2 pos, bool checkEnemy = true)
        {
            if (!ObjectCache.menuCache.cache["PreventDodgingUnderTower"].Cast<CheckBox>().CurrentValue)
            {
                return false;
            }

            var turretRange = 875 + ObjectCache.myHeroCache.boundingRadius;

            foreach (var entry in ObjectCache.turrets)
            {
                var turret = entry.Value;
                if (turret == null || !turret.IsValid || turret.IsDead)
                {
                    Core.DelayAction(() => ObjectCache.turrets.Remove(entry.Key), 1);
                    continue;
                }

                if (checkEnemy && turret.IsAlly)
                {
                    continue;
                }

                var distToTurret = pos.Distance(turret.Position.To2D());
                if (distToTurret <= turretRange)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool ShouldDodge()
        {

            if (ObjectCache.menuCache.cache["DodgeSkillShots"].Cast<KeyBind>().CurrentValue == false
                || CommonChecks()
                )
            {
                //has spellshield - sivir, noc, morgana
                //vlad pool
                //tryndamere r?
                //kayle ult buff?
                //hourglass
                //invulnerable
                //rooted
                //sion ult -> tenacity = 100?
                //stunned
                //elise e
                //zilean ulted
                //isdashing

                return false;
            }

            return true;
        }

        public static bool ShouldUseEvadeSpell()
        {

            if (ObjectCache.menuCache.cache["ActivateEvadeSpells"].Cast<KeyBind>().CurrentValue == false
                || CommonChecks()
                || Evade.lastWindupTime - EvadeUtils.TickCount > 0
                )
            {
                return false;
            }

            return true;
        }

        public static bool CommonChecks()
        {
            return

                Evade.isChanneling
                || Player.Instance.IsDead
                || Player.Instance.IsInvulnerable
                || Player.Instance.IsTargetable == false
                || HasSpellShield(myHero)
                || ChampionSpecificChecks()
                || Player.Instance.IsDashing()
                || Evade.hasGameEnded == true;
        }

        public static bool ChampionSpecificChecks()
        {
            return (Player.Instance.ChampionName == "Sion" && myHero.HasBuff("SionR"));


            //Untargetable
            //|| (myHero.ChampionName == "KogMaw" && myHero.HasBuff("kogmawicathiansurprise"))
            //|| (myHero.ChampionName == "Karthus" && myHero.HasBuff("KarthusDeathDefiedBuff"))

            //Invulnerable
            //|| myHero.HasBuff("kalistarallyspelllock");
        }

        //from Evade by Esk0r
        public static bool HasSpellShield(AIHeroClient unit)
        {
            if (Player.Instance.HasBuffOfType(BuffType.SpellShield))
            {
                return true;
            }

            if (Player.Instance.HasBuffOfType(BuffType.SpellImmunity))
            {
                return true;
            }

            //Sivir E
            if (unit.LastCastedSpellName() == "SivirE" && (EvadeUtils.TickCount - Evade.lastSpellCastTime) < 300)
            {
                return true;
            }

            //Morganas E
            if (unit.LastCastedSpellName() == "BlackShield" && (EvadeUtils.TickCount - Evade.lastSpellCastTime) < 300)
            {
                return true;
            }

            //Nocturnes E
            if (unit.LastCastedSpellName() == "NocturneShit" && (EvadeUtils.TickCount - Evade.lastSpellCastTime) < 300)
            {
                return true;
            }

            return false;
        }

    }
}

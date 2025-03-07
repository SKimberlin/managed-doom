using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace ManagedDoom {

    public sealed class WaveController {

        private int wave = 0;

        private int monsterSpawnCount = 0;
        private int monstersSpawned = 0;
        private List<Mobj> spawnedMobs = new List<Mobj>();

        private int currentMonstersPerWave = 2;
        private int monstersPerWaveIncrease = 3;

        private int specialMonstersWaveInterval = 5;

        private float currentMonsterHealthMultiplyer = 0.5f;
        private float monsterHealthMultiplyerIncrease = 0.5f;

        private bool Started = false;
        private int waveStartTime;
        private int waveDelay = GameConst.TicRate * 5;


        private MobjType[] monsterTypes = {

            MobjType.Zombie,
            MobjType.Dog

        };

        private int currencyPerHit = 10;
        private int currencyPerKill = 50;

        private World world;
        private List<MapThing> spawnPoints;


        // Powerups

        private int instaKillTime = GameConst.TicRate * 10;
        private int instaKillStartTime;

        private int doublePointsTime = GameConst.TicRate * 10;
        private int doublePointsStartTime;

        private int nukePoints = 200;



        public WaveController( World world ) {

            this.world = world;

            spawnPoints = new List<MapThing>();
            foreach ( var thing in world.Map.Things ) {

                if ( thing.Type != 3004 && thing.Type != 9 && thing.Type != 64 && thing.Type != 66 ) continue;

                spawnPoints.Add( thing );

            }

        }

        public void Start() {

            if ( !Started ) {

                Started = true;
                instaKillStartTime = -instaKillTime;
                doublePointsStartTime = -doublePointsTime;

                world.ThingInteraction.OnMobKilled += OnMobKilled;
                world.ThingInteraction.OnMobDamaged += onMobDamaged;


            }

        }

        private void OnMobKilled( Mobj source, Mobj target ) {

            if ( source == null || source.Player == null ) return;

            GivePlayerPoints( source.Player, currencyPerKill );

            monstersSpawned--;
            spawnedMobs.Remove( target );

        }

        private void onMobDamaged( Mobj source, Mobj target ) {

            if ( target.Player != null || target.Health <= 0 ) return;

            if ( instaKillStartTime + instaKillTime > world.LevelTime ) {

                world.ThingInteraction.DamageMobj( target, source, source, target.Health );

            }

            if ( source.Player == null ) return;

            GivePlayerPoints( source.Player, currencyPerHit );

        }

        private void GivePlayerPoints( Player player, int points ) {

            player.Currency += ( doublePointsStartTime + doublePointsTime > world.LevelTime ) ? points * 2 : points;

        }

        public void ActivateMaxAmmo() {




            foreach ( Player player in world.Options.Players ) {

                for ( var i = 0; i < player.WeaponOwned.Length; i++ ) {

                    if ( !player.WeaponOwned[i] ) continue;
                    if ( DoomInfo.WeaponInfos[i].Ammo == AmmoType.NoAmmo ) continue;
                    player.Ammo[(int) DoomInfo.WeaponInfos[i].Ammo] = player.MaxAmmo[(int) DoomInfo.WeaponInfos[i].Ammo];

                }
                player.SendMessage( "Max Ammo Activated!" );

            }

        }

        public void ActivateInstaKill() {

            instaKillStartTime = world.LevelTime;
            foreach ( Player player in world.Options.Players ) player.SendMessage( "InstaKill Activated!" );

        }

        public void ActivateDoublePoints() {

            doublePointsStartTime = world.LevelTime;
            foreach ( Player player in world.Options.Players ) player.SendMessage( "Double Points Activated!" );

        }

        public void ActivateNuke() {

            monsterSpawnCount = 0;
            monstersSpawned = 0;
            foreach ( Mobj mobj in spawnedMobs ) world.ThingInteraction.DamageMobj( mobj, null, null, mobj.Health ); ;
            foreach ( Player player in world.Options.Players ) {

                player.Currency += nukePoints;
                player.SendMessage( "Nuke Activated!" );

            }

        }

        public void Update() {

            if ( !Started ) return;


            if ( monstersSpawned == 0 && monsterSpawnCount == 0 ) {

                waveStartTime = world.LevelTime;
                StartWave();


            }


            if ( waveStartTime + waveDelay > world.LevelTime ) return;

            if ( monsterSpawnCount <= 0 ) return;
            SpawnMonster();

        }

        private void StartWave() {

            wave++;
            world.Options.Players[0].SendMessage( "Wave " + wave + " Starting..." );

            currentMonstersPerWave += monstersPerWaveIncrease;

            monsterSpawnCount = currentMonstersPerWave;
            currentMonsterHealthMultiplyer += monsterHealthMultiplyerIncrease;

        }

        private void SpawnMonster() {

            MobjType type = monsterTypes[0];
            if ( wave % specialMonstersWaveInterval == 0 ) type = MobjType.Troop;
            else if (wave > 10) {

                type = (new Random().Next(10) < 9) ? monsterTypes[0] : monsterTypes[1];

            }

            MapThing spawnPoint = spawnPoints[ new Random().Next( spawnPoints.Count ) ];

            if ( !CheckOpenPoint( Fixed.FromInt( 20 ), spawnPoint.X, spawnPoint.Y ) ) return;

            var mobj = world.ThingAllocation.SpawnMobj( spawnPoint.X, spawnPoint.Y, Mobj.OnFloorZ, type );
            mobj.SpawnPoint = spawnPoint;
            mobj.Health = (int) (float) currentMonsterHealthMultiplyer * mobj.Health;

            spawnedMobs.Add( mobj );
            monstersSpawned++;
            monsterSpawnCount--;

        }

        private bool CheckOpenPoint( Fixed radius, Fixed x, Fixed y ) {

            var thinkers = world.Thinkers;
            foreach ( Thinker thinker in thinkers ) {

                if ( thinker is not Mobj mobj ) continue;
                if ( ( mobj.Flags & MobjFlags.Solid ) == 0 ) continue;

                if ( mobj.X >= x - radius &&
                     mobj.X <= x + radius &&
                     mobj.Y >= y - radius &&
                     mobj.Y <= y + radius ) {

                    return false;

                }

            }

            return true;

        }

    }

}

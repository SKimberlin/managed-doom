using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace ManagedDoom {

    public sealed class WaveController {

        private int wave = 0;

        private int monsterSpawnCount = 0;
        private int monstersSpawned = 0;

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

        private World world;
        private List<MapThing> spawnPoints;

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

            }

            world.Options.Players[0].OnMobKilled += ( mobj ) => {

                monstersSpawned--;

            };

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

        public void StartWave() {

            wave++;
            world.Options.Players[0].SendMessage( "Wave " + wave + " Starting..." );

            currentMonstersPerWave += monstersPerWaveIncrease;

            monsterSpawnCount = currentMonstersPerWave;
            currentMonsterHealthMultiplyer += monsterHealthMultiplyerIncrease;

        }

        public void SpawnMonster() {

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
            monstersSpawned++;
            monsterSpawnCount--;

        }

        public bool CheckOpenPoint( Fixed radius, Fixed x, Fixed y ) {

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

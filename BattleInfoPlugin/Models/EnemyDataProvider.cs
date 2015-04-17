﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using BattleInfoPlugin.Models.Raw;
using BattleInfoPlugin.Properties;
using Grabacr07.KanColleWrapper;

namespace BattleInfoPlugin.Models
{
    [DataContract]
    public class EnemyDataProvider
    {
        private static DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(EnemyDataProvider));

        // EnemyId, EnemyMasterIDs
        [DataMember]
        private Dictionary<int, int[]> EnemyDictionary { get; set; }

        // EnemyId, Formation
        [DataMember]
        private Dictionary<int, Formation> EnemyFormation { get; set; }


        //以下はとりあえず保存だけする

        // EnemyId, api_eSlot
        [DataMember]
        private Dictionary<int, int[][]> EnemySlotItems { get; set; }

        // MapInfoID, CellNo, EnemyId
        [DataMember]
        private Dictionary<int, Dictionary<int, HashSet<int>>> MapEnemyData { get; set; }

        // MapInfoID, FromCellNo, ToCellNo
        [DataMember]
        private Dictionary<int, HashSet<KeyValuePair<int, int>>> MapRoute { get; set; }

        // MapInfoID, CellNo
        [DataMember]
        private Dictionary<int, int> MapBossCellNo { get; set; }

        [NonSerialized] private int currentEnemyID;

        [NonSerialized] private int previousCellNo;

        public EnemyDataProvider()
        {
            this.Reload();
            if (this.EnemyDictionary == null) this.EnemyDictionary = new Dictionary<int, int[]>();
            if (this.EnemyFormation == null) this.EnemyFormation = new Dictionary<int, Formation>();
            if (this.EnemySlotItems == null) this.EnemySlotItems = new Dictionary<int, int[][]>();
            if (this.MapEnemyData == null) this.MapEnemyData = new Dictionary<int, Dictionary<int, HashSet<int>>>();
            if (this.MapRoute == null) this.MapRoute = new Dictionary<int, HashSet<KeyValuePair<int, int>>>();
            if (this.MapBossCellNo == null) this.MapBossCellNo = new Dictionary<int, int>();
            this.previousCellNo = 0;
            this.Dump("GetNextEnemyFormation");
        }

        public Formation GetNextEnemyFormation(map_start_next startNext)
        {
            this.Dump("GetNextEnemyFormation");

            if (startNext.api_enemy == null) return Formation.なし;
            this.currentEnemyID = startNext.api_enemy.api_enemy_id;

            return this.EnemyFormation.ContainsKey(startNext.api_enemy.api_enemy_id)
                ? this.EnemyFormation[startNext.api_enemy.api_enemy_id]
                : Formation.不明;
        }

        public ShipData[] GetNextEnemies(map_start_next startNext)
        {
            this.Dump("GetNextEnemies");

            if (startNext.api_enemy == null) return new ShipData[0];
            this.currentEnemyID = startNext.api_enemy.api_enemy_id;

            var master = KanColleClient.Current.Master.Ships;
            return this.EnemyDictionary.ContainsKey(startNext.api_enemy.api_enemy_id)
                ? this.EnemyDictionary[startNext.api_enemy.api_enemy_id].Select(x => new ShipData(master[x])).ToArray()
                : Enumerable.Repeat(new ShipData(), 6).ToArray();
        }

        public void UpdateMapData(map_start_next startNext)
        {
            this.UpdateMapEnemyData(startNext);
            this.UpdateMapRoute(startNext);
            this.UpdateMapBossCellNo(startNext);
            this.Save();
            this.Dump("UpdateMapData");
        }

        private void UpdateMapEnemyData(map_start_next startNext)
        {
            if (startNext.api_enemy == null) return;

            var mapInfo = GetMapInfo(startNext);

            if (!this.MapEnemyData.ContainsKey(mapInfo))
                this.MapEnemyData.Add(mapInfo, new Dictionary<int, HashSet<int>>());
            if (!this.MapEnemyData[mapInfo].ContainsKey(startNext.api_no))
                this.MapEnemyData[mapInfo].Add(startNext.api_no, new HashSet<int>());

            this.MapEnemyData[mapInfo][startNext.api_no].Add(startNext.api_enemy.api_enemy_id);
        }

        private void UpdateMapRoute(map_start_next startNext)
        {
            var mapInfo = GetMapInfo(startNext);
            if (!this.MapRoute.ContainsKey(mapInfo))
                this.MapRoute.Add(mapInfo, new HashSet<KeyValuePair<int, int>>());

            this.MapRoute[mapInfo].Add(new KeyValuePair<int, int>(this.previousCellNo, startNext.api_no));

            this.previousCellNo = 0 < startNext.api_next ? startNext.api_no : 0;
        }

        private void UpdateMapBossCellNo(map_start_next startNext)
        {
            var mapInfo = GetMapInfo(startNext);
            if (!this.MapBossCellNo.ContainsKey(mapInfo))
                this.MapBossCellNo.Add(mapInfo, startNext.api_bosscell_no);
            else
                this.MapBossCellNo[mapInfo] = startNext.api_bosscell_no;
        }

        private static int GetMapInfo(map_start_next startNext)
        {
            return KanColleClient.Current.Master.MapInfos
                .Select(x => x.Value)
                .Where(m => m.MapAreaId == startNext.api_maparea_id)
                .Single(m => m.IdInEachMapArea == startNext.api_mapinfo_no)
                .Id;
        }

        public void UpdateEnemyData(int[] api_ship_ke, int[] api_formation, int[][] api_eSlot)
        {
            var enemies = api_ship_ke.Where(x => x != -1).ToArray();
            var formation = (Formation)api_formation[1];

            if (this.EnemyDictionary.ContainsKey(this.currentEnemyID))
                this.EnemyDictionary[this.currentEnemyID] = enemies;
            else
                this.EnemyDictionary.Add(this.currentEnemyID, enemies);

            if (this.EnemyFormation.ContainsKey(this.currentEnemyID))
                this.EnemyFormation[this.currentEnemyID] = formation;
            else
                this.EnemyFormation.Add(this.currentEnemyID, formation);

            if (this.EnemySlotItems.ContainsKey(this.currentEnemyID))
                this.EnemySlotItems[this.currentEnemyID] = api_eSlot;
            else
                this.EnemySlotItems.Add(this.currentEnemyID, api_eSlot);

            this.Save();
            this.Dump("UpdateEnemyData");
        }

        public void Dump(string title = "")
        {
            Debug.WriteLine(title);
            //this.EnemyDictionary.SelectMany(x => x.Value, (key, value) => new { key, value })
            //    .ToList().ForEach(x => Debug.WriteLine(x.key + " : " + x.value));
            //this.EnemyFormation
            //    .ToList().ForEach(x => Debug.WriteLine(x.Key + " : " + x.Value));
        }

        private void Reload()
        {
            //deserialize
            var path = Environment.CurrentDirectory + "\\" + Settings.Default.EnemyDataFilePath;
            if (!File.Exists(path)) return;

            using (var stream = Stream.Synchronized(new FileStream(path, FileMode.OpenOrCreate)))
            {
                var obj = serializer.ReadObject(stream) as EnemyDataProvider;
                if (obj == null) return;
                this.EnemyDictionary = obj.EnemyDictionary;
                this.EnemyFormation = obj.EnemyFormation;
                this.EnemySlotItems = obj.EnemySlotItems;
                this.MapEnemyData = obj.MapEnemyData;
                this.MapRoute = obj.MapRoute;
                this.MapBossCellNo = obj.MapBossCellNo;
            }
        }

        private void Save()
        {
            //serialize
            var path = Environment.CurrentDirectory + "\\" + Settings.Default.EnemyDataFilePath;
            using (var stream = Stream.Synchronized(new FileStream(path, FileMode.OpenOrCreate)))
            {
                serializer.WriteObject(stream, this);
            }
        }
    }
}

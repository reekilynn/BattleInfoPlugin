using System;
using System.Collections.Generic;
using System.Linq;

namespace BattleInfoPlugin.Models.Raw
{
    public static class CommonTypeExtensions
    {
        private static readonly FleetDamages defaultValue = new FleetDamages();

        #region �x��

        public static FleetDamages GetEnemyDamages(this Api_Support_Info support)
        {
            if (support == null) return defaultValue;
            if (support.api_support_airatack != null
                && support.api_support_airatack.api_stage3 != null
                && support.api_support_airatack.api_stage3.api_edam != null)
            {
                return support.api_support_airatack.api_stage3.api_edam.GetDamages();
            }
            if (support.api_support_hourai != null
                && support.api_support_hourai.api_damage != null)
            {
                return support.api_support_hourai.api_damage.GetDamages();
            }
            return defaultValue;
        }

        #endregion

        #region �C��

        public static FleetDamages GetFriendDamages(this Hougeki hougeki)
        {
            return hougeki != null
                ? hougeki.api_damage != null
                    ? hougeki.api_damage.GetFriendDamages(hougeki.api_df_list)
                    : defaultValue : defaultValue;
        }

        public static FleetDamages GetEnemyDamages(this Hougeki hougeki)
        {
            return hougeki != null
                ? hougeki.api_damage != null
                    ? hougeki.api_damage.GetEnemyDamages(hougeki.api_df_list)
                    : defaultValue : defaultValue;
        }

        #endregion

        #region ���

        public static FleetDamages GetFriendDamages(this Midnight_Hougeki hougeki)
        {
            return hougeki != null
                ? hougeki.api_damage != null
                    ? hougeki.api_damage.GetFriendDamages(hougeki.api_df_list)
                    : defaultValue : defaultValue;
        }
        public static FleetDamages GetEnemyDamages(this Midnight_Hougeki hougeki)
        {
            return hougeki != null
                ? hougeki.api_damage != null
                    ? hougeki.api_damage.GetEnemyDamages(hougeki.api_df_list)
                    : defaultValue : defaultValue;
        }

        #endregion

        #region �q���

        public static FleetDamages GetFirstFleetDamages(this Api_Kouku kouku)
        {
            return kouku != null
                ? kouku.api_stage3 != null
                    ? kouku.api_stage3.api_fdam.GetDamages()
                    : defaultValue : defaultValue;
        }
        public static FleetDamages GetSecondFleetDamages(this Api_Kouku kouku)
        {
            return kouku != null
                ? kouku.api_stage3_combined != null
                    ? kouku.api_stage3_combined.api_fdam != null
                        ? kouku.api_stage3_combined.api_fdam.GetDamages()
                        : defaultValue : defaultValue : defaultValue;
        }
        public static FleetDamages GetEnemyDamages(this Api_Kouku kouku)
        {
            return kouku != null
                ? kouku.api_stage3 != null
                    ? kouku.api_stage3.api_edam != null
                        ? kouku.api_stage3.api_edam.GetDamages()
                        : defaultValue : defaultValue : defaultValue;
        }

        public static AirSupremacy GetAirSupremacy(this Api_Kouku kouku)
        {
            return kouku != null && kouku.api_stage1 != null
                ? (AirSupremacy) kouku.api_stage1.api_disp_seiku
                : AirSupremacy.�q���Ȃ�;
        }

        #endregion

        #region ������

        public static FleetDamages GetFriendDamages(this Raigeki raigeki)
        {
            return raigeki != null
                ? raigeki.api_fdam != null
                    ? raigeki.api_fdam.GetDamages()
                    : defaultValue : defaultValue;
        }
        public static FleetDamages GetEnemyDamages(this Raigeki raigeki)
        {
            return raigeki != null
                ? raigeki.api_edam != null
                    ? raigeki.api_edam.GetDamages()
                    : defaultValue : defaultValue;
        }

        #endregion

        #region �_���[�W�v�Z

        /// <summary>
        /// 12���ڒ��擪6���ڎ擾
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="origin">�S�~-1���t���Ă�ꍇ1�I���W��</param>
        /// <returns></returns>
        public static IEnumerable<T> GetFriendData<T>(this IEnumerable<T> source, int origin = 1)
        {
            return source.Skip(origin).Take(6);
        }

        /// <summary>
        /// 12���ڒ�����6���ڎ擾
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="origin">�S�~-1���t���Ă�ꍇ1�I���W��</param>
        /// <returns></returns>
        public static IEnumerable<T> GetEnemyData<T>(this IEnumerable<T> source, int origin = 1)
        {
            return source.Skip(origin + 6).Take(6);
        }

        /// <summary>
        /// �����E�q���_���[�W���X�g�Z�o
        /// </summary>
        /// <param name="damages">api_fdam/api_edam</param>
        /// <returns></returns>
        public static FleetDamages GetDamages(this double[] damages)
        {
            return damages
                .GetFriendData()    //�G��������
                .Select(Convert.ToInt32)
                .ToArray()
                .ToFleetDamages();
        }

        #region �C����_���[�W���X�g�Z�o

        /// <summary>
        /// �C����F�R�_���[�W���X�g�Z�o
        /// </summary>
        /// <param name="damages">api_damage</param>
        /// <param name="df_list">api_df_list</param>
        /// <returns></returns>
        public static FleetDamages GetFriendDamages(this object[] damages, object[] df_list)
        {
            return damages
                .ToIntArray()
                .ToSortedDamages(df_list.ToIntArray())
                .GetFriendData(0)
                .ToFleetDamages();
        }

        /// <summary>
        /// �C����G�R�_���[�W���X�g�Z�o
        /// </summary>
        /// <param name="damages">api_damage</param>
        /// <param name="df_list">api_df_list</param>
        /// <returns></returns>
        public static FleetDamages GetEnemyDamages(this object[] damages, object[] df_list)
        {
            return damages
                .ToIntArray()
                .ToSortedDamages(df_list.ToIntArray())
                .GetEnemyData(0)
                .ToFleetDamages();
        }

        /// <summary>
        /// �C����_���[�W���X�gint�z��
        /// �e���ϑ��ˌ��f�[�^�̓t���b�g������
        /// api_df_list�����l�̌^�Ȃ̂ŗ��p�\
        /// </summary>
        /// <param name="damages">api_damage</param>
        /// <returns></returns>
        private static int[] ToIntArray(this object[] damages)
        {
            return damages
                .Where(x => x is Array)
                .Select(x => ((Array)x).Cast<object>())
                .SelectMany(x => x.Select(Convert.ToInt32))
                .ToArray();
        }

        /// <summary>
        /// �t���b�g������api_damage��api_df_list������
        /// ���R6�ǁ{�G�R6�ǂ̒���12�̃_���[�W���v�z����쐬
        /// </summary>
        /// <param name="damages">api_damage</param>
        /// <param name="dfList">api_df_list</param>
        /// <returns></returns>
        private static int[] ToSortedDamages(this int[] damages, int[] dfList)
        {
            var zip = damages.Zip(dfList, (da, df) => new { df, da });
            var ret = new int[12];
            foreach (var d in zip)
            {
                ret[d.df - 1] += d.da;
            }
            return ret;
        }

        #endregion

        #endregion
    }
}
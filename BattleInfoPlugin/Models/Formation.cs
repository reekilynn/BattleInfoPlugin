using System.Runtime.Serialization;

namespace BattleInfoPlugin.Models
{
    [DataContract]
    public enum Formation
    {
        [EnumMember]
        �s�� = -1,
        [EnumMember]
        �Ȃ� = 0,
        [EnumMember]
        �P�c�w = 1,
        [EnumMember]
        ���c�w = 2,
        [EnumMember]
        �֌`�w = 3,
        [EnumMember]
        ��`�w = 4,
        [EnumMember]
        �P���w = 5,
        [EnumMember]
        �ΐ��w�` = 11,
        [EnumMember]
        �O���w�` = 12,
        [EnumMember]
        �֌`�w�` = 13,
        [EnumMember]
        �퓬�w�` = 14,
    }
}
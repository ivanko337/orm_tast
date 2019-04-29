using System;
using ORM;
using ORM.Attributes;

namespace WPF_task_new_level_2
{
    [TableName("PROFILES")]
    [SequenceName("INCREMENT_ID")]
    public class Profile : DatabaseObject
    {
        [FieldName("PROFILE_ID")]
        [PrimaryKey]
        public decimal Id { get; set; }

        [FieldName("PROFILE_URL")]
        public string Url { get; set; } = "https://vk.com/";

        [FieldName("REGISTER_DATE")]
        public DateTime? RegisterDate { get; set; } = null;

        [FieldName("P_NAME")]
        public string Name { get; set; }

        [FieldName("P_SURNAME")]
        public string Surname { get; set; }

        [FieldName("FRIENDS_COUNT")]
        public decimal? FriendsCount { get; set; } = null;
    }
}

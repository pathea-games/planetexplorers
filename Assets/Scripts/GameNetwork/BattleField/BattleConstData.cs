using System.Collections;
using System.Collections.Generic;

using System;

using Mono.Data.SqliteClient;

internal class BattleConstData
{
    internal int _camp_min;
    internal int _camp_max;
    internal int _player_max;
    internal float _win_point;
    internal int _win_site;
    internal int _win_kill;
    internal float _points_kill;
    internal float _points_assist;
    internal float _points_fell;
    internal float _points_dig;
    internal float _points_build;
    internal float _points_site;
	internal float _points_capture;
    internal int _meat_kill;
    internal int _meat_assist;
    internal int _meat_site;
    internal int _site_interval;
    internal int _meat_time;

    private static BattleConstData _instance;
    internal static BattleConstData Instance
    {
        get
        {
            if (null == _instance)
                _instance = new BattleConstData();

            return _instance;
        }
    }

	internal static string _scoreInfo = string.Empty;
	internal static string _meatInfo = string.Empty;

    internal static void LoadData()
    {
        SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("multi_AS");

        while (reader.Read())
        {
            Instance._camp_min = reader.GetInt32(reader.GetOrdinal("camp_min"));
            Instance._camp_max = reader.GetInt32(reader.GetOrdinal("camp_max"));
            Instance._player_max = reader.GetInt32(reader.GetOrdinal("player_max"));
            Instance._win_point = reader.GetFloat(reader.GetOrdinal("win_point"));
            Instance._win_site = reader.GetInt32(reader.GetOrdinal("win_site"));
            Instance._win_kill = reader.GetInt32(reader.GetOrdinal("win_kill"));
            Instance._points_kill = reader.GetFloat(reader.GetOrdinal("point_kill"));
            Instance._points_assist = reader.GetFloat(reader.GetOrdinal("point_assist"));
            Instance._points_fell = reader.GetFloat(reader.GetOrdinal("point_fell"));
            Instance._points_dig = reader.GetFloat(reader.GetOrdinal("point_dig"));
            Instance._points_build = reader.GetFloat(reader.GetOrdinal("point_build"));
			Instance._points_capture = reader.GetFloat(reader.GetOrdinal("point_capture"));
            Instance._points_site = reader.GetFloat(reader.GetOrdinal("point_site"));
            Instance._meat_kill = reader.GetInt32(reader.GetOrdinal("meat_kill"));
            Instance._meat_assist = reader.GetInt32(reader.GetOrdinal("meat_assist"));
            Instance._meat_site = reader.GetInt32(reader.GetOrdinal("meat_site"));
            Instance._site_interval = reader.GetInt32(reader.GetOrdinal("site_interval"));
            Instance._meat_time = reader.GetInt32(reader.GetOrdinal("meat_time"));
        }

		_scoreInfo = string.Format(@"
Player Slain:{0}
Assist:{1}
Dig:{2}
Chop:{3}
Build a Block:{4}
Territory Captured:{5}
Territory Hold Per {6} Seconds:{7}", Instance._points_kill, Instance._points_assist, Instance._points_dig, Instance._points_fell, Instance._points_build, Instance._points_capture, Instance._site_interval, Instance._points_site);

		_meatInfo = string.Format(@"
Player Slain:{0}
Assist:{1}
Territory Captured:{2}
Territory Hold Per {3} Seconds:{4}", Instance._meat_kill, Instance._meat_assist, Instance._meat_site, Instance._site_interval, Instance._meat_time);
    }
}

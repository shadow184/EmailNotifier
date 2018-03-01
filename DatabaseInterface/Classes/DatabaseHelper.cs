﻿using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseInterface.Classes
{
    public static class DatabaseHelper
    {
        public static void ExecuteNonQuery(string sql, string workingDir)
        {
            using (var sqlConn = new SQLiteConnection($"Data Source={workingDir}\\SonarrInfoDatabase;Version=3;"))
            {
                sqlConn.Open();

                using (var sqlCmd = new SQLiteCommand(sql, sqlConn))
                {
                    sqlCmd.ExecuteNonQuery();
                }
            }
        }
    }
}
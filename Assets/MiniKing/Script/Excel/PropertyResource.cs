﻿using Spring.Storage.Model.Anno;

 namespace MiniKing.Script.Excel
{
    [Resource]
    public class PropertyResource
    {
        [Id]
        public string key;

        public string value;
    }
}
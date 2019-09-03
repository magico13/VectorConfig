﻿using AccountManagement;
using System;

namespace VectorConfig
{
    class Program
    {
        const string EMAIL = "";
        const string PASSWORD = "";

        static void Main(string[] args)
        {
            AnkiAccountManager manager = new AnkiAccountManager();

            System.Net.Http.HttpResponseMessage result = manager.Login(EMAIL, PASSWORD);

            Console.WriteLine(result);
            Console.WriteLine(result.Content.ReadAsStringAsync().Result);
        }
    }
}

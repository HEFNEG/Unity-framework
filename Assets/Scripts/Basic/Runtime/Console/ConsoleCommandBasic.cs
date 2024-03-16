using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Game.Basic.Console {
    public static class ConsoleCommandBasic{

        public static void RegisteredWaitHandle() {
            AppBootstrap.console.RegisterCmd("clear", ClearConsole);
        }

        private static void ClearConsole(string[] args) {
            AppBootstrap.console.ClearConsole();
        }
    }
}

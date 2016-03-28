using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Drace.Optimization.FSSP {
    class TabuList {
        private int[,] tabuList;
        private int tabuLength;

        public int TabuLength {
            get {
                return this.tabuLength;
            }
        }

        public bool IsTabu(int i, int j, int loop) {
            return !IsNotTabu(i, j, loop);
        }

        public bool IsNotTabu(int i, int j, int loop) {
            return tabuList[i, j] <= loop;
        }

        public void AddList(int i, int j, int loop) {
            tabuList[i, j] = tabuList[j, i] = loop + tabuLength + 1;
        }

        public TabuList(FlowShopSchedulingProblem fssp, int tabuLength) {
            this.tabuLength = tabuLength;
            tabuList = new int[fssp.NumberOfItems, fssp.NumberOfItems];
        }
    }
}

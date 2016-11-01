/*
* Copyright 2016 NHN Entertainment Corp.
*
* NHN Entertainment Corp. licenses this file to you under the Apache License,
* version 2.0 (the "License"); you may not use this file except in compliance
* with the License. You may obtain a copy of the License at:
*
*   http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

using System;
using System.Threading;

namespace Haste.Network
{
    internal class RoundTripTime
    {
        internal const int InitialMeanOfRoundTripTime = 1000;
        internal const int InitialMeanOfRoundTripTimeVariance = 20;

        protected int _meanOfRoundTripTime;
        protected int _meanOfRoundTripTimeVariance = 1;

        protected int _lowestRoundTripTime = 0;
        protected int _highestRoundTripTimeVariance = 0;

        internal int MeanOfRoundTripTime { get { return _meanOfRoundTripTime; } set { Interlocked.Exchange(ref _meanOfRoundTripTime, value); } }

        internal int MeanOfRoundTripTimeVariance { get { return _meanOfRoundTripTimeVariance; } }

        internal int LowestRoundTripTime { get { return _lowestRoundTripTime; } }

        internal int HighestRoundTripTimeVariance { get { return _highestRoundTripTimeVariance; } }

        internal RoundTripTime()
        {
            _meanOfRoundTripTime = InitialMeanOfRoundTripTime;
            _meanOfRoundTripTimeVariance = InitialMeanOfRoundTripTimeVariance;
        }

        internal uint NewRoundTripTime()
        {
            return (uint) (MeanOfRoundTripTime + (MeanOfRoundTripTimeVariance << 2));
        }

        /// <summary>
        /// updates a round trip time to mean of round trip time, and varaince.
        /// </summary>
        /// <param name="currentRoundTripTime"></param>
        internal void Update(int lastRoundtripTime)
        {
            if (lastRoundtripTime < 0)
                throw new ArgumentException("RoundTripTime Error");

            int rtt = _meanOfRoundTripTime;
            int newRtt = rtt;

            int rttVar = _meanOfRoundTripTimeVariance;
            int newRttVar = rttVar;

            newRttVar -= (rttVar >> 3);
            newRtt -= (rtt >> 3);

            int offset = lastRoundtripTime >> 3;
            int var = Math.Abs(lastRoundtripTime - rtt) >> 3;

            newRtt += offset;
            newRttVar += var;

            if (newRttVar <= 0)
                rttVar = 1;

            if (newRtt <= 0)
                newRtt = 1;

            Interlocked.Exchange(ref _meanOfRoundTripTime, newRtt);
            Interlocked.Exchange(ref _meanOfRoundTripTimeVariance, rttVar);

            if (newRtt < _lowestRoundTripTime)
            {
                Interlocked.Exchange(ref _lowestRoundTripTime, newRtt);
            }

            if (newRttVar > _highestRoundTripTimeVariance)
            {
                Interlocked.Exchange(ref _highestRoundTripTimeVariance, newRttVar);
            }
        }
    }
}

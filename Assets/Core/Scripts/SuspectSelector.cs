using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using InvestigationGame.Data;

namespace InvestigationGame.Core
{
    public static class SuspectSelector
    {
        public static List<SuspectData> PickSuspects(List<SuspectData> masterPool, int culpritCount = 1, int innocentCount = 3)
        {
            if (masterPool == null || masterPool.Count < (culpritCount + innocentCount))
            {
                Debug.LogWarning("Master pool doesn't have enough suspects. Returning what we have.");
                return masterPool?.ToList() ?? new List<SuspectData>();
            }

            var culprits = masterPool.Where(s => s.Role != SuspectRole.OrangBiasa).ToList();
            var innocents = masterPool.Where(s => s.Role == SuspectRole.OrangBiasa).ToList();

            if (culprits.Count < culpritCount)
            {
                Debug.LogWarning("Not enough 'Culprit' suspects found in master pool! Picking random suspects.");
                return masterPool.OrderBy(x => Random.value).Take(culpritCount + innocentCount).ToList();
            }

            if (innocents.Count < innocentCount)
            {
                Debug.LogWarning("Not enough 'Orang Biasa' suspects found! Picking random suspects.");
                return masterPool.OrderBy(x => Random.value).Take(culpritCount + innocentCount).ToList();
            }

            var selectedCulprits = culprits.OrderBy(x => Random.value).Take(culpritCount).ToList();
            var selectedInnocents = innocents.OrderBy(x => Random.value).Take(innocentCount).ToList();

            var combined = new List<SuspectData>();
            combined.AddRange(selectedCulprits);
            combined.AddRange(selectedInnocents);

            // Shuffle
            return combined.OrderBy(x => Random.value).ToList();
        }
    }
}
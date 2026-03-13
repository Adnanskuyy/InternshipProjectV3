using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using InvestigationGame.Data;

namespace InvestigationGame.Core
{
    public static class SuspectSelector
    {
        public static List<SuspectData> PickSuspects(List<SuspectData> masterPool)
        {
            if (masterPool == null || masterPool.Count < 4)
            {
                Debug.LogWarning("Master pool doesn't have enough suspects (need at least 4). Returning what we have.");
                return masterPool?.ToList() ?? new List<SuspectData>();
            }

            var culprits = masterPool.Where(s => s.Role != SuspectRole.OrangBiasa).ToList();
            var innocents = masterPool.Where(s => s.Role == SuspectRole.OrangBiasa).ToList();

            if (culprits.Count < 1)
            {
                Debug.LogWarning("No 'Culprit' (Pengguna/Pengedar) suspect found in master pool! Picking random suspects.");
                return masterPool.OrderBy(x => Random.value).Take(4).ToList();
            }

            if (innocents.Count < 3)
            {
                Debug.LogWarning("Not enough 'Orang Biasa' suspects found! Picking random suspects.");
                return masterPool.OrderBy(x => Random.value).Take(4).ToList();
            }

            var selectedCulprits = culprits.OrderBy(x => Random.value).Take(1).ToList();
            var selectedInnocents = innocents.OrderBy(x => Random.value).Take(3).ToList();

            var combined = new List<SuspectData>();
            combined.AddRange(selectedCulprits);
            combined.AddRange(selectedInnocents);

            // Shuffle
            return combined.OrderBy(x => Random.value).ToList();
        }
    }
}
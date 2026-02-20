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

            var users = masterPool.Where(s => s.IsUser).ToList();
            var nonUsers = masterPool.Where(s => !s.IsUser).ToList();

            if (users.Count < 1)
            {
                Debug.LogWarning("No 'User' suspect found in master pool! Picking random suspects.");
                return masterPool.OrderBy(x => Random.value).Take(4).ToList();
            }

            if (nonUsers.Count < 3)
            {
                Debug.LogWarning("Not enough 'Non-User' suspects found! Picking random suspects.");
                return masterPool.OrderBy(x => Random.value).Take(4).ToList();
            }

            var selectedUsers = users.OrderBy(x => Random.value).Take(1).ToList();
            var selectedNonUsers = nonUsers.OrderBy(x => Random.value).Take(3).ToList();

            var combined = new List<SuspectData>();
            combined.AddRange(selectedUsers);
            combined.AddRange(selectedNonUsers);

            // Shuffle
            return combined.OrderBy(x => Random.value).ToList();
        }
    }
}
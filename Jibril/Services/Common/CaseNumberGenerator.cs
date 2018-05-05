using System;
using System.Collections.Generic;
using Discord;
using Jibril.Modules.Administration.Services;

namespace Jibril.Services.Common
{
    public class CaseNumberGenerator
    {
        public static List<int> InsertCaseID(IUser user)
        {
            var time = DateTime.Now;
            AdminDb.AddActionCase(user, time);
            var caseId = AdminDb.GetActionCaseId(time);
            return caseId;
        }

        public static void UpdateCase(string msgid, int id)
        {
            AdminDb.UpdateActionCase(msgid, id);
        }
    }
}
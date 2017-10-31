using Discord;
using Jibril.Modules.Administration.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace Jibril.Services.Common
{
    public class CaseNumberGenerator
    {
        public static List<int> InsertCaseID(IUser user)
        {
            var time = DateTime.Now;
            AdminDb.AddActionCase(user, time);
            var caseId = AdminDb.GetActionCaseID(time);
            return caseId;
        }

        public static void UpdateCase(string msgid, int id)
        {
            AdminDb.UpdateActionCase(msgid, id);
            return;
        }
    }
}

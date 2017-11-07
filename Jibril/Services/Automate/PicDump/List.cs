using Google.Apis.Drive.v3;
using System;
using System.Collections.Generic;
using System.Text;

namespace Jibril.Services.Automate.PicDump
{
    public class List
    {
        public class FilesListOptionalParms
        {
            public string Corpus { get; set; }
            public string OrderBy { get; set; }
            public int? PageSize { get; set; }
            public string PageToken { get; set; }
            public string Q { get; set; }
            public string Spaces { get; set; }
            public string Fields { get; set; }
            public string QuotaUser { get; set; }
            public string UserIp { get; set; }
        }

        public static Google.Apis.Drive.v3.Data.FileList ListFiles(DriveService service, FilesListOptionalParms optional = null)
        {
            try
            {
                if (service == null)
                    throw new ArgumentNullException("service");

                var request = service.Files.List();
                request = (FilesResource.ListRequest)SampleHelpers.ApplyOptionalParms(request, optional);
                return request.Execute();
            }
            catch (Exception ex)
            {
                throw new Exception("Request Files.List failed.", ex);
            }
        }


    }
    public static class SampleHelpers
    {
        public static object ApplyOptionalParms(object request, object optional)
        {
            if (optional == null)
                return request;

            System.Reflection.PropertyInfo[] optionalProperties = (optional.GetType()).GetProperties();

            foreach (System.Reflection.PropertyInfo property in optionalProperties)
            {
                System.Reflection.PropertyInfo piShared = (request.GetType()).GetProperty(property.Name);
                if (property.GetValue(optional, null) != null)
                    piShared.SetValue(request, property.GetValue(optional, null), null);
            }

            return request;
        }
    }
}

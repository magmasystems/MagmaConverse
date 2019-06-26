using System;
using System.IO;
using MagmaConverse.Data;
using MagmaConverse.Data.Fields;
using MagmaConverse.Framework;

namespace MagmaConverse.Views.Console
{
    public class ConsoleUploadField : ConsoleEditField
    {
        public ConsoleUploadField(ISBSFormField formField, ISBSFormView formView) : base(formField, formView)
        {
        }

        public override bool Validate()
        {
            if (!base.Validate())
                return false;

            // Make sure that the file exists
            var filename = this.SBSFormField.Value as string;
            if (!File.Exists(filename))
            {
                this.ColoredOutput($"The file [{filename}] does not exist", ConsoleColor.Red);
            }

            return true;
        }

        public override FieldActionResult PerformActions(int idxCurrent)
        {
            var rc = base.PerformActions(idxCurrent);

            var filename = this.SBSFormField.Value as string;
            if (string.IsNullOrEmpty(filename))
                return rc;

            SBSUploadField uploadField = (SBSUploadField) this.SBSFormField;

            /*
              "properties": {
                "target":  "database",
                "table":   "BusinessPhotos",
                "column":  "Image" 
               }
            */

            Properties props = this.SBSFormField.Properties;

            // We need to know where to upload the contents to. It could be to a database, another file on the server, or an in-memory cache on the server.
            using (FileStream fs = new FileStream(filename, FileMode.Open))
            {
                var len = fs.Length;
                var chunksize = props?.Get("chunksize", (int) len) ?? (int) len;
                var offset = 0;
                var data = new byte[chunksize];

                while (len > 0)
                {
                    int n = fs.Read(data, offset, chunksize);
                    len -= n;
                    offset += n;

                    // TODO - transfer the chunk
                }

                uploadField.UploadedData = data;
                uploadField.WasUploaded = true;
                uploadField.Length = fs.Length;
            }

            return rc;
        }
    }
}
namespace MagmaConverse.Data.Fields
{
    public class SBSUploadField : SBSEditField
    {
        // The value of the edit field is the filename
        public object UploadedData { get; set; }
        public bool WasUploaded { get; set; }
        public long Length { get; set; }
    }
}